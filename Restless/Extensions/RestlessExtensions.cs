/*
    Copyright (C) 2014  Muraad Nofal
    Contact: muraad.nofal@gmail.com
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * */

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Reflection;

namespace Nulands.Restless
{
    /// <summary>
    /// Extensions needed for Restless.
    /// </summary>
    public static class RestlessExtensions
    {

        #region ThrowIf... extensions

        /// <summary>
        /// Throws an ArgumentException if the given predicate returns true for the given object.
        /// </summary>
        /// <typeparam name="T">The type of the given object. Must not be set explicit.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="predicate">the predicate that is called with the given object as argument.</param>
        /// <param name="msg">The message added to the exception.</param>
        /// <param name="memberName">The name of the method that called ThrowIf (CallerMemberName).</param>
        public static void ThrowIf<T>(this T obj, Func<T, bool> predicate, string msg, [CallerMemberName]string memberName = "")
        {
            if (predicate(obj))
                throw new ArgumentException(memberName + " - " + (msg == null ? "" : msg));
        }

        /// <summary>
        /// Throws an ArgumentException if the given predicate returns false for the given object.
        /// </summary>
        /// <typeparam name="T">The type of the given object. Must not be set explicit.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="predicate">the predicate that is called with the given object as argument.</param>
        /// <param name="msg">The message that is added to the exception if it is thrown.</param>
        /// <param name="memberName">The name of the method that called ThrowIf (CallerMemberName).</param>
        public static void ThrowIfNot<T>(this T obj, Func<T, bool> predicate, string msg, [CallerMemberName]string memberName = "")
        {
            if (!predicate(obj))
                throw new ArgumentException(memberName + "-" + (msg == null ? "" : msg));
        }

        /// <summary>
        /// Throws an ArgumentNullException when the given IEnumerable is null, or an ArgumentException if it is empty.
        /// Can be used for arrays too.
        /// </summary>
        /// <typeparam name="T">The type of the given objects inside the IEnumerable. Must not be set explicit.</typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <param name="msg">The message that is added to the exception if it is thrown.</param>
        /// <param name="memberName">The name of the method that called ThrowIf (CallerMemberName).</param>
        public static void ThrowIfNullOrEmpty<T>(this IEnumerable<T> enumerable, string msg = "", [CallerMemberName]string memberName = "")
        {
            if(enumerable == null)
                throw new ArgumentNullException(memberName + "-" + (msg == null ? "" : msg));
            if (enumerable.Count() == 0)
                throw new ArgumentException("IEnumerable is empty.", memberName + "-" + (msg == null ? "" : msg));
        }
        
        /// <summary>
        /// Throws an ArgumentNullException when the given string is null, or an ArgumentException if it is empty.
        /// </summary>
        /// <param name="obj">The string.</param>
        /// <param name="msg">The message that is added to the exception if it is thrown.</param>
        /// <param name="memberName">The name of the method that called ThrowIf (CallerMemberName).</param>
        public static void ThrowIfNullOrEmpty(this string obj, string msg = "", [CallerMemberName]string memberName = "")
        {
            if (obj == null)
                throw new ArgumentNullException(memberName + "-" + (msg == null ? "" : msg));

            if (obj.Length == 0)
                throw new ArgumentException("String is empty", memberName + "-" + (msg == null ? "" : msg));
        }

        /// <summary>
        /// Treats a string as a file/folder path and throws an exceptin if the file/folder is not found.
        /// </summary>
        /// <param name="path">The path string.</param>
        /// <param name="isFile">If true the given string is a path to a file, otherwise its a path to a folder.</param>
        /// <param name="msg">The message that is added to the exception if it is thrown.</param>
        /// <param name="memberName">The name of the method that called ThrowIf (CallerMemberName).</param>
        public static void ThrowIfNotFound(this string path, bool isFile = true, string msg = "", [CallerMemberName]string memberName = "")
        {
            path.ThrowIfNull("ThrowIfNotFound - path is null.");

            if (isFile && !Rest.File.Exists(path))
                throw new System.IO.FileNotFoundException(
                    String.Format("File {0} not found. {1}.", path, memberName + "-" + (msg == null ? "" : msg)));

            if (!isFile && !Rest.Directory.Exists(path))
                throw new Exception(
                    String.Format("Directory {0} not found. {1}.", path, memberName + "-" + (msg == null ? "" : msg)));

        }

