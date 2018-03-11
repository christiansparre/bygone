﻿using System.Threading.Tasks;

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
        /// Gets the number of events in the stream
        /// </summary>
        Task<int> Count(string stream);

        /// <summary>
        /// Lists all the stream in the store
        /// </summary>
        /// <returns></returns>
        Task<StreamInfo[]> List();
    }
}