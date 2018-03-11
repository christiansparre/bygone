using System;
using Bygone.SqlServer;
using Xunit.Abstractions;

namespace Bygone.PersistenceTests.SqlServer
{
    public class SqlServerEventStreamPerformanceTests : EventStreamPerformanceTests
    {
        public SqlServerEventStreamPerformanceTests(ITestOutputHelper testOutputHelper, TestConfiguration config) : base(testOutputHelper, config)
        {
            Subject = new SqlServerEventStream(Guid.NewGuid().ToString(), config.Configuration["SqlServer:ConnectionString"], Serializer, true);
        }

        public override EventStream Subject { get; }
        protected override void OnDispose()
        {
            Subject.Delete().GetAwaiter().GetResult();
        }
    }
}