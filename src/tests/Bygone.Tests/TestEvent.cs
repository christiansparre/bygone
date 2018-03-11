using ProtoBuf;

namespace Bygone.Tests
{
    [ProtoContract]
    public class TestEvent
    {
        [ProtoMember(1)]
        public string Hest { get; set; }
    }
}