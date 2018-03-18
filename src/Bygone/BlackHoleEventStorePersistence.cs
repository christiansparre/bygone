using System.Threading.Tasks;
using Bygone.Serialization;

namespace Bygone
{
    public class BlackHoleEventStorePersistence : IEventStorePersistence
    {
        public Task Append(string stream, SerializedEvent[] events)
        {
            return Task.FromResult(0);
        }

        public Task<SerializedEvent[]> Read(string stream, int firstEventNumber, int lastEventNumber)
        {
            return Task.FromResult(new SerializedEvent[0]);
        }

        public Task<int> Delete(string stream)
        {
            return Task.FromResult(0);
        }

        public Task<SerializedStreamInfo[]> List(int skip = 0, int take = 1000, long createdOnOrAfterTimestampTicks = 0, bool ascendingByTimestamp = true)
        {
            return Task.FromResult(new SerializedStreamInfo[0]);
        }
    }
}