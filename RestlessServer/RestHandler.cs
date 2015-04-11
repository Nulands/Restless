using System;
using System.Threading.Tasks;

namespace Nulands.Restless
{
    public enum RestHandlerType
    {
        HttpListenerContextAction,
        HttpListenerContextTaskFunc,
        String,
        Unknown
    }

    public enum SerializationType
    {
        Xml,
        Json
    }
    public sealed class RestHandler
    {
        public static RestHandler Create(RestHandlerType type, object value)
        {
            return new RestHandler() { Type = type, Value = value };
        }
        private RestHandler() { }

        public RestHandlerType Type { get; private set; }
        public object Value { get; private set; }

        public void SetValue(object value, RestHandlerType type = RestHandlerType.Unknown)
        {
            if (type == RestHandlerType.Unknown)        // Handler type is not given
                type = HandlerTypeFromInstance(value);

            if (type != RestHandlerType.Unknown)        // If handler type given or just found.
                Value = value;
        }

        private RestHandlerType HandlerTypeFromInstance(object value)
        {
            RestHandlerType type = RestHandlerType.Unknown;
            if (value is Action<RestListenerContext>)
                type = RestHandlerType.HttpListenerContextAction;
            else if (value is Func<RestListenerContext, Task>)
                type = RestHandlerType.HttpListenerContextTaskFunc;
            else if (value is string)
                type = RestHandlerType.String;
            return type;
        }
    }
}
