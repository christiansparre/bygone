﻿using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Polly;
using Polly.Retry;

namespace Bygone.MongoDb
{
    public class MongoDbEventStorePersistence : IEventStorePersistence
    {
        private readonly IMongoCollection<MongoDbEventDocument> _collection;
        private readonly RetryPolicy _appendRetryPolicy;

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

            _collection.Indexes.CreateOne(new CreateIndexModel<MongoDbEventDocument>(Builders<MongoDbEventDocument>.IndexKeys.Ascending(s => s.Stream).Ascending(s => s.EventNumber), new CreateIndexOptions { Background = false, Unique = true }));

            _appendRetryPolicy = Policy.Handle<MongoCommandException>(e => e.Code == 24)
                .WaitAndRetryAsync(5, i => TimeSpan.FromMilliseconds(5));
        }

        public async Task Append(string stream, SerializedEvent[] events)
        {
            var mongoDbEventDocuments = events.Select(s => new MongoDbEventDocument
            {
                Stream = stream,
                EventNumber = s.EventNumber,
                EventType = s.EventType,
                Timestamp = s.TimestampTicks,
                Event = s.Event,
                Metadata = s.Metadata
            }).ToList();

            await _appendRetryPolicy.ExecuteAsync(async () =>
            {
                using (var session = await _collection.Database.Client.StartSessionAsync())
                {
                    session.StartTransaction();

                    try
                    {
                        await _collection.InsertManyAsync(session, mongoDbEventDocuments);
                        await session.CommitTransactionAsync();
                    }
                    catch (MongoCommandException ex) when (ex.Code == 11000)
                    {
                        await session.AbortTransactionAsync();
                        throw new DuplicateEventException(stream, events.Select(s => s.EventNumber).ToArray(), ex);
                    }
                }
            });
        }

        public async Task<SerializedEvent[]> Read(string stream, int firstEventNumber, int lastEventNumber)
        {
            var mongoDbEventDocuments = await _collection
                .Find(f => f.Stream == stream
                           && f.EventNumber >= firstEventNumber
                           && f.EventNumber <= lastEventNumber)
                .SortBy(s => s.EventNumber)
                .ToListAsync();

            return mongoDbEventDocuments.Select(s =>
                new SerializedEvent(
                    s.EventNumber,
                    s.Timestamp,
                    s.EventType, s.Event, s.Metadata
                )).ToArray();
        }

        public async Task<int> Delete(string stream)
        {
            var deleteResult = await _collection.DeleteManyAsync(Builders<MongoDbEventDocument>.Filter.Eq(s => s.Stream, stream));

            return (int)deleteResult.DeletedCount;
        }

        public async Task<SerializedStreamInfo[]> List(int skip = 0, int take = 1000, long createdOnOrAfterTimestampTicks = 0,
            bool ascendingByTimestampTicks = true)
        {
            var result = await _collection
                .Find(f => f.Timestamp >= createdOnOrAfterTimestampTicks && f.EventNumber == 1)
                .Sort(ascendingByTimestampTicks
                       ? Builders<MongoDbEventDocument>.Sort.Ascending(a => a.Timestamp)
                       : Builders<MongoDbEventDocument>.Sort.Descending(a => a.Timestamp))
                .Skip(skip)
                .Limit(take)
                .Project(p => new { p.Stream, p.Timestamp })
                .ToListAsync();

            return result.Select(s => new SerializedStreamInfo(s.Stream, s.Timestamp)).ToArray();
        }
    }
}
