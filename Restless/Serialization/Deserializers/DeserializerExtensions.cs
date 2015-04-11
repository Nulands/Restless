
//   Copyright 2014 Muraad Nofal
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 


using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

namespace Nulands.Restless.Deserializers
{
    public static class DeserializerExtensions
    {
        public static T Deserialize<T>(this IDeserializer deserializer, HttpWebResponse response)
        {
            string content = "";

            using (Stream stream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(stream);
                content = reader.ReadToEnd();
            }

            return deserializer.Deserialize<T>(content);
        }

        public static async Task<T> Deserialize<T>(this IDeserializer deserializer, HttpResponseMessage response)
        {
            string content = await response.Content.ReadAsStringAsync();
            return deserializer.Deserialize<T>(content);
        }

    }
}
