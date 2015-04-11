using System;
using System.Threading.Tasks;

using Nulands;

namespace Nulands.Restless
{
    public class RestServer
    {
        RouteGroup routes = new RouteGroup();

        public Action<RestListenerContext> DefaultHandler { get; set; }
        public Func<Task<RestListenerContext>> GetContextAsync { get; set; }
        public Action StartFunc { get; set; }
        public Action StopFunc { get; set; }
        public RouteGroup Routes { get { return routes; } }
        public FProperty<bool> IsListening { get; set; }

        public async Task Start()
        {
            StartFunc();
            while(true)
            {
                var httpContext = await GetContextAsync();
                switch(httpContext.Request.HttpMethod)
                {
                    case "GET": await HandleContext(routes.Get, httpContext); break;
                    case "POST": await HandleContext(routes.Post, httpContext); break;
                    case "PUT": await HandleContext(routes.Put, httpContext); break;
                    case "UPDATE": await HandleContext(routes.Update, httpContext); break;
                    case "DELETE": await HandleContext(routes.Delete, httpContext); break;
                    default: DefaultHandler(httpContext); break;
                }
            }
        }

        public void Stop()
        {
            if (StopFunc != null)
                StopFunc();
        }

        private Task HandleContext(Route route, RestListenerContext context)
        {
            Action<RestListenerContext> handlerAction = route[context.Request.RawUrl];
            Task handlerTask = null;
            if (handlerAction != null)
                handlerTask = Task.Run(() => handlerAction(context));
            return handlerTask;
        }
    }
}
