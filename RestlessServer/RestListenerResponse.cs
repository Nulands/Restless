using System;
using System.IO;

namespace Nulands.Restless
{
    public sealed class RestListenerResponse
    {
        public NameValueList Header { get; set; }
        public FProperty<string> ContentType { get; set; }
        public FProperty<long> ContentLength64 { get; set; }
        public FProperty<int> StatusCode { get; set; }
        public Stream OutputStream { get; set; }
        public Action Close { get; set; }
    }
}
