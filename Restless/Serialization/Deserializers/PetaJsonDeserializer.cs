using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

using PetaJson;

namespace Nulands.Restless.Deserializers
{
    public class PetaJsonDeserializer : IDeserializer
    {
        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }
        public CultureInfo Culture { get; set; }

        public PetaJsonDeserializer()
        {
            Culture = CultureInfo.InvariantCulture;
        }

        public T Deserialize<T>(string content)
        {
            return Json.Parse<T>(content, JsonOptions.NonStrictParser);
        }
    }
}
