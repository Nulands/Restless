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
    public abstract class BaseRestRequest
    {
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

        protected System.Threading.CancellationToken cancellation = new System.Threading.CancellationToken();
        
        /// <summary>
        /// HttpClient used to send the request message.
        /// </summary>
        protected HttpClient client = new HttpClient();

        /// <summary>
        /// Internal request message.
        /// </summary>
        protected HttpRequestMessage request = new HttpRequestMessage();

        /// <summary>
        /// HttpClient property.
        /// </summary>
        protected virtual HttpClient HttpClient
        {
            get { return client; }
            set { client = value; }
        }

        /// <summary>
        /// HttpRequestMessage property.
        /// </summary>
        protected virtual HttpRequestMessage Request
        {
            get { return request; }
            set { request = value; }
        }

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

        #region Set request methods

        protected virtual BaseRestRequest Get()
        {
            request.Method = new HttpMethod("GET");
            return this;
        }

        protected virtual BaseRestRequest Head()
        {
            request.Method = new HttpMethod("HEAD");
            return this;
        }

        protected virtual BaseRestRequest Post()
        {
            request.Method = new HttpMethod("POST");
            return this;
        }

        protected virtual BaseRestRequest Put()
        {
            request.Method = new HttpMethod("PUT");
            return this;
        }

        protected virtual BaseRestRequest Delete()
        {
            request.Method = new HttpMethod("DELETE");
            return this;
        }

        protected virtual BaseRestRequest Trace()
        {
            request.Method = new HttpMethod("TRACE");
            return this;
        }

        protected virtual BaseRestRequest Connect()
        {
            request.Method = new HttpMethod("CONNECT");
            return this;
        }

        #endregion

        #region Set and add content

        protected virtual BaseRestRequest AddContent(HttpContent content, string name = "", string fileName = "")
        {
            if (request.Content is MultipartContent)
                (request.Content as MultipartContent).Add(content);
            else if (request.Content is MultipartFormDataContent)
                (request.Content as MultipartFormDataContent).Add(content, name, fileName);
            else
                request.Content = content;
            return this;
        }

        protected virtual BaseRestRequest ClearContent()
        {
            request.Content = null;
            return this;
        }

        protected virtual BaseRestRequest AddByteArray(byte[] buffer, string name = "", string fileName = "")
        {
            return AddContent(new ByteArrayContent(buffer), name, fileName);
        }

        protected virtual BaseRestRequest AddFormUrl(params string[] kvPairs)
        {
            if (kvPairs.Length % 2 != 0)
                throw new ArgumentException("No value for every name given!");

            List<KeyValuePair<string, string>> keyValues = new List<KeyValuePair<string, string>>();

            if (kvPairs.Length == 0)
            {
                // use the parameters added with Param(..)
                foreach (var element in param)
                    keyValues.Add(new KeyValuePair<string, string>(element.Key, (string)element.Value));
            }
            else
            {
                // use the given parameters.
                for (int i = 0; i < kvPairs.Length; i += 2)
                    keyValues.Add(new KeyValuePair<string, string>(kvPairs[i], kvPairs[i + 1]));
            }
            return AddContent(new FormUrlEncodedContent(keyValues));
        }

        protected virtual BaseRestRequest AddMultipart(string subtype = "", string boundary = "")
        {
            return AddContent(new MultipartContent(subtype, boundary));
        }

        protected virtual BaseRestRequest AddMultipartForm(string boundary = "")
        {
            return AddContent(new MultipartFormDataContent(boundary));
        }

        protected virtual BaseRestRequest AddStream(Stream stream, string mediaType, int buffersize = 1024, string name = "", string fileName = "")
        {
            var content = new StreamContent(stream, buffersize);
            content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            return AddContent(content, name, fileName);
        }

        protected virtual BaseRestRequest AddString(string content, Encoding encoding, string mediaType, string name = "", string fileName = "")
        {
            return AddContent(new StringContent(content, encoding, mediaType), name, fileName);
        }

        #endregion 

        #region Url, CancellationToken, parameters and headers

        protected virtual BaseRestRequest CancelToken(CancellationToken token)
        {
            token.ThrowIfNull("CancelToken");

            cancellation = token;
            return this;
        }

        protected virtual BaseRestRequest Url(string url)
        {
            url.ThrowIfNull("Url");            
            this.url = url;
            return this;
        }

        protected virtual BaseRestRequest UrlFormat(params object[] objects)
        {
            objects.ThrowIfNullOrEmpty("UrlFormat");

            String.Format(url, objects);
            return this;
        }

        protected virtual BaseRestRequest RequestAction(Action<HttpRequestMessage> action)
        {
            action.ThrowIfNull("RequestAction");
            action(request);
            return this;
        }

        protected virtual BaseRestRequest ClientAction(Action<HttpClient> action)
        {
            action.ThrowIfNull("ClientAction");
            action(client);
            return this;
        }

        protected virtual BaseRestRequest Basic(string username, string password)
        {
            username.ThrowIfNullOrEmpty("Basic - username");
            password.ThrowIfNullOrEmpty("Basic - password");

            string base64authStr = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64authStr);
            return this;
        }

        protected virtual BaseRestRequest Bearer(string token)
        {
            token.ThrowIfNullOrEmpty("Bearer");
            string base64AccessToken = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(token));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", base64AccessToken);
            return this;
        }

        protected virtual BaseRestRequest Param(string name, object value, ParameterType type)
        {
            name.ThrowIfNullOrEmpty("Param - name");
            value.ThrowIfNullOrToStrEmpty("Param - value");

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

        protected virtual BaseRestRequest UrlParam(string name, object value)
        {
            name.ThrowIfNullOrEmpty("UrlParam - name");
            value.ThrowIfNullOrToStrEmpty("UrlParam - value");
            url = url.Replace("{" + name + "}", value.ToString());
            return this;
        }

        protected virtual BaseRestRequest Param(string name, object value)
        {
            name.ThrowIfNullOrEmpty("Param - name");
            value.ThrowIfNullOrToStrEmpty("Param - value");

            param[name] = value;
            return this;
        }

        protected virtual BaseRestRequest QParam(string name, object value)
        {
            name.ThrowIfNullOrEmpty("QParam - name");
            value.ThrowIfNullOrToStrEmpty("QParam - value");

            query_params[name] = value;
            return this;

        }

        protected virtual BaseRestRequest Header(string name, string value)
        {
            name.ThrowIfNullOrEmpty("Header - name");
            value.ThrowIfNullOrEmpty("Header - value");

            request.Headers.Add(name, value);
            return this;
        }

        protected virtual BaseRestRequest Header(string name, IEnumerable<string> values)
        {
            name.ThrowIfNullOrEmpty("Header - name");
            values.ThrowIfNullOrEmpty("Header - values");

            request.Headers.Add(name, values);
            return this;
        }

        #endregion 

        #region Get HttpWebResponse async 

        protected virtual async Task<HttpResponseMessage> GetResponseAsync()
        {
            //makeQueryUrl();
            //makeRequestUri();

            if (request.Method.Method != "GET" && request.Content == null && param.Count > 0)
                AddFormUrl();           // Add form url encoded parameter to request if needed

            //request.RequestUri = new Uri(url);
            request.RequestUri = new Uri(makeRequestUri());

            return await client.SendAsync(request);
        }

        #endregion 

        #region Fetch RestResponse and deserialize directly

        protected virtual async Task<RestResponse<T>> Fetch<T>(
            HttpStatusCode wantedStatusCode = HttpStatusCode.OK,
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            HttpResponseMessage response = null;
            RestResponse<T> result = new RestResponse<T>();

            // Add query parameter to the url
            // If method is GET then paramater added with QParam AND with Param are treated as
            // Query parameter.
            //url = makeQueryUrl(url);
            //makeRequestUri();

            if (request.Method.Method != "GET" && request.Content == null && param.Count > 0)
                AddFormUrl();           // Add form url encoded parameter to request if needed

            //request.RequestUri = new Uri(url);
            string requestUri = makeRequestUri();
            request.RequestUri = new Uri(makeRequestUri());

            try
            {
                response = await client.SendAsync(request);
                //response = response.EnsureSuccessStatusCode();

                result.Response = response;

                if (response.StatusCode == wantedStatusCode)
                {
                    if (!(typeof(T) is INot))
                        result.Data = await tryDeserialization<T>(response);

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
                Console.WriteLine(exc);
                result.Exception = exc;
                ActionIfNotNull<T>(result, errorAction);
            }

            return result;
        }

        #endregion 
    
        #region Upload file binary with StreamContent

        protected virtual async Task<RestResponse<T>> UploadFileBinary<T>(
            string localPath, string contentType,
            Action<RestResponse<T>> successAction = null,Action<RestResponse<T>> errorAction = null)
        {
            localPath.ThrowIfNotFound(true, "UploadFileBinary - localPath");

            RestResponse<T> result = new RestResponse<T>();
            using (FileStream fileStream = File.OpenRead(localPath))
            {
                result = await UploadFileBinary<T>(fileStream, contentType, successAction, errorAction);
            }
            return result;
        }

        protected virtual async Task<RestResponse<T>> UploadFileBinary<T>(
            Stream fileStream, String contentType,
            Action<RestResponse<T>> successAction = null, Action<RestResponse<T>> errorAction = null)
        {
            fileStream.ThrowIfNull("UploadFileBinary - fileStream");
            contentType.ThrowIfNullOrEmpty("UploadFileBinary - contentType");

            RestResponse<T> result = new RestResponse<T>();
            try
            {
                // TODO: clear complete request to be sure we have a fresh one?
                // TODO: Verify method is POST or PUT ?

                // Clear _request.Content to be sure we are not in a multipart ?
                // ClearContent();
                AddStream(fileStream, contentType);

                HttpResponseMessage response = null;

                // Add query parameter to the url
                // Some apis need query parameter even with post and put
                //makeQueryUrl();
                //makeRequestUri();

                //request.RequestUri = new Uri(url);
                request.RequestUri = new Uri(makeRequestUri());

                response = await client.SendAsync(request);

                response = response.EnsureSuccessStatusCode();

                result.Response = response;

                if (response.IsSuccessStatusCode)
                {
                    result.Data = await tryDeserialization<T>(response);
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

        #endregion 

        #region Upload file via multipart form and stream content. Possible parameters are added via FormUrlEncoded content.

        protected virtual async Task<RestResponse<T>> UploadFileFormData<T>(
            string localPath, string contentType,
            Action<RestResponse<T>> successAction = null, Action<RestResponse<T>> errorAction = null)
        {
            localPath.ThrowIfNotFound(true, "UploadFileBinary - localPath");

            RestResponse<T> result = new RestResponse<T>();
            using (FileStream fileStream = File.OpenRead(localPath))
            {
                result = await UploadFileFormData<T>(fileStream, contentType, localPath, successAction, errorAction); 
            }
            return result;
        }

        
        protected virtual async Task<RestResponse<T>> UploadFileFormData<T>(
            Stream fileStream, string contentType, string localPath, 
            Action<RestResponse<T>> successAction = null,Action<RestResponse<T>> errorAction = null)
        {
            fileStream.ThrowIfNull("UploadFileFormData - fileStream");
            contentType.ThrowIfNullOrEmpty("UploadFileFormData - contentType");

            // Only check for null or empty, not for existing
            // here its only used for content-disposition
            // the file is already loaded, see fileStream
            localPath.ThrowIfNullOrEmpty("UploadFileFormData - localPath");

            RestResponse<T> result = new RestResponse<T>();
            try
            {
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

                HttpResponseMessage response = null;

                // Add query parameter to the url
                // Some apis need query parameter even with post and put
                //makeQueryUrl();
                //makeRequestUri();

                //request.RequestUri = new Uri(url);
                request.RequestUri = new Uri(makeRequestUri());

                response = await client.SendAsync(request);

                //response = response.EnsureSuccessStatusCode();

                result.Response = response;

                if (response.IsSuccessStatusCode)
                {
                    result.Data = await tryDeserialization<T>(response);
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

        #endregion 

        #region Helper functions

        private async Task<T> tryDeserialization<T>(HttpResponseMessage response)
        {
            T result = default(T);
            //if (!(typeof(T) is INot))
            //{
                // TODO: Check media type for json and xml?
                IDeserializer deserializer = GetHandler(response.Content.Headers.ContentType.MediaType);
                result = deserializer.Deserialize<T>(await response.Content.ReadAsStringAsync());
            //}
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

        private string makeRequestUri()
        {
            string requestUrl = makeUrlParams(url);
            requestUrl = makeQueryUrl(requestUrl);
            return requestUrl;
        }

        private string makeUrlParams(string url)
        {
            StringBuilder builder = new StringBuilder(url);
            foreach (var element in url_params)
            {
                string pattern = "{" + element.Key + "}";
                if (url.Contains(pattern))
                    builder.Replace(pattern, element.Value.ToString());
            }
            //url = builder.ToString();
            return builder.ToString();
        }

        private string makeQueryUrl(string url)
        {
            // Add query parameter to url
            string query = makeParameterString(query_params);

            // if method is GET treat all parameters as query parameter
            if (request.Method.Method == "GET")
            {
                string pQuery = makeParameterString(param);
                if (String.IsNullOrEmpty(query))
                    query = pQuery;
                else
                    query += "&" + pQuery;
            }

            if (!String.IsNullOrEmpty(query))
                url += "?" + query;
            return url;
        }

        private string makeParameterString(Dictionary<string, object> paramList)
        {
            StringBuilder str = new StringBuilder();
            int i = 0;
            foreach(var element in paramList)
            {
                str.Append(element.Key);
                str.Append("=");
                str.Append(element.Value);
                if (i < paramList.Count - 1)
                    str.Append("&");
                i++;
            }
            return str.ToString();
        }

        #endregion 

    }
}
