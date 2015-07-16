using System;
using System.IO;
using System.Text;
using System.Net;

namespace Nulands.Restless
{
    public sealed class RestListenerRequest
    {
        public string[] AcceptTypes { get; set; }
        public int ClientCertificateError { get; set; }
        public Encoding ContentEncoding { get; set; }
        public long ContentLength64 { get; set; }
        public string ContentType { get; set; }
        public CookieCollection Cookies { get; set; }
        public bool HasEntityBody { get; set; }
        public NameValueList QueryParameter { get; set; }
        public NameValueList FormParameter { get; set; }
        public NameValueList Header { get; set;}
        public string HttpMethod { get; set; }
        public Stream InputStream { get; set; }
        public string RawUrl { get; set; }
        public bool IsAuthenticated { get; set; }
        public bool IsLocal { get; set; }
        public bool IsSecureConnection { get; set; }
        public bool IsWebSocketRequest { get; set; }
        public bool KeepAlive { get; set; }
        public IPEndPoint LocalEndPoint { get; set; }
        public Version ProtocolVersion { get; set; }
        public IPEndPoint RemoteEndPoint { get; set; }
        public Guid RequestTraceIdentifier { get; set; }
        public string ServiceName { get; set; }
        public TransportContext TransportContext { get; set; }
        public Uri Url { get; set; }
        public Uri UrlReferrer { get; set; }
        public string UserAgent { get; set; }
        public string UserHostAddress { get; set; }
        public string UserHostName { get; set; }
        public string[] UserLanguages { get; set; }
    }
}
