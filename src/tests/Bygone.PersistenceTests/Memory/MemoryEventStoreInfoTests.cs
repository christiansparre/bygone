using Xunit.Abstractions;

namespace Bygone.PersistenceTests.Memory
{
    public class MemoryEventStoreInfoTests : EventStoreInfoTests
    {
        public MemoryEventStoreInfoTests(ITestOutputHelper testOutputHelper, TestConfiguration config) : base(testOutputHelper, config)
        {
            Subject = new MemoryEventStore(Serializer);
        }

        public override EventStore Subject { get; }
        protected override void OnDispose()
        {

        }
    }
}