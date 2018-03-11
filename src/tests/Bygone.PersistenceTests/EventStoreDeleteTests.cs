using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Bygone.PersistenceTests
{
    public abstract class EventStoreDeleteTests : EventStoreTestBase
    {
        protected EventStoreDeleteTests(ITestOutputHelper testOutputHelper, TestConfiguration config) : base(testOutputHelper, config)
        {
            Stream = Guid.NewGuid().ToString();
        }

        protected string Stream { get; }

        [Fact]
        public async Task should_delete_all_events_in_stream()
        {
            await Subject.Append(Stream, new EventData(1, DateTime.UtcNow, new SomethingHappened { What = "stuff" }), new EventData(2, DateTime.UtcNow, new SomethingHappened { What = "some other stuff" }));

            await Subject.Delete(Stream);

            var events = await Subject.Read(Stream);

            Assert.Empty(events);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(42)]
        [InlineData(1337)]
        public async Task should_return_the_number_of_events_deleted(int count)
        {
            for (int i = 0; i < count; i++)
            {
                await Subject.Append(Stream, new EventData(i + 1, DateTime.UtcNow, new SomethingHappened { What = "stuff" }));
            }

            var events = await Subject.Delete(Stream);

            Assert.Equal(count, events);
        }
    }
}