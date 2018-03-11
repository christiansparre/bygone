using System.Threading.Tasks;

namespace Bygone
{
    public interface IEventStream
    {
        /// <summary>
        /// The stream
        /// </summary>
        string Stream { get; }

        /// <summary>
        /// Apppends events to the stream
        /// </summary>
        Task Append(params EventData[] events);

        /// <summary>
        /// Reads the events from the first event number provided to the last event number provided
        /// </summary>
        Task<EventData[]> Read(int firstEventNumber = 1, int lastEventNumber = int.MaxValue);

        /// <summary>
        /// Deletes all events from the stream
        /// </summary>
        Task<int> Delete();
    }
}