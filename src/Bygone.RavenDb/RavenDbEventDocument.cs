namespace Bygone.RavenDb
{
    public class RavenDbEventDocument
    {
        public string Id => "events/" + Stream + "#" + EventNumber;

        public string Stream { get; set; }
        public int EventNumber { get; set; }
        public string EventType { get; set; }
        public long Timestamp { get; set; }
        public byte[] Event { get; set; }
        public byte[] Metadata { get; set; }
    }
}