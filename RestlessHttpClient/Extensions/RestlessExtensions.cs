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

namespace Restless
{
    public static class RestlessExtensions
    {
        #region ThrowIf... extensions

        public static void ThrowIf<T>(this T obj, Func<T, bool> predicate, string msg, [CallerMemberName]string memberName = "")
        {
            if (predicate(obj))
                throw new ArgumentException(memberName + " - " + (msg == null ? "" : msg));
        }

        public static void ThrowIfNot<T>(this T obj, Func<T, bool> predicate, string msg, [CallerMemberName]string memberName = "")
        {
            if (!predicate(obj))
                throw new ArgumentException(memberName + "-" + (msg == null ? "" : msg));
        }

        public static void ThrowIfNullOrEmpty<T>(this IEnumerable<T> enumerable, string msg = "", [CallerMemberName]string memberName = "")
        {
            if(enumerable == null)
                throw new ArgumentNullException(memberName + "-" + (msg == null ? "" : msg));
            if (enumerable.Count() == 0)
                throw new ArgumentException("IEnumerable is empty.", memberName + "-" + (msg == null ? "" : msg));
        }
        
        public static void ThrowIfNullOrEmpty(this string obj, string msg = "", [CallerMemberName]string memberName = "")
        {
            if (obj == null)
                throw new ArgumentNullException(memberName + "-" + (msg == null ? "" : msg));

            if (obj.Length == 0)
                throw new ArgumentException("String is empty", memberName + "-" + (msg == null ? "" : msg));
        }

        public static void ThrowIfNotFound(this string path, bool isFile = true, string msg = "", [CallerMemberName]string memberName = "")
        {
            path.ThrowIfNull("ThrowIfNotFound - path is null.");

            if (isFile && !System.IO.File.Exists(path))
                throw new System.IO.FileNotFoundException(
                    String.Format("File {0} not found. {1}.", path, memberName + "-" + (msg == null ? "" : msg)));

            if (!isFile && !System.IO.Directory.Exists(path))
                throw new System.IO.DirectoryNotFoundException(
                    String.Format("Directory {0} not found. {1}.", path, memberName + "-" + (msg == null ? "" : msg)));

        }

        public static void ThrowIfNull<T>(this T obj, string msg = "", [CallerMemberName]string memberName = "")
        {
            if (obj == null)
            {
                if (String.IsNullOrEmpty(msg))
                    msg = obj.GetType().Name;
                throw new ArgumentNullException(memberName + "-" + (msg == null ? "" : msg));
            }
        }

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
        /// Example: 
        ///     name1=value1&name2=value2...
        /// </summary>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public static string CreateParamStr(this Dictionary<string, object> paramList)
        {
            int index = 0;
            string result = paramList.Aggregate("", (s, p) => s + p.Key + "=" + p.Value + (index++ < paramList.Count - 1 ? "&" : ""));
            return result;
        }

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

        #endregion

        public static void SetFrom<T>(this T to, T from, params string[] excludePropertys)
        {
            var propertys = to.GetType().GetProperties();
            for (int i = 0; i < propertys.Length; i++)
            {
                var prop = propertys[i];
                if (prop.CanWrite && prop.CanRead && !excludePropertys.Contains(prop.Name))
                {
                    prop.SetValue(to, prop.GetValue(from));
                }
            }
        }
    }
}
