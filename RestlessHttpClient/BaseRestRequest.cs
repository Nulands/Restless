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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;

using System.Text;
using System.Net;
using System.IO;
using Restless.Deserializers;
using Restless.Extensions;
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

        private Dictionary<string, IDeserializer> _contentHandler = new Dictionary<string, IDeserializer>();

        protected Dictionary<string, object> _qParameter = new Dictionary<string, object>(); 
        protected Dictionary<string, object> _parameter = new Dictionary<string, object>();
        private string _url = "";

        protected HttpClient _client = new HttpClient();
        protected HttpRequestMessage _request = new HttpRequestMessage();

        protected virtual HttpClient HttpClient
        {
            get { return _client; }
            set { _client = value; }
        }

        protected virtual HttpRequestMessage Request
        {
            get { return _request; }
            set { _request = value; }
        }

        protected BaseRestRequest(HttpRequestMessage defaultRequest = null, HttpClient client = null)
        {
            if (defaultRequest != null)
                _request = defaultRequest;
            if (client != null)
                _client = client;
            registerDefaultHandlers();
        }

        #region Set request methods

        protected virtual BaseRestRequest Get()
        {
            _request.Method = new HttpMethod("GET");
            return this;
        }

        protected virtual BaseRestRequest Head()
        {
            _request.Method = new HttpMethod("HEAD");
            return this;
        }

        protected virtual BaseRestRequest Post()
        {
            _request.Method = new HttpMethod("POST");
            return this;
        }

        protected virtual BaseRestRequest Put()
        {
            _request.Method = new HttpMethod("PUT");
            return this;
        }

        protected virtual BaseRestRequest Delete()
        {
            _request.Method = new HttpMethod("DELETE");
            return this;
        }

        protected virtual BaseRestRequest Trace()
        {
            _request.Method = new HttpMethod("TRACE");
            return this;
        }

        protected virtual BaseRestRequest Connect()
        {
            _request.Method = new HttpMethod("CONNECT");
            return this;
        }

        #endregion

        #region Set and add content

        protected virtual BaseRestRequest AddContent(HttpContent content, string name = "", string fileName = "")
        {
            if (_request.Content is MultipartContent)
                (_request.Content as MultipartContent).Add(content);
            else if (_request.Content is MultipartFormDataContent)
                (_request.Content as MultipartFormDataContent).Add(content, name, fileName);
            else
                _request.Content = content;
            return this;
        }

        protected virtual BaseRestRequest ClearContent()
        {
            _request.Content = null;
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
                foreach (var element in _parameter)
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

        protected virtual BaseRestRequest Url(string url)
        {
            if (url == null)
                throw new ArgumentException("Url is null!");
            else
                _url = url;
            return this;
        }

        protected virtual BaseRestRequest UrlFormat(params object[] objects)
        {
            if (objects != null && objects.Length > 0)
                String.Format(_url, objects);
            return this;
        }

        protected virtual BaseRestRequest RequestAction(Action<HttpRequestMessage> action)
        {
            action(_request);
            return this;
        }
        
        protected virtual BaseRestRequest Basic(string username, string password)
        {
            
            string base64authStr = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));
            _request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64authStr);
            return this;
        }

        protected virtual BaseRestRequest Bearer(string token)
        {
            string base64AccessToken = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(token));
            _request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", base64AccessToken);
            return this;
        }

        protected virtual BaseRestRequest Param(string name, object value)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("Parameter name is null or empty!");
            if (value == null)
                throw new ArgumentException("Parameter value is null!");
            _parameter[name] = value;
            return this;
        }

        protected virtual BaseRestRequest QParam(string name, object value)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("Parameter name is null or empty!");
            if (value == null)
                throw new ArgumentException("Parameter value is null!");
            _qParameter[name] = value;
            return this;

        }

        protected virtual BaseRestRequest Header(string name, string value)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("Parameter name is null!");
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("Parameter value is null!");

            _request.Headers.Add(name, value);
            return this;
        }

        protected virtual BaseRestRequest Header(string name, IEnumerable<string> values)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("Parameter name is null!");
            if (values == null)
                throw new ArgumentException("Parameters are null!");

            _request.Headers.Add(name, values);
            return this;
        }

        #region Get response HttpWebResponse

        protected virtual async Task<HttpResponseMessage> GetResponseAsync()
        {
            makeQueryUrl();
            _request.RequestUri = new Uri(_url);

            return await _client.SendAsync(_request);
        }

        #endregion 

        #region Fetch RestResponse and deserialize directly

        protected virtual RestResponse<T> Fetch<T>(HttpStatusCode wantedStatusCode = HttpStatusCode.OK,
                                                               Action<RestResponse<T>> successAction = null,
                                                               Action<RestResponse<T>> errorAction = null)
        {
            throw new NotImplementedException();
        }


        protected virtual async Task<RestResponse<T>> FetchAsync<T>(HttpStatusCode wantedStatusCode = HttpStatusCode.OK,
                                                               Action<RestResponse<T>> successAction = null,
                                                               Action<RestResponse<T>> errorAction = null)
        {
            HttpResponseMessage response = null;
            RestResponse<T> result = new RestResponse<T>();

            // Add query parameter to the url
            // If method is GET then paramater added with QParam AND with Param are treated as
            // Query parameter.
            makeQueryUrl();

            if (_request.Method.Method != "GET" && _request.Content == null && _parameter.Count > 0)
                AddFormUrl();           // Add form url encoded parameter to request if needed

            _request.RequestUri = new Uri(_url);
            try
            {
                response = await _client.SendAsync(_request);
                //response = response.EnsureSuccessStatusCode();

                result.Response = response;

                if (response.StatusCode == wantedStatusCode)
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
    

        protected virtual async Task<RestResponse<T>> UploadFileBinary<T>(string localPath,
                                                                        string contentType,
                                                                        Action<RestResponse<T>> successAction = null,
                                                                        Action<RestResponse<T>> errorAction = null)
        {
            RestResponse<T> result = new RestResponse<T>();

            using (FileStream fileStream = File.OpenRead(localPath))
            {
                result = await UploadFileBinary<T>(fileStream, contentType, successAction, errorAction);
            }
            return result;
        }

        protected virtual async Task<RestResponse<T>> UploadFileFormData<T>(string localPath,
                                                                    string contentType,
                                                                    Action<RestResponse<T>> successAction = null,
                                                                    Action<RestResponse<T>> errorAction = null)
        {
            RestResponse<T> result = new RestResponse<T>();

            using (FileStream fileStream = File.OpenRead(localPath))
            {
                result = await UploadFileFormData<T>(fileStream, contentType, localPath, successAction, errorAction); 
            }
            return result;
        }

        protected virtual async Task<RestResponse<T>> UploadFileBinary<T>(Stream fileStream,
                                                                            string contentType,
                                                                    Action<RestResponse<T>> successAction = null,
                                                                    Action<RestResponse<T>> errorAction = null)
        {
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
                makeQueryUrl();

                _request.RequestUri = new Uri(_url);

                response = await _client.SendAsync(_request);

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

        protected virtual async Task<RestResponse<T>> UploadFileFormData<T>(Stream fileStream,
                                                                            string contentType,
                                                                            string localPath,
                                                                            Action<RestResponse<T>> successAction = null,
                                                                            Action<RestResponse<T>> errorAction = null)
        {
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
                AddStream(fileStream, contentType, 1024, Path.GetFileNameWithoutExtension(localPath), Path.GetFileName(localPath));

                HttpResponseMessage response = null;

                // Add query parameter to the url
                // Some apis need query parameter even with post and put
                makeQueryUrl();

                _request.RequestUri = new Uri(_url);

                response = await _client.SendAsync(_request);

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
        #region Helper functions

        private async Task<T> tryDeserialization<T>(HttpResponseMessage response)
        {
            T result = default(T);
            if (!(typeof(T) is INot))
            {
                // TODO: Check media type for json and xml?
                IDeserializer deserializer = GetHandler(response.Content.Headers.ContentType.MediaType);
                result = deserializer.Deserialize<T>(await response.Content.ReadAsStringAsync());
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
            return _parameter.ContainsKey(name);
        }

        private void registerDefaultHandlers()
        {
            // register default handlers
            _contentHandler.Add("application/json", new JsonDeserializer());
            _contentHandler.Add("application/xml", new XmlDeserializer());
            _contentHandler.Add("text/json", new JsonDeserializer());
            _contentHandler.Add("text/x-json", new JsonDeserializer());
            _contentHandler.Add("text/javascript", new JsonDeserializer());
            _contentHandler.Add("text/xml", new XmlDeserializer());
            _contentHandler.Add("*", new XmlDeserializer());
        }

        /// <summary>
        /// Retrieve the handler for the specified MIME content type
        /// </summary>
        /// <param name="contentType">MIME content type to retrieve</param>
        /// <returns>IDeserializer instance</returns>
        protected IDeserializer GetHandler(string contentType)
        {
            if (string.IsNullOrEmpty(contentType) && _contentHandler.ContainsKey("*"))
            {
                return _contentHandler["*"];
            }

            var semicolonIndex = contentType.IndexOf(';');
            if (semicolonIndex > -1) contentType = contentType.Substring(0, semicolonIndex);
            IDeserializer handler = null;
            if (_contentHandler.ContainsKey(contentType))
            {
                handler = _contentHandler[contentType];
            }
            else if (_contentHandler.ContainsKey("*"))
            {
                handler = _contentHandler["*"];
            }

            return handler;
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

        private void makeQueryUrl()
        {
            // Add query parameter to url
            string query = makeParameterString(_qParameter);

            // if method is GET treat all parameters as query parameter
            if(_request.Method.Method == "GET")
            {
                string pQuery = makeParameterString(_parameter);
                if(String.IsNullOrEmpty(query))
                    query = pQuery;
                else
                    query += "&" + pQuery;
            }

            if(!String.IsNullOrEmpty(query))
                _url += "?" + query;
        }

        #endregion 

    }
}
