using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Bygone.PersistenceTests
{
    public abstract class EventStoreAppendTests : EventStoreTestBase
    {

        protected EventStoreAppendTests(ITestOutputHelper testOutputHelper, TestConfiguration config) : base(testOutputHelper, config)
        {
            Stream = Guid.NewGuid().ToString();
        }

        protected string Stream { get; }

        [Fact]
        public async Task should_append_single_event()
        {
            await Subject.Append(Stream, new EventData(1, DateTime.UtcNow, new SomethingHappened { What = "Stuff" }));

            var events = await Subject.Read(Stream);

            Assert.Single(events);
            Assert.Equal(1, events[0].EventNumber);
        }

        [Fact]
        public async Task should_append_batch_of_events()
        {
            await Subject.Append(Stream,
                new EventData(1, DateTime.UtcNow, new SomethingHappened { What = "Stuff" }),
                new EventData(2, DateTime.UtcNow, new SomethingHappened { What = "Stuff" }),
                new EventData(3, DateTime.UtcNow, new SomethingHappened { What = "Stuff" }));

            var events = await Subject.Read(Stream);

            Assert.Equal(3, events.Length);
            Assert.Equal(1, events[0].EventNumber);
            Assert.Equal(2, events[1].EventNumber);
            Assert.Equal(3, events[2].EventNumber);
        }

        [Fact]
        public async Task should_not_write_events_with_the_same_eventNumber()
        {
            await Subject.Append(Stream, new EventData(1, DateTime.UtcNow, new SomethingHappened { What = "Stuff" }), new EventData(2, DateTime.UtcNow, new SomethingHappened { What = "Other stuff" }));

            var exception = await Assert.ThrowsAsync<DuplicateEventException>(async () =>
            {
                await Subject.Append(Stream, new EventData(1, DateTime.UtcNow, new SomethingHappened { What = "Stuff" }));
            });

            Assert.Equal(Stream, exception.Stream);
            Assert.Equal(1, exception.AttemptedEventNumbers[0]);
        }

        [Fact]
        public async Task should_persist_metadata()
        {
            await Subject.Append(Stream, new EventData(1, DateTime.UtcNow, new SomethingHappened { What = "Stuff" }, new Dictionary<string, string> { ["Hest"] = "test" }));

            var persistedEventDatas = await Subject.Read(Stream);

            var e = persistedEventDatas[0];

            Assert.Equal("test", e.Metadata["Hest"]);
        }

    }
}