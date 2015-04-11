using System;

namespace Nulands.Restless
{
    public sealed class RouteGroup
    {
        Route get = null;
        Route post = null;
        Route put = null;
        Route delete = null;
        Route update = null;
        string basePath = "";

        public static RouteGroup Create(string basePath = "", params Action<RouteGroup>[] routeAdder)
        {
            var group = RouteGroup.Create("/Persons");
            group.Get["/ById", writeAsLine: true] = _ => "Hello World";
            group.Post["", writeAsLine: true] = context => "Get parameter (Query, Form, ..) from context.Request and send result via context.Response";
            group.Get["", writeAsLine: true] = _ => "Hello World";
            group.Get["", serializeAs: SerializationType.Xml] = _ => "Add a function that returns an object depending on the context.Reques";
            group.Get[""] = context => context.Request.Header["HeaderName"] = "";
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

        public Route Get { get { return get == null ? (get = new Route("GET", basePath)) : get; } }
        public Route Post { get { return post == null ? (post = new Route("POST", basePath)) : post; } }
        public Route Put { get { return put == null ? (put = new Route("PUT", basePath)) : put; } }
        public Route Delete { get { return delete == null ? (delete = new Route("DELETE", basePath)) : delete; } }
        public Route Update { get { return update == null ? (update = new Route("UPDATE", basePath)) : update; } }
    }
}
