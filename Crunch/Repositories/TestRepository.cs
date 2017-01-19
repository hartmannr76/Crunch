using Microsoft.Extensions.Logging;
using Crunch.Models.Experiments;
using Crunch.Extensions;
using Crunch.Services;
using Crunch.Contexts;
using EasyIoC;
using EasyIoC.Attributes;
using System;

namespace Crunch.Repositories
{
    public interface ITestRepository
    {
        void ConfigureTest(string name, TestConfiguration config);
        long GetConversionCountForTestAndVariant(
                string test,
                string variant, 
                long version,
                string goal);
    }

    [AutoRegister(ServiceLifetime.Singleton)]
    public class TestRepository : ITestRepository {
        private readonly ILogger<TestRepository> _logger;
        private readonly IDBConnector _dbConnector;
        private readonly ITestContext _testContext;
        
        public TestRepository(
            ILogger<TestRepository> logger,
            IDBConnector dbConnector,
            ITestContext testContext) {
            _logger = logger;
            _dbConnector = dbConnector;
            _testContext = testContext;
        }

        public void ConfigureTest(string name, TestConfiguration config)
        {  
            _logger.LogDebug(name);
            var db = _dbConnector.GetDatabase();
            var currentTest = _testContext.GetTest(name, dbContext: db);

            if (currentTest == null) {
                _logger.LogDebug("test {0} does not exist, creating".FormatWith(name));
                config.Version = 1;

                _testContext.SaveTest(config);
            } else {
                _logger.LogDebug("test {0} exists, updating".FormatWith(name));
                currentTest.Variants = config.Variants;
                currentTest.Version += 1;

                _testContext.SaveTest(currentTest);
            }
        }

        public long GetConversionCountForTestAndVariant(
                string test,
                string variant, 
                long version,
                string goal) {
            var db = _dbConnector.GetDatabase();

            return (long)db.StringGet(DatabaseKeys.ExperimentGoalCountKeyFormat.FormatWith(
                        test,
                        version,
                        variant,
                        goal));
        }
    }
}
