using Xunit;

namespace Bygone.PersistenceTests.RavenDb
{
    [CollectionDefinition(nameof(RavenDbTestsCollection))]
    public class RavenDbTestsCollection : ICollectionFixture<RavenDbTestContext>
    {

    }
}