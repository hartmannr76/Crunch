using StackExchange.Redis;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using NuGet.Protocol.Core.v3;
using System.Linq;
using Crunch.Models.Experiments;
using System.Threading.Tasks;
using Crunch.Attributes;

namespace Crunch.Services
{
    public interface IDBConnector
    {
        void ConfigureTest(string name, TestConfiguration config);
        TestConfiguration GetTest(string name, IDatabase dbContext = null);
        string EnrollParticipantInTest(string clientId, string test);
        void RecordGoal(string clientId, string goal, IDatabase dbContext = null);
        void SetConnection(bool isOn);
        bool IsConnected { get; }
    }

    [AutoRegister(ServiceLifetime.Singleton)]
    public class DBConnector : IDBConnector {
        #region DB Key Constants
        private const string ExperimentKeyFormat = "e:{0}";
        private const string ExperimentCurrentCountKeyFormat = "e:{0}:_all";
        private const string ExperimentGoalCountKeyFormat = "e:{0}:v:{1}:g:{2}";
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

        public void SetConnection(bool isOn) {
            IsConnected = isOn;
        }

        public string EnrollParticipantInTest(string clientId, string test) {
            var db = _redisContext.GetDatabase();
            
            var currentTest = GetTest(test, dbContext: db);

            // check to see if the test is valid
            if (currentTest == null) {
                throw new Exception(
                    string.Format("Test {0} has not been configured, cannot enroll", test));
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
                db.StringIncrement(string.Format(ExperimentCurrentCountKeyFormat, test));
            } else {
                if (existingTest.VersionAtSelection == currentTest.Version) {
                    _logger.LogDebug("User already had variant");
                    return existingTest.SelectedVariant;
                } else {
                    _logger.LogDebug("User variant version differs, updating");
                    existingTest.SelectedVariant = selectedVariant;
                    existingTest.VersionAtSelection = currentTest.Version;
                    db.StringIncrement(string.Format(ExperimentCurrentCountKeyFormat, test));

                    // clean existing recorded goals for the new test
                    var goalsAsSet = GetParticipantGoalSet(participant.ClientId, db);
                    var experimentGoals = goalsAsSet.Keys.Where(x => x.StartsWith(test+":")).ToList();
                    Console.Out.WriteLine(goalsAsSet.ToJson());
                    Console.Out.WriteLine(string.Format("Removing {0} goals from user set", experimentGoals.Count));
                    var compoundCommands = new List<Task>();
                    experimentGoals.ForEach(x => compoundCommands.Add(db.SetRemoveAsync(
                            string.Format(ParticipantGoalsFormat, clientId),
                            x)));
                    db.WaitAll(compoundCommands.ToArray());
                }
            }

            var participantBytes = SerializeObject(participant);
            db.StringSet(string.Format(ParticipantKeyFormat, clientId), participantBytes);

            return selectedVariant;
        }

        public void ConfigureTest(string name, TestConfiguration config)
        {  
            _logger.LogDebug(name);
            var db = _redisContext.GetDatabase();
            var currentTest = GetTest(name, dbContext: db);
            byte[] configBytes;

            if (currentTest == null) {
                _logger.LogDebug(string.Format("test {0} does not exist, creating", name));
                config.Version = 1;
                configBytes = SerializeObject(config);
            } else {
                _logger.LogDebug(string.Format("test {0} exists, updating", name));
                currentTest.Variants = config.Variants;
                currentTest.Version += 1;

                configBytes = SerializeObject(currentTest);
            }

            db.StringSet(string.Format(ExperimentKeyFormat, name), configBytes);
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
                if(goalsAsSet.ContainsKey(string.Format("{0}:{1}", test.Name, goal))) {
                    _logger.LogDebug(string.Format("Previously recorded {0} for test {1}", goal, test.Name));
                    continue;
                }

                _logger.LogDebug(string.Format("Tracking {0} for test {1}", goal, test.Name));
                var setRecorded = db.SetAddAsync(
                        string.Format(ParticipantGoalsFormat, clientId),
                        string.Format("{0}:{1}", test.Name, goal));
                var incrementCounter = db.StringIncrementAsync(
                    string.Format(ExperimentGoalCountKeyFormat, test.Name, test.VersionAtSelection, goal));

                compoundCommands.Add(setRecorded);
                compoundCommands.Add(incrementCounter);
            }

            db.WaitAll(compoundCommands.ToArray());
        }

        public TestConfiguration GetTest(string name, IDatabase dbContext = null) {
            var db = dbContext ?? _redisContext.GetDatabase();

            var configAsBytes = db.StringGet(string.Format(ExperimentKeyFormat, name));
            
            if (configAsBytes.IsNullOrEmpty) {
                return null;
            }

            var configAsString = System.Text.Encoding.Unicode.GetString(configAsBytes);
            return configAsString.FromJson<TestConfiguration>();
        }

        public Participant GetParticipant(string clientId, IDatabase dbContext = null) {
            var db = dbContext ?? _redisContext.GetDatabase();
            var participantAsBytes = db.StringGet(string.Format(ParticipantKeyFormat, clientId));
            
            if (participantAsBytes.IsNullOrEmpty) {
                return null;
            }

            var participantString = System.Text.Encoding.Unicode.GetString(participantAsBytes);
            return participantString.FromJson<Participant>();
        }

        #region Helpers

        private byte[] SerializeObject(object data) {
            var configAsString = data.ToJson();
            var bytes = System.Text.Encoding.Unicode.GetBytes(configAsString);

            return bytes;
        }

        private Dictionary<string, bool> GetParticipantGoalSet(string clientId, IDatabase db) {
            var existingGoals = db.SetMembers(string.Format(ParticipantGoalsFormat, clientId));
            return existingGoals.ToDictionary(x => x.ToString(), y => true);
        }

        #endregion
    }
}
