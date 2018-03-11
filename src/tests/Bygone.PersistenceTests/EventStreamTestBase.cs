using System;
using Bygone.Serialization.ProtoBufNet;
using Xunit;
using Xunit.Abstractions;

namespace Bygone.PersistenceTests
{
    public abstract class EventStreamTestBase : IDisposable, IClassFixture<TestConfiguration>
    {
        public ITestOutputHelper TestOutputHelper { get; }
        public TestConfiguration Config { get; }

        protected EventStreamTestBase(ITestOutputHelper testOutputHelper, TestConfiguration config)
        {
            TestOutputHelper = testOutputHelper;
            Config = config;

            Serializer = new ProtoBufEventSerializer().Scan(typeof(SomethingHappened).Assembly);
        }

        public ProtoBufEventSerializer Serializer { get; }

        public abstract EventStream Subject { get; }
        protected abstract void OnDispose();

        public void Dispose()
        {
            OnDispose();
        }

    }
}
