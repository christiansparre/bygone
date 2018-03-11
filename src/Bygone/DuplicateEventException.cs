using System;

namespace Bygone
{
    public class DuplicateEventException : Exception
    {
        public string Stream { get; set; }
        public int[] AttemptedEventNumbers { get; set; }

        public DuplicateEventException(string stream, int[] attemptedEventNumbers, Exception innerException) : base($"A duplicate event error occurred on stream {stream}", innerException)
        {
            Stream = stream;
            AttemptedEventNumbers = attemptedEventNumbers;
        }
    }
}