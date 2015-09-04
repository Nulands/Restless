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
        public T Internal { get; private set; }

        public RestServerContext ServerContext { get; set; }
        
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

            while(true)
            {
                RestListenerContext httpContext = await ServerContext.GetContextAsync();
                AddSession(httpContext);
                HandleContext(httpContext);
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

        void HandleContext(RestListenerContext context)
        {
            switch (context.Request.HttpMethod)
            {
                case "GET": HandleContextRoute(Get, context); break;
                case "POST": HandleContextRoute(Post, context); break;
                case "PUT": HandleContextRoute(Put, context); break;
                case "UPDATE": HandleContextRoute(Update, context); break;
                case "DELETE": HandleContextRoute(Delete, context); break;
                default: ServerContext.DefaultHandler(context); break;
            }
        }

        Task HandleContextRoute(Route route, RestListenerContext context)
        {
            Action<RestListenerContext> handlerAction = route[context.Request.RawUrl];
            Task handlerTask = null;
            if (handlerAction != null)
                handlerTask = pool.Run(() => handlerAction(context));
            return handlerTask;
        }
    }
}
