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
#if UNIVERSAL
using Windows.Foundation;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
#else
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
#endif
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;

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
    /// RestRequest class.
    /// </summary>
    /// <remarks>Currently the RestRequest does not verify that the underlying HttpRequestMessage.Content
    /// is set correctly. The developer is responsible for setting a correct HttpContent.
    /// For example a POST request should use FormUrlEncoded content when parameters are needed.
    /// By default (ctor) every RestRequest got his own underlying HttpRequestMessage and HttpClient
    /// to send the constructed request.</remarks>
    public class RestRequest : IDisposable
    {
        public object this[string key]
        {
            set
            {
                key.ThrowIfNullOrEmpty("RestRequest parameter key is null or empty");
                value.ThrowIfNullOrToStrEmpty("RestRequest parameter value is null, empty or unable to get a valid ToString");

                List<object> values = null;
                if(!Params.TryGetValue(key, out values))
                {
                    values = new List<object>();
                    Params[key] = values;
                }
                values.Add(value);
            }
        }
        #region Variables 

        /// <summary>
        /// Content (de)serialization handler.
        /// </summary>
        public readonly Dictionary<string, IDeserializer> ContentHandler = new Dictionary<string, IDeserializer>();

        /// <summary>
        /// Url query parameters: ?name=value
        /// </summary>
        public readonly Dictionary<string, object> QueryParams = new Dictionary<string, object>();

        /// <summary>
        /// When method is GET then added as query parameters too.
        /// Otherwise added as FormUrlEncoded parameters: name=value
        /// </summary>
        public readonly Dictionary<string, List<object>> Params = new Dictionary<string, List<object>>();

        /// <summary>
        /// Url parameters ../{name}.
        /// </summary>
        public readonly Dictionary<string, object> UrlParams = new Dictionary<string, object>();

        /// <summary>
        /// The url string. Can contain {name} and/or format strings {0}.
        /// </summary>
        internal string url = "";

        /// <summary>
        /// Last url format {} set with UrlFormat.
        /// </summary>
        internal object[] urlFormatParams = null;

        CancellationTokenSource internalTokenSource = new CancellationTokenSource();
        CancellationToken internalToken;
        internal CancellationToken externalToken;

        /// <summary>
        /// HttpClient used to send the request message.
        /// </summary>
        internal HttpClient httpClient = null;

        /// <summary>
        /// Internal request message.
        /// </summary>
        internal HttpRequestMessage request = new HttpRequestMessage();
        
        #endregion

        #region CancellationToken, HttpClient and HttpRequestMessage propertys

        /// <summary>
        /// HttpClient property.
        /// </summary>
        public HttpClient HttpClient
        {
            get { return httpClient; }
            set { httpClient = value; }
        }

        /// <summary>
        /// HttpRequestMessage property.
        /// </summary>
        internal HttpRequestMessage HttpRequest
        {
            get { return request; }
            set { request = value; }
        }

        public bool AllowFormUrlWithGET { get; set; }

        #endregion

        public RestRequest()
        {
            AllowFormUrlWithGET = false;
            RegisterDefaultHandlers();
        }

        public RestRequest(IDeserializer jsonDeserializer, IDeserializer xmlDeserializer = null)
        {
            AllowFormUrlWithGET = false;
            RegisterDefaultHandlers(jsonDeserializer, xmlDeserializer);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="defaultRequest">The initial request message, or null if not used.</param>
        /// <param name="httpClient">The initial http client, or null if not used.</param>
        public RestRequest(
            HttpRequestMessage defaultRequest, HttpClient httpClient = null, 
            IDeserializer jsonDeserializer = null, IDeserializer xmlDeserializer = null)
        {
            if (defaultRequest != null)
                request = defaultRequest;
            if (httpClient != null)
                this.httpClient = httpClient;
            RegisterDefaultHandlers(jsonDeserializer, xmlDeserializer);
        }

        #region Helper functions

        public RestRequest HandleFormUrlParameter()
        {
            bool canAddFormUrl = request.Method.Method != "GET" || AllowFormUrlWithGET;

            if (canAddFormUrl && request.Content == null && Params.Count > 0)
                this.AddFormUrl();           // Add form url encoded parameter to request if needed
            return this;
        }

        public async Task<HttpResponseMessage> GetResponseAsync(
             string clientId, bool toBase64 = true)
        {
            if (httpClient == null)
                httpClient = new HttpClient();

            if (!String.IsNullOrEmpty(clientId))
            {
                var token = await Rest.TokenManager.Get(clientId);
                if (token != null && !String.IsNullOrEmpty(token.AccessToken))
                    this.Bearer(token.AccessToken, String.IsNullOrEmpty(token.TokenType) ? "Bearer" : token.TokenType, toBase64);
            }

            this.HandleFormUrlParameter();

            request.RequestUri = new Uri(
                url.CreateRequestUri(
                    QueryParams,
                    Params,
                    request.Method.Method,
                    AllowFormUrlWithGET));
#if UNIVERSAL
            return await httpClient.SendRequestAsync(request);
#else
            return await httpClient.SendAsync(request);
#endif
        }

        internal async Task<RestResponse<T>> buildAndSendRequest<T>(
            string clientId,
            bool toBase64 = true,
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null,
            HttpClient client = null)
        {
            if (!String.IsNullOrEmpty(clientId))
            {
                var token = await Rest.TokenManager.Get(clientId);
                if (token != null && !String.IsNullOrEmpty(token.AccessToken))
                    this.Bearer(token.AccessToken, String.IsNullOrEmpty(token.TokenType) ? "Bearer" : token.TokenType, toBase64);
            }
            return await buildAndSendRequest<T>(successAction, errorAction, client);
        }

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
            Action<RestResponse<T>> errorAction = null,
            HttpClient client = null)
        {
            httpClient = client;
            if (httpClient == null)
                httpClient = new HttpClient();

            // RestResponse<T> result = new RestResponse<T>();
            // TODO: Good or bad to have a reference from the response to the request?!
            // TODO: the result.Response.RequestMessage already points to this.Request?! (underlying Http request).
            RestResponse<T> result = new RestResponse<T>(this);
            try
            {
                // First format the Url
                if(urlFormatParams != null && urlFormatParams.Length > 0)
                    url = String.Format(url, urlFormatParams);  

                if(UrlParams != null && UrlParams.Count > 0)
                {
                    foreach (var item in UrlParams)
                        url = url.Replace("{" + item.Key + "}", item.Value.ToString());
                }

                request.RequestUri = new Uri(url.CreateRequestUri(QueryParams, Params, request.Method.Method, AllowFormUrlWithGET));

#if UNIVERSAL
                var tmp = client.SendRequestAsync(request);
                result.HttpResponse = await tmp.AsTask(CancellationTokenSource.CreateLinkedTokenSource(internalToken, externalToken).Token);
#else
                result.HttpResponse = await httpClient.SendAsync(
                    request, 
                    CancellationTokenSource.CreateLinkedTokenSource(internalToken, externalToken).Token);
#endif
                if (result.HttpResponse.IsSuccessStatusCode)
                    result.Data = await tryDeserialization<T>(result.HttpResponse);
            }
            catch (Exception exc)
            {
                result.Exception = exc;
            }
            
            // call success or error action if necessary
            if (result.IsException || !result.HttpResponse.IsSuccessStatusCode)
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
            return Params.ContainsKey(name);
        }
        
        #region Serialization

        private async Task<T> tryDeserialization<T>(HttpResponseMessage response)
        {
            T result = default(T);
            if (!(typeof(T).GetTypeInfo().IsAssignableFrom(typeof(IVoid).GetTypeInfo())))
            {
                // TODO: Check media type for json and xml?
                IDeserializer deserializer = GetHandler(response.Content.Headers.ContentType.MediaType);
                result = deserializer.Deserialize<T>(await response.Content.ReadAsStringAsync());
            }
            return result;
        }

        public void RegisterDefaultHandlers(IDeserializer jsonDeserializer = null, IDeserializer xmlDeserializer = null)
        {
            //JsonDeserializer jsonDeserializer = new JsonDeserializer();
            if (jsonDeserializer == null)
                jsonDeserializer = new PetaJsonDeserializer();

            if(xmlDeserializer == null)
                xmlDeserializer = new XmlDeserializer();

            // register default handlers
            ContentHandler.Add("application/json", jsonDeserializer);
            ContentHandler.Add("application/xml", xmlDeserializer);
            ContentHandler.Add("text/json", jsonDeserializer);
            ContentHandler.Add("text/x-json", jsonDeserializer);
            ContentHandler.Add("text/javascript", jsonDeserializer);
            ContentHandler.Add("text/xml", xmlDeserializer);
            ContentHandler.Add("*", xmlDeserializer);
        }

        /// <summary>
        /// Retrieve the handler for the specified MIME content type
        /// </summary>
        /// <param name="contentType">MIME content type to retrieve</param>
        /// <returns>IDeserializer instance</returns>
        protected IDeserializer GetHandler(string contentType)
        {
            if (string.IsNullOrEmpty(contentType) && ContentHandler.ContainsKey("*"))
            {
                return ContentHandler["*"];
            }

            var semicolonIndex = contentType.IndexOf(';');
            if (semicolonIndex > -1) contentType = contentType.Substring(0, semicolonIndex);
            IDeserializer handler = null;
            if (ContentHandler.ContainsKey(contentType))
            {
                handler = ContentHandler[contentType];
            }
            else if (ContentHandler.ContainsKey("*"))
            {
                handler = ContentHandler["*"];
            }

            return handler;
        }

        #endregion 

        #endregion

        /// <summary>
        /// Dispose the request.
        /// </summary>
        public void Dispose()
        {
            if (httpClient != null)
            {
                httpClient.Dispose();
                httpClient = null;
            }
            if (request != null)
            {
                request.Dispose();
                request = null;
            }
            GC.SuppressFinalize(this);
        }
        
    }
}
