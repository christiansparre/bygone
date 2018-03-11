using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Bygone.PersistenceTests
{
    public abstract class EventStoreInfoTests : EventStoreTestBase
    {
        protected EventStoreInfoTests(ITestOutputHelper testOutputHelper, TestConfiguration config) : base(testOutputHelper, config)
        {
            Stream1 = Guid.NewGuid().ToString();
            Stream2 = Guid.NewGuid().ToString();
        }

        protected string Stream1 { get; }
        protected string Stream2 { get; }

        [Fact]
        public async Task should_count_events()
        {
            await Subject.Append(Stream1, new EventData(1, DateTime.UtcNow, new SomethingHappened { What = "stuff" }), new EventData(2, DateTime.UtcNow, new SomethingHappened { What = "some other stuff" }));

            var count = await Subject.Count(Stream1);

            Assert.Equal(2, count);
        }

        [Fact]
        public async Task should_list_streams()
        {
            var stream1Created = DateTime.UtcNow;
            var stream2Created = DateTime.UtcNow.AddDays(1);

            await Subject.Append(Stream1, new EventData(1, stream1Created, new SomethingHappened { What = "stuff" }), new EventData(2, DateTime.UtcNow, new SomethingHappened { What = "some other stuff" }));
            await Subject.Append(Stream2, new EventData(1, stream2Created, new SomethingHappened { What = "stuff" }), new EventData(2, DateTime.UtcNow, new SomethingHappened { What = "some other stuff" }));

            var streamInfos = await Subject.List();

            Assert.Equal(2, streamInfos.Length);
            Assert.Equal(stream1Created, streamInfos[0].Created);
            Assert.Equal(stream2Created, streamInfos[1].Created);
        }

        [Fact]
        public async void should_get_highest_event_number()
        {
            await Subject.Append(Stream1, new EventData(1, DateTime.UtcNow, new SomethingHappened { What = "stuff" }), new EventData(2, DateTime.UtcNow, new SomethingHappened { What = "some other stuff" }));

            var i = await Subject.HighestEventNumber(Stream1);

            Assert.Equal(2, i);
        }
    }
}