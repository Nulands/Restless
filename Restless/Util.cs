using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

using System.Threading;
using System.Reflection;

namespace Nulands.Restless.Util
{

    public static class Util
    {
        public static Func<T> GetFuncStatic<T>(this Type type, string methodName)
        {
            Func<T> result = null;
            var lambda = CreateFuncLambda(null, type, methodName, typeof(T));
            if (lambda.Item1 != null)
                result = Expression.Lambda<Func<T>>(lambda.Item1, lambda.Item2).Compile();
            return result;
        }

        public static Func<T, U> GetFuncStatic<T, U>(this Type type, string methodName)
        {
            Func<T, U> result = null;
            var lambda = CreateFuncLambda(null, type, methodName, typeof(T), typeof(U));
            if (lambda.Item1 != null)
                result = Expression.Lambda<Func<T, U>>(lambda.Item1, lambda.Item2).Compile();
            return result;
        }

        public static Func<T, U, V> GetFuncStatic<T, U, V>(this Type type, string methodName)
        {
            Func<T, U, V> result = null;
            var lambda = CreateFuncLambda(null, type, methodName, typeof(T), typeof(U), typeof(V));
            if (lambda.Item1 != null)
                result = Expression.Lambda<Func<T, U, V>>(lambda.Item1, lambda.Item2).Compile();
            return result;
        }

        public static Func<U> GetFuncStatic<T, U>(string methodName)
        {
            return GetFuncStatic<U>(typeof(T), methodName);
        }

        public static Func<U, V> GetFuncStatic<T, U, V>(string methodName)
        {
            return GetFuncStatic<U, V>(typeof(T), methodName);
        }

        public static Func<U, V, W> GetFuncStatic<T, U, V, W>(string methodName)
        {
            return GetFuncStatic<U, V, W>(typeof(T), methodName);
        }

        private static Tuple<MethodCallExpression, ParameterExpression[]> CreateFuncLambda(object x, Type type, string methodName, params Type[] funcTypes)
        {
            var result = Tuple.Create<MethodCallExpression, ParameterExpression[]>(null, null);
            MethodInfo methodInfo = null;
            if (x == null) // type must be set
                methodInfo = type.GetRuntimeMethod(methodName, funcTypes.Take(funcTypes.Length - 1).ToArray());
            else   // get type from the given object instance
                methodInfo = x.GetType().GetTypeInfo().GetDeclaredMethod(methodName);

            if (methodInfo != null
                && funcTypes.Last().GetTypeInfo().IsAssignableFrom(methodInfo.ReturnType.GetTypeInfo())
                && methodInfo.GetParameters().Length == funcTypes.Length - 1)
            {
                bool typesMatch = true;
                int idx = 0;
                foreach (var param in methodInfo.GetParameters())
                {
                    if (param.ParameterType != funcTypes[idx++])
                    {
                        typesMatch = false;
                        break;
                    }
                }

                if (typesMatch)
                {
                    var parameters = new List<ParameterExpression>();
                    foreach (var t in funcTypes.Take(funcTypes.Length - 1))
                        parameters.Add(Expression.Parameter(t));

                    ConstantExpression xRef = null;
                    TypeInfo typeInfo = null;

                    if (x != null)
                        typeInfo = x.GetType().GetTypeInfo();
                    else
                        typeInfo = type.GetTypeInfo();

                    if (!typeInfo.IsAbstract || !typeInfo.IsSealed)   // both together says this is a static class!
                        xRef = Expression.Constant(x);

                    // Creating an expression for the method call and specifying its parameter.
                    MethodCallExpression methodCall = Expression.Call(
                        xRef, methodInfo, parameters.ToArray());

                    result = Tuple.Create(methodCall, parameters.ToArray());
                }
            }
            return result;
        }

