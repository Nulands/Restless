/* Copyright 2014 Muraad Nofal
   Contact: muraad.nofal@gmail.com

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Linq;

namespace Restless.Extensions
{
    public static class TExtension
    {
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
