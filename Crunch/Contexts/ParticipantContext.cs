using StackExchange.Redis;
using Microsoft.Extensions.Logging;
using Crunch.Extensions;
using Crunch.Services;
using EasyIoC.Attributes;
using System.Collections.Generic;
using System.Linq;
using Crunch.Models.Experiments;

namespace Crunch.Contexts
{
    public interface IParticipantContext
    {
        Participant GetParticipant(string clientId, IDatabase dbContext = null);
        IDictionary<string, bool> GetParticipantsGoals(string clientId, IDatabase dbContext = null);
        long GetTotalUserCountForVariant(string test, string variant, long version, IDatabase dbContext = null);
    }

    [AutoRegister]
    public class ParticipantContext : IParticipantContext {
        private readonly ILogger<ParticipantContext> _logger;
        private readonly IDBConnector _dbConnector;
        
        public ParticipantContext(
            ILogger<ParticipantContext> logger,
            IDBConnector dbConnector) {
            _logger = logger;
            _dbConnector = dbConnector;
        }

        public Participant GetParticipant(string clientId, IDatabase dbContext = null) {
            var db = dbContext ?? _dbConnector.GetDatabase();
            var participant = db.StringGet(DatabaseKeys.ParticipantKeyFormat.FormatWith(clientId));
            
            if (participant.IsNullOrEmpty) {
                return null;
            }

            return participant.SerializeFromJson<Participant>();
        }

        public IDictionary<string, bool> GetParticipantsGoals(string clientId, IDatabase dbContext = null) {
            var db = dbContext ?? _dbConnector.GetDatabase();
            var existingGoals = db.SetMembers(DatabaseKeys.ParticipantGoalsFormat.FormatWith(clientId));
            return existingGoals.ToDictionary(x => x.ToString(), y => true);
        }

        public long GetTotalUserCountForVariant(string test, string variant, long version, IDatabase dbContext = null) {
            var db = dbContext ?? _dbConnector.GetDatabase();

            return (long)db.StringGet(DatabaseKeys.ExperimentVariantVersionKeyFormat.FormatWith(
                        test,
                        version,
                        variant));
        }
    }
}
