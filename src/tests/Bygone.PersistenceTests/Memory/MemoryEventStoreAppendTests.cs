using Xunit.Abstractions;

namespace Bygone.PersistenceTests.Memory
{
    public class MemoryEventStoreAppendTests : EventStoreAppendTests
    {
        public MemoryEventStoreAppendTests(ITestOutputHelper testOutputHelper, TestConfiguration config) : base(testOutputHelper, config)
        {
            Subject = new EventStore(Serializer, new MemoryEventStorePersistence());
        }

        public override EventStore Subject { get; }
        protected override void OnDispose()
        {

        }
    }
}