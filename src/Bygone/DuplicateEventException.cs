using System;

namespace Bygone
{
    public class DuplicateEventException : Exception
    {
        public string Stream { get; }
        public int[] AttemptedEventNumbers { get; }

        public DuplicateEventException(string stream, int[] attemptedEventNumbers, Exception innerException) : base($"A duplicate event error occurred on stream {stream}", innerException)
        {
            Stream = stream;
            AttemptedEventNumbers = attemptedEventNumbers;
        }
    }
}