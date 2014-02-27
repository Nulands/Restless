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

namespace Restless
{
    public static class Extensions
    {
        public static void ThrowIfNullOrEmpty<T>(this IEnumerable<T> enumerable, string name = "")
        {
            
            if (String.IsNullOrEmpty(name))
                name = enumerable.GetType().Name;

            if(enumerable == null)
                throw new ArgumentNullException("Parameter is null.", name);


            int count = 0;
            foreach(T value in enumerable){
                count = 1;
                break;
            }

            if (count == 0)
                throw new ArgumentException("Parameter is empty.", name);
        }

        public static void ThrowIfNullOrEmpty(this object[] array, string name = "")
        {
            if (array == null || array.Length == 0)
            {
                if (String.IsNullOrEmpty(name))
                    name = array.GetType().Name;
                throw new ArgumentException("Parameter is null or empty.", name);
            }
        }
        
        public static void ThrowIfNullOrEmpty(this string obj, string name = "")
        {
            if (String.IsNullOrEmpty(obj))
            {
                if (String.IsNullOrEmpty(name))
                    name = obj.GetType().Name;
                throw new ArgumentException("Parameter is null or empty.", name);
            }
        }

        public static void ThrowIfNotFound(this string path, bool isFile = true, string name = "")
        {
            path.ThrowIfNull("ThrowIfNotFound - path");

            if (String.IsNullOrEmpty(name))
                name = path.GetType().Name;

            if (isFile && !System.IO.File.Exists(path))
            {
                throw new System.IO.FileNotFoundException("File not found.", path);
            }

            if (!isFile && !System.IO.Directory.Exists(path))
            {
                string msg = String.Format("Directory {0} not found.", path);
                throw new System.IO.DirectoryNotFoundException(msg);
            }
        }
       
        public static void ThrowIfNull<T>(this T obj, string name = "")
        {
            if (obj == null)
            {
                if (String.IsNullOrEmpty(name))
                    name = obj.GetType().Name;
                throw new ArgumentNullException(name);
            }
        }

        public static void ThrowIfNullOrToStrEmpty<T>(this T obj, string name = "")
        {
            if (String.IsNullOrEmpty(name))
                name = obj.GetType().Name;

            if (obj == null)
                throw new ArgumentNullException(name);

            obj.ToString().ThrowIfNullOrEmpty(name);
        }
        
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
