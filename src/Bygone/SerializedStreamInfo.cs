namespace Bygone
{
    public class SerializedStreamInfo
    {
        public SerializedStreamInfo(string stream, long createdTicks)
        {
            Stream = stream;
            CreatedTicks = createdTicks;
        }

        public string Stream { get; }
        public long CreatedTicks { get; }
    }
}