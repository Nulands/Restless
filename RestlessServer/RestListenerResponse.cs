using System;
using System.IO;
using System.Net;
using System.Text;

namespace Nulands.Restless
{
    public sealed class RestListenerResponse
    {
        public NameValueList Header { get; set; }
        public FProperty<string> ContentType { get; set; }
        public FProperty<long> ContentLength64 { get; set; }
        public FProperty<Encoding> ContentEncoding { get; set; }
        public FProperty<CookieCollection> Cookies { get; set; }
        public FProperty<bool> KeepAlive { get; set; }
        public FProperty<int> StatusCode { get; set; }
        public Stream OutputStream { get; set; }
        public FProperty<Version> ProtocolVersion { get; set; }
        public FProperty<string> RedirectLocation { get; set; }
        public FProperty<bool> SendChunked { get; set; }
        public FProperty<string> StatusDescription { get; set; }

        public Action Abort { get; set; }
        public Action<Cookie> AppendCookie { get; set; }
        public Action Close { get; set; }
        public Action<string> Redirect { get; set; }
        public Action<Cookie> SetCookie { get; set; }
    }
}
