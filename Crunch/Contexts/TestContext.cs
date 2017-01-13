using StackExchange.Redis;
using Microsoft.Extensions.Logging;
using Crunch.Models.Experiments;
using Crunch.Extensions;
using Crunch.Services;
using EasyIoC.Attributes;

namespace Crunch.Contexts
{
    public interface ITestContext
    {
        void SaveTest(TestConfiguration config, IDatabase dbContext = null);
        TestConfiguration GetTest(string name, IDatabase dbContext = null);
    }

    [AutoRegister]
    public class TestContext : ITestContext {
        private readonly ILogger<TestContext> _logger;
        private readonly IDBConnector _dbConnector;
        
        public TestContext(
            ILogger<TestContext> logger,
            IDBConnector dbConnector,
            IVariantPicker variantPicker) {
            _logger = logger;
            _dbConnector = dbConnector;
        }

        public void SaveTest(TestConfiguration config, IDatabase dbContext = null) {
            var db = dbContext ?? _dbConnector.GetDatabase();

            var dataStream = config.SerializeAsJson();

            db.StringSet(DatabaseKeys.ExperimentKeyFormat.FormatWith(config.Name), dataStream);
        }

        public TestConfiguration GetTest(string name, IDatabase dbContext = null) {
            var db = dbContext ?? _dbConnector.GetDatabase();

            var config = db.StringGet(DatabaseKeys.ExperimentKeyFormat.FormatWith(name));
            
            if (config.IsNullOrEmpty) {
                return null;
            }

            return config.SerializeFromJson<TestConfiguration>();
        }
    }
}
