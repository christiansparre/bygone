using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bygone.Serialization;

namespace Bygone
{
    public class MemoryEventStream : EventStream
    {
        private readonly SortedDictionary<int, SerializedEvent> _events = new SortedDictionary<int, SerializedEvent>();

        public MemoryEventStream(string stream, EventSerializer serializer) : base(stream, serializer)
        {
        }

        protected override Task WriteEvents(SerializedEvent[] events)
        {
            for (var i = 0; i < events.Length; i++)
            {
                if (_events.ContainsKey(events[i].EventNumber))
                {
                    throw new DuplicateEventException(Stream, events.Select(s => s.EventNumber).ToArray(), null);
                }
            }

            foreach (var @event in events)
            {
                _events[@event.EventNumber] = @event;
            }
            return Task.FromResult(0);
        }

        protected override Task<SerializedEvent[]> ReadEvents(int firstEventNumber, int lastEventNumber)
        {
            var serializedEvents = new List<SerializedEvent>();

            foreach (var @event in _events)
            {
                if (@event.Key > lastEventNumber)
                {
                    continue;
                }

                if (@event.Key >= firstEventNumber)
                {
                    serializedEvents.Add(@event.Value);
                }
            }
            return Task.FromResult(serializedEvents.ToArray());
        }

        protected override Task<int> DeleteStream()
        {
            var eventsCount = _events.Count;
            _events.Clear();
            return Task.FromResult(eventsCount);
        }
    }
}