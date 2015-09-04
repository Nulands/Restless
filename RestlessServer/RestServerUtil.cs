using System;
using System.Net;
using System.Collections.Generic;
using System.Threading;

namespace Nulands.Restless
{
    public class Ringbuffer<T>
    {
        T[] buffer;
        int tail = 0;
        int head = 0;

        public Ringbuffer(int capacity)
        {
            buffer = new T[capacity];
        }

        public void Preallocate<U>() where U : T, new()
        {
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = new U();
        }

        public void Enqueue(T value)
        {
            // Increment head position
            int oldHead = Interlocked.Increment(ref head) - 1;
            int index = oldHead % (buffer.Length);
            //Console.WriteLine("Enqueue[" + index + "] = " + value.ToString());
            buffer[index] = value;
        }

        public T Dequeue()
        {
            int oldTail = Interlocked.Increment(ref tail) - 1;
            int index = oldTail % (buffer.Length);
            T value = buffer[index];
            //Console.WriteLine("Dequeue[" + index + "] = " + value.ToString());
            return value;
        }
    }


    public static class RestServerUtil
    {
        private static List<RestListenerContext> contextCache = new List<RestListenerContext>(250);
 
        public static RestListenerContext ToRestListenerContext(this HttpListenerContext httpContext)
        {
            RestListenerRequest request = new RestListenerRequest();
            HttpListenerRequest httpRequest = httpContext.Request;
            request.AcceptTypes = httpRequest.AcceptTypes;
            //request.ClientCertificateError = httpRequest.ClientCertificateError;
            request.ContentEncoding = httpRequest.ContentEncoding;
            request.ContentLength64 = httpRequest.ContentLength64;
            request.ContentType = httpRequest.ContentType;
            request.ContentLength64 = httpRequest.ContentLength64;
            request.Cookies = httpRequest.Cookies;
            // TODO: Form parameter
            request.HasEntityBody = httpRequest.HasEntityBody;
            request.Header = httpRequest.Headers.ToNameValueList();
            request.HttpMethod = httpRequest.HttpMethod;
            request.InputStream = httpRequest.InputStream;
            request.IsAuthenticated = httpRequest.IsAuthenticated;
            request.IsLocal = httpRequest.IsLocal;
            request.IsSecureConnection = httpRequest.IsSecureConnection;
            request.IsWebSocketRequest = httpRequest.IsWebSocketRequest;
            request.KeepAlive = httpRequest.KeepAlive;
            request.LocalEndPoint = httpRequest.LocalEndPoint;
            request.ProtocolVersion = httpRequest.ProtocolVersion;
            request.QueryParameter = httpRequest.QueryString.ToNameValueList();
            request.RawUrl = httpRequest.RawUrl;
            request.RemoteEndPoint = httpRequest.RemoteEndPoint;
            request.RequestTraceIdentifier = httpRequest.RequestTraceIdentifier;
            request.ServiceName = httpRequest.ServiceName;
            request.UrlReferrer = httpRequest.UrlReferrer;
            request.Url = httpRequest.Url;
            request.UserAgent = httpRequest.UserAgent;
            request.UserHostAddress = httpRequest.UserHostAddress;
            request.UserHostName = httpRequest.UserHostName;
            request.UserLanguages = httpRequest.UserLanguages;

            

            // TODO: Set form url parameter
            RestListenerResponse response = new RestListenerResponse();
            HttpListenerResponse httpResponse = httpContext.Response;

            response.Abort = httpResponse.Abort;
            response.AppendCookie = httpResponse.AppendCookie;
            response.Close = httpResponse.Close;
            response.Redirect = httpResponse.Redirect;
            response.SetCookie = httpResponse.SetCookie;

            response.ContentLength64 = FProperty.Create(() => httpResponse.ContentLength64, cl => httpResponse.ContentLength64 = cl);
            response.ContentType = FProperty.Create(() => httpResponse.ContentType, ct => httpResponse.ContentType = ct);
            response.ContentEncoding = FProperty.Create(() => httpResponse.ContentEncoding, ce => httpResponse.ContentEncoding = ce);
            response.Cookies = FProperty.Create(() => httpResponse.Cookies, cookies => httpResponse.Cookies = cookies);
            response.Header = httpResponse.Headers.ToNameValueList();
            response.KeepAlive = FProperty.Create(() => httpResponse.KeepAlive, ka => httpResponse.KeepAlive = ka);
            response.OutputStream = httpResponse.OutputStream;
            response.ProtocolVersion = FProperty.Create(() => httpResponse.ProtocolVersion, pv => httpResponse.ProtocolVersion = pv);
            response.RedirectLocation = FProperty.Create(() => httpResponse.RedirectLocation, rl => httpResponse.RedirectLocation = rl);
            response.StatusCode = FProperty.Create(() => httpResponse.StatusCode, sc => httpResponse.StatusCode = sc);
            response.SendChunked = FProperty.Create(() => httpResponse.SendChunked, sc => httpResponse.SendChunked = sc);
            response.StatusDescription = FProperty.Create(() => httpResponse.StatusDescription, sd => httpResponse.StatusDescription = sd);

            return RestListenerContext.Create(request, response);
        }

        private static NameValueList ToNameValueList(this System.Collections.Specialized.NameValueCollection nvCollection)
        {
            NameValueList nvList = new NameValueList() { AddFunc = nvCollection.Add, RetreiveFunc = nvCollection.Get };
            return nvList;
        }

        public static RestServer<HttpListener> ToRestServer(this HttpListener listener)
        {
            RestServer<HttpListener> server = null;

            if (listener != null)
            {
                RestServerContext context = new RestServerContext();
                server = new RestServer<HttpListener>(listener, context);

                context.StartFunc = listener.Start;
                context.StopFunc = listener.Stop;
                context.GetContextAsync = () => listener.GetContextAsync().ContinueWith(task => task.Result.ToRestListenerContext());
                server.IsListening = FProperty.Create(() => listener.IsListening);
            }
            return server;
        }

        public static RestServer<HttpListener> CreateFromHttpListener(params string[] prefixes)
        {
            HttpListener listener = new HttpListener();
            foreach (string prefix in prefixes)
                listener.Prefixes.Add(prefix);
            return listener.ToRestServer();
        }
    }
}
