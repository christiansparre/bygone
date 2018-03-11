using System.Threading.Tasks;
using Bygone.Serialization;

namespace Bygone
{
    /// <summary>
    /// An even stream that does nothing, throws everything away!
    /// </summary>
    public class BlackHoleEventStore : EventStore
    {
        public BlackHoleEventStore(EventSerializer serializer) : base(serializer)
        {
        }

        protected override Task WriteEvents(string stream, SerializedEvent[] events)
        {
            return Task.FromResult(0);
        }

        protected override Task<SerializedEvent[]> ReadEvents(string stream, int firstEventNumber, int lastEventNumber)
        {
            return Task.FromResult(new SerializedEvent[0]);
        }

        protected override Task<int> DeleteStream(string stream)
        {
            return Task.FromResult(0);
        }

        protected override Task<SerializedStreamInfo[]> ListStreams()
        {
            return Task.FromResult(new SerializedStreamInfo[0]);
        }

        protected override Task<int> CountEvents(string stream)
        {
            return Task.FromResult(0);
        }

        protected override Task<int> GetHighestEventNumber(string stream)
        {
            return Task.FromResult(0);
        }
    }
}