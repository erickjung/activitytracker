using System;
using System.Collections.Generic;

namespace ActivityTracker
{
    public class Snapshot
    {
        public DateTime Time { get; set; }
        public Dictionary<long, SnapshotProcess> Processes { get; set; }
        public SnapshotProcess ActiveProcess { get; set; }
    }
}