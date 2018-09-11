﻿using System;
using Bygone.MongoDb;
using MongoDB.Driver;
using Xunit.Abstractions;

namespace Bygone.PersistenceTests.MongoDb
{
    public class MongoDbEventStoreReadTests : EventStoreReadTests
    {
        private readonly string _collectionName;

        public MongoDbEventStoreReadTests(ITestOutputHelper testOutputHelper, TestConfiguration config) : base(testOutputHelper, config)
        {
            _collectionName = Guid.NewGuid().ToString();
            Subject = new EventStore(Serializer, new MongoDbEventStorePersistence(GetConnectionString(), _collectionName));
        }

        public override EventStore Subject { get; }
        protected override void OnDispose()
        {
            var mongoUrl = new MongoUrl(GetConnectionString());
            new MongoClient(mongoUrl).DropDatabase(mongoUrl.DatabaseName);
        }

        private string GetConnectionString()
        {
            return Config.Configuration["MongoDb:ConnectionString"].Replace("/Bygone-Tests", "/" + Guid.NewGuid());
        }
    }
}