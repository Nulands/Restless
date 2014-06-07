Restless
========

Rest like api.
One uses HttpWebRequest and HttpWebResponse internally, the other version uses HttpClient.


Uses Json and Xml (de)serializer from RestSharp (https://github.com/restsharp/RestSharp).

It is refactored in a single project https://github.com/Muraad/RestlessSerialization 
that is added as submodule.
To see how to work with submodules a good source is  http://www.git-scm.com/book/en/Git-Tools-Submodules 
In short:

```

$ git clone https://github.com/Muraad/Restless.git
$ git submodule init
$ git submodule update 

    
```
More documentation will follow:

Published under GPLv3 license. 
For a overview of the license visit 
http://choosealicense.com/licenses/gpl-v3/


```

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
    

```


----------------------------------------------------

Restless API
=======


Live interface
----

NEW Live interface functionality like Refit API (https://github.com/paulcbetts/refit).
No third party library is involved and byte code is created
during runtime.



```c#


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

        [QParam("user_age")]
        RestRequest Age(int age);

        [Fetch]
        RestResponse<User> Test();

        [UploadFileBinary(ContentType="application/octet-stream")]
        RestResponse<IVoid> UploadFileBinary(string localPath);

        [Url("www.duckduckgo.com")]
        [UploadFileBinary("C:\\Log.txt", "application/octet-stream")]
        RestResponse<IVoid> UploadConstantFileBinary(string localPath);
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




```

Extending BaseRestRequest
----

(Theoretical) example:

```c#

    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class PersonGetRequest : BaseRestRequest
    {
        public PersonGetRequest()
            : base()
        {
            Url("http://www.example.com/Person");
            Get();
        }

        public PersonGetRequest Name(string name)
        {
            return QParam("name", name) as PersonGetRequest;

            // or
            // Param("name", name);
            // return this;
        }

        public new async Task<RestResponse<Person>> Fetch(
            Action<RestResponse<Person>> successAction = null,
            Action<RestResponse<Person>> errorAction = null)
        {
            return await base.Fetch<Person>(successAction, errorAction);
        }

    }

    public class PersonCreateRequest : BaseRestRequest
    {
        public PersonCreateRequest()
            : base()
        {
            Url("http://www.example.com/Person");
            Post();
        }

        public PersonCreateRequest Name(string name)
        {
            return Param("name", name) as PersonCreateRequest;
        }

        public PersonCreateRequest Age(long age)
        {
            return Param("age", age) as PersonCreateRequest;
        }

        public new async Task<RestResponse<IVoid>> Fetch(
            Action<RestResponse<IVoid>> successAction = null,
            Action<RestResponse<IVoid>> errorAction = null)
        {
            return await base.Fetch<IVoid>(successAction, errorAction);
        }
    }

    public class Persons
    {
        public static PersonGetRequest GetByName()
        {
            return new PersonGetRequest();
        }

        public static PersonCreateRequest Create()
        {
            return new PersonCreateRequest();
        }

    }

    public class SampleTest
    {
        public static async Task Test()
        {
            RestRequest request = new RestRequest();
            string url = "https://duckduckgo.com/";

            var response = await request.Get().
                Url(url).QParam("q", "RestlessHttpClient").Fetch<IVoid>();

            if (response.IsStatusCodeMissmatch)
            {
                HttpStatusCode status = response.Response.StatusCode;
                //...
            }
            else if (response.IsException)
                Console.WriteLine(response.Exception);
            else
            {
                HttpContent content = response.Response.Content;
                // ...
            }


            // Get http://www.example.com/Person?name=<PersonName> 
            // Example Person class 
            // class Person
            // {
            //    string Name{get;set;}
            //    int    Age{get; set;}
            // }
            url = "http://www.example.com/Person";

            var getPerson = new RestRequest();
            RestResponse<Person> personResponse = await getPerson.
                Get().Url(url).QParam("name", "TestUser").Fetch<Person>();

            if (personResponse.HasData)
            {
                Person person = personResponse.Data;
                //...
            }
            else
            {
                // Error handling
                // Check personResponse.isStatusCodeMissmatch and personResponse.isException
            }

            // POST http://www.example.com/Person
            // name=<name>,age=<age>

            var createPerson = new RestRequest();

            // Add form url encoded content, all params that are added before the AddFormUrl call will be
            // added automatically. 
            createPerson.Post().Url(url).Param("name", "NewUser").Param("age", 99).AddFormUrl();

            // or equivalent
            //createPerson.Post().Url(url).AddFormUrl("name", "NewUser", "age", 99.ToString());        

            RestResponse<IVoid> createResponse = await createPerson.Fetch<IVoid>();

            if (createResponse.IsStatusCodeMissmatch)
            {
                // do error handling
            }

            // or get the HttpResponseMessage directly if no deserialization is needed.
            HttpResponseMessage httpResponse = await createPerson.GetResponseAsync();

            if (httpResponse.StatusCode != HttpStatusCode.Created)
            {
                // Error handling
            }


            // Do an action on the underlying HttpRequestMessage
            request = new RestRequest().
                RequestAction(r => r.Headers.Host = "http://www.test.com").
                RequestAction(r => r.Method = new HttpMethod("GET"));

            request.ClientAction((c) => c.Timeout = new TimeSpan(50000));

            // Fetch request with success and error actions.
            await getPerson.Fetch<Person>(
                (r) => Console.WriteLine(r.Data.Name + " is " + r.Data.Age + " years old."),
                (r) => Console.WriteLine(r.Exception.Message));


            // Now the PersonRequest only exposes the Name() and Fetch(...) methods. 
            // All BaseRestRequest methods are protected and cannot be used.
            // Thats why the BaseRestRequest methods are mostly protected.
            // RestRequest is basically just some kind of decorator 
            // for BaseRestRequest that makes all methods public.

            PersonGetRequest personGetRequest = new PersonGetRequest();

            RestResponse<Person> persGetResponse = await personGetRequest.Name("testUser").Fetch();

            if (persGetResponse.HasData)
            {
                Person person = persGetResponse.Data;
                //...
            }


            // or with actions
            await personGetRequest.Name("testUser").Fetch(
                (r) => Console.WriteLine(r.Data.Name + " is " + r.Data.Age + " years old."),
                (r) => Console.WriteLine(r.Exception.Message));


            // One can make a class with static methods that creates a custom request:

            // usage:

            persGetResponse = await Persons.GetByName().Name("testUser").Fetch();
            if (persGetResponse.HasData)
            {
                var person = persGetResponse.Data;
                //...
            }
            else
            {
                // Do error processing. 
                if (persGetResponse.IsException)
                {
                    //...
                }
                if (persGetResponse.IsStatusCodeMissmatch)
                {
                    // persGetResponse.Response.StatusCode;  
                }
            }

            var persCreateResponse = await Persons.Create().Name("testUser2").Age(42).Fetch();

            // because PersonCreateRequest uses Fetch with IVoid,
            // there is no deserialized object in the Data property of the response.
            if (persCreateResponse.IsStatusCodeMissmatch)
            {
                //...
            }
            else if (persCreateResponse.IsException)
            {

            }

        }
    }

```

-----------------------------------------------------


    
