Restless
========

```

$ git clone https://github.com/Nulands/Restless.git

    
```
More documentation will follow:

Published under Apache License 2.0
For a overview of the license visit 
http://choosealicense.com/licenses/apache-2.0/


```

    Copyright 2015 Muraad Nofal

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
    

```


----------------------------------------------------

Restless API
=======



Using the RestRequest class
----

#### Sample.cs

```c#
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    /// <summary>
    /// It is possible to inherit from RestRequest.
    /// This can be usefull if a rest request has lots of parameter,
    /// specially when some of the are optional and/or there are
    /// lots of possible parameter combinations.
    /// This is an example custom RestRequest that implements a fluent api style.
    /// </summary>
    public class PersonGetRequest : RestRequest
    {
        public PersonGetRequest()
            : base()
        {
            this.Url("http://www.example.com/Person").Get();
        }

        public PersonGetRequest Name(string name)
        {
            return this.QParam("name", name);
        }

        public async Task<RestResponse<Person>> GetPerson(
            Action<RestResponse<Person>> successAction = null,
            Action<RestResponse<Person>> errorAction = null)
        {
            return await this.Fetch(successAction, errorAction);
        }

    }

    public class PersonCreateRequest : RestRequest
    {
        public PersonCreateRequest()
            : base()
        {
            this.Url("http://www.example.com/Person").Post();
        }

        public PersonCreateRequest Name(string name)
        {
            return this.Param("name", name);
        }

        public PersonCreateRequest Age(long age)
        {
            return this.Param("age", age);
        }

        public async Task<RestResponse<IVoid>> Create(
            Action<RestResponse<IVoid>> successAction = null,
            Action<RestResponse<IVoid>> errorAction = null)
        {
            return await this.Fetch(successAction, errorAction);
        }
    }

    /// <summary>
    /// Static factory methods can be useful and can
    /// make the usage of the api even more fluent and intuitive.
    /// They can be used to set common header. Examples could be
    /// User-Agent or some Authentication
    /// </summary>
    public class Persons
    {
        public static PersonGetRequest GetByName(string name = "")
        {
            var personGet = new PersonGetRequest();
            if (!String.IsNullOrEmpty(name))
                personGet.Name(name);
            return personGet;
        }

        public static PersonCreateRequest Create()
        {
            return new PersonCreateRequest();
        }

    }


    public class SampleTest
    {
        public static async void RestRequest_Raw()
        {
            ///
            /// First a test using the "raw" RestRequest class.
            ///
            RestRequest request = new RestRequest();
            string url = "https://duckduckgo.com/";

            RestResponse<IVoid> response = await request
                .Get()                              // Set HTTP method to "GET"
                .Url(url)                           // Set our url
                .QParam("q", "RestlessHttpClient")  // Set a request query parameter
                .Fetch();                           // Get response, without serialization

            if (!response.IsSuccessStatusCode)
            {
                HttpStatusCode status = response.HttpResponse.StatusCode;
                //...
            }
            else if (response.IsException)
                Debug.WriteLine(response.Exception.Message);
            else
            {
                HttpContent content = response.HttpResponse.Content;
                string strContent = await content.ReadAsStringAsync();
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
            RestResponse<Person> personResponse = await getPerson
                .Get()                          // HTTP Method "GET"
                .Url(url)                       // Set url of Person get REST endpoint
                .QParam("name", "TestUser")     // The api wants the person name as query parameter
                .Fetch<Person>();                // Get response and deserialize to Person object.

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
            createPerson.
                Post().                             // Set HTTP method to "POST", we are creating a new Person
                Url(url).                           // Set Url
                Param("name", "NewUser").           // The new person name as parameter
                Param("age", 99).                   // The new person age as parameter
                AddFormUrl();                       // Add all currently added parameter as Form Url parameter

            // or equivalent
            //createPerson.Post().Url(url).AddFormUrl("name", "NewUser", "age", 99.ToString());        

            // Now create the new person and get the response.
            RestResponse<IVoid> createResponse = await createPerson.Fetch();

            // createResponse.Response is the underlying HttpResponseMessage
            if (createResponse.HttpResponse.StatusCode != HttpStatusCode.Created)
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
                (r) => Debug.WriteLine(r.Data.Name + " is " + r.Data.Age + " years old."),
                (r) => Debug.WriteLine(r.Exception.Message));
        }

        public static async Task Using_Custom_Requests()
        {
            PersonGetRequest personGetRequest = new PersonGetRequest();

            RestResponse<Person> persGetResponse = await personGetRequest.Name("testUser").GetPerson();

            if (persGetResponse.HasData)
            {
                Person person = persGetResponse.Data;
                //...
            }

            // or with actions
            await personGetRequest.Name("testUser").GetPerson(
                (r) => Debug.WriteLine(r.Data.Name + " is " + r.Data.Age + " years old."),
                (r) => Debug.WriteLine(r.Exception.Message));

        }

        public static async Task Using_Factory_Methods()
        {   
            RestResponse<Person> persGetResponse = await Persons.GetByName("testUser").GetPerson();
            if (persGetResponse.HasData)
            {
                var person = persGetResponse.Data;
                //...
            }
            else
            {
                // Do error processing. 
                if (!persGetResponse.IsSuccessStatusCode)
                {
                    // persGetResponse.Response.StatusCode;  
                }
                else if (persGetResponse.IsException)
                {
                    //...
                }

            }

            RestResponse<IVoid> persCreateResponse = await Persons.Create().Name("testUser2").Age(42).Create();

            // because PersonCreateRequest uses Fetch with IVoid,
            // there is no deserialized object in the Data property of the response.
            if (!persCreateResponse.IsSuccessStatusCode)
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


    
