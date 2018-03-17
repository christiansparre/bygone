using System.Threading.Tasks;

namespace Bygone
{
    public interface IEventStorePersistence
    {
        Task Append(string stream, SerializedEvent[] events);
        Task<SerializedEvent[]> Read(string stream, int firstEventNumber, int lastEventNumber);
        Task<int> Delete(string stream);
        Task<SerializedStreamInfo[]> List(int skip = 0, int take = 1000, bool ascendingByTimestamp = true);
    }
}