using StackExchange.Redis;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Crunch.Models.Experiments;
using System.Threading.Tasks;
using Crunch.Extensions;
using Crunch.Services;
using Crunch.Contexts;
using EasyIoC;
using EasyIoC.Attributes;

namespace Crunch.Repositories
{
    public interface IParticipantRepository
    {
        string EnrollParticipantInTest(string clientId, string test);
        void RecordGoal(string clientId, string goal, IDatabase dbContext = null);
    }

    [AutoRegister(ServiceLifetime.Singleton)]
    public class ParticipantRepository : IParticipantRepository {
        private readonly IVariantPicker _variantPicker;
        private readonly ILogger<TestRepository> _logger;
        private readonly IDBConnector _dbConnector;
        private readonly ITestContext _testContext;
        private readonly IParticipantContext _participantContext;
        
        public ParticipantRepository(
            ILogger<TestRepository> logger,
            IDBConnector dbConnector,
            IVariantPicker variantPicker,
            ITestContext testContext,
            IParticipantContext participantContext) {
            _logger = logger;
            _variantPicker = variantPicker;
            _dbConnector = dbConnector;
            _testContext = testContext;
            _participantContext = participantContext;
        }

        public string EnrollParticipantInTest(string clientId, string test) {
            var db = _dbConnector.GetDatabase();
            
            var currentTest = _testContext.GetTest(test, dbContext: db);

            // check to see if the test is valid
            if (currentTest == null) {
                throw new Exception(
                    "Test {0} has not been configured, cannot enroll".FormatWith(test));
            }

            var participant = _participantContext.GetParticipant(clientId);
            var selectedVariant = _variantPicker.SelectVariant(currentTest);

            // check to see if the user has already been setup
            if (participant == null) {
                _logger.LogDebug("User was not setup with any tests, creating key");
                participant = new Participant {
                    ClientId = clientId,
                    EnrolledTests = new List<Test> ()
                };
            }

            var existingTest = participant.EnrolledTests.Where(x => x.Name == test).FirstOrDefault();

            if (existingTest == null) {
                _logger.LogDebug("User did not have test, setting up");
                participant.EnrolledTests = participant.EnrolledTests.Append(new Test {
                    Name = test,
                    SelectedVariant = selectedVariant,
                    VersionAtSelection = currentTest.Version
                });
                db.StringIncrement(DatabaseKeys.ExperimentCurrentCountKeyFormat.FormatWith(test));
                db.StringIncrement(DatabaseKeys.ExperimentVariantVersionKeyFormat.FormatWith(test, currentTest.Version, selectedVariant));
            } else {
                if (existingTest.VersionAtSelection == currentTest.Version) {
                    _logger.LogDebug("User already had variant");
                    return existingTest.SelectedVariant;
                } else {
                    _logger.LogDebug("User variant version differs, updating");
                    existingTest.SelectedVariant = selectedVariant;
                    existingTest.VersionAtSelection = currentTest.Version;
                    db.StringIncrement(DatabaseKeys.ExperimentCurrentCountKeyFormat.FormatWith(test));

                    // clean existing recorded goals for the new test
                    var goalsAsSet = _participantContext.GetParticipantsGoals(participant.ClientId, db);
                    var experimentGoals = goalsAsSet.Keys.Where(x => x.StartsWith(test+":")).ToList();

                    var compoundCommands = new List<Task>();
                    compoundCommands.Add(
                        db.StringIncrementAsync(DatabaseKeys.ExperimentVariantVersionKeyFormat.FormatWith(test, currentTest.Version, selectedVariant))
                    );
                    experimentGoals.ForEach(x => compoundCommands.Add(db.SetRemoveAsync(
                            DatabaseKeys.ParticipantGoalsFormat.FormatWith(clientId),
                            x)));
                    db.WaitAll(compoundCommands.ToArray());
                }
            }

            var participantBytes = participant.SerializeAsJson();
            db.StringSet(DatabaseKeys.ParticipantKeyFormat.FormatWith(clientId), participantBytes);

            return selectedVariant;
        }

        public void RecordGoal(string clientId, string goal, IDatabase dbContext = null) {
            var db = dbContext ?? _dbConnector.GetDatabase();

            var participant = _participantContext.GetParticipant(clientId, db);

            if (participant == null) {
                return;
            }

            var goalsAsSet = _participantContext.GetParticipantsGoals(clientId, db);
            var compoundCommands = new List<Task>();

            foreach(var test in participant.EnrolledTests) {
                if(goalsAsSet.ContainsKey("{0}:{1}".FormatWith(test.Name, goal))) {
                    _logger.LogDebug("Previously recorded {0} for test {1}".FormatWith(goal, test.Name));
                    continue;
                }

                _logger.LogDebug("Tracking {0} for test {1}".FormatWith(goal, test.Name));
                var setRecorded = db.SetAddAsync(
                        DatabaseKeys.ParticipantGoalsFormat.FormatWith(clientId),
                        "{0}:{1}".FormatWith(test.Name, goal));
                var incrementCounter = db.StringIncrementAsync(
                    DatabaseKeys.ExperimentGoalCountKeyFormat.FormatWith(
                        test.Name,
                        test.VersionAtSelection,
                        test.SelectedVariant,
                        goal));

                compoundCommands.Add(setRecorded);
                compoundCommands.Add(incrementCounter);
            }

            db.WaitAll(compoundCommands.ToArray());
        }
    }
}
