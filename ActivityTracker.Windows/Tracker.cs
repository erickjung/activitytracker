using System;
using System.Threading.Tasks;

namespace ActivityTracker.Windows
{
    public class Tracker : ITracker
    {
        public Task<Snapshot> Now()
        {
            throw new NotImplementedException();
        }
    }
}