using System.Linq;
using Raven.Client.Documents.Indexes;

namespace Bygone.RavenDb
{
    public class Events_ByStreamAndEventNumber : AbstractIndexCreationTask<RavenDbEventDocument>
    {
        public Events_ByStreamAndEventNumber()
        {
            Map = events => from e in events
                select new
                {
                    e.Stream,
                    e.EventNumber
                };
        }
    }
}