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
using System.Net;
using System.Net.Http;
using System.IO;
using Restless.Deserializers;
using System.Threading.Tasks;
using System.Text;

namespace Restless
{

    public sealed class RestRequest : BaseRestRequest
    {
        public new HttpClient HttpClient
        {
            get { return base.HttpClient; }
            set { base.HttpClient = value; }
        }

        public new HttpRequestMessage Request
        {
            get { return base.Request; }
            set { base.Request = value; }
        }
        
        /*public RestRequest() : base()
        {
        }*/

        public RestRequest(HttpRequestMessage defaultRequest = null, HttpClient httpClient = null)
            : base(defaultRequest, httpClient)
        {
        }

        #region Set and add content

        public new RestRequest AddContent(HttpContent content, string name = "", string fileName = "")
        {
            return base.AddContent(content, name, fileName) as RestRequest;
        }

        public new RestRequest ClearContent()
        {
            return base.ClearContent() as RestRequest;
        }

        public new RestRequest AddByteArray(byte[] buffer, string name = "", string fileName = "")
        {
            return base.AddByteArray(buffer, name, fileName) as RestRequest;
        }

        public new RestRequest AddFormUrl(params string[] kvPairs)
        {
            return base.AddFormUrl(kvPairs) as RestRequest;
        }

        public new RestRequest AddMultipart(string subtype = "", string boundary = "")
        {
            return base.AddMultipart(subtype, boundary) as RestRequest;
        }

        public new RestRequest AddMultipartForm(string boundary = "")
        {
            return base.AddMultipartForm(boundary) as RestRequest;
        }

        public new RestRequest AddStream(Stream stream, string mediaType, int buffersize, string name = "", string fileName = "")
        {
            return base.AddStream(stream, mediaType, buffersize, name, fileName) as RestRequest;
        }

        public new RestRequest AddString(string content, Encoding encoding, string mediaType, string name = "", string fileName = "")
        {
            return base.AddString(content, encoding, mediaType, name, fileName) as RestRequest;
        }

        #endregion 

        #region Set Http method type

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

        #endregion 

        #region Url, CancellationToken, parameters and headers

        public new RestRequest CancelToken(CancellationToken token)
        {
            return base.CancelToken(token) as RestRequest;
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
        /// Do an action on the underlying HttpClient.
        /// Can be used to set "exotic" things, that are not exposed by the BaseRestRequest.
        /// </summary>
        /// <param name="action">An action that takes a HttpClient as argument.</param>
        /// <returns>this</returns>
        public new RestRequest RequestAction(Action<HttpRequestMessage> action)
        {
            return base.RequestAction(action) as RestRequest;
        }

        public new RestRequest ClientAction(Action<HttpClient> action)
        {
            return base.ClientAction(action) as RestRequest;
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
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="type">The type. Query, FormUrlEncoded or Url.</param>
        /// <returns>this.</returns>
        public new RestRequest Param(string name, object value, ParameterType type)
        {
            return base.Param(name, value, type) as RestRequest;
        }

        /// <summary>
        /// Adds a parameter to the url.
        /// The url should contain "{name}". 
        /// Example: url is www.test.com/{person_name}/{person_age}
        /// then ...UrlParam("person_name", "TestUser").UrlParam("person_age", 42); 
        /// result is www.test.com/TestUser/42
        /// Url is build up only when 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public new RestRequest UrlParam(string name, object value)
        {
            return base.UrlParam(name, value) as RestRequest;
        }

        /// <summary>
        /// Adds a parameter to the request.
        /// If method is Get() the parameters will be added as query parameters.
        /// Otherwise they are body parameters like specified with the POST request.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <returns>this.</returns>
        public new RestRequest Param(string name, object value)
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

        public new RestRequest Header(string name, IEnumerable<string> values)
        {
            return base.Header(name, values) as RestRequest;
        }

        public new RestRequest QParam(string name, object value)
        {
            return base.QParam(name, value) as RestRequest;
        }

        #endregion 

        public new async Task<HttpResponseMessage> GetResponseAsync()
        {
            return await base.GetResponseAsync();
        }

        #region Fetch response 

        public new async Task<RestResponse<T>> Fetch<T>(
            HttpStatusCode wantedStatusCode = HttpStatusCode.OK,
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            return await base.Fetch<T>(wantedStatusCode, successAction, errorAction);
        }

        public async Task<RestResponse<INothing>> Fetch(
            HttpStatusCode wantedStatusCode = HttpStatusCode.OK,
            Action<RestResponse<INothing>> successAction = null,
            Action<RestResponse<INothing>> errorAction = null)
        {
            return await base.Fetch(wantedStatusCode, successAction, errorAction);
        }

        #endregion 

        #region Upload file binary with StreamContent

        public new async Task<RestResponse<T>> UploadFileBinary<T>(
            string localPath, string contentType,
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            return await base.UploadFileBinary<T>(localPath, contentType, successAction, errorAction);
        }


        public async Task<RestResponse<INothing>> UploadFileBinary(
            string localPath, string contentType,
            Action<RestResponse<INothing>> successAction = null,
            Action<RestResponse<INothing>> errorAction = null)
        {
            return await base.UploadFileBinary(localPath, contentType, successAction, errorAction);

        }


        public new async Task<RestResponse<T>> UploadFileBinary<T>(
            Stream fileStream, string contentType,
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            return await base.UploadFileBinary<T>(fileStream, contentType, successAction, errorAction);
        }


        public async Task<RestResponse<INothing>> UploadFileBinary(
            Stream fileStream, string contentType,
            Action<RestResponse<INothing>> successAction = null,
            Action<RestResponse<INothing>> errorAction = null)
        {
            return await base.UploadFileBinary(fileStream, contentType, successAction, errorAction);
        }
        #endregion 

        #region Upload file with multipart content and formData

        public new async Task<RestResponse<T>> UploadFileFormData<T>(
            string localPath, string contentType,
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            return await base.UploadFileFormData<T>(localPath, contentType, successAction, errorAction);
        }

        public new async Task<RestResponse<T>> UploadFileFormData<T>(
            Stream fileStream, string contentType, string localPath,
            Action<RestResponse<T>> successAction = null, Action<RestResponse<T>> errorAction = null)
        {
            return await base.UploadFileFormData<T>(fileStream, contentType, localPath, successAction, errorAction);
        }

        public async Task<RestResponse<INothing>> UploadFileFormData(
            string localPath, string contentType,
            Action<RestResponse<INothing>> successAction = null,
            Action<RestResponse<INothing>> errorAction = null)
        {
            return await base.UploadFileFormData(localPath, contentType, successAction, errorAction);
        }

        public async Task<RestResponse<INothing>> UploadFileFormData(
            Stream fileStream, string contentType, string localPath,
            Action<RestResponse<INothing>> successAction = null,
            Action<RestResponse<INothing>> errorAction = null)
        {
            return await base.UploadFileFormData(fileStream, contentType, localPath, successAction, errorAction);
        }

        #endregion 

    }
}
