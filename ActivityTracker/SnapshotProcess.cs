using System.Collections.Generic;

namespace ActivityTracker
{
    public class SnapshotProcess
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public Dictionary<long, SnapshotWindow> Windows { get; set; }
    }
}