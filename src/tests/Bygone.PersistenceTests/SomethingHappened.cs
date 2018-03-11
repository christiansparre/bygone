using ProtoBuf;

namespace Bygone.PersistenceTests
{
    [ProtoContract]
    public class SomethingHappened
    {
        [ProtoMember(1)]
        public string What { get; set; }
    }
}