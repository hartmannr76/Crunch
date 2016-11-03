using StackExchange.Redis;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using NuGet.Protocol.Core.v3;
using System.Linq;
using Crunch.Models.Experiments;
using Crunch.Shared.Models.Experiments;
using System.Threading.Tasks;
using Crunch.Attributes;
using Crunch.Extensions;

namespace Crunch.Services
{
    public interface IDBConnector
    {
        void ConfigureTest(string name, TestConfiguration config);
        TestConfiguration GetTest(string name, IDatabase dbContext = null);
        string EnrollParticipantInTest(string clientId, string test);
        void RecordGoal(string clientId, string goal, IDatabase dbContext = null);
        long GetConversionCountForTestAndVariant(
                string test,
                string variant, 
                long version,
                string goal);
        long GetTotalUserCountForVariant(string test, string variant, long version);
        bool IsConnected { get; }
    }

    [AutoRegister(ServiceLifetime.Singleton)]
    public class DBConnector : IDBConnector {
        #region DB Key Constants
        private const string ExperimentKeyFormat = "e:{0}";
        private const string ExperimentCurrentCountKeyFormat = "e:{0}:_all";
        private const string ExperimentVariantVersionKeyFormat = "e:{0}:{1}:v{2}";
        private const string ExperimentGoalCountKeyFormat = "e:{0}:{1}:v:{2}:g:{3}";
        private const string ParticipantKeyFormat = "p:{0}";
        private const string ParticipantGoalsFormat = "p:{0}:_goals";
        #endregion

        private readonly ConnectionMultiplexer _redisContext;
        private readonly IVariantPicker _variantPicker;
        private readonly ILogger<DBConnector> _logger;
        public bool IsConnected { get; private set; }
        
        public DBConnector(
            ILogger<DBConnector> logger,
            IVariantPicker variantPicker) {
            _logger = logger;
            _variantPicker = variantPicker;

            try {
                var redisHost = Environment.GetEnvironmentVariable("REDIS_PORT_6379_TCP_ADDR");
                _redisContext = ConnectionMultiplexer.Connect(redisHost);
                IsConnected = true;
            } catch (Exception e) {
                logger.LogCritical(e.Message);

                IsConnected = false;
            }
        }

        public string EnrollParticipantInTest(string clientId, string test) {
            var db = _redisContext.GetDatabase();
            
            var currentTest = GetTest(test, dbContext: db);

            // check to see if the test is valid
            if (currentTest == null) {
                throw new Exception(
                    "Test {0} has not been configured, cannot enroll".FormatWith(test));
            }

            var participant = GetParticipant(clientId);
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
                db.StringIncrement(ExperimentCurrentCountKeyFormat.FormatWith(test));
                db.StringIncrement(ExperimentVariantVersionKeyFormat.FormatWith(test, currentTest.Version, selectedVariant));
            } else {
                if (existingTest.VersionAtSelection == currentTest.Version) {
                    _logger.LogDebug("User already had variant");
                    return existingTest.SelectedVariant;
                } else {
                    _logger.LogDebug("User variant version differs, updating");
                    existingTest.SelectedVariant = selectedVariant;
                    existingTest.VersionAtSelection = currentTest.Version;
                    db.StringIncrement(ExperimentCurrentCountKeyFormat.FormatWith(test));

                    // clean existing recorded goals for the new test
                    var goalsAsSet = GetParticipantGoalSet(participant.ClientId, db);
                    var experimentGoals = goalsAsSet.Keys.Where(x => x.StartsWith(test+":")).ToList();

                    var compoundCommands = new List<Task>();
                    compoundCommands.Add(
                        db.StringIncrementAsync(ExperimentVariantVersionKeyFormat.FormatWith(test, currentTest.Version, selectedVariant))
                    );
                    experimentGoals.ForEach(x => compoundCommands.Add(db.SetRemoveAsync(
                            ParticipantGoalsFormat.FormatWith(clientId),
                            x)));
                    db.WaitAll(compoundCommands.ToArray());
                }
            }

            var participantBytes = SerializeObject(participant);
            db.StringSet(ParticipantKeyFormat.FormatWith(clientId), participantBytes);

