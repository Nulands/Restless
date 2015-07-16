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
        Route get = null;
        Route post = null;
        Route put = null;
        Route delete = null;
        Route update = null;
        string basePath = "";

        public static RouteGroup Create(string basePath = "", params Action<RouteGroup>[] routeAdder)
        {
            var routeGroup = new RouteGroup(basePath);
            foreach (var rAdder in routeAdder)
                rAdder(routeGroup);
            return routeGroup;
        }

        public RouteGroup(string basePath = "", bool createAllGroups = false)
        {
            this.basePath = basePath;
            if (createAllGroups)
            {
                get = new Route("GET", basePath);
                post = new Route("POST", basePath);
                put = new Route("PUT", basePath);
                delete = new Route("DELETE", basePath);
                update = new Route("UPDATE", basePath);
            }
        }

        public void ApplyModule(RouteGroupModule module)
        {
            module.Apply(this);
        }

        public Route Get { get { return get == null ? (get = new Route("GET", basePath)) : get; } }
        public Route Post { get { return post == null ? (post = new Route("POST", basePath)) : post; } }
        public Route Put { get { return put == null ? (put = new Route("PUT", basePath)) : put; } }
        public Route Delete { get { return delete == null ? (delete = new Route("DELETE", basePath)) : delete; } }
        public Route Update { get { return update == null ? (update = new Route("UPDATE", basePath)) : update; } }
    }
}
