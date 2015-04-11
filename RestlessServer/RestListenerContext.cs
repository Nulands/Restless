using System;

namespace Nulands.Restless
{
    public class RestListenerContext
    {
        public static RestListenerContext Create(RestListenerRequest request, RestListenerResponse response)
        {
            return new RestListenerContext() { Request = request, Response = response };
        }
        public RestListenerRequest Request { get; private set; }
        public RestListenerResponse Response { get; private set; }
    }
}
