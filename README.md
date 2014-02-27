Restless
========

Rest like api.
One uses HttpWebRequest and HttpWebResponse internally, the other version uses HttpClient.


Uses Json and Xml (de)serializer from RestSharp (https://github.com/restsharp/RestSharp).

More documentation will follow:

-----------------------------------------------------

HttpWebRequest version 
=======
HttpClient version 

Usage:

```c#

    RestRequest request = new RestRequest();
    string url = "https://duckduckgo.com/";

    var response = await request.Get().Url(url).QParam("q", "RestlessHttpClient").Fetch<INot>();

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
    RestResponse<Person> personResponse = await getPerson.Get().Url(url).QParam("name", "TestUser").Fetch<Person>();
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
    
    // Default wanted status code for Fetch is OK, now Created is needed to indicate success.
    RestResponse<INot> createResponse = await createPerson.Fetch<INot>(HttpStatusCode.Created);

    if(createResponse.IsStatusCodeMissmatch)
    {
        // status code is not Created!
        // do error handling
    }

    // or get the HttpResponseMessage directly if no deserialization is needed.
    HttpResponseMessage httpResponse = await createPerson.GetResponseAsync();

    if(httpResponse.StatusCode != HttpStatusCode.Created)
    {
        // Error handling
    }


    // Do an action on the underlying HttpRequestMessage
    request = new RestRequest().RequestAction((r) => r.Headers.Host = "http://www.test.com").
                                RequestAction((r) => r.Method = new HttpMethod("GET"));

    // Do an action on the underlying HttpClient
    request.ClientAction((c) => c.Timeout = new TimeSpan(50000));

    // Fetch request with success and error actions.
    await getPerson.Fetch<Person>(
        HttpStatusCode.OK, 
        (r) => Console.WriteLine(r.Data.Name + " is " + r.Data.Age + " years old."), 
        (r) => Console.WriteLine(r.Exception.Message));

```


One can subclass BaseRestRequest to make custom requests, that fits more to a given api.

For example the person requests:

```c#

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
    
        public new async Task<RestResponse<Person>> Fetch(Action<RestResponse<Person>> successAction = null,
                                                    Action<RestResponse<Person>> errorAction = null)
        {
            return await base.Fetch<Person>(HttpStatusCode.OK, successAction, errorAction);
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
    
        public new async Task<RestResponse<INot>> Fetch(Action<RestResponse<INot>> successAction = null,
                                                        Action<RestResponse<INot>> errorAction = null)
        {
            return await base.Fetch<INot>(HttpStatusCode.Created, successAction, errorAction);
        }    
    }
    
```


Now the PersonRequest only exposes the Name() and Fetch(...) methods. 
All BaseRestRequest methods are protected and cannot be used.
Thats why the BaseRestRequest methods are mostly protected.
RestRequest is basically just some kind of decorator for BaseRestRequest that makes all methods public.

```c#

    PersonGetRequest personGetRequest = new PersonGetRequest();

    RestResponse<Person> persGetResponse = await personGetRequest.Name("testUser").Fetch();

    if(persGetResponse.HasData)
    {
        Person person = persGetResponse.Data;
        //...
    }
    
    
    // or with actions
    await personGetRequest.Name("testUser").Fetch(
        (r) => Console.WriteLine(r.Data.Name + " is " + r.Data.Age + " years old."),
        (r) => Console.WriteLine(r.Exception.Message));
 
```


One can make a class with static methods that creates a custom request.

```c#

    public class Persons
    {
        public static PersonGetRequest GetByName()
        {
            // can be used to set some default stuff. Authorization for example.
            return new PersonGetRequest();
        }
    
        public static PersonCreateRequest Create()
        {
            return new PersonCreateRequest();
        }
    
    }
        
    // Usage:
    
    persGetResponse = await Persons.GetByName().Name("testUser").Fetch();
    if(persGetResponse.HasData)
    {
        var person = persGetResponse.Data;
        //...
    }
    else
    {
        // Do error processing. 
        // Check exception and HttpWebResponse in the RestResponse for example..

    }




    var persCreateResponse = await Persons.Create().Name("testUser2").Age(42).Fetch();

    // because PersonCreateRequest uses Fetch with INot,
    // there is no deserialized object in the Data property of the response.
    if(persCreateResponse.IsStatusCodeMissmatch)
    {
        //...
    }
    else if(persCreateResponse.IsException)
    {

    }
    

```

-----------------------------------------------------


    
