using System;
using System.IO;
using NUnit.Framework;
using Nulands.Restless.Dynamic;
using Nulands.Restless;
using System.Dynamic;
using System.Threading.Tasks;

namespace RestlessNUnit
{
    public class User
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    //[Get("www.google.de")]
    //[Get]
    [Get("www.google.de")]
    [Url("www.google.de")]
    [Headers("User-Agent: Awesome Octocat App")]
    public interface ITestRest
    {

        //[Get("www.test.de/users")]
        Task<RestResponse<User>> GetUser([QParam("user_name")] string username);

        RestRequest Name(
            [Param("user_name", ParameterType.Query)]string name);


        RestRequest Age(
            [QParam("user_age")]int age);

        [Fetch]
        RestResponse<User> Test();

        [UploadFileBinary(ContentType="application/octet-stream")]
        RestResponse<IVoid> UploadFileBinary(
            [UploadFileBinary] string localPath);

        [Url("www.duckduckgo.com")]
        [UploadFileBinary("C:\\Log.txt", "application/octet-stream")]
        RestResponse<IVoid> UploadConstantFileBinary(string localPath);
    }

    [TestFixture]
    public class DynamicTest
    {
        [Test]
        public void Test()
        {
            DynamicRequest.InitHandler();
            DynamicRequest<ITestRest> request = DynamicRequest.Create<ITestRest>();

            /*request = request 
                * (iTest => iTest.Age(42)) 
                * (iTest => iTest.Name("name")) 
                * (iTest => iTest.Name("name"));

            dynamic dynReq = DynamicRequest.Create<ITestRest>();
            dynReq.Age(42);
            dynReq.Name("name");

            request = request
                | (iTest => iTest.Name("name"))
                | (iTest => iTest.Age(42))
                | (iTest => iTest.Age(42));*/
            
            request
                .Do(iTest => iTest.Name("name"))
                .Do(iTest => iTest.Age(42));

            dynamic dynRequest = request;
            dynRequest.Age(42);
            dynRequest.Name("name");
            /*
            DynamicRequest<ITestRest> request = DynamicRequest.Create<ITestRest>();
            // t is an ITestRest, Do returns the DynamicRequest again.
            RestResponse<IVoid> response =
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
            object test = request; */
        }
    }
}
