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
using Restless.Deserializers;
using System.Threading.Tasks;

namespace Restless
{

    public enum Content
    {
        ByteArray,
        FormUrlEncoded,
        Multipart,
        MultipartFormData,
        Stream,
        String
    }

    public enum ParameterType
    {
        Query,
        FormUrlEncoded,
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
    public abstract class BaseRestRequest : IDisposable
    {
        #region Variables 

        /// <summary>
        /// Content (de)serialization handler.
        /// </summary>
        private Dictionary<string, IDeserializer> content_handler = new Dictionary<string, IDeserializer>();

        /// <summary>
        /// Url query parameters: ?name=value
        /// </summary>
        protected Dictionary<string, object> query_params = new Dictionary<string, object>(); 

        /// <summary>
        /// When method is GET then added as query parameters too.
        /// Otherwise added as FormUrlEncoded parameters: name=value
        /// </summary>
        protected Dictionary<string, object> param = new Dictionary<string, object>();

        /// <summary>
        /// Url parameters ../{name}.
        /// </summary>
        protected Dictionary<string, object> url_params = new Dictionary<string, object>();

        /// <summary>
        /// The url string. Can contain {name} and/or format strings {0}.
        /// </summary>
        private string url = "";

        /// <summary>
        /// A CancellationToken that is used in buildAndSendRequest (client.sendAsync(.., cancellation)).
        /// </summary>
        protected CancellationToken cancellation = new CancellationToken();
        
        /// <summary>
        /// HttpClient used to send the request message.
        /// </summary>
        protected HttpClient client = new HttpClient();

        /// <summary>
        /// Internal request message.
        /// </summary>
        protected HttpRequestMessage request = new HttpRequestMessage();
        
        #endregion

        #region CancellationToken, HttpClient and HttpRequestMessage propertys

        /// <summary>
        /// The CancellationToken for this request.
        /// </summary>
        protected  CancellationToken CancellationToken
        {
            get { return cancellation; }
            set { cancellation = value; }
        }

        /// <summary>
        /// HttpClient property.
        /// </summary>
        protected  HttpClient HttpClient
        {
            get { return client; }
            set { client = value; }
        }

        /// <summary>
        /// HttpRequestMessage property.
        /// </summary>
        protected  HttpRequestMessage Request
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
        protected BaseRestRequest(HttpRequestMessage defaultRequest = null, HttpClient httpClient = null)
        {
            if (defaultRequest != null)
                request = defaultRequest;
            if (httpClient != null)
                client = httpClient;

            registerDefaultHandlers();
        }

        #region Set request methods GET, HEAD, POST, PUT ...

        protected BaseRestRequest Get()
        {
            request.Method = new HttpMethod("GET");
            return this;
        }

        protected BaseRestRequest Head()
        {
            request.Method = new HttpMethod("HEAD");
            return this;
        }

        protected  BaseRestRequest Post()
        {
            request.Method = new HttpMethod("POST");
            return this;
        }

        protected  BaseRestRequest Put()
        {
            request.Method = new HttpMethod("PUT");
            return this;
        }

        protected  BaseRestRequest Delete()
        {
            request.Method = new HttpMethod("DELETE");
            return this;
        }

        protected  BaseRestRequest Trace()
        {
            request.Method = new HttpMethod("TRACE");
            return this;
        }

        protected  BaseRestRequest Connect()
        {
            request.Method = new HttpMethod("CONNECT");
            return this;
        }

        #endregion

        #region Set and add HttpContent, Byte, form url encoded, multipart, multipart form, stream and string content.

        protected  BaseRestRequest AddContent(HttpContent content, string name = "", string fileName = "")
        {
            content.ThrowIfNull("content");
            if (request.Content is MultipartContent)
                (request.Content as MultipartContent).Add(content);
            else if (request.Content is MultipartFormDataContent)
            {
                (request.Content as MultipartFormDataContent).Add(content, name, fileName);
                name.ThrowIfNullOrEmpty("name");
                fileName.ThrowIfNullOrEmpty("fileName");
            }
            else
                request.Content = content;
            return this;
        }

        protected  BaseRestRequest ClearContent()
        {
            request.Content = null;
            return this;
        }

        protected  BaseRestRequest AddByteArray(byte[] buffer, string name = "", string fileName = "")
        {
            buffer.ThrowIfNullOrEmpty("buffer");
            return AddContent(new ByteArrayContent(buffer), name, fileName);
        }

