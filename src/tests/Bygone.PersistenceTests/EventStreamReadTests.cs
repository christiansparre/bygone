using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Bygone.PersistenceTests
{
    public abstract class EventStreamReadTests : EventStreamTestBase
    {
        protected EventStreamReadTests(ITestOutputHelper testOutputHelper, TestConfiguration config) : base(testOutputHelper, config)
        {
        }

        private async Task Arrange()
        {
            for (int i = 0; i < 100; i++)
            {
                await Subject.Append(new EventData(i + 1, DateTime.UtcNow, new SomethingHappened { What = $"Thing {i + 1}" }));
            }
        }

        [Fact]
        public async Task should_read_full_stream()
        {
            await Arrange();

            var persistedEventDatas = await Subject.Read(1);

            Assert.Equal(100, persistedEventDatas.Length);
        }

        [Fact]
        public async Task should_read_partial_stream()
        {
            await Arrange();

            var persistedEventDatas = await Subject.Read(10, 20);

            Assert.Equal(11, persistedEventDatas.Length);
            Assert.Equal(10, persistedEventDatas.First().EventNumber);
            Assert.Equal(20, persistedEventDatas.Last().EventNumber);
        }

        [Fact]
        public async Task should_read_from_empty_stream()
        {
            var persistedEventDatas = await Subject.Read();

            Assert.Empty(persistedEventDatas);
        }

    }
}