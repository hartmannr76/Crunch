using StackExchange.Redis;
using System;
using Microsoft.Extensions.Logging;
using EasyIoC;
using EasyIoC.Attributes;

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
        public bool IsConnected { get; private set; }
        
        public DBConnector(
            ILogger<DBConnector> logger) {
            _logger = logger;

            try {
                var redisHost = Environment.GetEnvironmentVariable("REDIS_PORT_6379_TCP_ADDR");
                _redisContext = ConnectionMultiplexer.Connect(redisHost);

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
