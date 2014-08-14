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
using System.Linq;

using System.Threading;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using Nulands.Restless.Deserializers;
using Nulands.Restless.Extensions;


namespace Nulands.Restless
{

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

    /// <summary>
    /// Base class for a RestRequest based on HttpWebRequest.
    /// Protected methods are commented in RestRequest.
    /// If a custom RestRequest is needed, then subclass BaseRestRequest.
    /// Make a public "new" method that delegates to the BaseRestRequest protected method if some
    /// base methods are needed. 
    /// Otherwise the new class has a "clean" interface, and only higher level methods are exposed public.
    /// See RestRequest.
    /// </summary>
    /// <remarks>Currently the BaseRestRequest does not verify that the underlying HttpRequestMessage.Content
    /// is set correctly. The developer is responsible for setting a correct HttpContent.
    /// For example a POST request should use FormUrlEncoded content when parameters are needed...</remarks>
    public class RestRequest : IDisposable
    {
        #region Variables 

        /// <summary>
        /// Content (de)serialization handler.
        /// </summary>
        internal Dictionary<string, IDeserializer> content_handler = new Dictionary<string, IDeserializer>();

        /// <summary>
        /// Url query parameters: ?name=value
        /// </summary>
        internal Dictionary<string, object> query_params = new Dictionary<string, object>(); 

        /// <summary>
        /// When method is GET then added as query parameters too.
        /// Otherwise added as FormUrlEncoded parameters: name=value
        /// </summary>
        internal Dictionary<string, List<object>> param = new Dictionary<string, List<object>>();

        /// <summary>
        /// Url parameters ../{name}.
        /// </summary>
        internal Dictionary<string, object> url_params = new Dictionary<string, object>();

        /// <summary>
        /// The url string. Can contain {name} and/or format strings {0}.
        /// </summary>
        internal string url = "";

        /// <summary>
        /// Last url format {} set with UrlFormat.
        /// </summary>
        internal object[] urlFormatParams = null;

        /// <summary>
        /// A CancellationToken that is used in buildAndSendRequest (client.sendAsync(.., cancellation)).
        /// </summary>
        internal CancellationToken cancellation = new CancellationToken();
        
        /// <summary>
        /// HttpClient used to send the request message.
        /// </summary>
        internal HttpClient client = new HttpClient();

        /// <summary>
        /// Internal request message.
        /// </summary>
        internal HttpRequestMessage request = new HttpRequestMessage();
        
        #endregion

        #region CancellationToken, HttpClient and HttpRequestMessage propertys

        /// <summary>
        /// The CancellationToken for this request.
        /// </summary>
        internal CancellationToken CancellationToken
        {
            get { return cancellation; }
            set { cancellation = value; }
        }

        /// <summary>
        /// HttpClient property.
        /// </summary>
        internal HttpClient HttpClient
        {
            get { return client; }
            set { client = value; }
        }

