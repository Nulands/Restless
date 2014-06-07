using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

namespace Restless
{
    class DynamicRest
    {
        public void test()
        {
            dynamic expando = new ExpandoObject();
            var restRequest = new Restless.RestRequest();
            expando.Name = new Func<object, Restless.RestRequest>(obj => restRequest.Param("nam", obj));
            expando.Test = restRequest;
            expando["Request"] = restRequest;

            expando.Name("test");
        }
    }
}
