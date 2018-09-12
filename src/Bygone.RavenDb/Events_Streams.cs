using System.Linq;
using Raven.Client.Documents.Indexes;

namespace Bygone.RavenDb
{
    public class Events_Streams : AbstractIndexCreationTask<RavenDbEventDocument>
    {
        public Events_Streams()
        {
            Map = events => from e in events
                where e.EventNumber == 1
                select new
                {
                    e.Stream,
                    e.EventNumber,
                    e.Timestamp
                };
        }
    }
}