            return selectedVariant;
        }

        public void ConfigureTest(string name, TestConfiguration config)
        {  
            _logger.LogDebug(name);
            var db = _redisContext.GetDatabase();
            var currentTest = GetTest(name, dbContext: db);
            byte[] configBytes;

            if (currentTest == null) {
                _logger.LogDebug("test {0} does not exist, creating".FormatWith(name));
                config.Version = 1;
                configBytes = SerializeObject(config);
            } else {
                _logger.LogDebug("test {0} exists, updating".FormatWith(name));
                currentTest.Variants = config.Variants;
                currentTest.Version += 1;

                configBytes = SerializeObject(currentTest);
            }

            db.StringSet(ExperimentKeyFormat.FormatWith(name), configBytes);
        }

        public void RecordGoal(string clientId, string goal, IDatabase dbContext = null) {
            var db = dbContext ?? _redisContext.GetDatabase();

            var participant = GetParticipant(clientId, db);

            if (participant == null) {
                return;
            }

            var goalsAsSet = GetParticipantGoalSet(clientId, db);
            var compoundCommands = new List<Task>();

            foreach(var test in participant.EnrolledTests) {
                if(goalsAsSet.ContainsKey("{0}:{1}".FormatWith(test.Name, goal))) {
                    _logger.LogDebug("Previously recorded {0} for test {1}".FormatWith(goal, test.Name));
                    continue;
                }

                _logger.LogDebug("Tracking {0} for test {1}".FormatWith(goal, test.Name));
                var setRecorded = db.SetAddAsync(
                        ParticipantGoalsFormat.FormatWith(clientId),
                        "{0}:{1}".FormatWith(test.Name, goal));
                var incrementCounter = db.StringIncrementAsync(
                    ExperimentGoalCountKeyFormat.FormatWith(
                        test.Name,
                        test.VersionAtSelection,
                        test.SelectedVariant,
                        goal));

                compoundCommands.Add(setRecorded);
                compoundCommands.Add(incrementCounter);
            }

            db.WaitAll(compoundCommands.ToArray());
        }

        public TestConfiguration GetTest(string name, IDatabase dbContext = null) {
            var db = dbContext ?? _redisContext.GetDatabase();

            var configAsBytes = db.StringGet(ExperimentKeyFormat.FormatWith(name));
            
            if (configAsBytes.IsNullOrEmpty) {
                return null;
            }

            var configAsString = System.Text.Encoding.Unicode.GetString(configAsBytes);
            return configAsString.FromJson<TestConfiguration>();
        }

        public Participant GetParticipant(string clientId, IDatabase dbContext = null) {
            var db = dbContext ?? _redisContext.GetDatabase();
            var participantAsBytes = db.StringGet(ParticipantKeyFormat.FormatWith(clientId));
            
            if (participantAsBytes.IsNullOrEmpty) {
                return null;
            }

            var participantString = System.Text.Encoding.Unicode.GetString(participantAsBytes);
            return participantString.FromJson<Participant>();
        }

        public long GetTotalUserCountForVariant(string test, string variant, long version) {
            var db = _redisContext.GetDatabase();

            return (long)db.StringGet(ExperimentVariantVersionKeyFormat.FormatWith(
                        test,
                        version,
                        variant));
        }

        public long GetConversionCountForTestAndVariant(
                string test,
                string variant, 
                long version,
                string goal) {
            var db = _redisContext.GetDatabase();

            return (long)db.StringGet(ExperimentGoalCountKeyFormat.FormatWith(
                        test,
                        version,
                        variant,
                        goal));
        }

        #region Helpers

        private byte[] SerializeObject(object data) {
            var configAsString = data.ToJson();
            var bytes = System.Text.Encoding.Unicode.GetBytes(configAsString);

            return bytes;
        }

        private Dictionary<string, bool> GetParticipantGoalSet(string clientId, IDatabase db) {
            var existingGoals = db.SetMembers(ParticipantGoalsFormat.FormatWith(clientId));
            return existingGoals.ToDictionary(x => x.ToString(), y => true);
        }

        #endregion
    }
}