        private static Tuple<MethodCallExpression, ParameterExpression[]> CreateFuncLambda(object x, string methodName, params Type[] funcTypes)
        {
            var result = Tuple.Create<MethodCallExpression, ParameterExpression[]>(null, null);
            MethodInfo methodInfo = methodInfo = x.GetType().GetTypeInfo().GetDeclaredMethod(methodName);

            if (methodInfo != null
                && methodInfo.ReturnType == funcTypes.Last()
                && methodInfo.GetParameters().Length == funcTypes.Length - 1)
            {
                bool typesMatch = true;
                int idx = 0;
                foreach (var param in methodInfo.GetParameters())
                {
                    if (param.ParameterType != funcTypes[idx++])
                    {
                        typesMatch = false;
                        break;
                    }
                }

                if (typesMatch)
                {
                    var parameters = new List<ParameterExpression>();
                    foreach (var type in funcTypes.Take(funcTypes.Length - 1))
                        parameters.Add(Expression.Parameter(type));

                    ConstantExpression xRef = null;
                    var typeInfo = x.GetType().GetTypeInfo();
                    if (!typeInfo.IsAbstract || !typeInfo.IsSealed)   // both together says this is a static class!
                        xRef = Expression.Constant(x);

                    // Creating an expression for the method call and specifying its parameter.
                    MethodCallExpression methodCall = Expression.Call(
                        xRef, methodInfo, parameters.ToArray());

                    result = Tuple.Create(methodCall, parameters.ToArray());
                }
            }
            return result;
        }

