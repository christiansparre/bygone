using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Bygone.MongoDb
{
    public class MongoDbEventStorePersistence : IEventStorePersistence
    {
        private IMongoCollection<MongoDbEventDocument> _collection;

        public MongoDbEventStorePersistence(string connectionString, string eventsCollectionName = "events")
        {
            var url = new MongoUrl(connectionString);

            var mongoDatabase = new MongoClient(url).GetDatabase(url.DatabaseName);
            _collection = mongoDatabase.GetCollection<MongoDbEventDocument>(eventsCollectionName);

            var result = mongoDatabase.RunCommand(new BsonDocumentCommand<BsonDocument>(new BsonDocument("buildinfo", "1")));

            if (result["versionArray"].AsBsonArray[0].AsInt32 < 4)
            {
                throw new NotSupportedException("Only MongoDB version 4.0.0 and above is supported");
            }
        }

        public Task Append(string stream, SerializedEvent[] events)
        {
            throw new NotImplementedException();
        }

        public Task<SerializedEvent[]> Read(string stream, int firstEventNumber, int lastEventNumber)
        {
            throw new NotImplementedException();
        }

        public Task<int> Delete(string stream)
        {
            throw new NotImplementedException();
        }

        public Task<SerializedStreamInfo[]> List(int skip = 0, int take = 1000, long createdOnOrAfterTimestampTicks = 0,
            bool ascendingByTimestampTicks = true)
        {
            throw new NotImplementedException();
        }
    }
}
