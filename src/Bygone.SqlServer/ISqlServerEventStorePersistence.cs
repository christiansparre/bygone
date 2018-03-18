using System.Threading.Tasks;

namespace Bygone.SqlServer
{
    public interface ISqlServerEventStorePersistence : IEventStorePersistence
    {
        Task<SqlServerStreamInfo[]> ListExtra(int skip = 0, int take = 1000, long createdOnOrAfterTimestampTicks = 0, bool ascendingByTimestamp = true);
    }
}