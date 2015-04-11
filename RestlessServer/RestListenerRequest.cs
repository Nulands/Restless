using System;
using System.IO;

namespace Nulands.Restless
{
    public sealed class RestListenerRequest
    {
        public NameValueList QueryParameter { get; set; }
        public NameValueList FormParameter { get; set; }
        public NameValueList Header { get; set; }
        public string HttpMethod { get; set; }
        public string ContentType { get; set; }
        public long ContentLength64 { get; set; }
        public Stream InputStream { get; set; }
        public Uri Url { get; set; }
        public string RawUrl { get; set; }
    }
}