        /// <summary>
        /// Throws an exception if the given object is null.
        /// </summary>
        /// <typeparam name="T">The type of the object. Must not be set explicit.</typeparam>
        /// <param name="obj">The given object.</param>
        /// <param name="msg">The message that is added to the exception if it is thrown.</param>
        /// <param name="memberName">The name of the method that called ThrowIf (CallerMemberName).</param>
        public static void ThrowIfNull<T>(this T obj, string msg = "", [CallerMemberName]string memberName = "")
        {
            if (obj == null)
            {
                if (String.IsNullOrEmpty(msg))
                    msg = obj.GetType().Name;
                throw new ArgumentNullException(memberName + "-" + (msg == null ? "" : msg));
            }
        }

        /// <summary>
        /// Throws an exception if the given object is null or if the obj.ToString() is null or empty.
        /// </summary>
        /// <typeparam name="T">The type of the object. Must not be set explicit.</typeparam>
        /// <param name="obj">The given object.</param>
        /// <param name="msg">The message that is added to the exception if it is thrown.</param>
        /// <param name="memberName">The name of the method that called ThrowIf (CallerMemberName).</param>
        public static void ThrowIfNullOrToStrEmpty<T>(this T obj, string msg = "", [CallerMemberName]string memberName = "")
        {
            if (String.IsNullOrEmpty(msg))
                msg = obj.GetType().Name;

            if (obj == null)
                throw new ArgumentNullException(memberName + "-" + (msg == null ? "" : msg));

            obj.ToString().ThrowIfNullOrEmpty(msg + " ToString()", memberName);
        }

        #endregion


        #region Parameter and url (Dictionary<string, object>) extensions

        /// <summary>
        /// Make a parameter string.
        /// </summary>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public static string CreateParamStr(this Dictionary<string, object> paramList)
        {
            int index = 0;
            string result = paramList.Aggregate("", (s, p) => s + p.Key + "=" + p.Value + (index++ < paramList.Count - 1 ? "&" : ""));
            return result;
        }

        private static string AndStr(string str)
        {
            return str == "" ? "" : "&";
        }

        /// <summary>
        /// Make a parameter string.
        /// </summary>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public static string CreateParamStr(this Dictionary<string, List<object>> paramList)
        {
            //var query = from value in 
            return paramList.Aggregate("", (s, curr) => s + AndStr(s) +
                    (from value in curr.Value
                     select curr.Key + "=" + value).
                        Aggregate("", (seed, c) => seed + AndStr(seed) + c)
                    );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url_params"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string FormatUrlWithParams(this Dictionary<string, object> url_params, string url)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(url);
            if (url_params != null)
            {
                foreach (var element in url_params)
                {
                    string pattern = "{" + element.Key + "}";
                    if (url.Contains(pattern))
                        builder.Replace(pattern, element.Value.ToString());
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="query_params"></param>
        /// <param name="param"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static string CreateRequestUri(this string url,
            Dictionary<string, object> query_params,
            Dictionary<string, object> param,
            string method)
        {
            url = query_params.FormatUrlWithParams(url);

            // Add query parameter to url
            string query = query_params.CreateParamStr(); ;

            // if method is GET treat all added parameters as query parameter
            if (method == "GET")
            {
                // Add parameter that are added with Param(..) too, because this is a GET method.
                string pQuery = param.CreateParamStr();
                // set query to post param. query string if query is still emtpy (because no QParam(..) were added)
                // only Param(..) was used even this is a GET method.
                query = (string.IsNullOrEmpty(query) ? pQuery : query + "&" + pQuery);
            }

            if (!String.IsNullOrEmpty(query))
                url += "?" + query;
            return url;
        }

        public static string CreateRequestUri(this string url,
            Dictionary<string, object> query_params,
            Dictionary<string, List<object>> param,
            string method)
        {
            url = query_params.FormatUrlWithParams(url);

            // Add query parameter to url
            string query = query_params.CreateParamStr(); ;

            // if method is GET treat all added parameters as query parameter
            if (method == "GET")
            {
                // Add parameter that are added with Param(..) too, because this is a GET method.
                string pQuery = param.CreateParamStr();
                // set query to post param. query string if query is still emtpy (because no QParam(..) were added)
                // only Param(..) was used even this is a GET method.
                query = (string.IsNullOrEmpty(query) ? pQuery : query + "&" + pQuery);
            }

            if (!String.IsNullOrEmpty(query))
                url += "?" + query;
            return url;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <param name="excludePropertys"></param>
        public static void SetFrom<T>(this T to, T from, params string[] excludePropertys)
        {
            var propertys = to.GetType().GetRuntimeProperties();
            foreach (var prop in propertys)
            {
                if (prop.CanWrite && prop.CanRead && !excludePropertys.Contains(prop.Name))
                    prop.SetValue(to, prop.GetValue(from));
            }
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
                action(item);
        }

        public static T As<T>(this object obj)
        {
            return (T)obj;
        }
    }
}
