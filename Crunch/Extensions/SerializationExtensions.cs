using NuGet.Protocol.Core.v3;
using StackExchange.Redis;

namespace Crunch.Extensions
{
    public static class SerializationExtensions {
        public static byte[] SerializeAsJson(this object data) {
            var configAsString = data.ToJson();
            var bytes = System.Text.Encoding.Unicode.GetBytes(configAsString);

            return bytes;
        }

        public static T SerializeFromJson<T>(this RedisValue data) {
            var dataAsString = System.Text.Encoding.Unicode.GetString(data);
            return dataAsString.FromJson<T>();
        }
    }
}