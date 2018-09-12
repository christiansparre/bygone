using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Raven.Embedded;

namespace Bygone.PersistenceTests.RavenDb
{
    public class RavenDbTestContext : IDisposable
    {
        public RavenDbTestContext(TestConfiguration configuration)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"))
                .AddEnvironmentVariables()
                .Build();
            
            EmbeddedServer.Instance.StartServer();
        }

        public void Dispose()
        {
            EmbeddedServer.Instance.Dispose();
        }
    }
}