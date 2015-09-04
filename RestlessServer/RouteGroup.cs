using System;

namespace Nulands.Restless
{
    public class RouteGroupModule
    {
        public static RouteGroupModule New(Action<RouteGroup> apply)
        {
            return new RouteGroupModule() { Apply = apply };
        }
        public Action<RouteGroup> Apply { get; set; }

        public void Add(Action<RouteGroup> apply)
        {
            if (Apply == null)
                Apply = apply;
            else
            {
                Action<RouteGroup> tmp = Apply;
                // compose apply actions
                Apply = rg => {
                    apply(rg);
                    tmp(rg);
                };
            }
        }
    }

    public class RouteGroup
    {
        Lazy<Route> get = null;
        Lazy<Route> post = null;
        Lazy<Route> put = null;
        Lazy<Route> delete = null;
        Lazy<Route> update = null;
        string basePath = "";

        public static RouteGroup Create(string basePath = "", params Action<RouteGroup>[] routeAdder)
        {
            
            var routeGroup = new RouteGroup(basePath);
            foreach (var rAdder in routeAdder)
                rAdder(routeGroup);
            return routeGroup;
        }

        public RouteGroup(string basePath = "")
        {
            this.basePath = basePath;
            get = new Lazy<Route>(() => new Route("GET", basePath));
            post = new Lazy<Route>(() => new Route("POST", basePath));
            put = new Lazy<Route>(() => new Route("PUT", basePath));
            delete = new Lazy<Route>(() => new Route("DELETE", basePath));
            update = new Lazy<Route>(() => new Route("UPDATE", basePath));
        }

        public void ApplyModule(RouteGroupModule module)
        {
            module.Apply(this);
        }

        public Route Get { get { return get.Value; } }
        public Route Post { get { return post.Value; } }
        public Route Put { get { return put.Value; } }
        public Route Delete { get { return delete.Value; } }
        public Route Update { get { return update.Value; } }
    }
}
