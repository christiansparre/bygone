using System;
using Bygone.MongoDb;
using MongoDB.Driver;
using Xunit.Abstractions;

namespace Bygone.PersistenceTests.MongoDb
{
    //  docker run -i -p 37017:27017 --rm mongo:4 --replSet test
    //  rs.initiate() in shell

    public class MongoDbEventStoreAppendTests : EventStoreAppendTests
    {
        private readonly string _collectionName;

        public MongoDbEventStoreAppendTests(ITestOutputHelper testOutputHelper, TestConfiguration config) : base(testOutputHelper, config)
        {
            _collectionName = Guid.NewGuid().ToString();
            Subject = new EventStore(Serializer, new MongoDbEventStorePersistence(config.Configuration["MongoDb:ConnectionString"]));
        }

        public override EventStore Subject { get; }
        protected override void OnDispose()
        {
            var mongoUrl = new MongoUrl(Config.Configuration["MongoDb:ConnectionString"]);
            new MongoClient(mongoUrl).GetDatabase(mongoUrl.DatabaseName).DropCollection(_collectionName);
        }
    }
}