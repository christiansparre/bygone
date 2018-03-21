using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bygone
{
    public class MemoryEventStorePersistence : IEventStorePersistence
    {
        private readonly Dictionary<string, SortedDictionary<int, SerializedEvent>> _streams = new Dictionary<string, SortedDictionary<int, SerializedEvent>>();

        public Task Append(string stream, SerializedEvent[] events)
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

        public Task<SerializedEvent[]> Read(string stream, int firstEventNumber, int lastEventNumber)
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

        public Task<int> Delete(string stream)
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

        public Task<SerializedStreamInfo[]> List(int skip = 0, int take = 1000, long createdOnOrAfterTimestampTicks = 0, bool ascendingByTimestamp = true)
        {
            var events = _streams.Select(s => new { stream = s.Key, @event = s.Value[1] }).Where(a => a.@event.TimestampTicks >= createdOnOrAfterTimestampTicks);

            events = ascendingByTimestamp ? events.OrderBy(o => o.@event.TimestampTicks) : events.OrderByDescending(o => o.@event.TimestampTicks);

            return Task.FromResult(events.Skip(skip).Take(take).Select(s => new SerializedStreamInfo(s.stream, s.@event.TimestampTicks)).ToArray());
        }
    }
}