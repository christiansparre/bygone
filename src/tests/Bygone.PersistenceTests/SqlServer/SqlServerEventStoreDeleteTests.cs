using System;
using System.Data.SqlClient;
using Bygone.SqlServer;
using Xunit.Abstractions;

namespace Bygone.PersistenceTests.SqlServer
{
    public class SqlServerEventStoreDeleteTests : EventStoreDeleteTests
    {
        private string _tableName;

        public SqlServerEventStoreDeleteTests(ITestOutputHelper testOutputHelper, TestConfiguration config) : base(testOutputHelper, config)
        {
            _tableName = Guid.NewGuid().ToString();
            Subject = new SqlServerEventStore(config.Configuration["SqlServer:ConnectionString"], Serializer, true, _tableName);
        }

        public override EventStore Subject { get; }
        protected override void OnDispose()
        {
            using (var conn = new SqlConnection(Config.Configuration["SqlServer:ConnectionString"]))
            {
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = $"DROP TABLE [{_tableName}]";
                cmd.ExecuteNonQuery();
            }
        }
    }
}