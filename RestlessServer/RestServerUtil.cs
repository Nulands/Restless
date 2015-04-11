using System;
using System.Net;

namespace Nulands.Restless
{
    public static class RestServerUtil
    {
        public static RestListenerContext ToRestListenerContext(this HttpListenerContext httpContext)
        {
            RestListenerRequest request = new RestListenerRequest();
            HttpListenerRequest httpRequest = httpContext.Request;
            request.ContentType = httpRequest.ContentType;
            request.ContentLength64 = httpRequest.ContentLength64;
            request.Header = httpRequest.Headers.ToNameValueList();
            request.QueryParameter = httpRequest.QueryString.ToNameValueList();
            request.HttpMethod = httpRequest.ContentType;
            request.InputStream = httpRequest.InputStream;
            request.Url = httpRequest.Url;
            request.RawUrl = httpRequest.RawUrl;

            // TODO: Set form url parameter

            RestListenerResponse response = new RestListenerResponse();
            var httpResponse = httpContext.Response;
            response.Close = httpResponse.Close;
            response.ContentLength64 = FProperty.Create(() => httpResponse.ContentLength64, cl => httpResponse.ContentLength64 = cl);
            response.ContentType = FProperty.Create(() => httpResponse.ContentType, ct => httpResponse.ContentType = ct);
            response.StatusCode = FProperty.Create(() => httpResponse.StatusCode, sc => httpResponse.StatusCode = sc);
            response.OutputStream = httpResponse.OutputStream;

            return RestListenerContext.Create(request, response);
        }

        private static NameValueList ToNameValueList(this System.Collections.Specialized.NameValueCollection nvCollection)
        {
            NameValueList nvList = new NameValueList() { AddFunc = nvCollection.Add, RetreiveFunc = nvCollection.Get };
            return nvList;
        }

        public static RestServer ToRestServer(this HttpListener listener)
        {
            RestServer server = new RestServer();
            if (listener != null)
            {
                server.StartFunc = listener.Start;
                server.StopFunc = listener.Stop;
                server.IsListening = FProperty.Create(() => listener.IsListening);
                server.GetContextAsync = () => listener.GetContextAsync().ContinueWith(task => task.Result.ToRestListenerContext());
            }
            return null;
        }

        public static Tuple<HttpListener, RestServer> CreateFromHttpListener()
        {
            HttpListener listener = new HttpListener();
            return Tuple.Create(listener, listener.ToRestServer());
        }
    }
}
