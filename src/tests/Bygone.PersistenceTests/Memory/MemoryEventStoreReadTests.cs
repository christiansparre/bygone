﻿using Xunit.Abstractions;

namespace Bygone.PersistenceTests.Memory
{
    public class MemoryEventStoreReadTests : EventStoreReadTests
    {
        public MemoryEventStoreReadTests(ITestOutputHelper testOutputHelper, TestConfiguration config) : base(testOutputHelper, config)
        {
            Subject = new MemoryEventStore(Serializer);
        }

        public override EventStore Subject { get; }
        protected override void OnDispose()
        {

        }
    }
}