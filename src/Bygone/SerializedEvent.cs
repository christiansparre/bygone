using System;

namespace Bygone
{
    public class SerializedEvent
    {
        public SerializedEvent(int eventNumber, DateTime timestamp, string eventType, byte[] @event, byte[] metadata)
        {
            EventNumber = eventNumber;
            Timestamp = timestamp;
            EventType = eventType;
            Event = @event;
            Metadata = metadata;
        }

        public int EventNumber { get; }
        public DateTime Timestamp { get; }
        public string EventType { get; }
        public byte[] Event { get; }
        public byte[] Metadata { get; }
    }
}