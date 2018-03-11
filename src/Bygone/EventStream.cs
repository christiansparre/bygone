using System;
using System.Linq;
using System.Threading.Tasks;
using Bygone.Serialization;

namespace Bygone
{
    public abstract class EventStream : IEventStream
    {
        private readonly EventSerializer _serializer;
        public string Stream { get; }

        protected EventStream(string stream, EventSerializer serializer)
        {
            _serializer = serializer;
            Stream = stream;
        }
        
        public async Task Append(params EventData[] events)
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
                    eventData.Timestamp,
                    eventType,
                    _serializer.SerializeEvent(eventData.Event),
                    eventData.Metadata == null ? new byte[0] : _serializer.SerializeMetadata(eventData.Metadata));
            }

            await WriteEvents(serializedEvents.ToArray());
        }

        /// <summary>
        /// Reads the events from the first event number provided to the last event number provided
        /// </summary>
        public async Task<EventData[]> Read(int firstEventNumber = 1, int lastEventNumber = int.MaxValue)
        {
            var serializedEvents = await ReadEvents(firstEventNumber, lastEventNumber);

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
                    r.Timestamp,
                    _serializer.DeserializeEvent(eventType, r.Event),
                   r.Metadata.Length == 0 ? null : _serializer.DeserializeMetadata(r.Metadata));

            }

            return events;
        }

        public Task<int> Delete()
        {
            return DeleteStream();
        }

        protected abstract Task WriteEvents(SerializedEvent[] events);
        protected abstract Task<SerializedEvent[]> ReadEvents(int firstEventNumber, int lastEventNumber);
        protected abstract Task<int> DeleteStream();
    }
}