using System.Data.SqlClient;
using System.IO;
using Microsoft.Extensions.Configuration;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Bygone.PersistenceTests
{
    public class TestConfiguration
    {
        public TestConfiguration()
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"))
                .AddEnvironmentVariables()
                .Build();

            EnsureSqlDatabase();
        }

        private void EnsureSqlDatabase()
        {
            using (var conn = new SqlConnection(Configuration["SqlServer:ConnectionString"].Replace("Database=Bygone_Tests;", "")))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'Bygone_Tests')
                                    BEGIN
	                                    CREATE DATABASE [Bygone_Tests]
                                    END";

                cmd.ExecuteNonQuery();
            }
        }

        public IConfigurationRoot Configuration { get; }
    }
}