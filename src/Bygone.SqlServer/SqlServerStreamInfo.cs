namespace Bygone.SqlServer
{
    public class SqlServerStreamInfo
    {
        public SqlServerStreamInfo(string stream, long minTimestamp, long maxTimestamp, int minEventNumber, int maxEventNumber)
        {
            Stream = stream;
            MinTimestamp = minTimestamp;
            MaxTimestamp = maxTimestamp;
            MinEventNumber = minEventNumber;
            MaxEventNumber = maxEventNumber;
        }

        public string Stream { get; }
        public long MinTimestamp { get; }
        public long MaxTimestamp { get; }
        public int MinEventNumber { get; }
        public int MaxEventNumber { get; }
    }
}