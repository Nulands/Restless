using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using System.Reflection;
using System.Dynamic;
using Nulands.Restless.Extensions;

namespace Nulands.Restless.Dynamic
{
    internal class MethodAttributesInfo
    {
        public MethodInfo Method { get; set; }
        public IEnumerable<CustomAttributeData> Attributes { get; set; }
        public IEnumerable<ParameterAttributeInfo> ParameterAttributes { get; set; }

        public MethodAttributesInfo()
        {
            Attributes = new List<CustomAttributeData>();
            ParameterAttributes = new List<ParameterAttributeInfo>();
        }
    }
    internal class ParameterAttributeInfo
    {
        public ParameterInfo Parameter { get; set; }
        public IEnumerable<CustomAttributeData> Attributes { get; set; }

        public ParameterAttributeInfo()
        {
            Attributes = new List<CustomAttributeData>();
        }
    }
    internal class ParameterTuple
    {
        public string Name { get; set; }
        public ParameterType Type { get; set; }
        public object Value { get; set; }
    }

    public static class RequestBuilder
    {
        #region Create Func´s 

        internal static Func<T, U, RestRequest> CreateFunc2<T, U>(
            dynamic expando,
            RestRequest request,
            string methodName,
            params ParameterTuple[] parameter)
        {
            // paramNames should contain TWO parameter!
            return new Func<T, U, RestRequest>((t, u) =>
            {
                return request.
                    Param(parameter[0].Name, t, parameter[0].Type).
                    Param(parameter[1].Name, u, parameter[1].Type);
            });
        }

        internal static Func<T, U, V, RestRequest> CreateFunc3<T, U, V>(
            dynamic expando,
            RestRequest request,
            string methodName,
            params ParameterTuple[] parameter)
        {
            // parameter should contain TRHEE parameter!
            return new Func<T, U, V, RestRequest>((t, u, v) =>
            {
                return request.
                    Param(parameter[0].Name, t, parameter[0].Type).
                    Param(parameter[1].Name, u, parameter[1].Type).
                    Param(parameter[2].Name, u, parameter[2].Type);
            });
        }

        internal static Func<T, U, V, W, RestRequest> CreateFunc4<T, U, V, W>(
            dynamic expando,
            RestRequest request,
            string methodName,
            params ParameterTuple[] parameter)
        {
            // parameter should contain FOUR parameter!
            return new Func<T, U, V, W, RestRequest>((t, u, v, w) =>
            {
                return request.
                    Param(parameter[0].Name, t, parameter[0].Type).
                    Param(parameter[1].Name, u, parameter[1].Type).
                    Param(parameter[2].Name, u, parameter[2].Type).
                    Param(parameter[3].Name, u, parameter[3].Type);
            });
        }

        internal static Func<T, U, V, W, X, RestRequest> CreateFunc5<T, U, V, W, X>(
            dynamic expando,
            RestRequest request,
            string methodName,
            params ParameterTuple[] parameter)
        {
            // parameter should contain FIVE parameter!
            return new Func<T, U, V, W, X, RestRequest>((t, u, v, w, x) =>
            {
                return request.
                    Param(parameter[0].Name, t, parameter[0].Type).
                    Param(parameter[1].Name, u, parameter[1].Type).
                    Param(parameter[2].Name, u, parameter[2].Type).
                    Param(parameter[3].Name, u, parameter[3].Type).
                    Param(parameter[4].Name, u, parameter[4].Type);
            });
        }

        #endregion

        #region Add parameter Func´s to expando.

        internal static dynamic AddParam(dynamic expando, RestRequest request, string name, string methodName)
        {
            ((IDictionary<string, object>)expando)[methodName] = new Func<object, RestRequest>(obj => request.Param(name, obj));
            return expando;
        }

        internal static dynamic AddQParam(dynamic expando, RestRequest request, string name, string methodName)
        {
            ((IDictionary<string, object>)expando)[methodName] = new Func<object, RestRequest>(obj => request.QParam(name, obj));
            return expando;
        }

        internal static dynamic AddUrlParam(dynamic expando, RestRequest request, string name, string methodName)
        {
            ((IDictionary<string, object>)expando)[methodName] = new Func<object, RestRequest>(obj => request.UrlParam(name, obj));
            return expando;
        }

        #endregion



    }

}
