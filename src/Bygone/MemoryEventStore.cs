using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bygone.Serialization;

namespace Bygone
{
    public class MemoryEventStore : EventStore
    {
        private readonly Dictionary<string, SortedDictionary<int, SerializedEvent>> _streams = new Dictionary<string, SortedDictionary<int, SerializedEvent>>();

        public MemoryEventStore(EventSerializer serializer) : base(serializer)
        {

        }

        protected override Task WriteEvents(string stream, SerializedEvent[] events)
        {
            if (!_streams.TryGetValue(stream, out var sd))
            {
                sd = _streams[stream] = new SortedDictionary<int, SerializedEvent>();
            }

            for (var i = 0; i < events.Length; i++)
            {
                if (sd.ContainsKey(events[i].EventNumber))
                {
                    throw new DuplicateEventException(stream, events.Select(s => s.EventNumber).ToArray(), null);
                }
            }

            foreach (var @event in events)
            {
                sd[@event.EventNumber] = @event;
            }
            return Task.FromResult(0);
        }

        protected override Task<SerializedEvent[]> ReadEvents(string stream, int firstEventNumber, int lastEventNumber)
        {
            if (!_streams.TryGetValue(stream, out var sd))
            {
                sd = _streams[stream] = new SortedDictionary<int, SerializedEvent>();
            }

            var serializedEvents = new List<SerializedEvent>();

            foreach (var @event in sd)
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

        protected override Task<int> DeleteStream(string stream)
        {
            var eventsCount = 0;

            if (_streams.TryGetValue(stream, out var sd))
            {
                eventsCount = sd.Count;
                sd.Clear();
            }
;
            return Task.FromResult(eventsCount);
        }
    }
}