namespace Bygone
{
    public class SerializedEvent
    {
        public SerializedEvent(int eventNumber, long timestampTicks, string eventType, byte[] @event, byte[] metadata)
        {
            EventNumber = eventNumber;
            TimestampTicks = timestampTicks;
            EventType = eventType;
            Event = @event;
            Metadata = metadata;
        }

        public int EventNumber { get; }
        public long TimestampTicks { get; }
        public string EventType { get; }
        public byte[] Event { get; }
        public byte[] Metadata { get; }
    }
}