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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.IO;

using Nulands.Restless;
using Nulands.Restless.Dynamic;
using Nulands.Restless.Extensions;

namespace Nulands.Restless.Dynamic.Sample
{
    public class User
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    //[Get("www.google.de")]
    [Get]
    [Url("www.google.de")]
    [Headers("User-Agent: Awesome Octocat App")]
    public interface ITestRest
    {
        [Param("user_name", Restless.ParameterType.Query)]
        RestRequest Name(string name);

        [QParam]
        RestRequest Age(int age);

        RestRequest Name2([QParam("user_name")] string name);

        [Fetch]
        RestResponse<User> Test();

        [UploadFileBinary(ContentType = "application/octet-stream")]
        RestResponse<IVoid> UploadFileBinary(string localPath);

        [Url("www.duckduckgo.com")]
        [UploadFileBinary("C:\\Log.txt", "application/octet-stream")]
        RestResponse<IVoid> UploadConstantFileBinary();
    }

    class RestTest
    {

        static void Main()
        {
            DynamicRequest<ITestRest> request = DynamicRequest.Create<ITestRest>();
            // t is an ITestRest, Do returns the DynamicRequest again.
            RestResponse<Restless.IVoid> response =
                request.
                Do(t => t.Name("testRestName2")).
                Request.
                UploadFileBinary(new MemoryStream(), "").
                Result;

            // request.Dyn has all methods defined in ITestRest
            // but no intelli sense support because its a dynamic method
            request.Dyn.Name("testRestName2");

            // t is an ITestRest, DoR returns the underlying RestRequest.
            response =
                request.
                DoR(t => t.Name("testRestNAme")).
                UploadFileBinary(new MemoryStream(), "").
                Result;

            // t is an ITestRest, all together
            response =
                request.
                Do(t => t.Name("testRestNAme")).
                Do(t => t.Age(42)).
                DoR(t => t.Name("testRestName2")).
                UploadFileBinary(new MemoryStream(), "").
                Result;
            object test = request;
        }
    }
     
}
