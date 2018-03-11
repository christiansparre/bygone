using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Bygone.PersistenceTests
{
    public abstract class EventStreamAppendTests : EventStreamTestBase
    {

        protected EventStreamAppendTests(ITestOutputHelper testOutputHelper, TestConfiguration config) : base(testOutputHelper, config)
        {
        }

        [Fact]
        public async Task should_append_single_event()
        {
            await Subject.Append(new EventData(1, DateTime.UtcNow, new SomethingHappened { What = "Stuff" }));

            var events = await Subject.Read();

            Assert.Single(events);
            Assert.Equal(1, events[0].EventNumber);
        }

        [Fact]
        public async Task should_not_write_events_with_the_same_eventNumber()
        {
            await Subject.Append(new EventData(1, DateTime.UtcNow, new SomethingHappened { What = "Stuff" }), new EventData(2, DateTime.UtcNow, new SomethingHappened { What = "Other stuff" }));

            var exception = await Assert.ThrowsAsync<DuplicateEventException>(async () =>
            {
                await Subject.Append(new EventData(1, DateTime.UtcNow, new SomethingHappened { What = "Stuff" }));
            });

            Assert.Equal(Subject.Stream, exception.Stream);
            Assert.Equal(1, exception.AttemptedEventNumbers[0]);
        }

        [Fact]
        public async Task should_persist_metadata()
        {
            await Subject.Append(new EventData(1, DateTime.UtcNow, new SomethingHappened { What = "Stuff" }, new Dictionary<string, string> { ["Hest"] = "test" }));

            var persistedEventDatas = await Subject.Read();

            var e = persistedEventDatas[0];

            Assert.Equal("test", e.Metadata["Hest"]);
        }

    }
}