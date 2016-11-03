using System.Collections.Generic;

namespace Crunch.Shared.Models.Experiments {
    public class TestConfiguration {
        public string Name { get; set; }
        public long Version { get; set; }
        public IEnumerable<Variant> Variants { get; set; }
    }   

    public class Variant {
        public string Name { get; set; }
        public float Influence { get; set; }
    }
}