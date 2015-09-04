Restless
========

``` $ git clone https://github.com/Nulands/Restless.git ```

Restless API
=======

- [HTTP Methods](#http-methods)
- [Request paramter](#request-parameter)
- [Adding content](#http-content)
- [Authentication](#http-authentication)
- [Sending the request and getting the response](#request-response)
- [RestResponse class](#RestResponse)
- [OAauth2](#oauth2)
- [Sample](#sample)

## License

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

For a overview of the license visit 
http://choosealicense.com/licenses/apache-2.0/

----------------------------------------------------


##<a name="http-methods"></a> HTTP Methods

```c#

RestRequest request = new RestRequest();
request.Get("www.example.com/endpoint/", "queryparam1", "value1", "queryparam2", "value2");
request.Post("www.example.com/endpoint/", "form-url-param1", "value1", ...);
request.Put("www.example.com/endpoint/", "form-url-param1", "value1", ...);
request.Delete("www.example.com/endpoint/", "form-url-param1", "value1", ...);
request.Head(...);
request.Post(...);
request.Trace(...);
request.Connect(...);

// Via static Rest class

RestRequest request = Rest.Post(...);

```

####<a name="request-parameter"></a>  Request parameter

```c#

/// <summary>
/// Parameter type enum. Query, FormUrlEncoded or Url.
/// </summary>
public enum ParameterType
{
    NotSpecified,
    /// <summary>
    /// Parameter is added to the URL as query parameter (?name=value).
    /// </summary>
    Query,
    /// <summary>
    /// Parameter is added to a POST request with FormUrlEncoded Http content.
    /// </summary>
    FormUrlEncoded,
    /// <summary>
    /// Parameter is used to format the URL string (replaces a {name}).
    /// </summary>
    Url
}

.... 


// Query parameter  (GET)
request.QParam("query-parameter", "value");
request.QParam("query-parameter", 42);

// Form url encoded parameter (POST, PUT, ..)
request.Param("form-url-param1", "value", addAsMultiple: true);
request.Param("form-url-param1", "value2", addAsMultiple: true);

// Add multiple at once, value can be any object
request.Param("form-url-param1", "value2", "form-url-param2", 42, ...);

// Specify parameter type explicit
request.Param("param-name", "value", ParameterType.FormUrlEncoded);

// ...
request.Url("www.example.com/endpoint/{id}");
request.UrlParam("id", 123456);

// ..
request.Url("www.example.com/{0}/{1}")
       .UrlFormat("endpoint", 123456);

// Additional things

// Allow POST (Form-Url-Encoded) parameters with HTTP method GET
// otherwise (default) all params added via request.Param(..)
// becomes query parameters
request.SetAllowFormUrlWithGET(true);

// Throws exception if valueObj is null, and only adds the parameter
// if valueObj.ToString() is not null or empty
request.ParamIfNotEmpty("name", valueObj, ParameterType.FormUrlEncoded);
```

####<a name="http-content"></a>  Add HTTP content

```c#
// Multipart
request.AddMultipart("subtype", "boundary");
// Multipart form data
request.AddMultipartForm("boundary");

// All AddXY methods take additional parameters for "boundary names"
// if the underlying content is a multipart

byte[] data = ...;
request.AddByteArray(data);

Stream stream = ...;
request.AddStream(stream, "media/type");

request.AddString("Hello world", Encoding.UTF8);

Person person = ...;
request.AddJson(person);

request.AddXml(person);

```

####<a name="http-authentication"></a>  Authentication

```c#
// added as Authorization header (username:password, Base64 encoded)
request.Basic("username", "password");
// or
request.Basic("username:password");

// Add OAuth token
// default is "Bearer" and toBase64=true
request.Bearer("token", tokenType: "Bearer", toBase64: true);

// Cookie
request.Cookie("name", "value");

```

####<a name="request-response"></a>  Sending the request and getting the response

```c#
Request request = ...;

// Without response deserialization
RestResponse<IVoid> response = await request.Fetch();

// All Fetch methods lets you specify a HttpClient to use (optional)
// if null (default) a new one is created for this request
HttpClient client = ...;
var response = await request.Fetch(client);

// With response content deserialization
RestResponse<Person> response = await request.Fetch<Person>();

// Upload file as binary content using StreamContent
RestResponse<IVoid> response = await request.UploadFileBinary(stream, "content/type");

// Upload file and MultipartFormDataContent
// on Fetch
// 1. Creates Multipart-form-data content
// 2. Adds all form url encoded parameters
// 3. Adds a stream content 
Request request = Rest.Post(...);
request.Param("param1", "value1", "param2", 42, ...)
RestResponse<IVoid> response = await request.UploadFileFormData(stream, "content/type", "/local/Path/To/File.xy");

// with response deserialization
RestResponse<FileMetaData> response = await request.UploadFileFormData<FileMetaData(
    stream, 
    "content/type", 
    "/local/Path/To/File.xy");
    
if(response.IsSuccessStatusCode && response.HasData)
{
    FileMetaData uploadedFile = response.Data;
    ...
}
else
{
    HttpStatusCode statusCode = response.HttpResponse.StatusCode;
    ..
    if(response.IsException)
    {
        Console.WriteLine(response.Exception.Message);
        ...
    }
}
```

####<a name="RestResponse"></a>  RestResponse

```c#

RestResponse<Person> response = await request.Fetch<Person>();

// The RestRequest that lead to this response
response.Request;

// If exception was thrown
bool isException = request.IsException; 
Exception exc = request.Exception;

// Check if is RestResponse<IVoid> 
response.IsNothing;

// Successfull response
bool isSuccStatusCode = response.IsSuccessStatusCode; 

// Deserialized data;
if(response.HasData)
{
    Person person = response.Data;
    ...
}

// Underlying native System.Net.Http.HttpResponseMessage
var httpResponseMsg = response.HttpResponse;

```

##<a name="oauth2"></a> OAuth2

```c#

// Constants
public class OAuth2
{
    public const string RESPONSE_TYPE_CODE = "code";
    public const string REDIRECT_OOB = "urn:ietf:wg:oauth:  2.0:oob";
    public const string REDIRECT_OOB_AUTO = "urn:ietf:wg:oauth:2.0:oob:auto";
    public const string REDIRECT_LOCALHOST = "http://localhost";

    public const string GRANT_TYPE_AUTH_CODE = "authorization_code";
    public const string GRANT_TYPE_REFRESH_TOKEN = "refresh_token";
    public const string GRANT_TYPE_PASSWORD = "password";
    public const string GRANT_TYPE_CLIENT_CREDENTIALS = "client_credentials";
}
    
// Usage
RestResponse<IVoid> response = Rest.OAuth.CodeAuthorizationUrl(
    "authorizationUrl",
    "clientId",
    "redirect_uri",
    "scope",
    "state");
    
    

// Get an access token (grant_type : authorization_code)
RestResponse<OAuthToken> tokenResponse = Rest.OAuth.TokenFromAuthorizationCode(
    "tokenEndpoint",
    "code",
    "clientId",
    "clientSecret",     // optional from here on
    "redirectUri",      // See OAauth2.REDIRECT_OOB, .REDIRECT_OOB_AUTO, .REDIRECT_LOCALHOST strings
    "userName",
    "password",
    addToDefaultTokenManager: true);
    
OAuthToken token = tokenResponse.Data;

string accessToken = token.AccessToken;
string refreshToken = token.RefreshToken;
long? expiration = token.ExpiresIn;

// Refreshing an access token   (grant_type : refresh_token)

RestResponse<OAuthToken> tokenResponse = Rest.OAuth.RefreshAccessToken(
    "tokenEndpoint",
    "refreshToken",
    "clientId",     // Optional from here on
    "clientSecret",
    "scope",
    "userName",
    "password");

// Using the TokenManager class to automatically refresh tokens
// on retreival

// Create a new one
TokenManager tokenManager = new TokenManager();
// or use static one
Rest.TokenManager.XY(...);

// Adding a token
OAuthToken token = ...;
Rest.TokenManager.Add("clientId", "clientId", "tokenEndpoint", token);

...
// Retreive
OAuthToken token = await Rest.TokenManager.Get("clientId");
// Or custom OAuthToken
MyOAuthToken token = await Rest.TokenManager.Get<MyOAuthToken>("clientId");

// Remove
Rest.TokenManager.Remove("clientId");

// Loading
TokenItem[] tokens = ...;
Rest.TokenManager.Load(tokens);
// Or
Rest.TokenManager.Load(token1, token2, ...);

// Saving (for persistance)
var serializer = new MySerializer();
Rest.TokenManager.Save(token => serializer.Serialize(token));
// or simply
Rest.TokenManager.Save(serializer.Serialize);

// argument can be 
Action<TokenItem> saveAction = item => ...;
//or
Action<IEnumerable<TokenItem>> saveAction = items => ...;


```

----

##<a name="sample"></a> Sample.cs

(May not be up to date)

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


    
