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

        CancellationTokenSource internalTokenSource = new CancellationTokenSource();
        CancellationToken internalToken;
        internal CancellationToken externalToken;

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
        internal HttpRequestMessage HttpRequest
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
        public RestRequest()
        {
            registerDefaultHandlers();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="defaultRequest">The initial request message, or null if not used.</param>
        /// <param name="httpClient">The initial http client, or null if not used.</param>
        public RestRequest(HttpRequestMessage defaultRequest, HttpClient httpClient = null)
        {
            if (defaultRequest != null)
                request = defaultRequest;
            if (httpClient != null)
                client = httpClient;
            registerDefaultHandlers();
        }

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
                // First format the Url
                if(urlFormatParams != null && urlFormatParams.Length > 0)
                    url = String.Format(url, urlFormatParams);  

                if(url_params != null && url_params.Count > 0)
                {
                    foreach (var item in url_params)
                        url = url.Replace("{" + item.Key + "}", item.Value.ToString());
                }

                request.RequestUri = new Uri(url.CreateRequestUri(query_params, param, request.Method.Method));

#if UNIVERSAL
                var tmp = client.SendRequestAsync(request);
                result.HttpResponse = await tmp.AsTask(CancellationTokenSource.CreateLinkedTokenSource(internalToken, externalToken).Token);
#else
                result.HttpResponse = await client.SendAsync(
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
            if (!(typeof(T).GetTypeInfo().IsAssignableFrom(typeof(IVoid).GetTypeInfo())))
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

        /// <summary>
        /// Dispose the request.
        /// </summary>
        public void Dispose()
        {
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
            GC.SuppressFinalize(this);
        }
        
    }
}