        /// <summary>
        /// HttpRequestMessage property.
        /// </summary>
        internal HttpRequestMessage Request
        {
            get { return request; }
            set { request = value; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="defaultRequest">The initial request message, or null if not used.</param>
        /// <param name="httpClient">The initial http client, or null if not used.</param>
        public RestRequest(HttpRequestMessage defaultRequest = null, HttpClient httpClient = null)
        {
            
            if (defaultRequest != null)
                request = defaultRequest;
            if (httpClient != null)
                client = httpClient;
            registerDefaultHandlers();
        }

        /*
        #region Set request methods GET, HEAD, POST, PUT ...

        /// <summary>
        /// Sets the HttpMethod given by string.
        /// </summary>
        /// <param name="method">The HttpMethod string. For example "GET".</param>
        /// <returns>this.</returns>
        protected BaseRestRequest Method(string method)
        {
            request.Method = new HttpMethod(method);
            return this;
        }

        /// <summary>
        /// Set the HttpMethod to GET.
        /// </summary>
        /// <returns>this.</returns>
        protected BaseRestRequest Get()
        {
            request.Method = HttpMethod.Get;
            return this;
        }

        /// <summary>
        /// Set the HttpMethod to HEAD.
        /// </summary>
        /// <returns>this.</returns>
        protected BaseRestRequest Head()
        {
            request.Method = new HttpMethod("HEAD");
            return this;
        }

        /// <summary>
        /// Set the HttpMethod to POST.
        /// </summary>
        /// <returns>this.</returns>
        protected  BaseRestRequest Post()
        {
            request.Method = new HttpMethod("POST");
            return this;
        }

        /// <summary>
        /// Set the HttpMethod to PUT.
        /// </summary>
        /// <returns>this.</returns>
        protected  BaseRestRequest Put()
        {
            request.Method = new HttpMethod("PUT");
            return this;
        }

        /// <summary>
        /// Set the HttpMethod to DELETE.
        /// </summary>
        /// <returns>this.</returns>
        protected  BaseRestRequest Delete()
        {
            request.Method = new HttpMethod("DELETE");
            return this;
        }

        /// <summary>
        /// Set the HttpMethod to TRACE.
        /// </summary>
        /// <returns>this.</returns>
        protected  BaseRestRequest Trace()
        {
            request.Method = new HttpMethod("TRACE");
            return this;
        }

        /// <summary>
        /// Set the HttpMethod to CONNECT.
        /// </summary>
        /// <returns>this.</returns>
        protected  BaseRestRequest Connect()
        {
            request.Method = new HttpMethod("CONNECT");
            return this;
        }

        #endregion

        #region Set and add HttpContent, Byte, form url encoded, multipart, multipart form, stream and string content.

        /// <summary>
        /// Adds a HttpContent to the Request.
        /// Multiple contents can be set. 
        /// For example first a MultipartContent can be added with AddMultipart(..).
        /// Then a StreamContent can be added to this MultipartContent with AddStream(..).
        /// If the underlying request.Content is a MultipartContent or MultipartFormDataContent
        /// -> the content is added to this MultipartContent.
        /// Otherwise the request.Content is simply set to the given content.
        /// </summary>
        /// <param name="content">The HttpContent.</param>
        /// <param name="name">A name can be needed when content is a MultipartFormDataContent already.</param>
        /// <param name="fileName">A file name can be needed when content is a MultipartFormDataContent already.</param>
        /// <returns>this.</returns>
        protected  BaseRestRequest AddContent(HttpContent content, string name = "", string fileName = "")
        {
            content.ThrowIfNull("content");

            // If content is a multipart already then add it as sub content to the multipart.

            if (request.Content is MultipartContent)
                (request.Content as MultipartContent).Add(content);
            else if (request.Content is MultipartFormDataContent)
            {
                // For MultipartFormDataContent name and fileName must be set, so chech them first.
                name.ThrowIfNullOrEmpty("name");
                fileName.ThrowIfNullOrEmpty("fileName");
                (request.Content as MultipartFormDataContent).Add(content, name, fileName);
            }
            else
                request.Content = content;
            return this;
        }

        /// <summary>
        /// Sets the underlying HttpContent to null.
        /// </summary>
        /// <returns>this.</returns>
        protected  BaseRestRequest ClearContent()
        {
            request.Content = null;
            return this;
        }

        /// <summary>
        /// Adds a ByteArrayContent to the request.
        /// </summary>
        /// <param name="buffer">The buffer containing data.</param>
        /// <param name="name">A name is needed if underlying HttpContent is MultipartFormDataContent. (for example multiple file uploads)</param>
        /// <param name="fileName">A file name is needed if underlying HttpContent is MultipartFormDataContent.</param>
        /// <returns>this</returns>
        protected  BaseRestRequest AddByteArray(byte[] buffer, string name = "", string fileName = "")
        {
            buffer.ThrowIfNullOrEmpty("buffer");
            return AddContent(new ByteArrayContent(buffer), name, fileName);
        }

        /// <summary>
        /// Adds a FormUrlEncodedContent to the request.
        /// If kvPairs are given and kvPairs.Length % 2 is even and length is not zero
        /// the kvPairs array is treated as a key value pair list. 
        /// These key-value pairs are added to the FormUrlEncodedContent on construction.
        /// If no kvPairs are given all parameters added with Param(..) are added to the new 
        /// FromUrlEncodedContent.
        /// </summary>
        /// <param name="kvPairs">The list of key-value pairs. Must contain an even number of string objects if used.</param>
        /// <returns>this.</returns>
        protected  BaseRestRequest AddFormUrl(params string[] kvPairs)
        {
            List<KeyValuePair<string, string>> keyValues = new List<KeyValuePair<string, string>>();

            if (kvPairs == null || kvPairs.Length == 0)
            {
                foreach (var element in param)  
                {
                    foreach(var value in element.Value) // we can have the parameter with element.Key set multiple times.
                        keyValues.Add(new KeyValuePair<string, string>(element.Key, value.ToString()));
                }
            }
            else
            {
                kvPairs.ThrowIf(pairs => pairs.Length % 2 != 0, "kvPairs. No value for every name given.");

                for (int i = 0; i < kvPairs.Length; i += 2)
                    keyValues.Add(new KeyValuePair<string, string>(kvPairs[i], kvPairs[i + 1]));
            }
            return AddContent(new FormUrlEncodedContent(keyValues));
        }

        /// <summary>
        /// Adds a MultipartContent to the request.
        /// </summary>
        /// <param name="subtype">The sub type if needed.</param>
        /// <param name="boundary">The boundary if needed.</param>
        /// <returns>this.</returns>
        protected  BaseRestRequest AddMultipart(string subtype = "", string boundary = "")
        {
            return AddContent(new MultipartContent(subtype, boundary));
        }

        /// <summary>
        /// Adds a MultipartFormDataContent to the request.
        /// </summary>
        /// <param name="boundary">The boundary if needed.</param>
        /// <returns>this.</returns>
        protected  BaseRestRequest AddMultipartForm(string boundary = "")
        {
            return AddContent(new MultipartFormDataContent(boundary));
        }

        /// <summary>
        /// Adds a StreamContent to the request.
        /// </summary>
        /// <param name="stream">The stream to be added.</param>
        /// <param name="mediaType">The media type of the stream.</param>
        /// <param name="buffersize">The buffer size used to process the stream. Default is 1024.</param>
        /// <param name="name">A name needed when content is a MultipartFormDataContent already.</param>
        /// <param name="fileName">A file name needed when content is a MultipartFormDataContent already.</param>
        /// <returns>this.</returns>
        protected  BaseRestRequest AddStream(Stream stream, string mediaType, int buffersize = 1024, string name = "", string fileName = "")
        {
            stream.ThrowIfNull("stream");
            mediaType.ThrowIfNullOrEmpty("mediaType");
            buffersize.ThrowIf(b => b <= 0, "bufferSize");

            var content = new StreamContent(stream, buffersize);
            content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            return AddContent(content, name, fileName);
        }

        /// <summary>
        /// Adds a StringContent to the request.
        /// </summary>
        /// <param name="content">The string content.</param>
        /// <param name="encoding">The content encoding.</param>
        /// <param name="mediaType">The content media type.</param>
        /// <param name="name">A name needed when content is a MultipartFormDataContent already.</param>
        /// <param name="fileName">A file name needed when content is a MultipartFormDataContent already.</param>
        /// <returns>this.</returns>
        protected  BaseRestRequest AddString(string content, Encoding encoding, string mediaType, string name = "", string fileName = "")
        {
            content.ThrowIfNullOrEmpty("content");
            encoding.ThrowIfNull("encoding");
            mediaType.ThrowIfNullOrEmpty("mediaType");

            return AddContent(new StringContent(content, encoding, mediaType), name, fileName);
        }

        /// <summary>
        /// Adds an object as serialized json string.
        /// </summary>
        /// <remarks>Throws exception if the given object is null, or if the
        /// serialized json string is null or empty.</remarks>
        /// <param name="obj">The object that will be serialized and added as json string content.</param>
        /// <param name="name">A name needed when content is a MultipartFormDataContent already.</param>
        /// <param name="fileName">A file name needed when content is a MultipartFormDataContent already.</param>
        /// <returns>this.</returns>
        protected BaseRestRequest AddJson(object obj, string name = "", string fileName = "")
        {
            obj.ThrowIfNull("BaseRestRequest");
            var serializer = new Serializers.JsonSerializer();
            var jsonContent = serializer.Serialize(obj);
            jsonContent.ThrowIfNullOrEmpty("BaseRestRequest", "jsonStr");
            // .net default encoding is UTF-8
            if (!String.IsNullOrEmpty(jsonContent))
                AddContent(new StringContent(jsonContent, Encoding.Default, serializer.ContentType), name, fileName);
            return this;
        }

        /// <summary>
        /// Adds an object as serialized xml string.
        /// </summary>
        /// <remarks>Throws exception if the given object is null, or if the
        /// serialized xml string is null or empty.</remarks>
        /// <param name="obj">The object that will be serialized and added as xml string content.</param>
        /// <param name="name">A name needed when content is a MultipartFormDataContent already.</param>
        /// <param name="fileName">A file name needed when content is a MultipartFormDataContent already.</param>
        /// <returns>this.</returns>
        protected BaseRestRequest AddXml(object obj, string name = "", string fileName = "")
        {
            obj.ThrowIfNull("BaseRestRequest");
            var serializer = new Serializers.DotNetXmlSerializer();
            var xmlContent = serializer.Serialize(obj);
            xmlContent.ThrowIfNullOrEmpty("BaseRestRequest", "xmlContent");
            // .net default encoding is UTF-8
            if (!String.IsNullOrEmpty(xmlContent))
                AddContent(new StringContent(xmlContent, Encoding.Default, serializer.ContentType), name, fileName);
            return this;
        }

        #endregion 

        #region Url, CancellationToken, parameters and headers

        /// <summary>
        /// Set the CancellationToken for this request.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>this.</returns>
        protected BaseRestRequest CancelToken(CancellationToken token)
        {
            token.ThrowIfNull("token");
            cancellation = token;
            return this;
        }

        /// <summary>
        /// Sets the URL string for this request.
        /// </summary>
        /// <param name="url">The URL string.</param>
        /// <returns>this.</returns>
        protected BaseRestRequest Url(string url)
        {
            url.ThrowIfNull("url");            
            this.url = url;
            return this;
        }

        /// <summary>
        /// Sets the URL format parameter for this request.
        /// A test String.Format is done to verify the input objects.
        /// </summary>
        /// <param name="objects">The format parameter objects.</param>
        /// <returns>this.</returns>
        protected BaseRestRequest UrlFormat(params object[] objects)
        {
            objects.ThrowIfNullOrEmpty("objects");
            // Do a test format only to see if everything is ok otherwise it will throw an exception.
            String.Format(url, objects);
            // Do not save the formated test url only the format parameter.
            // Url will be build before sending the request.
            urlFormatParams = objects;
            return this;
        }

        /// <summary>
        /// Map an action over the underlying HttpRequestMessage.
        /// Can be used to set "exotic" things, that are not exposed by the BaseRestRequest.
        /// Usage: request.RequestAction(r => r.Content = ...);
        /// </summary>
        /// <param name="action">An action that takes a HttpRequestMessage as argument.</param>
        /// <returns>this.</returns>
        protected  BaseRestRequest RequestAction(Action<HttpRequestMessage> action)
        {
            action.ThrowIfNull("action");
            action(request);
            return this;
        }

        /// <summary>
        /// Map an action over the underlying HttpClient.
        /// Can be used to set "exotic" things, that are not exposed by the BaseRestRequest.
        /// Usage: request.ClientAction(c => c.Timeout = ...);
        /// </summary>
        /// <param name="action">An action that takes a HttpClient as argument.</param>
        /// <returns>this.</returns>
        protected  BaseRestRequest ClientAction(Action<HttpClient> action)
        {
            action.ThrowIfNull("action");
            action(client);
            return this;
        }

        protected BaseRestRequest Basic(string authentication)
        {
            authentication.ThrowIfNullOrEmpty("authentication");

            string base64authStr = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(authentication));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64authStr);
            return this;
        }

        /// <summary>
        /// Adds a Http Basic authorization header to the request. 
        /// The result string is Base64 encoded internally.
        /// </summary>
        /// <param name="username">The user name.</param>
        /// <param name="password">The user password.</param>
        /// <returns>this.</returns>
        protected  BaseRestRequest Basic(string username, string password)
        {
            username.ThrowIfNullOrEmpty("username");
            password.ThrowIfNullOrEmpty("password");

            string base64authStr = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64authStr);
            return this;
        }

        /// <summary>
        /// Adds a Http Bearer authorization header to the request.
        /// The given token string is Base64 encoded internally.
        /// </summary>
        /// <param name="token">The token string.</param>
        /// <returns>this.</returns>
        protected  BaseRestRequest Bearer(string token, string tokenType = "Bearer")
        {
            token.ThrowIfNullOrEmpty("token");
            string base64AccessToken = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(token));
            request.Headers.Authorization = new AuthenticationHeaderValue(tokenType, base64AccessToken);
            return this;
        }

        /// <summary>
        /// Adds a parameter to the request. Can be a Query, FormUrlEncoded or Url parameter.
        /// If a value for the given name is already set, the old parameter value is overwritten silently.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value (should be convertible to string).</param>
        /// <param name="type">The ParameterType.</param>
        /// <returns>this.</returns>
        protected  BaseRestRequest Param(string name, object value, ParameterType type)
        {
            name.ThrowIfNullOrEmpty("name");
            value.ThrowIfNullOrToStrEmpty("value");

            if (type == ParameterType.FormUrlEncoded || type == ParameterType.NotSpecified)
                Param(name, value);
                //param[name] = value;
            else if (type == ParameterType.Query)
                query_params[name] = value;
            else // type == ParameterType.Url
            {
                if (url.Contains("{" + name + "}"))
                    url_params[name] = value;
                else
                    throw new ArgumentException("BaseRestRequest - ParameterType.Url - Url does not contain a parameter : " + name);
            }

            return this;
        }

        /// <summary>
        /// Adds an url parameter to the request.
        /// Url parameters are part of the set url string of the form {name}.
        /// The {name} is replaced by the given value before the request is sent.
        /// If an url parameter value for the given name already exists the
        /// old value is overwritten silently.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value (should be convertible to string).</param>
        /// <returns>this.</returns>
        protected  BaseRestRequest UrlParam(string name, object value)
        {
            name.ThrowIfNullOrEmpty("name");
            value.ThrowIfNullOrToStrEmpty("value");
            url.ThrowIfNullOrEmpty("url - cannot set UrlParameter. Url is null or empty.");

            if (url.Contains("{" + name + "}"))
                url_params[name] = value;
            else
                throw new ArgumentException("BaseRestRequest - UrlParam - Url does not contain a parameter : " + name);

            return this;
        }

        /// <summary>
        /// Add a (FormUrlEncoded) parameter to the request.
        /// </summary>
        /// <remarks>
        /// Should be used with POST/PUT.
        /// If added multiple times the content will contain
        /// ?name=value1&name=value2&name=value3...</remarks>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value (should be convertible to string).</param>
        /// <param name="addAsMultiple">If true the parameter with this name can be set multiple times.</param>
        /// <returns>this.</returns>
        protected  BaseRestRequest Param(string name, object value, bool addAsMultiple = false)
        {
            name.ThrowIfNullOrEmpty("name");
            value.ThrowIfNullOrToStrEmpty("value");

            List<object> paramValues = null;
            if (param.TryGetValue(name, out paramValues))
            {
                if (addAsMultiple)
                    paramValues.Add(value);
                else
                    paramValues[0] = value;     // overwrite the value if not addAsMultiple
            }
            else
            {
                // First time this parameter with given name is added.
                paramValues = new List<object>();
                paramValues.Add(value);
                param[name] = paramValues;
            }
            //param[name] = value;
            return this;
        }

        /// <summary>
        /// Adds a query parameter (?name=value) to the request.
        /// The parameter-value pair is added to the URL before sending the request.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value (should be convertible to string).</param>
        /// <returns>this.</returns>
        protected  BaseRestRequest QParam(string name, object value)
        {
            name.ThrowIfNullOrEmpty("name");
            value.ThrowIfNullOrToStrEmpty("value");

            query_params[name] = value;
            return this;

        }

        /// <summary>
        /// Adds a header with a single value to the request.
        /// </summary>
        /// <param name="name">The header name.</param>
        /// <param name="value">The header value (should be convertible to string).</param>
        /// <returns>this.</returns>
        protected  BaseRestRequest Header(string name, string value)
        {
            name.ThrowIfNullOrEmpty("name");
            value.ThrowIfNullOrEmpty("value");

            request.Headers.Add(name, value);
            return this;
        }

        /// <summary>
        /// Adds a header with multiple values to the request.
        /// </summary>
        /// <param name="name">The header name.</param>
        /// <param name="values">The header values (should be convertible to string).</param>
        /// <returns>this.</returns>
        protected  BaseRestRequest Header(string name, IEnumerable<string> values)
        {
            name.ThrowIfNullOrEmpty("name");
            values.ThrowIfNullOrEmpty("values");

            request.Headers.Add(name, values);
            return this;
        }

        #endregion 

        #region Get HttpWebResponse or RestResponse<IVoid> async 

        /// <summary>
        /// Sends the request and return the raw HttpResponseMessage.
        /// </summary>
        /// <returns>Task containing the HttpResponseMessage.</returns>
        protected async Task<HttpResponseMessage> GetResponseAsync()
        {
            if (request.Method.Method != "GET" && request.Content == null && param.Count > 0)
                AddFormUrl();           // Add form url encoded parameter to request if needed

            request.RequestUri = new Uri(url.CreateRequestUri(query_params, param, request.Method.Method));
            return await client.SendAsync(request);
        }

        /// <summary>
        /// Sends the request and returns a RestResponse with generic type IVoid.
        /// </summary>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>A Task containing the RestRespone. There will be no deserialized data, but the RestResponse.Response 
        /// (HttpResponseMessage) will be set.</returns>
        protected async Task<RestResponse<IVoid>> GetRestResponseAsync(
            Action<RestResponse<IVoid>> successAction = null,
            Action<RestResponse<IVoid>> errorAction = null)
        {
            if (request.Method.Method != "GET" && request.Content == null && param.Count > 0)
                AddFormUrl();           // Add form url encoded parameter to request if needed

            return await buildAndSendRequest<IVoid>(successAction, errorAction);
        }

        #endregion 

        #region Fetch RestResponse and deserialize directly

        /// <summary>
        /// Sends the request and returns the RestResponse containing deserialized data 
        /// from the HttpResponseMessage.Content if T is not IVoid.
        /// </summary>
        /// <typeparam name="T">The type of the deserialized data. Set to IVoid if no deserialization is wanted.</typeparam>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>A taks containing the RestResponse with the deserialized data if T is not IVoid and no error occured.</returns>
        protected async Task<RestResponse<IVoid>> Fetch(
            Action<RestResponse<IVoid>> successAction = null,
            Action<RestResponse<IVoid>> errorAction = null)
        {
            if (request.Method.Method != "GET" && request.Content == null && param.Count > 0)
                AddFormUrl();           // Add form url encoded parameter to request if needed

            return await buildAndSendRequest<IVoid>(successAction, errorAction);
        }

        /// <summary>
        /// Sends the request and returns the RestResponse containing deserialized data 
        /// from the HttpResponseMessage.Content if T is not IVoid.
        /// </summary>
        /// <typeparam name="T">The type of the deserialized data. Set to IVoid if no deserialization is wanted.</typeparam>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>A taks containing the RestResponse with the deserialized data if T is not IVoid and no error occured.</returns>
        protected async Task<RestResponse<T>> Fetch<T>(
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            if (request.Method.Method != "GET" && request.Content == null && param.Count > 0)
                AddFormUrl();           // Add form url encoded parameter to request if needed

            return await buildAndSendRequest<T>(successAction, errorAction);
        }

        #endregion 
    
        #region Upload file binary with StreamContent

        /// <summary>
        /// Uploads a binary file using StreamContent.
        /// The file is opened by this function.
        /// </summary>
        /// <typeparam name="T">The type of the deserialized data. Set to IVoid if no deserialization is wanted.</typeparam>
        /// <param name="localPath">The path to the file that will be uploaded.</param>
        /// <param name="contentType">The file content type.</param>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>A taks containing the RestResponse with the deserialized data if T is not IVoid and no error occured.</returns>
        protected  async Task<RestResponse<T>> UploadFileBinary<T>(
            string localPath, 
            string contentType,
            Action<RestResponse<T>> successAction = null, 
            Action<RestResponse<T>> errorAction = null)
        {
            // check if given file exists
            localPath.ThrowIfNotFound(true, "localPath");

            RestResponse<T> result = new RestResponse<T>();
            using (FileStream fileStream = File.OpenRead(localPath))
            {
                result = await UploadFileBinary<T>(fileStream, contentType, successAction, errorAction);
            }
            return result;
        }

        /// <summary>
        /// Uploads a binary (file) stream using StreamContent.
        /// </summary>
        /// <typeparam name="T">The type of the deserialized data. Set to IVoid if no deserialization is wanted.</typeparam>
        /// <param name="streamContent">The (file) stream that will be uploaded.</param>
        /// <param name="contentType">The file content type.</param>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>A taks containing the RestResponse with the deserialized data if T is not IVoid and no error occured.</returns>
        protected  async Task<RestResponse<T>> UploadFileBinary<T>(
            Stream streamContent, String contentType,
            Action<RestResponse<T>> successAction = null, Action<RestResponse<T>> errorAction = null)
        {
            streamContent.ThrowIfNull("fileStream");
            contentType.ThrowIfNullOrEmpty("contentType");

            // TODO: clear complete request to be sure we have a fresh one?
            // TODO: Verify method is POST or PUT ?

            // Clear _request.Content to be sure we are not in a multipart ?
            // ClearContent();
            AddStream(streamContent, contentType);

            return await buildAndSendRequest<T>(successAction, errorAction);
        }

        #endregion 

        #region Upload file via multipart form and stream content. Possible parameters are added via FormUrlEncoded content.

        /// <summary>
        /// Uploads a binary file using a MultipartFormDataContent and a (sub) StreamContent.
        /// AddFormUrl() is called before the StreamContent is added to the MultipartFormDataContent.
        /// AddFormUrl() will add all parameter to the request that are added with Param(..).
        /// The file is opened by this function.
        /// </summary>
        /// <typeparam name="T">The type of the deserialized data. Set to IVoid if no deserialization is wanted.</typeparam>
        /// <param name="localPath">The path to the file that will be uploaded.</param>
        /// <param name="contentType">The file content type.</param>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>A taks containing the RestResponse with the deserialized data if T is not IVoid and no error occured.</returns>
        protected  async Task<RestResponse<T>> UploadFileFormData<T>(
            string localPath, 
            string contentType,
            Action<RestResponse<T>> successAction = null, 
            Action<RestResponse<T>> errorAction = null)
        {
            localPath.ThrowIfNotFound(true, "localPath");
            // contenttype string is checked from call above

            RestResponse<T> result = new RestResponse<T>();
            using (FileStream fileStream = File.OpenRead(localPath))
            {
                result = await UploadFileFormData<T>(fileStream, contentType, localPath, successAction, errorAction); 
            }
            return result;
        }

        /// <summary>
        /// Uploads a binary (file) stream using a MultipartFormDataContent and a (sub) StreamContent.
        /// AddFormUrl() is called before the StreamContent is added to the MultipartFormDataContent.
        /// AddFormUrl() will add all parameter to the request that are added with Param(..).
        /// </summary>
        /// <typeparam name="T">The type of the deserialized data. Set to IVoid if no deserialization is wanted.</typeparam>
        /// <param name="streamContent">The (file) stream that will be uploaded.</param>
        /// <param name="contentType">The file content type.</param>
        /// <param name="localPath">The "path" of the (file) stream that will be uploaded.</param>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>A taks containing the RestResponse with the deserialized data if T is not IVoid and no error occured.</returns>
        protected  async Task<RestResponse<T>> UploadFileFormData<T>(
            Stream streamContent, 
            string contentType, 
            string localPath, 
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            streamContent.ThrowIfNull("fileStream");
            contentType.ThrowIfNullOrEmpty("contentType");

            // Only check for null or empty, not for existing
            // here its only used for content-disposition
            // the file is should be loaded allready, see fileStream
            localPath.ThrowIfNullOrEmpty("localPath");

            // TODO: clear complete request to be sure we have a fresh one?
            // TODO: Verify method is POST or PUT ?

            //FileInfo info = new FileInfo(localPath);

            // Clear _request.Content to be sure we are not in a multipart ?
            // ClearContent();

            // TODO: create and add (random?) boundary
            AddMultipartForm();
            if (param.Count > 0)
                AddFormUrl();           // Add form url encoded parameter to request if needed    

            AddStream(streamContent, contentType, 1024, Path.GetFileNameWithoutExtension(localPath), Path.GetFileName(localPath));

            return await buildAndSendRequest<T>(successAction, errorAction);
        }

        #endregion 
        */
        #region Helper functions

        /// <summary>
        /// A helper function that is doing all the "hard" work setting up the request and sending it.
        /// 1) The Url is formated using String.Format if UrlParam´s where added.
        /// 2) The query parameter are added to the URL with RestlessExtensions.CreateRequestUri
        /// 3) The request is send.
        /// 4) The RestResponse is set. 
        /// </summary>
        /// <typeparam name="T">The type of the deserialized data. Set to IVoid if no deserialization is wanted.</typeparam>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>A taks containing the RestResponse with the deserialized data if T is not IVoid and no error occured.</returns>
        internal async Task<RestResponse<T>> buildAndSendRequest<T>(
            Action<RestResponse<T>> successAction = null, 
            Action<RestResponse<T>> errorAction = null)
        {
            // RestResponse<T> result = new RestResponse<T>();
            // TODO: Good or bad to have a reference from the response to the request?!
            // TODO: the result.Response.RequestMessage already points to this.Request?! (underlying Http request).
            RestResponse<T> result = new RestResponse<T>(this);
            try
            {
                if(urlFormatParams != null && urlFormatParams.Length > 0)
                    url = String.Format(url, urlFormatParams);  // format url if needed, this should work without exception.

                request.RequestUri = new Uri(url.CreateRequestUri(query_params, param, request.Method.Method));

                // TODO: Maybe its better that cancellation is set from an external source/caller?
                // TODO: At least is hould not be null here because it is created at definition.
                // TODO: And if some external object is holding a reference to the cancellation token already
                // TODO: then better dont create a new one.
                //cancellation = new System.Threading.CancellationToken();

                result.Response = await client.SendAsync(request, cancellation);

                if (result.Response.IsSuccessStatusCode)
                    result.Data = await tryDeserialization<T>(result.Response);
                else
                    result.IsStatusCodeMissmatch = true;
            }
            catch (Exception exc)
            {
                result.Exception = exc;
            }
            
            // call success or error action if necessary
            if (result.Response.IsSuccessStatusCode || result.IsException)
                ActionIfNotNull<T>(result, errorAction);
            else
                ActionIfNotNull<T>(result, successAction);

            return result;
        }
        
        private void ActionIfNotNull<T>(RestResponse<T> response, Action<RestResponse<T>> action)
        {
            if (action != null)
                action(response);
        }

        /// <summary>
        /// Check if param contains a value for the given name already
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <returns>True if already containing value for given name, false otherwise.</returns>
        protected bool containsParam(string name)
        {
            return param.ContainsKey(name);
        }
        
        #region Serialization
        
        private void registerDefaultHandlers()
        {
            // TODO: Why not reusing the deserializer?
            // register default handlers
            content_handler.Add("application/json", new JsonDeserializer());
            content_handler.Add("application/xml", new XmlDeserializer());
            content_handler.Add("text/json", new JsonDeserializer());
            content_handler.Add("text/x-json", new JsonDeserializer());
            content_handler.Add("text/javascript", new JsonDeserializer());
            content_handler.Add("text/xml", new XmlDeserializer());
            content_handler.Add("*", new XmlDeserializer());
        }

        

        private async Task<T> tryDeserialization<T>(HttpResponseMessage response)
        {
            T result = default(T);
            if (!(typeof(T).IsAssignableFrom(typeof(IVoid))))
            {
                // TODO: Check media type for json and xml?
                IDeserializer deserializer = GetHandler(response.Content.Headers.ContentType.MediaType);
                result = deserializer.Deserialize<T>(await response.Content.ReadAsStringAsync());
            }
            return result;

        }

        /// <summary>
        /// Retrieve the handler for the specified MIME content type
        /// </summary>
        /// <param name="contentType">MIME content type to retrieve</param>
        /// <returns>IDeserializer instance</returns>
        protected IDeserializer GetHandler(string contentType)
        {
            if (string.IsNullOrEmpty(contentType) && content_handler.ContainsKey("*"))
            {
                return content_handler["*"];
            }

            var semicolonIndex = contentType.IndexOf(';');
            if (semicolonIndex > -1) contentType = contentType.Substring(0, semicolonIndex);
            IDeserializer handler = null;
            if (content_handler.ContainsKey(contentType))
            {
                handler = content_handler[contentType];
            }
            else if (content_handler.ContainsKey("*"))
            {
                handler = content_handler["*"];
            }

            return handler;
        }

        #endregion 

        #endregion

        #region  IDisposable implementation

        /// <summary>
        /// Dispose the request.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        /// <summary>
        /// Underlying dispose method.
        /// Calls HttpClient and HttpRequestMessage Dispose().
        /// </summary>
        /// <param name="disposing">True if should dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (client != null)
                {
                    client.Dispose();
                    client = null;
                }
                if (request != null)
                {
                    request.Dispose();
                    request = null;
                }
            }
        }

        #endregion
        
    }
}
