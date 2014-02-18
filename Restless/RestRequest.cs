/* Copyright 2014 Muraad Nofal
   Contact: muraad.nofal@gmail.com

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Net;
using System.IO;
using Restless.Deserializers;
using Restless.Extensions;
using System.Threading.Tasks;

namespace Restless
{

    public sealed class RestRequest : BaseRestRequest
    {
        public new HttpWebRequest HttpWebRequest
        {
            get { return base.HttpWebRequest; }
            private set { base.HttpWebRequest = value; }
        }
        
        public RestRequest() : base()
        {
        }

        public RestRequest(HttpWebRequest defaultRequest) : base(defaultRequest)
        {
        }

        /// <summary>
        /// Set the URL for this request.
        /// Can be a format string too.
        /// </summary>
        /// <param name="url">The URL string.</param>
        /// <returns>this</returns>
        public new RestRequest Url(string url)
        {
            return base.Url(url) as RestRequest;
        }

        /// <summary>
        /// Formats the url format string.
        /// </summary>
        /// <param name="objects">Format objects.</param>
        /// <returns>this</returns>
        public new RestRequest UrlFormat(params object[] objects)
        {
            return base.UrlFormat(objects) as RestRequest;
        }

        /// <summary>
        /// Sets the method to GET.
        /// </summary>
        /// <returns>this</returns>
        public new RestRequest Get()
        {
            return base.Get() as RestRequest;
        }

        /// <summary>
        /// Sets the method to HEAD.
        /// </summary>
        /// <returns>this</returns>
        public new RestRequest Head()
        {
            return base.Head() as RestRequest;
        }

        /// <summary>
        /// Sets the method to POST.
        /// </summary>
        /// <returns>this</returns>
        public new RestRequest Post()
        {
            return base.Post() as RestRequest;
        }

        /// <summary>
        /// Sets the method to PUT.
        /// </summary>
        /// <returns>this</returns>
        public new RestRequest Put()
        {
            return base.Put() as RestRequest;
        }

        /// <summary>
        /// Sets the method to DELETE.
        /// </summary>
        /// <returns>this</returns>
        public new RestRequest Delete()
        {
            return base.Delete() as RestRequest;
        }

        /// <summary>
        /// Sets the method to TRACE.
        /// </summary>
        /// <returns>this</returns>
        public new RestRequest Trace()
        {
            return base.Trace() as RestRequest;
        }

        /// <summary>
        /// Sets the method to CONNECT.
        /// </summary>
        /// <returns>this</returns>
        public new RestRequest Connect()
        {
            return base.Connect() as RestRequest;
        }

        /// <summary>
        /// Do an action on the underlying HttpWebRequest.
        /// Can be used to set "exotic" things, that are not exposed by the BaseRestRequest.
        /// </summary>
        /// <param name="action">An action that takes a HttpWebRequest as argument.</param>
        /// <returns>this</returns>
        public new RestRequest RequestAction(Action<HttpWebRequest> action)
        {
            return base.RequestAction(action) as RestRequest;
        }

        /// <summary>
        /// Adds client credentials to the HttpWebRequest.
        /// </summary>
        /// <param name="credentials">The credentials</param>
        /// <returns>this.</returns>
        public new RestRequest Credentials(ICredentials credentials)
        {
            return base.Credentials(credentials) as RestRequest;

        }

        /// <summary>
        /// Adds a basic authorization header to the request.
        /// </summary>
        /// <param name="basicAuth">The basic auth value.</param>
        /// <returns>this</returns>
        public new RestRequest Basic(string username, string password)
        {
            return base.Basic(username, password) as RestRequest;
        }

        /// <summary>
        /// Adds a bearer authorization token header to the request.
        /// </summary>
        /// <param name="token">The token. Is Base64 encoded internally.</param>
        /// <returns>this</returns>
        public new RestRequest Bearer(string token)
        {
            return base.Bearer(token) as RestRequest;
        }

        /// <summary>
        /// Adds a parameter to the request.
        /// If method is Get() the parameters will be added as query parameters.
        /// Otherwise they are body parameters like specified with the POST request.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <returns>this.</returns>
        public new RestRequest Param(string name, string value)
        {
            return base.Param(name, value) as RestRequest;
        }

        /// <summary>
        /// Adds a header to the request.
        /// </summary>
        /// <param name="name">Header name.</param>
        /// <param name="value">Header value.</param>
        /// <returns>this</returns>
        public new RestRequest Header(string name, string value)
        {
            return base.Header(name, value) as RestRequest;
        }

        /// <summary>
        /// Call GetResponse() on the underlying HttpWebRequest.
        /// </summary>
        /// <returns>The response.</returns>
        public new HttpWebResponse GetResponse()
        {
            return base.GetResponse();
        }

        public new async Task<HttpWebResponse> GetResponseAsync()
        {
            return await base.GetResponseAsync();
        }

        /// <summary>
        /// Calls GetResponse() and tries to deserialize the result.
        /// </summary>
        /// <typeparam name="T">The type of the deserialized object.</typeparam>
        /// <param name="wantedStatusCode">Only tries to deserialize if response HttpStatus matches.</param>
        /// <returns>The RestResponse. Use Data property to get the deserialized object.</returns>
        public new RestResponse<T> Fetch<T>(HttpStatusCode wantedStatusCode = HttpStatusCode.OK)
        {
            return base.Fetch<T>(wantedStatusCode);
        }

        /// <summary>
        /// Calls GetResponse() and tries to deserialize the result.
        /// </summary>
        /// <typeparam name="T">The type of the deserialized object.</typeparam>
        /// <param name="wantedStatusCode">Only tries to deserialize if response HttpStatus matches.</param>
        /// <returns>The RestResponse. Use Data property to get the deserialized object.</returns>
        public new async Task<RestResponse<T>> FetchAsync<T>(HttpStatusCode wantedStatusCode = HttpStatusCode.OK)
        {
            return await base.FetchAsync<T>(wantedStatusCode);
        }

    }
}
