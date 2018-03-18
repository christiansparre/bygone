using System.Threading.Tasks;

namespace Bygone.SqlServer
{
    public interface ISqlServerEventStorePersistence : IEventStorePersistence
    {
        Task<SqlServerStreamInfo[]> ListExtra(int skip = 0, int take = 1000, bool ascendingByTimestamp = true);
    }
}