        /*
        #region Get generic Func<> from object and method name

        public static Func<T> GetFunc<T>(object x, string methodName)
        {
            Func<T> result = null;
            var lambda = CreateFuncLambda(x, methodName, typeof(T));
            if(lambda.Item1 != null)
                result = Expression.Lambda<Func<T>>(lambda.Item1, lambda.Item2).Compile();
            return result;
        }

        public static Func<T, U> GetFunc<T, U>(object x, string methodName)
        {
            Func<T, U> result = null;
            var lambda = CreateFuncLambda(x, methodName, typeof(T), typeof(U));
            if (lambda.Item1 != null)
                result = Expression.Lambda<Func<T, U>>(lambda.Item1, lambda.Item2).Compile();
            return result;
        }
        
        public static Func<T, U, V> GetFunc<T, U, V>(object x, string methodName)
        {
            Func<T, U, V> result = null;
            var lambda = CreateFuncLambda(x, methodName, typeof(T), typeof(U), typeof(V));
            if (lambda.Item1 != null)
                result = Expression.Lambda<Func<T, U, V>>(lambda.Item1, lambda.Item2).Compile();
            return result;
        }

        public static Func<T, U, V, W> GetFunc<T, U, V, W>(object x, string methodName)
        {
            Func<T, U, V, W> result = null;
            var lambda = CreateFuncLambda(x, methodName, typeof(T), typeof(U), typeof(V), typeof(W));
            if (lambda.Item1 != null)
                result = Expression.Lambda<Func<T, U, V, W>>(lambda.Item1, lambda.Item2).Compile();
            return result;
        }

        public static Func<T, U, V, W, X> GetFunc<T, U, V, W, X>(object x, string methodName)
        {
            Func<T, U, V, W, X> result = null;
            var lambda = CreateFuncLambda(
                x, 
                methodName, 
                typeof(T), 
                typeof(U), 
                typeof(V), 
                typeof(W), 
                typeof(X));

            if (lambda.Item1 != null)
                result = Expression.Lambda<Func<T, U, V, W, X>>(lambda.Item1, lambda.Item2).Compile();
            return result;
        }

        public static Func<T, U, V, W, X, Y> GetFunc<T, U, V, W, X, Y>(object x, string methodName)
        {
            Func<T, U, V, W, X, Y> result = null;

            var lambda = CreateFuncLambda(
                x,
                methodName,
                typeof(T),
                typeof(U),
                typeof(V),
                typeof(W),
                typeof(X), 
                typeof(Y));

            if (lambda.Item1 != null)
                result = Expression.Lambda<Func<T, U, V, W, X, Y>>(lambda.Item1, lambda.Item2).Compile();
            return result;
        }

        public static Func<T, U, V, W, X, Y, Z> GetFunc<T, U, V, W, X, Y, Z>(object x, string methodName)
        {
            Func<T, U, V, W, X, Y, Z> result = null;

            var lambda = CreateFuncLambda(
                x,
                methodName,
                typeof(T),
                typeof(U),
                typeof(V),
                typeof(W),
                typeof(X),
                typeof(Y),
                typeof(Z));

            if (lambda.Item1 != null)
                result = Expression.Lambda<Func<T, U, V, W, X, Y, Z>>(lambda.Item1, lambda.Item2).Compile();
            return result;
        }


        #endregion

        #region Get generic Actino from object and method name

        public static Action GetAction(object x, string methodName = "")
        {
            Action result = null;
            var lambda = CreateActionLambda(x, methodName);
            if (lambda.Item1 != null)
                result = Expression.Lambda<Action>(lambda.Item1, lambda.Item2).Compile();
            return result;
        }

        public static Action<T> GetAction<T>(object x, string methodName = "")
        {
            Action<T> result = null;
            var lambda = CreateActionLambda(x, methodName, typeof(T));
            if (lambda.Item1 != null)
                result = Expression.Lambda<Action<T>>(lambda.Item1, lambda.Item2).Compile();
            return result;
        }

        public static Action<T, U> GetAction<T, U>(object x, string methodName = "")
        {
            Action<T, U> result = null;
            var lambda = CreateActionLambda(x, methodName, typeof(T), typeof(U));
            if (lambda.Item1 != null)
                result = Expression.Lambda<Action<T, U>>(lambda.Item1, lambda.Item2).Compile();
            return result;
        }

        public static Action<T, U, V> GetAction<T, U, V>(object x, string methodName = "")
        {
            Action<T, U, V> result = null;
            var lambda = CreateActionLambda(x, methodName, typeof(T), typeof(U), typeof(V));
            if (lambda.Item1 != null)
                result = Expression.Lambda<Action<T, U, V>>(lambda.Item1, lambda.Item2).Compile();
            return result;
        }

        public static Action<T, U, V, W> GetAction<T, U, V, W>(object x, string methodName = "")
        {
            Action<T, U, V, W> result = null;
            var lambda = CreateActionLambda(x, methodName, typeof(T), typeof(U), typeof(V), typeof(W));
            if (lambda.Item1 != null)
                result = Expression.Lambda<Action<T, U, V, W>>(lambda.Item1, lambda.Item2).Compile();
            return result;
        }

        public static Action<T, U, V, W, X> GetAction<T, U, V, W, X>(object x, string methodName = "")
        {
            Action<T, U, V, W, X> result = null;
            var lambda = CreateActionLambda(
                x,
                methodName,
                typeof(T),
                typeof(U),
                typeof(V),
                typeof(W),
                typeof(X));

            if (lambda.Item1 != null)
                result = Expression.Lambda<Action<T, U, V, W, X>>(lambda.Item1, lambda.Item2).Compile();
            return result;
        }

        public static Action<T, U, V, W, X, Y> GetAction<T, U, V, W, X, Y>(object x, string methodName = "")
        {
            Action<T, U, V, W, X, Y> result = null;

            var lambda = CreateActionLambda(
                x,
                methodName,
                typeof(T),
                typeof(U),
                typeof(V),
                typeof(W),
                typeof(X),
                typeof(Y));

            if (lambda.Item1 != null)
                result = Expression.Lambda<Action<T, U, V, W, X, Y>>(lambda.Item1, lambda.Item2).Compile();
            return result;
        }

        public static Action<T, U, V, W, X, Y, Z> GetAction<T, U, V, W, X, Y, Z>(object x, string methodName = "")
        {
            Action<T, U, V, W, X, Y, Z> result = null;

            var lambda = CreateActionLambda(
                x,
                methodName,
                typeof(T),
                typeof(U),
                typeof(V),
                typeof(W),
                typeof(X),
                typeof(Y),
                typeof(Z));

            if (lambda.Item1 != null)
                result = Expression.Lambda<Action<T, U, V, W, X, Y, Z>>(lambda.Item1, lambda.Item2).Compile();
            return result;
        }


        private static Tuple<MethodCallExpression, ParameterExpression[]> CreateActionLambda(object x, string methodName, params Type[] funcTypes)
        {
            var result = Tuple.Create<MethodCallExpression, ParameterExpression[]>(null, null);
            var methodInfos = Enumerable.Empty<MethodInfo>();

            if (String.IsNullOrEmpty(methodName))
                methodInfos = x.GetType().GetTypeInfo().DeclaredMethods;
            else
                methodInfos = new List<MethodInfo>() { x.GetType().GetTypeInfo().GetDeclaredMethod(methodName) };

            foreach (var methodInfo in methodInfos.Where(m => m != null))
            {
                if (methodInfo != null
                    && methodInfo.GetParameters().Length == funcTypes.Length)
                {
                    bool typesMatch = true;
                    int idx = 0;
                    foreach (var param in methodInfo.GetParameters())
                    {
                        if (param.ParameterType != funcTypes[idx++])
                        {
                            typesMatch = false;
                            break;
                        }
                    }

                    if (typesMatch)
                    {
                        var parameters = new List<ParameterExpression>();
                        foreach (var type in funcTypes.Take(funcTypes.Length - 1))
                            parameters.Add(Expression.Parameter(type));

                        var xRef = Expression.Constant(x);
                        // Creating an expression for the method call and specifying its parameter.
                        MethodCallExpression methodCall = null;
                        if (parameters.Count > 0)
                            methodCall = Expression.Call(xRef, methodInfo, parameters.ToArray());
                        else
                            methodCall = Expression.Call(xRef, methodInfo);

                        result = Tuple.Create(methodCall, parameters.ToArray());
                        break;
                    }
                }
            }
            return result;
        }


        #endregion

        public static PropertyInfo PropInfoFromExpr<T>(this Expression<Func<T>> expr)
		{

			var propExpr = expr.Body as MemberExpression;
			if (propExpr == null)
				throw new ArgumentException("Given expression is not a MemberExpression", "outExpr");

			var property = propExpr.Member as PropertyInfo;
			if (property == null)
				throw new ArgumentException("Given expression member is not a PropertyInfo", "outExpr");
			return property;
		}

        public static PropertyInfo PropInfoFromExpr<T, U>(this Expression<Func<T, U>> expr)
        {

            var propExpr = expr.Body as MemberExpression;
            if (propExpr == null)
                throw new ArgumentException("Given expression is not a MemberExpression", "outExpr");

            var property = propExpr.Member as PropertyInfo;
            if (property == null)
                throw new ArgumentException("Given expression member is not a PropertyInfo", "outExpr");
            return property;
        }

        public static Func<S, T> GetAccessor<S, T>(this Expression<Func<S, T>> propertySelector)
        {
            return propertySelector.PropInfoFromExpr().GetMethod.CreateDelegate<Func<S, T>>();
        }

        public static Action<S, T> SetAccessor<S, T>(this Expression<Func<S, T>> propertySelector)
        {
            return propertySelector.PropInfoFromExpr().SetMethod.CreateDelegate<Action<S, T>>();
        }

        public static Func<S, T> GetAccessor<S, T>(this PropertyInfo property)
        {
            return property.GetMethod.CreateDelegate<Func<S, T>>();
        }

        public static Action<S, T> SetAccessor<S, T>(this PropertyInfo property)
        {
            return property.SetMethod.CreateDelegate<Action<S, T>>();
        }

        // a generic extension for CreateDelegate
        public static T CreateDelegate<T>(this MethodInfo method) where T : class
        {
            return method.CreateDelegate(typeof(T)) as T;
        }

        /// Gets the value of a Linq expression.
        /// </summary>
        /// <param name="expr">The expresssion.</param>
        public static object EvalExpression(this Expression expr)
        {
            // Easy case
            if (expr.NodeType == ExpressionType.Constant)
                return ((ConstantExpression)expr).Value;

            // General case
            var lambda = Expression.Lambda(expr, Enumerable.Empty<ParameterExpression>());
            return lambda.Compile().DynamicInvoke();
        }

        public static List<PropertyInfo> GetPropInfosFromExpressions<T>(this Expression<Func<T>>[] propExpressions)
        {
            List<PropertyInfo> props = new List<PropertyInfo>();
            foreach (var outExpr in propExpressions)
                props.Add(outExpr.PropInfoFromExpr());
            return props;
        }*/
    }
}
