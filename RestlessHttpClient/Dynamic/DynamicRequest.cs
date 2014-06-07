using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Dynamic;
using System.Reflection;

using Restless;
using Restless.Extensions;

namespace Restless.Dynamic
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

        private static MethodAttributesInfo parseMethodAttributes(
            MethodInfo method,
            RestRequest request,
            dynamic expando)
        {
            MethodAttributesInfo methodAttributes = new MethodAttributesInfo();
            methodAttributes.Method = method;

            foreach (var attr in method.CustomAttributes)
            {
                if (attr.AttributeType == typeof(QParamAttribute))
                {
                    methodAttributes.
                        MethodParameter.
                        Add(new ParameterTuple()
                        {
                            Value = attr.ConstructorArguments[0].Value,
                            Name = method.Name,
                            Type = ParameterType.Query
                        });
                }
                else if (attr.AttributeType == typeof(UrlParamAttribute))
                {
                    methodAttributes.
                        MethodParameter.
                        Add(new ParameterTuple()
                        {
                            Value = attr.ConstructorArguments[0].Value,
                            Name = method.Name,
                            Type = ParameterType.Url
                        });
                }
                // If explicit parameter type is used.
                else if (attr.AttributeType == typeof(ParamAttribute))
                {
                    ParameterType paramType = (ParameterType)attr.ConstructorArguments[1].Value;
                    methodAttributes.
                        MethodParameter.
                        Add(new ParameterTuple()
                        {
                            Value = attr.ConstructorArguments[0].Value,
                            Name = method.Name,
                            Type = paramType
                        });
                }
                else
                    methodAttributes.Attributes.Add(attr);

                ParameterAttributeInfo paramInfo = new ParameterAttributeInfo();
                foreach (var param in method.GetParameters())
                {

                    foreach (var a in param.CustomAttributes)
                        paramInfo.Attributes.Add(a);
                }
                methodAttributes.ParameterAttributes.Add(paramInfo);
            }

            return methodAttributes;
        }

        private static void createMethod(
            MethodAttributesInfo methodInfo,
            RestRequest request,
            dynamic expando)
        {

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
                        case Restless.ParameterType.Query:
                            RequestBuilder.AddQParam(expando, request, (string)attr.ConstructorArguments[0].Value, method.Name);
                            break;
                        case Restless.ParameterType.Url:
                            RequestBuilder.AddUrlParam(expando, request, (string)attr.ConstructorArguments[0].Value, method.Name);
                            break;
                        case Restless.ParameterType.FormUrlEncoded:
                        case Restless.ParameterType.NotSpecified:
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
