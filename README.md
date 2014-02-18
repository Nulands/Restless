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
    
    request = new Request().Post().Url(url).Param("name", "TestUser").Param("age", 99);
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
```
    
