﻿using Xunit.Abstractions;

namespace Bygone.PersistenceTests.Memory
{
    public class MemoryEventStorePerformanceTests : EventStorePerformanceTests
    {
        public MemoryEventStorePerformanceTests(ITestOutputHelper testOutputHelper, TestConfiguration config) : base(testOutputHelper, config)
        {
            Subject = new EventStore(Serializer, new MemoryEventStorePersistence());
        }

        public override EventStore Subject { get; }
        protected override void OnDispose()
        {

        }
    }
}