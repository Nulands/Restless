using System;
using System.Net.Http;

namespace Nulands.Restless
{
    #region ContentType enum

    public enum ContentType
    {
        Json, Xml, OctetStream, Text
    }

    #endregion

    public abstract class RestlessAttribute : Attribute
    {
    }

    // For AttributeTargets.Interface only

    #region HttpMethod (and optional url) attributes for Targets.Interface

    public abstract class HttpMethodAttribute : RestlessAttribute
    {
        public abstract HttpMethod Method { get; }

        protected string url;
        public virtual string Url
        {
            get { return url; }
            protected set { url = value; }
        }

        public HttpMethodAttribute(string path = "")
        {
            Url = path;
        }
    }

    [AttributeUsage(AttributeTargets.Interface)]
    public class GetAttribute : HttpMethodAttribute
    {
        public GetAttribute(string path = "") : base(path) { }

        public override HttpMethod Method
        {
            get { return HttpMethod.Get; }
        }
    }

    [AttributeUsage(AttributeTargets.Interface)]
    public class PostAttribute : HttpMethodAttribute
    {
        public PostAttribute(string path = "") : base(path) { }

        public override HttpMethod Method
        {
            get { return HttpMethod.Post; }
        }
    }

    [AttributeUsage(AttributeTargets.Interface)]
    public class PutAttribute : HttpMethodAttribute
    {
        public PutAttribute(string path = "") : base(path) { }

        public override HttpMethod Method
        {
            get { return HttpMethod.Put; }
        }
    }

    [AttributeUsage(AttributeTargets.Interface)]
    public class DeleteAttribute : HttpMethodAttribute
    {
        public DeleteAttribute(string path = "") : base(path) { }

        public override HttpMethod Method
        {
            get { return HttpMethod.Delete; }
        }
    }

    [AttributeUsage(AttributeTargets.Interface)]
    public class HeadAttribute : HttpMethodAttribute
    {
        public HeadAttribute(string path = "") : base(path) { }

        public override HttpMethod Method
        {
            get { return HttpMethod.Head; }
        }
    }

    [AttributeUsage(AttributeTargets.Interface)]
    public class ConnectAttribute : HttpMethodAttribute
    {
        public ConnectAttribute(string path = "") : base(path) { }

        public override HttpMethod Method
        {
            get { return HttpMethod.Options; }
        }
    }

    [AttributeUsage(AttributeTargets.Interface)]
    public class TraceAttribute : HttpMethodAttribute
    {
        public TraceAttribute(string path = "") : base(path) { }

        public override HttpMethod Method
        {
            get { return HttpMethod.Trace; }
        }
    }

    #endregion

    // For AttributeTargets.Interface and Method

    #region Header(s) attribute for interface and method

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class HeadersAttribute : RestlessAttribute
    {
        public string[] Headers { get; private set; }

        public HeadersAttribute(params string[] headers)
        {
            Headers = headers ?? new string[0];
        }
    }

    #endregion

    #region URL attribute for Targets.Interface and Targets.Method

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class UrlAttribute : RestlessAttribute
    {
        public string Url { get; set; }
        public UrlAttribute(string url)
        {
            Url = url;
        }
    }

    #endregion

    // For AttributeTargets.Method and Parameter
    #region Query and normal parameter attributes for Targets.Method and Targets.Parameter

    public abstract class BaseParamAttribute : RestlessAttribute
    {
        public string Name { get; set; }
        public BaseParamAttribute(string name = "", object value = null)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
    public class QParamAttribute : BaseParamAttribute
    {
        public QParamAttribute(string name = "", object value = null) : base(name) { }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
    public class ParamAttribute : BaseParamAttribute
    {
        public ParameterType Type { get; set; }
        public ParamAttribute(string name = "", ParameterType type = ParameterType.NotSpecified, object value = null) : base(name)
        {
            Type = type;
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
    public class UrlParamAttribute : BaseParamAttribute
    {
        public UrlParamAttribute(string name = "", object value = null) : base(name) { }
    }

    #endregion

    // For AttributeTargets.Method only
    #region Fetch, Upload... attributes for Targets.Method

    [AttributeUsage(AttributeTargets.Method)]
    public class FetchAttribute : RestlessAttribute
    {
        public FetchAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
    public class UploadFileBinaryAttribute : RestlessAttribute
    {
        public string LocalPath { get; set; }
        public string ContentType { get; set; }

        public UploadFileBinaryAttribute(string localPath = "", string contentType = "")
        {
            LocalPath = localPath;
            ContentType = contentType;
        }
    }

    #endregion

    // For AttributeTargets.Parameter only
    #region ContentType attribute for Targets.Parameter

    /// <summary>
    /// If ContentTypeAttribute is set in front of a method parameter,
    /// the parameter value is supposed to be added to the
    /// request as serialzed content.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ContentTypeAttribute : RestlessAttribute
    {
        public ContentType ContentType { get; protected set; }

        public ContentTypeAttribute(ContentType contentType = ContentType.Json)
        {
            ContentType = contentType;
        }
    }

    #endregion

    #region Header Targets.Parameter attribute

    [AttributeUsage(AttributeTargets.Parameter)]
    public class HeaderAttribute : RestlessAttribute
    {
        public string Header { get; private set; }

        public HeaderAttribute(string header)
        {
            Header = header;
        }
    }

    #endregion

}
