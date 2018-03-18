using System;
using System.Linq;
using System.Threading.Tasks;
using Bygone.Serialization;

namespace Bygone
{
    public class EventStore : IEventStore
    {
        private readonly EventSerializer _serializer;
        private readonly IEventStorePersistence _persistence;

        public EventStore(EventSerializer serializer, IEventStorePersistence persistence)
        {
            _serializer = serializer;
            _persistence = persistence;
        }

        public async Task Append(string stream, params EventData[] events)
        {
            var serializedEvents = new SerializedEvent[events.Length];

            for (int i = 0; i < events.Length; i++)
            {
                var eventData = events[i];

                var eventType = _serializer.Lookup(eventData.Event.GetType());
                if (eventType == null)
                {
                    throw new NotSupportedException("Event type could not be found");
                }

                serializedEvents[i] = new SerializedEvent(
                    eventData.EventNumber,
                    eventData.Timestamp.Ticks,
                    eventType,
                    _serializer.SerializeEvent(eventData.Event),
                    eventData.Metadata == null ? new byte[0] : _serializer.SerializeMetadata(eventData.Metadata));
            }

            await _persistence.Append(stream, serializedEvents);
        }

        /// <summary>
        /// Reads the events from the first event number provided to the last event number provided
        /// </summary>
        public async Task<EventData[]> Read(string stream, int firstEventNumber = 1, int lastEventNumber = int.MaxValue)
        {
            var serializedEvents = await _persistence.Read(stream, firstEventNumber, lastEventNumber);

            var events = new EventData[serializedEvents.Length];

            for (int i = 0; i < events.Length; i++)
            {
                var r = serializedEvents[i];

                var eventType = _serializer.Lookup(r.EventType);
                if (eventType == null)
                {
                    throw new NotSupportedException("Event type could not be found");
                }

                events[i] = new EventData(
                    r.EventNumber,
                    new DateTime(r.TimestampTicks, DateTimeKind.Utc),
                    _serializer.DeserializeEvent(eventType, r.Event),
                   r.Metadata.Length == 0 ? null : _serializer.DeserializeMetadata(r.Metadata));

            }

            return events;
        }

        public Task<int> Delete(string stream)
        {
            return _persistence.Delete(stream);
        }

        public async Task<StreamInfo[]> List(int skip = 0, int take = 1000, DateTime createdOnOrAfter = default(DateTime), bool ascendingByTimestamp = true)
        {
            return (await _persistence.List(skip, take, createdOnOrAfter.ToUniversalTime().Ticks, ascendingByTimestamp))
                .Select(s => new StreamInfo(s.Stream, new DateTime(s.CreatedTicks, DateTimeKind.Utc)))
                .ToArray();
        }
    }
}