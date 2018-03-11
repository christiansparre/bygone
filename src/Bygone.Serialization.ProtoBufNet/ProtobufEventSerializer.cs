using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ProtoBuf;
using ProtoBuf.Meta;

namespace Bygone.Serialization.ProtoBufNet
{
    public class ProtoBufEventSerializer : EventSerializer
    {
        private readonly RuntimeTypeModel _typeModel;

        public ProtoBufEventSerializer()
        {
            _typeModel = TypeModel.Create();

            // Explicitly register metadata dictionary with protobuf
            // otherwise the serializer is SLOW dealing with dictionaries
            _typeModel.Add(typeof(Dictionary<string, string>), true);
        }

        public ProtoBufEventSerializer Scan(Assembly assembly)
        {
            var types = assembly.ExportedTypes
                .Where(t => t.GetTypeInfo().GetCustomAttribute<ProtoContractAttribute>() != null)
                .Select(s => new { Type = s, s.GetTypeInfo().GetCustomAttribute<ProtoContractAttribute>().Name }).ToList();

            foreach (var type in types)
            {
                Map(type.Type, type.Name ?? type.Type.FullName);
            }

            return this;
        }

        public override byte[] SerializeEvent(object @event)
        {
            using (var stream = new MemoryStream())
            {
                _typeModel.Serialize(stream, @event);
                return stream.ToArray();
            }
        }

        public override object DeserializeEvent(Type type, byte[] @event)
        {
            using (var stream = new MemoryStream(@event))
            {
                return _typeModel.Deserialize(stream, null, type);
            }
        }

        public override byte[] SerializeMetadata(Dictionary<string, string> metadata)
        {
            using (var stream = new MemoryStream())
            {
                _typeModel.Serialize(stream, metadata);
                return stream.ToArray();
            }
        }

        public override Dictionary<string, string> DeserializeMetadata(byte[] metadata)
        {
            using (var stream = new MemoryStream(metadata))
            {
                return (Dictionary<string, string>)_typeModel.Deserialize(stream, null, typeof(Dictionary<string, string>));
            }
        }


    }
}