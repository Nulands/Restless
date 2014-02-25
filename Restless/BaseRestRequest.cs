﻿/*
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

using System.Text;
using System.Net;
using System.IO;

using Restless.Deserializers;
using Restless.Extensions;
using System.Threading.Tasks;

namespace Restless
{

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
        protected Dictionary<string, string> _parameter = new Dictionary<string, string>();
        private string _url = "";
        protected HttpWebRequest _tmpRequest = WebRequest.CreateHttp("http://www.test.com");
        protected virtual HttpWebRequest HttpWebRequest
        {
            get { return _tmpRequest; }
            set { _tmpRequest = value; }
        }

        protected BaseRestRequest()
        {
            _tmpRequest.ContentLength = 0;
            registerDefaultHandlers();
        }

        protected BaseRestRequest(HttpWebRequest defaultRequest)
        {
            _tmpRequest.ContentLength = 0;
            _tmpRequest = defaultRequest;
            registerDefaultHandlers();
        }

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

        protected virtual BaseRestRequest Get()
        {
            _tmpRequest.Method = "GET";
            return this;
        }

        protected virtual BaseRestRequest Head()
        {
            _tmpRequest.Method = "HEAD";
            return this;
        }

        protected virtual BaseRestRequest Post()
        {
            _tmpRequest.Method = "POST";
            return this;
        }

        protected virtual BaseRestRequest Put()
        {
             _tmpRequest.Method = "PUT";
            return this;
        }

        protected virtual BaseRestRequest Delete()
        {
            _tmpRequest.Method = "DELETE";
            return this;
        }

        protected virtual BaseRestRequest Trace()
        {
            _tmpRequest.Method = "TRACE";
            return this;
        }

        protected virtual BaseRestRequest Connect()
        {
            _tmpRequest.Method = "CONNECT";
            return this;
        }

        protected virtual BaseRestRequest RequestAction(Action<HttpWebRequest> action)
        {
            action(_tmpRequest);
            return this;
        }

        protected virtual BaseRestRequest Credentials(ICredentials credentials)
        {
            _tmpRequest.Credentials = credentials;
            return this;

        }
        
        protected virtual BaseRestRequest Basic(string username, string password)
        {
            string base64AccessToken = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));
            _tmpRequest.Headers.Add("Authorization", "Basic " + base64AccessToken);
            return this;
        }

        protected virtual BaseRestRequest Bearer(string token)
        {
            string base64AccessToken = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(token));
            _tmpRequest.Headers.Add("Authorization", "Bearer " + base64AccessToken);
            return this;
        }

        protected virtual BaseRestRequest Param(string name, string value)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("Parameter name is null!");
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("Parameter value is null!");
            _parameter[name] = value;
            return this;
        }

        protected virtual BaseRestRequest Header(string name, string value)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("Parameter name is null!");
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("Parameter value is null!");

            _tmpRequest.Headers[name] = value;
            return this;
        }

        #region Get response HttpWebResponse

        protected virtual HttpWebResponse GetResponse()
        {
            HttpWebResponse response = null;
            try
            {
                response = makeHttpWebRequest().GetResponse() as HttpWebResponse;
            }
            catch (WebException webExc)
            {
                response = webExc.Response as HttpWebResponse;
            }
            return response;
        }

        protected virtual async Task<HttpWebResponse> GetResponseAsync()
        {
            HttpWebResponse response = null;
            try
            {
                response = await makeHttpWebRequest().GetResponseAsync() as HttpWebResponse;
            }
            catch (WebException webExc)
            {
                response = webExc.Response as HttpWebResponse;
            }
            return response;
        }

        #endregion 

        #region Fetch RestResponse and deserialize directly

        protected virtual RestResponse<T> Fetch<T>(HttpStatusCode wantedStatusCode = HttpStatusCode.OK,
                                                   Action<RestResponse<T>> successAction = null,
                                                   Action<RestResponse<T>> errorAction = null)
        {
            HttpWebRequest request = makeHttpWebRequest();
            RestResponse<T> result = new RestResponse<T>();

            try
            {
                var httpResponse = request.GetResponse() as HttpWebResponse;
                result.HttpResponse = httpResponse;

                if (result.HttpResponse.StatusCode == wantedStatusCode)
                {
                    if (!(typeof(T) is INot))
                    {
                        IDeserializer deserializer = GetHandler(result.HttpResponse.ContentType);
                        result.Data = deserializer.Deserialize<T>(result.HttpResponse);
                    }
                    ActionIfNotNull<T>(result, successAction);
                }
                else
                    ActionIfNotNull<T>(result, errorAction);
            }
            catch (Exception exc)
            {
                result.Exception = exc;
                if(exc is WebException)
                    result.HttpResponse = (exc as WebException).Response as HttpWebResponse;

                ActionIfNotNull<T>(result, errorAction);
            }

            return result;
        }

        protected virtual async Task<RestResponse<T>> FetchAsync<T>(HttpStatusCode wantedStatusCode = HttpStatusCode.OK,
                                                                           Action<RestResponse<T>> successAction = null,
                                                                           Action<RestResponse<T>> errorAction = null)
        {
            HttpWebRequest request = makeHttpWebRequest();
            RestResponse<T> result = new RestResponse<T>();

            try
            {
                var httpResponse = (await request.GetResponseAsync()) as HttpWebResponse;
                result.HttpResponse = httpResponse;

                if (result.HttpResponse.StatusCode == wantedStatusCode)
                {
                    if (!(typeof(T) is INot))
                    {
                        IDeserializer deserializer = GetHandler(result.HttpResponse.ContentType);
                        result.Data = deserializer.Deserialize<T>(result.HttpResponse);
                    }
                    ActionIfNotNull<T>(result, successAction);
                }
                else
                    ActionIfNotNull<T>(result, errorAction);
            }
            catch (Exception exc)
            {
                result.Exception = exc;
                if (exc is WebException)
                    result.HttpResponse = (exc as WebException).Response as HttpWebResponse;

                ActionIfNotNull<T>(result, errorAction);
            }
            return result;
        }

        #endregion 

        #region Upload a file from local space to a net uri

        protected virtual RestResponse<T> UploadFile<T>(string localPath,
                                                    Action<RestResponse<T>> successAction = null,
                                                    Action<RestResponse<T>> errorAction = null)
        {
            WebClient client = setupWebClient();
            var restResponse = new RestResponse<T>();

            string responseStr = null;

            try
            {
                byte[] responseData = client.UploadFile(_url, "POST", localPath);
                responseStr = UTF8Encoding.UTF8.GetString(responseData);

                if (responseStr != null)
                {
                    IDeserializer deserializer = GetHandler(client.ResponseHeaders["Content-Type"]);
                    restResponse.Data = deserializer.Deserialize<T>(responseStr);
                }
                ActionIfNotNull<T>(restResponse, successAction);
            }
            catch (Exception exc)
            {
                restResponse.Exception = exc;
                if (exc is WebException)
                    restResponse.HttpResponse = (exc as WebException).Response as HttpWebResponse;

                ActionIfNotNull<T>(restResponse, errorAction);
            }
            return restResponse;
        }

        protected virtual async Task<RestResponse<T>> UploadFileAsync<T>(string localPath,
                                                                    Action<RestResponse<T>> successAction = null,
                                                                    Action<RestResponse<T>> errorAction = null)
        {
            WebClient client = setupWebClient();
            var restResponse = new RestResponse<T>();

            string responseStr = null;

            try
            {
                byte[] responseData = await client.UploadFileTaskAsync(_url, "POST", localPath);
                responseStr = UTF8Encoding.UTF8.GetString(responseData);

                if (responseStr != null)
                {
                    IDeserializer deserializer = GetHandler(client.ResponseHeaders["Content-Type"]);
                    restResponse.Data = deserializer.Deserialize<T>(responseStr);
                }
                ActionIfNotNull<T>(restResponse, successAction);
            }
            catch (Exception exc)
            {
                restResponse.Exception = exc;
                if (exc is WebException)
                    restResponse.HttpResponse = (exc as WebException).Response as HttpWebResponse;

                ActionIfNotNull<T>(restResponse, errorAction);
            }
            return restResponse;
        }


        #endregion 

        #region Helper functions

        private void ActionIfNotNull<T>(RestResponse<T> response, Action<RestResponse<T>> action)
        {
            if (action != null)
                action(response);
        }
        private WebClient setupWebClient()
        {
            WebClient client = new WebClient();

            foreach (var hName in _tmpRequest.Headers.AllKeys)
                client.Headers[hName] = _tmpRequest.Headers[hName];

            foreach (var param in _parameter)
                client.QueryString[param.Key] = param.Value;

            return client;
        }

        protected bool containsParam(string name)
        {
            return _parameter.ContainsKey(name);
        }
        protected HttpWebRequest makeHttpWebRequest()
        {
            if (_tmpRequest.Method == "GET")
                makeQueryUrl();
            HttpWebRequest request = WebRequest.CreateHttp(_url);
            request.SetFrom(_tmpRequest, "Host", "ContentLength");
            if (_tmpRequest.Method != "GET")
                addPostParameter(request);
            return request;
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

        private string makeParameterString(Dictionary<string, string> paramList)
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

        private void addPostParameter(HttpWebRequest request)
        {
            // Add (POST) parameter
            if (_parameter.Count > 0)
            {
                request.ContentType = "application/x-www-form-urlencoded";

                byte[] data = UTF8Encoding.UTF8.GetBytes(makeParameterString(_parameter));
                request.ContentLength = data.Length;
                using (Stream post = request.GetRequestStream())
                {
                    post.Write(data, 0, data.Length);
                }
            }
        }

        private void makeQueryUrl()
        {
            // Add query parameter to url
            if (_parameter.Count > 0)
            {
                string query = makeParameterString(_parameter);
                _url += "?" + query;
            }
        }

        #endregion 

    }
}
