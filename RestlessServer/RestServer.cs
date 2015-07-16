using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using Nulands;

using Proactive.Threading;

namespace Nulands.Restless
{
    public class RestServerContext
    {
        public Action<RestListenerContext> DefaultHandler { get; set; }
        public Func<Task<RestListenerContext>> GetContextAsync { get; set; }
        public Action StartFunc { get; set; }
        public Action StopFunc { get; set; }

        public Func<RestListenerContext, bool> NeedsSession { get; set; }
        public Func<RestListenerContext, object> GetSessionKey { get; set; }
        public Func<DateTime, DateTime> GetValidUntil { get; set; }
    }

    public class RestServer<T> : RouteGroup
    {
        //RouteGroup routes = new RouteGroup();
        public T Internal { get; private set; }
        public RestServerContext ServerContext { get; set; }
        //public RouteGroup Routes { get { return routes; } }
        public FProperty<bool> IsListening { get; set; }


        bool verbose = false;
        Dictionary<object, Session> sessions = new Dictionary<object, Session>();
        WorkerPool pool;

        public RestServer(T internalServer, RestServerContext serverContext, bool verbose = false)
        {
            Internal = internalServer;
            ServerContext = serverContext;
            this.verbose = verbose;
        }

        public async Task Start()
        {
            pool = new WorkerPool(4, true, false, 0);
            ServerContext.StartFunc();
            /*if (verbose)
            {
                Console.WriteLine("Server loop starting...");
                Console.WriteLine("Routes: ");
                foreach (var route in Get.ContextHandler)
                {
                    Console.WriteLine(route.Key);
                }
                Console.WriteLine("----------------------------------------------");
            }*/
            while(true)
            {
                RestListenerContext httpContext = await ServerContext.GetContextAsync();

                /*if (verbose)
                {
                    Console.WriteLine("----------------------------------------------");
                    Console.WriteLine("Httpcontext received");
                    Console.WriteLine("Method: " + httpContext.Request.HttpMethod);
                    Console.WriteLine("RawUrl: " + httpContext.Request.RawUrl);
                    Console.WriteLine("URL: " + httpContext.Request.Url);
                }*/

                AddSession(httpContext);
                switch(httpContext.Request.HttpMethod)
                {
                    case "GET": HandleContext(Get, httpContext); break;
                    case "POST": HandleContext(Post, httpContext); break;
                    case "PUT": HandleContext(Put, httpContext); break;
                    case "UPDATE": HandleContext(Update, httpContext); break;
                    case "DELETE": HandleContext(Delete, httpContext); break;
                    default: ServerContext.DefaultHandler(httpContext); break;
                }
            }
        }

        public void Stop()
        {
            ServerContext.StopFunc();
        }

        void AddSession(RestListenerContext context)
        {
            if (ServerContext.NeedsSession != null && ServerContext.GetSessionKey != null)
            {
                if (ServerContext.NeedsSession(context))
                {
                    Session session = null;
                    if (!sessions.TryGetValue(ServerContext.GetSessionKey(context), out session))
                    {
                        session = new Session();
                        if (ServerContext.GetValidUntil != null)
                            session.ValidUntil = ServerContext.GetValidUntil(session.CreationDate);
                    }
                    context.Session = session;
                }
            }
        }

        private Task HandleContext(Route route, RestListenerContext context)
        {
            Action<RestListenerContext> handlerAction = route[context.Request.RawUrl];
            /*if (verbose)
            {
                Console.WriteLine("----------------------------------------------");
                Console.WriteLine("Handler action " + handlerAction);
                Console.WriteLine("Handler action is null? " + (handlerAction == null));
            }*/

            Task handlerTask = null;
            if (handlerAction != null)
                handlerTask = pool.Run(() => handlerAction(context));
            return handlerTask;
        }
    }
}
