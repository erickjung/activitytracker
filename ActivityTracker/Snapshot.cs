using System;
using System.Collections.Generic;

namespace ActivityTracker
{
    public class Snapshot
    {
        public DateTime Time { get; set; }
        public Dictionary<long, SnapshotProcess> Process { get; set; }
        public long ActiveWindow { get; set; }
    }
}