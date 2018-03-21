using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Bygone.SqlServer;
using Xunit;

namespace Bygone.PersistenceTests.SqlServer
{
    public class SqlServerEventStoreListExtraTests : IDisposable, IClassFixture<TestConfiguration>
    {
        private readonly TestConfiguration _config;
        private readonly string _tableName;
        private readonly SqlServerEventStorePersistence _subject;

        public SqlServerEventStoreListExtraTests(TestConfiguration config)
        {
            _config = config;
            _tableName = Guid.NewGuid().ToString();
            _subject = new SqlServerEventStorePersistence(config.Configuration["SqlServer:ConnectionString"], true, _tableName);
        }

        [Fact]
        public async Task should_list_streams()
        {
            var tasks = new List<Task>();

            for (int i = 0; i < 100; i++)
            {
                tasks.Add(_subject.Append(i.ToString(), new[] { new SerializedEvent(1, DateTime.UtcNow.Ticks, "TestEvent", new byte[0], new byte[0]) }));
            }

            await Task.WhenAll(tasks);

            var streamInfos = (await _subject.List()).ToDictionary(d => d.Stream);

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
                await _subject.Append(i.ToString(), new[] { new SerializedEvent(1, DateTime.UtcNow.Ticks, "TestEvent", new byte[0], new byte[0]) });
                await Task.Delay(10);
            }

            var streamInfos = await _subject.List(ascendingByTimestamp: @ascending);

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
                await _subject.Append(i.ToString(), new[] { new SerializedEvent(1, DateTime.UtcNow.Ticks, "TestEvent", new byte[0], new byte[0]) });
                await Task.Delay(1);
            }

            var list = await _subject.List(10, 50);

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
                await _subject.Append(i.ToString(), new[] { new SerializedEvent(1, DateTime.UtcNow.Ticks, "TestEvent", new byte[0], new byte[0]) });
                await Task.Delay(1);
            }

            var list = await _subject.List(10, 50, ascendingByTimestamp: false);

            Assert.Equal(50, list.Length);

            var s = 89;
            foreach (var streamInfo in list)
            {
                Assert.Equal(s.ToString(), streamInfo.Stream);
                s--;
            }
        }

        public void Dispose()
        {
            using (var conn = new SqlConnection(_config.Configuration["SqlServer:ConnectionString"]))
            {
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = $"DROP TABLE [{_tableName}]";
                cmd.ExecuteNonQuery();
            }
        }
    }
}