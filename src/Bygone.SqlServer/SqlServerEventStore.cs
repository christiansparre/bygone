using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Bygone.Serialization;

namespace Bygone.SqlServer
{
    public class SqlServerEventStorePersistence : IEventStorePersistence
    {
        private readonly string _connectionString;
        private readonly string _eventsTableName;


        public SqlServerEventStorePersistence(string connectionString, bool ensureSchema = false, string eventsTableName = "Events")
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

                                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UIX_{eventsTableName}_Stream_EventNumber')
	                                    BEGIN
		                                    CREATE UNIQUE NONCLUSTERED INDEX [UIX_{eventsTableName}_Stream_EventNumber] ON [dbo].[{eventsTableName}]
		                                    (
                                                [Stream] ASC,
			                                    [EventNumber] ASC
			                                    
		                                    )
	                                    END

                                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_{eventsTableName}_EventNumber_Including_Stream,Timestamp')
	                                    BEGIN
                                            CREATE NONCLUSTERED INDEX [IX_{eventsTableName}_EventNumber_Including_Stream,Timestamp] ON [dbo].[{eventsTableName}] ([EventNumber] ASC) INCLUDE ([Stream], [Timestamp])                                                                               
	                                    END
                                    ";

                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = createSchemaSql;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public async Task Append(string stream, SerializedEvent[] events)
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

        public async Task<SerializedEvent[]> Read(string stream, int firstEventNumber, int lastEventNumber)
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

        public async Task<int> Delete(string stream)
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

        public async Task<SerializedStreamInfo[]> List(int skip = 0, int take = 1000, long createdOnOrAfterTimestampTicks = 0, bool ascendingByTimestamp = true)
        {
            var orderBy = ascendingByTimestamp ? "ASC" : "DESC";

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT Stream, Timestamp FROM [{_eventsTableName}] WHERE EventNumber = 1 AND Timestamp >= @CreatedOnOrAfter ORDER BY Timestamp {orderBy} OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";
                cmd.Parameters.Add(new SqlParameter("Skip", skip));
                cmd.Parameters.Add(new SqlParameter("Take", take));
                cmd.Parameters.Add(new SqlParameter("CreatedOnOrAfter", createdOnOrAfterTimestampTicks));

                var reader = await cmd.ExecuteReaderAsync();

                var events = new List<SerializedStreamInfo>();

                while (reader.Read())
                {
                    events.Add(new SerializedStreamInfo(
                        (string)reader["Stream"],
                        (long)reader["Timestamp"]));
                }

                return events.ToArray();
            }
        }
    }
}