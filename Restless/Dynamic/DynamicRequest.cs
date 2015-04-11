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
        #region UNUSED

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
                    result = (object obj, RestRequest req) => req.AddString(serializer.Serialize(obj), System.Text.Encoding.UTF8, contentTypeStr);
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

        #endregion

        #region Attribute handler function dictionarys and handler init

        static Dictionary<Type, Func<Attribute, Action<RestRequest>>> generalAttributeHandler =
            new Dictionary<Type, Func<Attribute, Action<RestRequest>>>();

        static Dictionary<Type, Func<Attribute, Action<object, RestRequest>>> methodAttributeHandler = 
            new Dictionary<Type, Func<Attribute, Action<object, RestRequest>>>();

        public static void InitHandler()
        {
            #region Handler for both interface/class and method attributes 

            generalAttributeHandler[typeof(HttpMethodAttribute)] = attr =>
                {
                    var getAttr = attr.As<HttpMethodAttribute>();
                    return req =>
                        {
                            req.Method(getAttr.Method.Method);
                            if (!String.IsNullOrEmpty(getAttr.Url))
                                req.Url(getAttr.Url);
                        };
                };

            generalAttributeHandler[typeof(HeadersAttribute)] = attr =>
            {
                var headersAttr = attr.As<HeadersAttribute>();
                List<string[]> headers = new List<string[]>();
                foreach (var h in headersAttr.Headers)
                {
                    string[] headerParts = h.Split(':');
                    headers.Add(new string[]{headerParts[0].Trim(), headerParts[1].Trim()});
                }
                return req =>
                    {
                        foreach (var header in headers)
                            req.Header(header[0], header[1]);
                    };
            };

            generalAttributeHandler[typeof(UrlAttribute)] = attr => 
            {
                var urlAttr = attr.As<UrlAttribute>();
                return req => req.Url(urlAttr.Url);
            };

            #endregion

            #region Handler for method attributes

            methodAttributeHandler[typeof(QParamAttribute)] = attr =>
                {
                    var qParamName = attr.As<QParamAttribute>().Name;
                    return (obj, req) =>
                        {
                            req.QParam(qParamName, obj);
                        };
                };

            methodAttributeHandler[typeof(ParamAttribute)] = attr =>
                {
                    Action<object, RestRequest> result = null;

                    var paramAttr = attr.As<ParamAttribute>();
                    ParameterType paramType = paramAttr.Type;
                    string name = paramAttr.Name;
                    switch (paramType)
                    {
                        case Nulands.Restless.ParameterType.Query:
                            result = (obj, req) => req.QParam(name, obj);
                            break;
                        case Nulands.Restless.ParameterType.Url:
                            result = (obj, req) => req.UrlParam(name, obj);
                            break;
                        case Nulands.Restless.ParameterType.FormUrlEncoded:
                        case Nulands.Restless.ParameterType.NotSpecified:
                            result = (obj, req) => req.Param(name, obj);
                            break;
                    }
                    return result;
                };

            methodAttributeHandler[typeof(UploadFileBinaryAttribute)] = attr =>
                {
                    var uploadAttr = attr.As<UploadFileBinaryAttribute>();
                    return (obj, req) => req.UploadFileBinary(uploadAttr.LocalPath, uploadAttr.ContentType);
                };
            #endregion
        }

        #endregion

        #region ProcessMethod(General)Attribute(..)

        private static Action<object, RestRequest> ProcessMethodAttribute(Attribute attribute)
        {
            Func<Attribute, Action<object, RestRequest>> handler = null;
            Action<object, RestRequest> result = null;
            if (methodAttributeHandler.TryGetValue(attribute.GetType(), out handler))
                result = handler(attribute);
            else
            {       // check base type, maybe its an attribute and a handler is registered for it
                var baseType = attribute.GetType().GetTypeInfo().BaseType;
                if (typeof(Attribute).GetTypeInfo().IsAssignableFrom(baseType.GetTypeInfo())
                    && methodAttributeHandler.TryGetValue(baseType, out handler))
                {
                    result = handler(attribute);
                }
            }
            return result;
        }

        private static Action<RestRequest> ProcessGeneralAttribute(Attribute attribute, RestRequest request)
        {
            Func<Attribute, Action<RestRequest>> handler = null;
            Action<RestRequest> result = null;
            if (generalAttributeHandler.TryGetValue(attribute.GetType(), out handler))
                result = handler(attribute);
            else
            {       // check base type, maybe its an attribute and a handler is registered for it
                var baseType = attribute.GetType().GetTypeInfo().BaseType;
                if (typeof(Attribute).GetTypeInfo().IsAssignableFrom(baseType.GetTypeInfo())
                    && generalAttributeHandler.TryGetValue(baseType, out handler))
                {
                    result = handler(attribute);
                }
            }
            return result;
        }

        #endregion

        public class InterfaceAttributeFunctions
        {
            internal Type InterfaceType { get; set; }
            internal Dictionary<MethodInfo, MethodAttributeFunctions> MethodAttributeFunctions { get; set; }
            internal Action<RestRequest>[] GenerallSetter { get; set; }
            public InterfaceAttributeFunctions()
            {
                MethodAttributeFunctions = new Dictionary<MethodInfo, MethodAttributeFunctions>();
            }
        }

        public class MethodAttributeFunctions
        {
            internal MethodAttributeFunctions(
                MethodInfo methodInfo)
            {
                MethodInfo = methodInfo;
                MethodParamSetter = new Dictionary<string, List<Action<object, RestRequest>>>();
            }

            internal MethodInfo MethodInfo { get; private set; }
            internal Dictionary<string, List<Action<object, RestRequest>>> MethodParamSetter { get; private set; }
            internal Func<RestRequest, object> MethodResultFunctions { get; set; }
            internal Func<string, RestRequest, RestResponse<IVoid>> UploadBinaryFunctions { get; set; }
        }
        
        public static DynamicRequest<T> Create<T>()
        {
            var restRequest = new RestRequest();
            Type type = typeof(T);
            var interfaceFunctions = new InterfaceAttributeFunctions();
            interfaceFunctions.InterfaceType = type;


            // First all class/interface attributes.
            //Action<RestRequest>[] generalSetter = type
            interfaceFunctions.GenerallSetter = type
                .GetTypeInfo()
                .GetCustomAttributes()
                .Select(attr => ProcessGeneralAttribute(attr, restRequest))
                .ToArray();

            interfaceFunctions.GenerallSetter.ForEach(action => action(restRequest));

            List<Delegate> methods = new List<Delegate>();

            // Go through all class/interface methods.
            foreach (var method in type.GetTypeInfo().DeclaredMethods)
            {

                string name = method.Name;
                // expando.name = Func<...>;
                var methodAttrFunctions = new MethodAttributeFunctions(method);
                interfaceFunctions.MethodAttributeFunctions[method] = methodAttrFunctions;

                foreach(var mParam in method.GetParameters())
                {
                    var setter = new List<Action<object, RestRequest>>();
                    foreach (var mParamAttr in mParam.GetCustomAttributes())
                    {
                        setter.Add(ProcessMethodAttribute(mParamAttr));
                    }
                    methodAttrFunctions.MethodParamSetter[mParam.Name] = setter;
                }
            }

            return new DynamicRequest<T>() { InterfaceFunctions = interfaceFunctions, Request = restRequest};
        }
    }

    public class DynamicRequest<T> : DynamicObject
    {
        #region Public propertys and operator overloadings

        public static DynamicRequest<T> operator *(DynamicRequest<T> request, Expression<Func<T, RestRequest>> expr)
        {
            return request.Do(expr);
        }
        public static DynamicRequest<T> operator |(DynamicRequest<T> request, Expression<Func<T, RestRequest>> expr)
        {
            return request.Do(expr);
        }

        internal DynamicRequest.InterfaceAttributeFunctions InterfaceFunctions { get; set; }
        public RestRequest Request { get; set; }

        #endregion

        #region Private methodCache dictionary and typeof(T) variable

        Dictionary<string, MethodInfo> methodCache = new Dictionary<string, MethodInfo>();
        Type type = typeof(T);

        #endregion

        #region Private GetRuntimeMethod (cached) and TryInvokeMemberInternal(..)

        private MethodInfo GetRuntimeMethod(string name, IEnumerable<Type> args)
        {
            MethodInfo mInfo = null;
            if (!methodCache.TryGetValue(name, out mInfo))
            {
                mInfo = type.GetRuntimeMethod(name, args.ToArray());
                if (mInfo != null)
                    methodCache[name] = mInfo;
            }
            return mInfo;
        }

        private bool TryInvokeMemberInternal(string name, object[] args, out object result)
        {
            RestRequest request = Request == null ? new RestRequest() : Request;
            result = null;

            var method = GetRuntimeMethod(name, args.Select(a => a.GetType()));
            if (method != null)
            {
                // Set up the RestRequest with everything that is common for every request of this interface
                InterfaceFunctions.GenerallSetter.ForEach(action => action(request));

                // Get all Rest informations for the found method
                var methodAttrFunction = InterfaceFunctions.MethodAttributeFunctions[method];
                var mSetter = methodAttrFunction.MethodParamSetter;

                int paramIndex = 0;
                // For every parameter of this index
                foreach(var param in method.GetParameters())
                {
                    List<Action<object, RestRequest>> methodParamSetter = null;
                    // Try to get all actions for this parameter that were created via the Attributes
                    if (mSetter.TryGetValue(param.Name, out methodParamSetter))
                    {
                        // Call all found setter with the corresponding argument object and the current rest request.
                        methodParamSetter.ForEach(setter => setter(args[paramIndex], request));
                    }
                    paramIndex++;
                }


                if (method.ReturnType != null && methodAttrFunction.MethodResultFunctions != null)
                    result = methodAttrFunction.MethodResultFunctions(request);
                else
                    result = request;
            }

            return method != null;
        }

        #endregion

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            return TryInvokeMemberInternal(binder.Name, args, out result);
        }

        public U DoR<U>(Expression<Func<T, U>> outExpr)
            where U : class
        {
            
            // TODO: Compile it and invoke created Func with the dynamic object as argument.
            // TODO: Or get method name and argument as done above and invoking it via reflection?
            // TODO: what´s faster?
            //outExpr.Compile()(Dyn);
            U result = default(U);
            var expr = outExpr.Body as MethodCallExpression;
            expr.ThrowIfNull("DynamicRequest - given expression is not a MemberExpression");

            var arguments = from arg in expr.Arguments
                            select (arg as ConstantExpression).Value;

            object[] args = arguments.ToArray();
            object obj = null;
            bool success = TryInvokeMemberInternal(expr.Method.Name, args, out obj);
            if(success)
            {
                if (obj != null)
                    result = (U)obj;
                else if(typeof(U).Equals(typeof(RestRequest)))
                    result = Request as U;  // Try to set the result to the underlying RestRequest or Fluent using.
            }
            return result;
        }

        public DynamicRequest<T> Do(Expression<Func<T, RestRequest>> outExpr)
        {
            DoR(outExpr);
            return this;
        }
    }
}
