using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Crunch.Database
{
    public class PipelineBuilder {
        private List<Task> _taskList = new List<Task>();
        private IDatabase _db;

        public PipelineBuilder(IDatabase db) {
            _db = db;
        }

        public void StringIncrement(string key) {
            _taskList.Add(_db.StringIncrementAsync(key));
        }

        public void StringSet(RedisKey key, RedisValue value) {
            _taskList.Add(_db.StringSetAsync(key, value));
        }

        public void SetAdd(string key, string value) {
            _taskList.Add(_db.SetAddAsync(key, value));
        }

        public void SetRemove(string key, string value) {
            _taskList.Add(_db.SetRemoveAsync(key, value));
        }

        public void SetRemoveRange(string key, IEnumerable<string> range) {
            var rangeList = range.ToList();
            rangeList.ForEach(x => SetRemove(key, x));
        }

        public void Execute() {
            _db.WaitAll(_taskList.ToArray());

            _taskList = null;
            _db = null;
        }
    }
}