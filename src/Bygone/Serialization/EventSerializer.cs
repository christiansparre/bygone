using System;
using System.Collections.Generic;

namespace Bygone.Serialization
{
    public abstract class EventSerializer
    {
        private Dictionary<Type, string> _eventTypeToTypeNameMap = new Dictionary<Type, string>();
        private Dictionary<string, Type> _eventTypeNameToEventTypeMap = new Dictionary<string, Type>();


        public abstract byte[] SerializeEvent(object @event);
        public abstract object DeserializeEvent(Type type, byte[] @event);
        public abstract byte[] SerializeMetadata(Dictionary<string, string> metadata);
        public abstract Dictionary<string, string> DeserializeMetadata(byte[] metadata);


        public Type Lookup(string eventType)
        {
            if (_eventTypeNameToEventTypeMap.TryGetValue(eventType, out var type))
            {
                return type;
            }

            return null;
        }

        public string Lookup(Type eventType)
        {
            if (_eventTypeToTypeNameMap.TryGetValue(eventType, out var type))
            {
                return type;
            }

            return null;
        }

        public void Map(Type eventType, string eventTypeName)
        {
            _eventTypeToTypeNameMap.Add(eventType, eventTypeName);
            _eventTypeNameToEventTypeMap.Add(eventTypeName, eventType);
        }
    }



}