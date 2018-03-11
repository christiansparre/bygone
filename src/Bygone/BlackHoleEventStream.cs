using System.Threading.Tasks;
using Bygone.Serialization;

namespace Bygone
{
    /// <summary>
    /// An even stream that does nothing, throws everything away!
    /// </summary>
    public class BlackHoleEventStream : EventStream
    {
        public BlackHoleEventStream(string stream, EventSerializer serializer) : base(stream, serializer)
        {
        }

        protected override Task WriteEvents(SerializedEvent[] events)
        {
            return Task.FromResult(0);
        }

        protected override Task<SerializedEvent[]> ReadEvents(int firstEventNumber, int lastEventNumber)
        {
            return Task.FromResult(new SerializedEvent[0]);
        }

        protected override Task<int> DeleteStream()
        {
            return Task.FromResult(0);
        }
    }
}