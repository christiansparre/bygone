using System;
using Xunit.Abstractions;

namespace Bygone.PersistenceTests.Memory
{
    public class MemoryEventStreamAppendTests : EventStreamAppendTests
    {
        public MemoryEventStreamAppendTests(ITestOutputHelper testOutputHelper, TestConfiguration config) : base(testOutputHelper, config)
        {
            Subject = new MemoryEventStream(Guid.NewGuid().ToString(), Serializer);
        }

        public override EventStream Subject { get; }
        protected override void OnDispose()
        {

        }
    }
}