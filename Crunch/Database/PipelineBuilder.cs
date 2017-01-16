using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Crunch.Database
{
    public class PipelineBuilder {
        private readonly List<Task> _taskList = new List<Task>();
        private readonly IDatabase _db;

        public PipelineBuilder(IDatabase db) {
            _db = db;
        }

        public void StringIncrement(string key) {
            _taskList.Add(_db.StringIncrementAsync(key));
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
        }
    }
}