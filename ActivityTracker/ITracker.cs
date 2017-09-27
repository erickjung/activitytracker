using System.Threading.Tasks;

namespace ActivityTracker
{
    public interface ITracker
    {
        Task<Snapshot> Now();
    }
}