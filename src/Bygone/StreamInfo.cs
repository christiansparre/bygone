using System;

namespace Bygone
{
    public class StreamInfo
    {
        public StreamInfo(string stream, DateTime created)
        {
            Stream = stream;
            Created = created;
        }

        public string Stream { get; }
        public DateTime Created { get; }
    }
}