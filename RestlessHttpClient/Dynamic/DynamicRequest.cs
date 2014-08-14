using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Dynamic;
using System.Reflection;

using Nulands.Restless;
using Nulands.Restless.Extensions;

namespace Nulands.Restless.Dynamic
{
    public static class DynamicRequest
    {
        #region Create the DynamicRequest for given type T

        private static void processInterfaceAttributes(
            IEnumerable<CustomAttributeData> attributes,
            RestRequest request)
        {
            foreach (var attr in attributes)
            {
                #region Get HTTP method attributes

                if (typeof(HttpMethodAttribute).IsAssignableFrom(attr.AttributeType))
                {
                    string url = (string)attr.ConstructorArguments[0].Value;
                    request.Url(url);
                    HttpMethodAttribute paramAttr = (HttpMethodAttribute)attr.Constructor.Invoke(new object[] { url });
                    request.Request.Method = paramAttr.Method;
                }

                #endregion

                #region Get HTTP header attributes

                else if (typeof(HeadersAttribute).IsAssignableFrom(attr.AttributeType))
                {

                    IList<CustomAttributeTypedArgument> header =
                        (IList<CustomAttributeTypedArgument>)attr.ConstructorArguments[0].Value;
                    foreach (var h in header)
                    {
                        string[] headerParts = ((string)h.Value).Split(':');
                        request.Header(headerParts[0].Trim(), headerParts[1].Trim());
                    }

                }

                #endregion

                #region URL attribute

                else if (typeof(UrlAttribute).IsAssignableFrom(attr.AttributeType))
                {
                    string url = (string)attr.ConstructorArguments[0].Value;
                    request.Url(url);
                }

                #endregion
            }
        }

        /// <summary>
        /// Get all method and parameter attributes for a given MethodInfo.
        /// </summary>
        /// <param name="method">The given MethodInfo.</param>
        /// <returns>The MethodAttributesInfo containing all method and parameter attributes.</returns>
        private static MethodAttributesInfo parseMethodAttributes( MethodInfo method)
        {
            MethodAttributesInfo methodAttributes = new MethodAttributesInfo();
            methodAttributes.Method = method;

            methodAttributes.Attributes = method.CustomAttributes;
            methodAttributes.ParameterAttributes = from param in method.GetParameters()
                                                   select new ParameterAttributeInfo() { Parameter = param, Attributes = param.CustomAttributes };

            return methodAttributes;
        }

        private static Func<RestRequest, RestRequest> createMethodAttrFunction(CustomAttributeData methodAttr)
        {
            return null;
        }

        /// <summary>
        /// Creates a function thats does an action on a RestRequest, 
        /// depending on the given ParameterAttributeInfo.
        /// </summary>
        /// <param name="paramInfo">The parameter attribute info.</param>
        /// <returns>The function.</returns>
        private static Func<object, RestRequest, RestRequest> createParamAttrFunction(ParameterAttributeInfo paramInfo)
        {
            string parameterName = paramInfo.Parameter.Name;
            Func<object, RestRequest, RestRequest> result = null;
            foreach (var attr in paramInfo.Attributes)
            {
                // This is a QParam, UrlParam or Param attribute.
                if(attr.AttributeType == typeof(BaseParamAttribute))
                {
                    // Check if the BaseParamAttribute ctor first argument (name) was given
                    // if so than use this as parameter name.
                    string tmp = (string)attr.ConstructorArguments.First().Value;
                    if (!String.IsNullOrEmpty(tmp))
                        parameterName = tmp;

                    if (attr.AttributeType == typeof(QParamAttribute))
                        result = (object obj, RestRequest req) => req.QParam(parameterName, obj); 
                    else if(attr.AttributeType == typeof(UrlParamAttribute))
                        result = (object obj, RestRequest req) => req.UrlParam(parameterName, obj);
                    else if(attr.AttributeType == typeof(ParamAttribute))
                    {
                        ParameterType paramType = (ParameterType) attr.ConstructorArguments.Skip(1).First().Value;
                        result = (object obj, RestRequest req) => req.Param(parameterName, obj, paramType);
                    }
                }
                else if(attr.AttributeType == typeof(ContentTypeAttribute))
                {
                    ContentType contentType = (ContentType)attr.ConstructorArguments.First().Value;
                    string contentTypeStr = "";
                    Serializers.ISerializer serializer = null;

                    if (contentType == ContentType.Json)
                    {
                        contentTypeStr = "text/json";
                        serializer = new Serializers.JsonSerializer();
                    }
                    else if (contentType == ContentType.Xml)
                    {
                        contentTypeStr = "text/xml";
                        serializer = new Serializers.XmlSerializer();
                    }
                    // TODO: Handle ContentType.OctetStream
                    // TODO: Handle ContentType.Text
                    // TODO: Handle generic ContentType? MimeDetective?
                    // TODO: AddBinary() ?!
                    result = (object obj, RestRequest req) => req.AddString(serializer.Serialize(obj), System.Text.Encoding.Default, contentTypeStr);
                }
                else if (attr.AttributeType == typeof(HeaderAttribute))
                {
                    // Get the header name this parameter is supposed to set
                    string headerName = (string)attr.ConstructorArguments.First().Value;
                    // create the function that is setting a header on a RestRequest
                    result = (object obj, RestRequest req) => req.Header(headerName, (string)obj);
                }
            }
            return result;
        }