        protected  BaseRestRequest AddFormUrl(params string[] kvPairs)
        {
            List<KeyValuePair<string, string>> keyValues = new List<KeyValuePair<string, string>>();

            if (kvPairs == null || kvPairs.Length == 0)
            {
                // use the parameters added with Param(..)
                foreach (var element in param)
                    keyValues.Add(new KeyValuePair<string, string>(element.Key, (string)element.Value));
            }
            else
            {
                kvPairs.ThrowIf(pairs => pairs.Length % 2 != 0, "kvPairs. No value for every name given.");

                // use the given parameters.
                for (int i = 0; i < kvPairs.Length; i += 2)
                    keyValues.Add(new KeyValuePair<string, string>(kvPairs[i], kvPairs[i + 1]));
            }
            return AddContent(new FormUrlEncodedContent(keyValues));
        }

        protected  BaseRestRequest AddMultipart(string subtype = "", string boundary = "")
        {
            return AddContent(new MultipartContent(subtype, boundary));
        }

        protected  BaseRestRequest AddMultipartForm(string boundary = "")
        {
            return AddContent(new MultipartFormDataContent(boundary));
        }

        protected  BaseRestRequest AddStream(Stream stream, string mediaType, int buffersize = 1024, string name = "", string fileName = "")
        {
            stream.ThrowIfNull("stream");
            mediaType.ThrowIfNullOrEmpty("mediaType");
            buffersize.ThrowIf(b => b <= 0, "bufferSize");

            var content = new StreamContent(stream, buffersize);
            content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            return AddContent(content, name, fileName);
        }

        protected  BaseRestRequest AddString(string content, Encoding encoding, string mediaType, string name = "", string fileName = "")
        {
            content.ThrowIfNullOrEmpty("content");
            encoding.ThrowIfNull("encoding");
            mediaType.ThrowIfNullOrEmpty("mediaType");

            return AddContent(new StringContent(content, encoding, mediaType), name, fileName);
        }

        #endregion 

        #region Url, CancellationToken, parameters and headers

        protected BaseRestRequest CancelToken(CancellationToken token)
        {
            token.ThrowIfNull("token");
            cancellation = token;
            return this;
        }

        protected BaseRestRequest Url(string url)
        {
            url.ThrowIfNull("url");            
            this.url = url;
            return this;
        }

        protected BaseRestRequest UrlFormat(params object[] objects)
        {
            objects.ThrowIfNullOrEmpty("objects");
            String.Format(url, objects);
            return this;
        }

        protected  BaseRestRequest RequestAction(Action<HttpRequestMessage> action)
        {
            action.ThrowIfNull("action");
            action(request);
            return this;
        }

        protected  BaseRestRequest ClientAction(Action<HttpClient> action)
        {
            action.ThrowIfNull("action");
            action(client);
            return this;
        }

        protected  BaseRestRequest Basic(string username, string password)
        {
            username.ThrowIfNullOrEmpty("username");
            password.ThrowIfNullOrEmpty("password");

            string base64authStr = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64authStr);
            return this;
        }

        protected  BaseRestRequest Bearer(string token)
        {
            token.ThrowIfNullOrEmpty("token");
            string base64AccessToken = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(token));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", base64AccessToken);
            return this;
        }

        protected  BaseRestRequest Param(string name, object value, ParameterType type)
        {
            name.ThrowIfNullOrEmpty("name");
            value.ThrowIfNullOrToStrEmpty("value");

            if (type == ParameterType.FormUrlEncoded)
                param[name] = value;
            else if (type == ParameterType.Query)
                query_params[name] = value;
            else // type == ParameterType.Url
            {
                if (url.Contains("{" + name + "}"))
                    url_params.Add(name, value);
                else
                    throw new ArgumentException("Url does not contain a parameter : " + name);
            }

            return this;
        }

        protected  BaseRestRequest UrlParam(string name, object value)
        {
            name.ThrowIfNullOrEmpty("name");
            value.ThrowIfNullOrToStrEmpty("value");
            url = url.Replace("{" + name + "}", value.ToString());
            return this;
        }

        protected  BaseRestRequest Param(string name, object value)
        {
            name.ThrowIfNullOrEmpty("name");
            value.ThrowIfNullOrToStrEmpty("value");

            param[name] = value;
            return this;
        }

        protected  BaseRestRequest QParam(string name, object value)
        {
            name.ThrowIfNullOrEmpty("name");
            value.ThrowIfNullOrToStrEmpty("value");

            query_params[name] = value;
            return this;

        }

