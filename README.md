Restless
========

Rest like api that uses HttpWebRequest and HttpWebResponse internally.

Uses Json and Xml (de)serializer from RestSharp (https://github.com/restsharp/RestSharp).

More documentation will follow:

Usage:

```c#

    RestRequest request = new RestRequest();
    
    // Get simple response with no deserialization
    string url = "https://duckduckgo.com/";
    RestResponse<INot> response = request.Get().Url(url).Param("q", "Restless").Fetch<INot>();
    // HttpWebResponse response = request.Get().Url(url).Param("q", "Restless").GetResponse();

    // Get http://www.example.com/Person?name=<PersonName> 
    // Example Person class 
    // class Person
    // {
    //    string Name{get;set;}
    //    int    Age{get; set;}
    // }
    url = "http://www.example.com/Person";
    
    // Parameters to GET Requests are query parameter.
    request = new RestRequest().Get().Url(url).Param("name", "TestUser");
    
    // Fetch only deserializes when response status code matches the wantedStatusCode parameter
    // But here its empty because the default is HttpStatusCode.Ok
    var personResponse = request.Fetch<Person>();
    if(personResponse.HttpResponse.StatusCode == HttpStatusCode.Ok)
    {
        Person person = personResponse.Data;
        ...
    }
    
    
    // POST http://www.example.com/Person
    // name=<name>,age=<age>
    
    request = new Request().Post().Url(url).Param("name", "TestUser").Param("age", 99.ToString());
    var createResponse = request.Fetch<INot>();
    // var createResponse = await request.FetchAsync<INot>();
    if(createResponse.HttpResponse.StatusCode != HttpStatusCode.Created)
    {
        // Error handling
        ...
    }
    
    // or get the HttpWebResponse directly if no deserialization is needed.
    HttpWebResponse httpResponse = request.GetResponse();
    // HttpWebResponse httpResponse = await request.GetResponseAsync();
    if(httpResponse.StatusCode != HttpStatusCode.Created)
    {
        // Error handling
        ...
    }
    
    
    
    // Do an action on the underlying HttpWebRequest
    request = new RestRequest().RequestAction((r) => r.ContinueTimeout = 500)
                               .RequestAction((r) => r.Method = "POST");


    // Fetch request with success and error actions.
    request.Fetch(HttpStatusCode.Ok, 
                  (r) => Console.WriteLine(r.Data.Name + " is " + r.Data.Age + " years old."), 
                  (r) => Console.WriteLine(r.Exception.Message);
                        
```


You can subclass BaseRestRequest to make custom requests, that fits more to a given api.

For example the person GET request:

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
            return Param("name", name) as PersonGetRequest;
            
            // or
            // Param("name", name);
            // return this;
        }
        
        public new RestResponse<Person> Fetch(Action<RestResponse<T>> successAction = null,
                                              Action<RestResponse<T>> errorAction = null)
        {
            return base.Fetch<Person>(HttpStatusCode.Ok, successAction, errorAction);
        }
        
        // Do the same with FetchAsync if needed.
    
    }
    
    
    
    public class PersonCreateRequest : BaseRestRequest
    {
        public PersonCreateRequest() 
            : base()
        {
            Url("http://www.example.com/Person");
            Get();
        }
        
        public PersonCreateRequest Name(string name)
        {
            return Param("name", name) as PersonCreateRequest;
        }
        
        public PersonCreateRequest Age(long age)
        {
            return Param("age", age.ToString() ) as PersonCreateRequest;
        }
        
        public new RestResponse<INot> Fetch(Action<RestResponse<T>> successAction = null,
                                              Action<RestResponse<T>> errorAction = null)
        {
            return base.Fetch<INot>(HttpStatusCode.Created, successAction, errorAction);
        }
        
        // Do the same with FetchAsync if needed.
    
    }

```

Now the PersonRequest only exposes the Name() and Fetch(...) methods. 
All BaseRestRequest methods are protected and cannot be used.
Thats why the BaseRestRequest methods are mostly protected.
RestRequest is basically just some kind of decorator for BaseRestRequest that makes all methods public.

```c#


    PersonGetRequest request = new PersonGetRequest();
    
    RestResponse<Person> response = request.Name("testUser").Fetch();
    
    if(response.HasData)
    {
        Person person = response.Data;
        ...
    }
    
    
    // or with actions

    request.Fetch((r) => Console.WriteLine(r.Data.Name + " is " + r.Data.Age + " years old."),
                  (r) => Console.WriteLine(r.Exception.Message));
           
         
                  
```

You can make a class with static methods that creates a custom request:

```c#
    
    public class Persons
    {
        public PersonGetRequest GetByName()
        {
            return new PersonRequest();
        }
        
        public PersonCreateRequest Create()
        {
            return new PersonCreateRequest();
        }
        
    }
    
    .....
    
    // usage:
    
    var response = Persons.GetByName().Name("testUser").Fetch();
    if(response.HasData)
    {
        var person = response.Data;
        ...
    }
    else
    {
        // Do error processing. 
        // Check exception and HttpWebResponse in the RestResponse for example..
    
    }
    
    
    
    
    var createResponse = Person.Create().Name("testUser2").Age(42).Fetch();
    
    // because PersonCreateRequest uses Fetch with INot,
    // there is no deserialized object in the Data property of the response.
    if(createResponse.Exception is WebException)
    {
        // if Exception was a WebException
        // the RestResponse HttpWebResponse contains a reference to 
        // WebException.HttpWebResponse directly
        Console.WriteLine(createResponse.HttpWebResponse.StatusDescription);
        
        // or equivalent via the exception.
        // Console.WriteLine((createResponse.Exception as WebException).Response.StatusDescription);
    }
    else
        Console.WriteLine(createResponse.Exception.Message);
    

```

    
