using System;
using System.Data.SqlClient;
using Bygone.SqlServer;
using Xunit.Abstractions;

namespace Bygone.PersistenceTests.SqlServer
{
    public class SqlServerEventStoreListStreamsTests : EventStoreListStreamsTests
    {
        private readonly string _tableName;

        public SqlServerEventStoreListStreamsTests(ITestOutputHelper testOutputHelper, TestConfiguration config) : base(testOutputHelper, config)
        {
            _tableName = Guid.NewGuid().ToString();
            Subject = new EventStore(Serializer, new SqlServerEventStorePersistence(config.Configuration["SqlServer:ConnectionString"], true, _tableName));
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