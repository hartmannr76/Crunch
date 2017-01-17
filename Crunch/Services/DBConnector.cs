using StackExchange.Redis;
using System;
using Microsoft.Extensions.Logging;
using EasyIoC;
using EasyIoC.Attributes;
using Crunch.Extensions;
using Microsoft.AspNetCore.Hosting;

namespace Crunch.Services
{
    public interface IDBConnector
    {
        IDatabase GetDatabase();
        bool IsConnected { get; }
    }

    [AutoRegister(ServiceLifetime.Singleton)]
    public class DBConnector : IDBConnector {
        private readonly ConnectionMultiplexer _redisContext;
        private readonly ILogger<DBConnector> _logger;
        private readonly IHostingEnvironment _hostingEnvironment;
        public bool IsConnected { get; private set; }
        
        public DBConnector(
            ILogger<DBConnector> logger,
            IHostingEnvironment hostingEnvironment) {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            

            var redisHost = Environment.GetEnvironmentVariable("REDIS_PORT_6379_TCP_ADDR");

            try {
                if (_hostingEnvironment.IsProduction()) {
                    var splitConfig = redisHost.Split(new [] { "//" }, StringSplitOptions.None);

                    var configParams = splitConfig[1].Split('@');

                    var creds = configParams[0].Split(':');
                    var password = creds[1];

                    var connectionParams = configParams[1].Split(':');
                    var host = connectionParams[0];
                    var port = connectionParams[1];

                    _redisContext = ConnectionMultiplexer.Connect(
                        "{0},ssl=true,password={1},name={2}".FormatWith(
                            connectionParams,
                            password,
                            "crunch_api"
                        )
                    );
                }
                else {
                    var config = ConfigurationOptions.Parse(redisHost);

                    _logger.LogInformation("Attempting to connect to: {0}".FormatWith(redisHost));
                    _redisContext = ConnectionMultiplexer.Connect(redisHost);
                }

                IsConnected = true;
            } catch (Exception e) {
                logger.LogCritical(e.Message);

                IsConnected = false;
            }
        }

        public IDatabase GetDatabase() {
            if(IsConnected) {
                return _redisContext.GetDatabase();
            }

            return null;
        }
    }
}
