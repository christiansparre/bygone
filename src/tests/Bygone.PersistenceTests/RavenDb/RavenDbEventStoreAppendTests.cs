using System;
using Bygone.RavenDb;
using Raven.Client.Documents;
using Raven.Embedded;
using Xunit;
using Xunit.Abstractions;

namespace Bygone.PersistenceTests.RavenDb
{
    [Collection(nameof(RavenDbTestsCollection))]
    public class RavenDbEventStoreAppendTests : EventStoreAppendTests
    {
        private IDocumentStore _store;

        public RavenDbEventStoreAppendTests(ITestOutputHelper testOutputHelper, TestConfiguration config, RavenDbTestContext ctx) : base(testOutputHelper, config)
        {
            _store = EmbeddedServer.Instance.GetDocumentStore(Guid.NewGuid().ToString());
            Subject = new EventStore(Serializer, new RavenDbEventStorePersistence(_store));
        }

        public override EventStore Subject { get; }
        protected override void OnDispose()
        {

        }
    }
}