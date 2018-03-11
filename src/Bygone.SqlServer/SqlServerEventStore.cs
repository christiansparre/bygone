using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Bygone.Serialization;

namespace Bygone.SqlServer
{
    public class SqlServerEventStore : EventStore
    {
        private readonly string _connectionString;
        private readonly string _eventsTableName;

        public SqlServerEventStore(string connectionString, EventSerializer serializer, bool ensureSchema = false, string eventsTableName = "Events") : base(serializer)
        {
            _connectionString = connectionString;
            _eventsTableName = eventsTableName;

            if (ensureSchema)
            {
                var createSchemaSql = $@"
                                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '{eventsTableName}')
	                                    BEGIN
		                                    CREATE TABLE [dbo].[{eventsTableName}] (
			                                    [Id] [uniqueidentifier] NOT NULL,
			                                    [Stream] [nvarchar](255) NOT NULL,
			                                    [EventNumber] [int] NOT NULL,
			                                    [EventType] [nvarchar](255) NOT NULL,
			                                    [Timestamp] [bigint] NOT NULL,
			                                    [Event] [varbinary](max) NOT NULL,
			                                    [Metadata] [varbinary](max) NOT NULL,
		                                        CONSTRAINT [PK_{eventsTableName}] PRIMARY KEY CLUSTERED ([Id] ASC))
	                                    END

                                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_{eventsTableName}_Stream_EventNumber')
	                                    BEGIN
		                                    CREATE UNIQUE NONCLUSTERED INDEX [IX_{eventsTableName}_Stream_EventNumber] ON [dbo].[{eventsTableName}]
		                                    (
			                                    [EventNumber] ASC,
			                                    [Stream] ASC
		                                    )
	                                    END";

                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = createSchemaSql;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        protected override async Task WriteEvents(string stream, SerializedEvent[] events)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var transaction = conn.BeginTransaction();

                try
                {
                    foreach (var e in events)
                    {
                        var cmd = conn.CreateCommand();
                        cmd.Transaction = transaction;
                        cmd.CommandText = $"INSERT INTO [{_eventsTableName}] (Id,Stream,EventNumber,EventType,Timestamp,Event,Metadata) VALUES (@Id,@Stream,@EventNumber,@EventType,@Timestamp,@Event,@Metadata)";

                        cmd.Parameters.Add(new SqlParameter("Id", Guid.NewGuid()));
                        cmd.Parameters.Add(new SqlParameter("Stream", stream));
                        cmd.Parameters.Add(new SqlParameter("EventNumber", e.EventNumber));
                        cmd.Parameters.Add(new SqlParameter("EventType", e.EventType));
                        cmd.Parameters.Add(new SqlParameter("Timestamp", e.TimestampTicks));
                        cmd.Parameters.Add(new SqlParameter("Event", e.Event));
                        cmd.Parameters.Add(new SqlParameter("Metadata", e.Metadata));

                        await cmd.ExecuteNonQueryAsync();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    if (ex is SqlException ex2 && ex2.Number == 2601)
                    {
                        throw new DuplicateEventException(stream, events.Select(s => s.EventNumber).ToArray(), ex);
                    }

                    throw;
                }
                finally
                {
                    transaction.Dispose();
                }
            }
        }

        protected override async Task<SerializedEvent[]> ReadEvents(string stream, int firstEventNumber, int lastEventNumber)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT  * FROM [{_eventsTableName}] WHERE Stream = @Stream AND EventNumber >= @FirstEventNumber AND EventNumber <= @LastEventNumber ORDER BY EventNumber ASC";
                cmd.Parameters.Add(new SqlParameter("Stream", stream));
                cmd.Parameters.Add(new SqlParameter("FirstEventNumber", firstEventNumber));
                cmd.Parameters.Add(new SqlParameter("LastEventNumber", lastEventNumber));

                var reader = await cmd.ExecuteReaderAsync();

                var events = new List<SerializedEvent>();

                while (reader.Read())
                {
                    events.Add(new SerializedEvent(
                        (int)reader["EventNumber"],
                        (long)reader["Timestamp"],
                        (string)reader["EventType"],
                        (byte[])reader["Event"],
                        (byte[])reader["Metadata"]));
                }

                return events.ToArray();
            }
        }

        protected override async Task<int> DeleteStream(string stream)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var cmd = conn.CreateCommand();
                cmd.CommandText = $"DELETE FROM [{_eventsTableName}] WHERE Stream = @Stream";
                cmd.Parameters.Add(new SqlParameter("Stream", stream));

                return await cmd.ExecuteNonQueryAsync();
            }
        }

        protected override async Task<SerializedStreamInfo[]> ListStreams()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT Stream, Timestamp AS Created FROM [{_eventsTableName}] WHERE EventNumber = 1 ORDER BY Created";
                
                var reader = await cmd.ExecuteReaderAsync();

                var infos = new List<SerializedStreamInfo>();

                while (reader.Read())
                {
                    infos.Add(new SerializedStreamInfo(
                        (string)reader["Stream"],
                        (long)reader["Created"]));
                }

                return infos.ToArray();
            }
        }

        protected override async Task<int> CountEvents(string stream)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT COUNT(*) FROM [{_eventsTableName}] WHERE Stream = @Stream";
                cmd.Parameters.Add(new SqlParameter("Stream", stream));

                return (int)await cmd.ExecuteScalarAsync();
            }
        }

        protected override async Task<int> GetHighestEventNumber(string stream)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT TOP 1 EventNumber FROM [{_eventsTableName}] WHERE Stream = @Stream ORDER BY EventNumber DESC";
                cmd.Parameters.Add(new SqlParameter("Stream", stream));

                return (int)await cmd.ExecuteScalarAsync();
            }
        }
    }
}