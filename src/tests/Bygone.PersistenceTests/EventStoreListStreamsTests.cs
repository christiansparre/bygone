using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Bygone.PersistenceTests
{
    public abstract class EventStoreListStreamsTests : EventStoreTestBase
    {
        protected EventStoreListStreamsTests(ITestOutputHelper testOutputHelper, TestConfiguration config) : base(testOutputHelper, config) { }

        [Fact]
        public async Task should_list_streams()
        {
            var tasks = new List<Task>();

            for (int i = 0; i < 100; i++)
            {
                tasks.Add(Subject.Append(i.ToString(), new EventData(1, DateTime.UtcNow, new SomethingHappened { What = "Test" })));
            }

            await Task.WhenAll(tasks);

            var streamInfos = (await Subject.List()).ToDictionary(d => d.Stream);

            Assert.Equal(100, streamInfos.Count);

            for (int i = 0; i < 100; i++)
            {
                Assert.True(streamInfos.ContainsKey(i.ToString()));
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task should_list_stream_in_correct_order(bool ascending)
        {
            for (int i = 0; i < 10; i++)
            {
                await Subject.Append(i.ToString(), new EventData(1, DateTime.UtcNow, new SomethingHappened { What = "Test" }));
                await Task.Delay(10);
            }

            var streamInfos = await Subject.List(ascendingByTimestamp: @ascending);

            var expectedStreams = (ascending ? Enumerable.Range(0, 10) : Enumerable.Range(0, 10).Reverse()).ToArray();

            expectedStreams = expectedStreams.ToArray();

            for (int i = 0; i < streamInfos.Length; i++)
            {
                Assert.Equal(expectedStreams[i].ToString(), streamInfos[i].Stream);
            }
        }

        [Fact]
        public async Task should_list_subset_of_streams()
        {
            for (int i = 0; i < 100; i++)
            {
                await Subject.Append(i.ToString(), new EventData(1, DateTime.UtcNow, new SomethingHappened { What = "Test" }));
                await Task.Delay(1);
            }

            var list = await Subject.List(10, 50);

            Assert.Equal(50, list.Length);

            var s = 10;
            foreach (var streamInfo in list)
            {
                Assert.Equal(s.ToString(), streamInfo.Stream);
                s++;
            }
        }

        [Fact]
        public async Task should_list_subset_of_streams_descending()
        {
            for (int i = 0; i < 100; i++)
            {
                await Subject.Append(i.ToString(), new EventData(1, DateTime.UtcNow, new SomethingHappened { What = "Test" }));
                await Task.Delay(1);
            }

            var list = await Subject.List(10, 50, ascendingByTimestamp: false);

            Assert.Equal(50, list.Length);

            var s = 89;
            foreach (var streamInfo in list)
            {
                Assert.Equal(s.ToString(), streamInfo.Stream);
                s--;
            }
        }

        [Fact]
        public async Task should_list_streams_created_onorafter()
        {
            var dt = new DateTime(0, DateTimeKind.Utc);

            for (int i = 0; i < 10; i++)
            {
                await Subject.Append(i.ToString(), new EventData(1, dt.AddYears(i), new SomethingHappened { What = "Test" }));
            }

            var list = await Subject.List(createdOnOrAfter: dt.AddYears(4));

            Assert.Equal(6, list.Length);
            Assert.Equal("4", list[0].Stream);
        }
    }
}