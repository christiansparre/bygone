using MongoDB.Bson;

namespace Bygone.MongoDb
{
    public class MongoDbEventDocument
    {
        public ObjectId Id { get; set; }
        public string Stream { get; set; }
        public int EventNumber { get; set; }
        public string EventType { get; set; }
        public long Timestamp { get; set; }
        public byte[] Event { get; set; }
        public byte[] Metadata { get; set; }
    }
}