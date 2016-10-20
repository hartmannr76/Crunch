using System;

namespace Crunch.Integration.Tests.Utils {
    public class RandomDataGenerator {
        private readonly Random _random;

        public RandomDataGenerator() {
            _random = new Random();
        }
        public string RandomString(string prepension = "foobar") {
            return string.Format(
                "{0}-{1}", 
                prepension, 
                _random.Next(maxValue: Int32.MaxValue).ToString());
        }
    }
}