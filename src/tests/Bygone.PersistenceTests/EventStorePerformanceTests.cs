using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Bygone.PersistenceTests
{
    public abstract class EventStorePerformanceTests : EventStoreTestBase
    {
        protected EventStorePerformanceTests(ITestOutputHelper testOutputHelper, TestConfiguration config) : base(testOutputHelper, config)
        {
            Stream = Guid.NewGuid().ToString();
        }

        protected string Stream { get; }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public async Task read_performance(int count)
        {
            TestOutputHelper.WriteLine($"Appending {count} test events");

            for (int i = 0; i < count; i++)
            {
                await Subject.Append(Stream, new EventData(i + 1, DateTime.UtcNow, new SomethingHappened { What = "Stuff" }, new Dictionary<string, string> { ["Hest"] = "Test" }));
            }

            TestOutputHelper.WriteLine($"Completed appending {count} test events");
            TestOutputHelper.WriteLine($"Reading all {count} test events from event stream");

            var timer = Stopwatch.StartNew();
            var events = await Subject.Read(Stream);
            timer.Stop();

            TestOutputHelper.WriteLine("Finished reading all test events from event stream");
            TestOutputHelper.WriteLine($"  Total elapsed time: {timer.Elapsed}, ({timer.Elapsed.TotalSeconds:N2} seconds)");

            var throughput = count / (decimal)timer.Elapsed.TotalSeconds;
            TestOutputHelper.WriteLine($"  Throughput: {throughput:N0} events/s");
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public async Task delete_performance(int count)
        {
            TestOutputHelper.WriteLine($"Appending {count} test events");

            for (int i = 0; i < count; i++)
            {
                await Subject.Append(Stream, new EventData(i + 1, DateTime.UtcNow, new SomethingHappened { What = "Stuff" }, new Dictionary<string, string> { ["Hest"] = "Test" }));
            }

            TestOutputHelper.WriteLine($"Completed appending {count} test events");
            TestOutputHelper.WriteLine($"Deleting all {count} test events from event stream");

            var timer = Stopwatch.StartNew();
            var events = await Subject.Delete(Stream);
            timer.Stop();

            TestOutputHelper.WriteLine("Finished deleting all test events from event stream");
            TestOutputHelper.WriteLine($"  Total elapsed time: {timer.Elapsed}, ({timer.Elapsed.TotalSeconds:N2} seconds)");

            var throughput = count / (decimal)timer.Elapsed.TotalSeconds;
            TestOutputHelper.WriteLine($"  Throughput: {throughput:N0} events/s");
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public async Task append_performance(int count)
        {
            var warmup = await Subject.Read(Stream);

            TestOutputHelper.WriteLine($"Appending {count} test events");
            var timer = Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                await Subject.Append(Stream, new EventData(i + 1, DateTime.UtcNow, new SomethingHappened { What = "Stuff" }, new Dictionary<string, string> { ["Hest"] = "Test" }));
            }
            timer.Stop();

            TestOutputHelper.WriteLine($"Completed appending {count} test events");
            TestOutputHelper.WriteLine($"  Total elapsed time: {timer.Elapsed}, ({timer.Elapsed.TotalSeconds:N2} seconds)");

            var throughput = count / (decimal)timer.Elapsed.TotalSeconds;
            TestOutputHelper.WriteLine($"  Throughput: {throughput:N0} events/s");
        }

        [Theory]
        [InlineData(100, 10, 10)]
        [InlineData(1000, 500, 500)]
        [InlineData(5000, 2000, 3000)]
        [InlineData(5000, 2000, 3000, false)]
        public async Task list_performance(int count, int skip, int take, bool ascending = true)
        {
            var tasks = new List<Task>();

            for (int i = 0; i < count; i++)
            {
                tasks.Add(Subject.Append(Guid.NewGuid().ToString(),
                    new EventData(1, DateTime.UtcNow, new SomethingHappened { What = "Stuff" }, new Dictionary<string, string> { ["Hest"] = "Test" }),
                    new EventData(2, DateTime.UtcNow, new SomethingHappened { What = "Stuff" }, new Dictionary<string, string> { ["Hest"] = "Test" }),
                    new EventData(3, DateTime.UtcNow, new SomethingHappened { What = "Stuff" }, new Dictionary<string, string> { ["Hest"] = "Test" })));
            }

            await Task.WhenAll(tasks);

            TestOutputHelper.WriteLine($"Completed appending {count} test streams");
            TestOutputHelper.WriteLine($"Listing streams. Skip={skip}, Take={take}, Order={(ascending ? "ASC" : "DESC")}");

            var timer = Stopwatch.StartNew();
            var streamInfos = await Subject.List(skip, take, ascendingByTimestamp: ascending);
            timer.Stop();
            var throughput = streamInfos.Length / (decimal)timer.Elapsed.TotalSeconds;

            TestOutputHelper.WriteLine($"  Throughput: {throughput:N0} events/s");

        }
    }
}