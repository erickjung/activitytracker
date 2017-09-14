using System.Collections.Generic;

namespace ActivityTracker
{
    public class SnapshotProcess
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public Dictionary<long, SnapshotWindow> Windows { get; set; }
    }
}