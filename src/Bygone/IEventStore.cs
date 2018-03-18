using System;
using System.Threading.Tasks;

namespace Bygone
{
    public interface IEventStore
    {
        /// <summary>
        /// Apppends events to the stream
        /// </summary>
        Task Append(string stream, params EventData[] events);

        /// <summary>
        /// Reads the events from the first event number provided to the last event number provided
        /// </summary>
        Task<EventData[]> Read(string stream, int firstEventNumber = 1, int lastEventNumber = int.MaxValue);

        /// <summary>
        /// Deletes all events from the stream
        /// </summary>
        Task<int> Delete(string stream);

        /// <summary>
        /// Lists all streams in the event store
        /// </summary>
        Task<StreamInfo[]> List(int skip = 0, int take = 1000, DateTime createdOnOrAfter = default(DateTime), bool ascendingByTimestamp = true);
    }
}