        protected  BaseRestRequest Header(string name, string value)
        {
            name.ThrowIfNullOrEmpty("name");
            value.ThrowIfNullOrEmpty("value");

            request.Headers.Add(name, value);
            return this;
        }

        protected  BaseRestRequest Header(string name, IEnumerable<string> values)
        {
            name.ThrowIfNullOrEmpty("name");
            values.ThrowIfNullOrEmpty("values");

            request.Headers.Add(name, values);
            return this;
        }

        #endregion 

        #region Get HttpWebResponse or RestResponse<IVoid> async 

        protected async Task<HttpResponseMessage> GetResponseAsync()
        {
            if (request.Method.Method != "GET" && request.Content == null && param.Count > 0)
                AddFormUrl();           // Add form url encoded parameter to request if needed

            request.RequestUri = new Uri(url.CreateRequestUri(query_params, param, request.Method.Method));
            return await client.SendAsync(request);
        }

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

        protected  async Task<RestResponse<T>> UploadFileBinary<T>(
            string localPath, string contentType,
            Action<RestResponse<T>> successAction = null, Action<RestResponse<T>> errorAction = null)
        {
            localPath.ThrowIfNotFound(true, "localPath");

            RestResponse<T> result = new RestResponse<T>();
            using (FileStream fileStream = File.OpenRead(localPath))
            {
                result = await UploadFileBinary<T>(fileStream, contentType, successAction, errorAction);
            }
            return result;
        }

        protected  async Task<RestResponse<T>> UploadFileBinary<T>(
            Stream fileStream, String contentType,
            Action<RestResponse<T>> successAction = null, Action<RestResponse<T>> errorAction = null)
        {
            fileStream.ThrowIfNull("fileStream");
            contentType.ThrowIfNullOrEmpty("contentType");

            // TODO: clear complete request to be sure we have a fresh one?
            // TODO: Verify method is POST or PUT ?

            // Clear _request.Content to be sure we are not in a multipart ?
            // ClearContent();
            AddStream(fileStream, contentType);

            return await buildAndSendRequest<T>(successAction, errorAction);
        }

        #endregion 

        #region Upload file via multipart form and stream content. Possible parameters are added via FormUrlEncoded content.

        protected  async Task<RestResponse<T>> UploadFileFormData<T>(
            string localPath, string contentType,
            Action<RestResponse<T>> successAction = null, Action<RestResponse<T>> errorAction = null)
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
        
        protected  async Task<RestResponse<T>> UploadFileFormData<T>(
            Stream fileStream, string contentType, string localPath, 
            Action<RestResponse<T>> successAction = null,Action<RestResponse<T>> errorAction = null)
        {
            fileStream.ThrowIfNull("fileStream");
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

            AddStream(fileStream, contentType, 1024, Path.GetFileNameWithoutExtension(localPath), Path.GetFileName(localPath));

            return await buildAndSendRequest<T>(successAction, errorAction);
        }

        #endregion 

        #region Helper functions

        private async Task<RestResponse<T>> buildAndSendRequest<T>(
            Action<RestResponse<T>> successAction = null, Action<RestResponse<T>> errorAction = null)
        {
            // RestResponse<T> result = new RestResponse<T>();
            // TODO: Good or bad to have a reference from the response to the request?!
            // TODO: the result.Response.RequestMessage already points to this.Request...
            RestResponse<T> result = new RestResponse<T>(this);
            try
            {
                request.RequestUri = new Uri(url.CreateRequestUri(query_params, param, request.Method.Method));

                // TODO: Maybe its better that cancellation is set from an external source/caller?
                //cancellation = new System.Threading.CancellationToken();

                result.Response = await client.SendAsync(request, cancellation);

                if (result.Response.IsSuccessStatusCode)
                {
                    result.Data = await tryDeserialization<T>(result.Response);
                    ActionIfNotNull<T>(result, successAction);
                }
                else
                {
                    result.IsStatusCodeMissmatch = true;
                    ActionIfNotNull<T>(result, errorAction);
                }
            }
            catch (Exception exc)
            {
                result.Exception = exc;
                ActionIfNotNull<T>(result, errorAction);
            }
            return result;
        }
        
        private void ActionIfNotNull<T>(RestResponse<T> response, Action<RestResponse<T>> action)
        {
            if (action != null)
                action(response);
        }

        protected bool containsParam(string name)
        {
            return param.ContainsKey(name);
        }
        
        #region Serialization
        
        private void registerDefaultHandlers()
        {
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
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
