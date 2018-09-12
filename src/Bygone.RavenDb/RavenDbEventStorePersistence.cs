using System;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Exceptions.Documents.Session;

namespace Bygone.RavenDb
{
    public class RavenDbEventStorePersistence : IEventStorePersistence
    {
        private readonly IDocumentStore _documentStore;

        public RavenDbEventStorePersistence(IDocumentStore documentStore)
        {
            _documentStore = documentStore;

            if (string.IsNullOrEmpty(_documentStore.Database))
            {
                throw new ArgumentNullException(nameof(IDocumentStore.Database), "The provided document store must provide a database");
            }

            IndexCreation.CreateIndexes(new AbstractIndexCreationTask[]
            {
                new Events_ByStreamAndEventNumber(),
                new Events_Streams()
            }, _documentStore);
        }

        public async Task Append(string stream, SerializedEvent[] events)
        {
            var documents = events.Select(e => new RavenDbEventDocument
            {
                Stream = stream,
                EventNumber = e.EventNumber,
                EventType = e.EventType,
                Timestamp = e.TimestampTicks,
                Event = e.Event,
                Metadata = e.Metadata
            }).ToList();

            using (var session = _documentStore.OpenAsyncSession())
            {
                session.Advanced.WaitForIndexesAfterSaveChanges();
                await session.LoadAsync<RavenDbEventDocument>(documents.Select(s => s.Id));

                foreach (var doc in documents)
                {
                    try
                    {
                        await session.StoreAsync(doc);
                    }
                    catch (Exception ex)
                    {
                        if (ex is NonUniqueObjectException nex)
                        {
                            throw new DuplicateEventException(stream, events.Select(s => s.EventNumber).ToArray(), nex);
                        }
                        throw;
                    }
                }

                await session.SaveChangesAsync();
            }
        }

        public async Task<SerializedEvent[]> Read(string stream, int firstEventNumber, int lastEventNumber)
        {
            using (var session = _documentStore.OpenAsyncSession())
            {
                var res = await session.Query<RavenDbEventDocument, Events_ByStreamAndEventNumber>()
                    .Customize(c => c.WaitForNonStaleResults())
                    .Where(e => e.Stream == stream && e.EventNumber >= firstEventNumber && e.EventNumber <= lastEventNumber)
                    .OrderBy(o => o.EventNumber)
                    .ToListAsync();

                return res.Select(s => new SerializedEvent(s.EventNumber, s.Timestamp, s.EventType, s.Event, s.Metadata)).ToArray();
            }
        }

        public async Task<int> Delete(string stream)
        {
            using (var session = _documentStore.OpenAsyncSession())
            {
                var res = await session.Query<RavenDbEventDocument, Events_ByStreamAndEventNumber>()
                    .Customize(c => c.WaitForNonStaleResults())
                    .Where(e => e.Stream == stream)
                    .OrderBy(o => o.EventNumber)
                    .ToListAsync();

                foreach (var d in res)
                {
                    session.Delete(d);
                }
                await session.SaveChangesAsync();

                return res.Count;
            }
        }

        public async Task<SerializedStreamInfo[]> List(int skip = 0, int take = 1000, long createdOnOrAfterTimestampTicks = 0, bool ascendingByTimestampTicks = true)
        {
            using (var session = _documentStore.OpenAsyncSession())
            {
                var query = session.Query<RavenDbEventDocument, Events_Streams>()
                    .Customize(c => c.WaitForNonStaleResults())
                    .Where(q => q.EventNumber == 1 && q.Timestamp >= createdOnOrAfterTimestampTicks);

                query = ascendingByTimestampTicks ? query.OrderBy(o => o.Timestamp) : query.OrderByDescending(o => o.Timestamp);

                var res = await query
                    .Select(s => new { s.Stream, s.Timestamp })
                    .ToListAsync();

                return res.Select(s => new SerializedStreamInfo(s.Stream, s.Timestamp)).ToArray();
            }
        }
    }
}