        private static void createMethod(
            MethodAttributesInfo methodInfo,
            RestRequest request,
            dynamic expando)
        {
            string methodName = methodInfo.Method.Name;

            var parameterFunctions = methodInfo.ParameterAttributes.Select(p => createParamAttrFunction(p));
            //first all method attributes
            foreach (var mAttr in methodInfo.Attributes)
            {

            }
        }

        private static void processMethodAttributes(
            MethodInfo method,
            RestRequest request,
            dynamic expando)
        {
            #region Get parameter attributes (query, url form encoded, url format or url parameter)

            foreach (var attr in method.CustomAttributes)
            {
                // Explicit query parameter attribute is used.
                if (attr.AttributeType == typeof(QParamAttribute))
                {
                    RequestBuilder.AddQParam(expando, request, (string)attr.ConstructorArguments[0].Value, method.Name);
                }

                // If explicit parameter type is used.
                if (attr.AttributeType == typeof(ParamAttribute))
                {
                    ParameterType paramType = (ParameterType)attr.ConstructorArguments[1].Value;
                    switch (paramType)
                    {
                        case Nulands.Restless.ParameterType.Query:
                            RequestBuilder.AddQParam(expando, request, (string)attr.ConstructorArguments[0].Value, method.Name);
                            break;
                        case Nulands.Restless.ParameterType.Url:
                            RequestBuilder.AddUrlParam(expando, request, (string)attr.ConstructorArguments[0].Value, method.Name);
                            break;
                        case Nulands.Restless.ParameterType.FormUrlEncoded:
                        case Nulands.Restless.ParameterType.NotSpecified:
                            RequestBuilder.AddParam(expando, request, (string)attr.ConstructorArguments[0].Value, method.Name);
                            break;
                    }
                }
            }
            #endregion
        }

        public static DynamicRequest<T> Create<T>()
        {
            dynamic expando = new ExpandoObject();
            var restRequest = new RestRequest();

            expando.RestRequest = restRequest;

            Type type = typeof(T);
            // First all class/interface attributes.
            processInterfaceAttributes(type.CustomAttributes, restRequest);

            // Go through all class/interface methods.
            foreach (var method in type.GetMethods())
                processMethodAttributes(method, restRequest, expando);

            return new DynamicRequest<T>() { Dyn = expando };
        }

        #endregion
    }

    public class DynamicRequest<T>
    {
        public dynamic Dyn { get; set; }

        public RestRequest Request
        {
            get { return Dyn.RestRequest; }
        }

        public DynamicRequest(dynamic dyn = null)
        {
            Dyn = dyn;
        }

        public RestRequest DoR(Expression<Func<T, RestRequest>> outExpr)
        {
            RestlessExtensions.ThrowIfNull(Dyn, "DynamicRequest.Do", "Dyn");

            // TODO: Compile it and invoke creted Func with the dynamic object as argument.
            // TODO: Or get method name and argument as done above and invoking it via reflection?
            // TODO: what´s faster?
            //outExpr.Compile()(Dyn);

            var expr = outExpr.Body as MethodCallExpression;
            expr.ThrowIfNull("DynamicRequest - given expression is not a MemberExpression");

            var arguments = from arg in expr.Arguments
                            select (arg as ConstantExpression).Value;

            return (RestRequest)
                ((Dyn as IDictionary<string, object>)[expr.Method.Name] as MulticastDelegate).
                DynamicInvoke(arguments.ToArray());
        }

        public DynamicRequest<T> Do(Expression<Func<T, RestRequest>> outExpr)
        {
            DoR(outExpr);
            return this;
        }

    }
}
