using System;
using System.Data.SqlClient;
using Bygone.SqlServer;
using Xunit.Abstractions;

namespace Bygone.PersistenceTests.SqlServer
{
    public class SqlServerEventStreamAppendTests : EventStreamAppendTests
    {
        public SqlServerEventStreamAppendTests(ITestOutputHelper testOutputHelper, TestConfiguration config) : base(testOutputHelper, config)
        {
            Subject = new SqlServerEventStream(Guid.NewGuid().ToString(), config.Configuration["SqlServer:ConnectionString"], Serializer, true);

            using (var conn = new SqlConnection(config.Configuration["SqlServer:ConnectionString"]))
            {
                conn.OpenAsync();

            }
        }

        public override EventStream Subject { get; }
        protected override void OnDispose()
        {
            Subject.Delete().GetAwaiter().GetResult();
        }
    }
}