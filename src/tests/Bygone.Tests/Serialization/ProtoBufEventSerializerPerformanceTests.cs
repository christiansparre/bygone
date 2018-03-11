using System.Collections.Generic;
using System.Diagnostics;
using Bygone.Serialization.ProtoBufNet;
using Xunit;
using Xunit.Abstractions;

namespace Bygone.Tests.Serialization
{
    public class ProtoBufEventSerializerPerformanceTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ProtoBufEventSerializerPerformanceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(10000)]
        [InlineData(100000)]
        public void Serializer(int count)
        {
            var serializer = new ProtoBufEventSerializer()
                .Scan(typeof(TestEvent).Assembly);

            var timer = Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                serializer.SerializeEvent(new TestEvent { Hest = "en hurtig en af slagsen" });
            }
            timer.Stop();

            var d = count / timer.Elapsed.TotalSeconds;

            _testOutputHelper.WriteLine($"Events/s: {d:N0}");
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(10000)]
        [InlineData(100000)]
        public void Deserializer(int count)
        {
            var serializer = new ProtoBufEventSerializer()
                .Scan(typeof(TestEvent).Assembly);

            var events = new List<byte[]>();
            for (int i = 0; i < count; i++)
            {
                events.Add(serializer.SerializeEvent(new TestEvent { Hest = "en hurtig en af slagsen" }));
            }

            var timer = Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                serializer.DeserializeEvent(typeof(TestEvent), events[i]);
            }
            timer.Stop();

            var d = count / timer.Elapsed.TotalSeconds;

            _testOutputHelper.WriteLine($"Events/s: {d:N0}");
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(10000)]
        [InlineData(100000)]
        public void SerializeMetadata(int count)
        {
            var serializer = new ProtoBufEventSerializer()
                .Scan(typeof(TestEvent).Assembly);

            var timer = Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                serializer.SerializeMetadata(new Dictionary<string, string>());
            }
            timer.Stop();

            var d = count / timer.Elapsed.TotalSeconds;

            _testOutputHelper.WriteLine($"Events/s: {d:N0}");
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(10000)]
        [InlineData(100000)]
        public void DeserializeMetadata(int count)
        {
            var serializer = new ProtoBufEventSerializer()
                .Scan(typeof(TestEvent).Assembly);

            var events = new List<byte[]>();
            for (int i = 0; i < count; i++)
            {
                events.Add(serializer.SerializeMetadata(new Dictionary<string, string>()));
            }

            var timer = Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                serializer.DeserializeMetadata(events[i]);
            }
            timer.Stop();

            var d = count / timer.Elapsed.TotalSeconds;

            _testOutputHelper.WriteLine($"Events/s: {d:N0}");
        }
    }
}