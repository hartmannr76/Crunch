
using StackExchange.Redis;
using netCoreTest.Attributes;
using System;

namespace netCoreTest.App
{
 public interface ITestClass
{
    string CallOut(string name);
}

[AutoRegister(ServiceLifetime.Singleton)]
public class TestClass : ITestClass {
    private readonly ConnectionMultiplexer _redisContext;
    
    public TestClass() {
         var redisHost = Environment.GetEnvironmentVariable("REDIS_PORT_6379_TCP_ADDR");
         _redisContext = ConnectionMultiplexer.Connect(redisHost);
        //  var db = _redisContext.GetDatabase();
        //  db.StringSet("key", "abc");
    }
    
     public string CallOut(string name) {
         var db = _redisContext.GetDatabase();
         return db.StringGet("key");
     }
}   

}