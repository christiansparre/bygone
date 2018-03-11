using System;
using System.Collections.Generic;

namespace Bygone
{
    public class EventData
    {
        public EventData(int eventNumber, DateTime timestamp, object @event, Dictionary<string, string> metadata = null)
        {
            EventNumber = eventNumber;
            Timestamp = timestamp;
            Event = @event;
            Metadata = metadata;
        }

        public int EventNumber { get; }
        public DateTime Timestamp { get; }
        public object Event { get; }
        public Dictionary<string, string> Metadata { get; }

    }
}