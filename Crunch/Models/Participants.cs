using System.Collections.Generic;

namespace Crunch.Models.Experiments {
    public class Participant {
        public string ClientId { get; set; }
        public IEnumerable<Test> EnrolledTests { get; set; }
    }

    public class Test {
        public string Name { get; set; }
        public string SelectedVariant { get; set; }
        public long VersionAtSelection { get; set; }
    }
}