using System.Threading.Tasks;

namespace ActivityTracker
{
    public enum TrackerOptions
    {
        FullProcess,
        ActiveProcess
    }
    
    public interface ITracker
    {
        Task<Snapshot> Now(TrackerOptions options);
    }
}