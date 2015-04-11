using System;
using System.Collections.Generic;
using System.IO;

namespace Nulands.Restless
{
    public sealed class Route
    {
        public string BasePath { get; private set; }
        public string HttpMethod { get; set; }
        public Dictionary<string, RestHandler> ContextHandler { get; private set; }

        public Route(string httpMethod = null, string basePath = "")
        {
            HttpMethod = httpMethod;
            BasePath = basePath;
            ContextHandler = new Dictionary<string, RestHandler>();
        }

        public void Clear()
        {
            ContextHandler.Clear();
        }

        public Action<RestListenerContext> this[string path, Func<RestListenerContext, bool> condition = null]
        {
            get
            {
                path += BasePath;
                RestHandler handler = null;
                Action<RestListenerContext> result = null;
                if (ContextHandler.TryGetValue(path, out handler) && handler.Type == RestHandlerType.HttpListenerContextAction)
                    result = (Action<RestListenerContext>)handler.Value;
                return result;
            }
            set
            {
                path += BasePath;
                Action<RestListenerContext> newValue = value;
                if (condition != null)
                {
                    newValue = context =>
                    {
                        if (condition(context))
                            value(context);
                    };
                }
                ContextHandler[path] = RestHandler.Create(RestHandlerType.HttpListenerContextAction, newValue);
            }
        }

        public Func<RestListenerContext, string> this[string path, bool writeAsLine, Func<RestListenerContext, bool> condition = null]
        {
            set
            {
                path += BasePath;
                Action<RestListenerContext> handlerAction = context =>
                {
                    using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
                    {
                        string strResponse = value(context);
                        if (writeAsLine)
                            writer.WriteLine(strResponse);
                        else
                            writer.Write(strResponse);
                    }
                    context.Response.Close();
                };
                this[path, condition] = handlerAction;
            }
        }

        public Func<RestListenerContext, object> this[string path, SerializationType serializeAs, Func<RestListenerContext, bool> condition = null]
        {
            set
            {
                path += BasePath;
                Action<RestListenerContext> handlerAction = context =>
                {
                    using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
                    {
                        object instanceToSerialize = value(context);
                        // TODO: if object is null then 404
                        if (serializeAs == SerializationType.Json)
                        {
                            context.Response.ContentType.Set("application/json");
                            string serializedObject = SimpleJson.SimpleJson.SerializeObject(instanceToSerialize);
                            // TODO: If serialization failed 404
                            context.Response.ContentLength64.Set(serializedObject.Length);
                            context.Response.StatusCode.Set(200);  // Everything is ok
                            writer.Write(serializedObject);
                        }
                    }
                    context.Response.Close();
                };
                this[path, condition] = handlerAction;
            }
        }
    }
}
