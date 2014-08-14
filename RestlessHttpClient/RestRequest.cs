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
using Nulands.Restless.Deserializers;
using System.Threading.Tasks;
using System.Text;

using Nulands.Restless.Extensions;

namespace Nulands.Restless
{
    /*
    /// <summary>
    /// Wrapper for BaseRestRequest that makes all base methods and propertys public.
    /// See BaseRestRequest.
    /// </summary>
    /// <remarks>See BaseRestRequest</remarks>
    public sealed class RestRequest : BaseRestRequest
    {

        #region CancellationToken, HttpClient and HttpRequestMessage propertys

        /// <summary>
        /// HttpClient property.
        /// </summary>
        public new HttpClient HttpClient
        {
            get { return base.HttpClient; }
            set { base.HttpClient = value; }
        }

        /// <summary>
        /// HttpRequestMessage property.
        /// </summary>
        public new HttpRequestMessage Request
        {
            get { return base.Request; }
            set { base.Request = value; }
        }

        /// <summary>
        /// The CancellationToken for this request.
        /// </summary>
        public new CancellationToken CancellationToken
        {
            get { return cancellation; }
            set { cancellation = value; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="defaultRequest">The initial request message, or null if not used.</param>
        /// <param name="httpClient">The initial http client, or null if not used.</param>
        public RestRequest(HttpRequestMessage defaultRequest = null, HttpClient httpClient = null)
            : base(defaultRequest, httpClient)
        {
        }

        #region  Set request methods GET, HEAD, POST, PUT ...

        public new RestRequest Method(string method)
        {
            return base.Method(method) as RestRequest;
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
        public new RestRequest AddContent(HttpContent content, string name = "", string fileName = "")
        {
            return base.AddContent(content, name, fileName) as RestRequest;
        }

        /// <summary>
        /// Sets the underlying HttpContent to null.
        /// </summary>
        /// <returns>this.</returns>
        public new RestRequest ClearContent()
        {
            return base.ClearContent() as RestRequest;
        }

        /// <summary>
        /// Adds a ByteArrayContent to the request.
        /// </summary>
        /// <param name="buffer">The buffer containing data.</param>
        /// <param name="name">A name is needed if underlying HttpContent is MultipartFormDataContent. (for example multiple file uploads)</param>
        /// <param name="fileName">A file name is needed if underlying HttpContent is MultipartFormDataContent.</param>
        /// <returns>this</returns>
        public new RestRequest AddByteArray(byte[] buffer, string name = "", string fileName = "")
        {
            return base.AddByteArray(buffer, name, fileName) as RestRequest;
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
        public new RestRequest AddFormUrl(params string[] kvPairs)
        {
            return base.AddFormUrl(kvPairs) as RestRequest;
        }

        /// <summary>
        /// Adds a MultipartContent to the request.
        /// </summary>
        /// <param name="subtype">The sub type if needed.</param>
        /// <param name="boundary">The boundary if needed.</param>
        /// <returns>this.</returns>
        public new RestRequest AddMultipart(string subtype = "", string boundary = "")
        {
            return base.AddMultipart(subtype, boundary) as RestRequest;
        }

        /// <summary>
        /// Adds a MultipartFormDataContent to the request.
        /// </summary>
        /// <param name="boundary">The boundary if needed.</param>
        /// <returns>this.</returns>
        public new RestRequest AddMultipartForm(string boundary = "")
        {
            return base.AddMultipartForm(boundary) as RestRequest;
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
        public new RestRequest AddStream(Stream stream, string mediaType, int buffersize, string name = "", string fileName = "")
        {
            return base.AddStream(stream, mediaType, buffersize, name, fileName) as RestRequest;
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
        public new RestRequest AddString(string content, Encoding encoding, string mediaType, string name = "", string fileName = "")
        {
            return base.AddString(content, encoding, mediaType, name, fileName) as RestRequest;
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
        protected RestRequest AddJson(object obj, string name = "", string fileName = "")
        {
            return base.AddJson(obj, name, fileName) as RestRequest;
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
        protected RestRequest AddXml(object obj, string name = "", string fileName = "")
        {
            return base.AddXml(obj, name, fileName) as RestRequest;
        }

        #endregion     

        #region Url, CancellationToken, parameters and headers

        /// <summary>
        /// Set the CancellationToken for this request.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>this.</returns>
        public new RestRequest CancelToken(CancellationToken token)
        {
            return base.CancelToken(token) as RestRequest;
        }

        /// <summary>
        /// Sets the URL string for this request.
        /// </summary>
        /// <param name="url">The URL string.</param>
        /// <returns>this.</returns>
        public new RestRequest Url(string url)
        {
            return base.Url(url) as RestRequest;
        }

        /// <summary>
        /// Sets the URL format parameter for this request.
        /// A test String.Format is done to verify the input objects.
        /// </summary>
        /// <param name="objects">The format parameter objects.</param>
        /// <returns>this.</returns>
        public new RestRequest UrlFormat(params object[] objects)
        {
            return base.UrlFormat(objects) as RestRequest;
        }

        /// <summary>
        /// Map an action over the underlying HttpRequestMessage.
        /// Can be used to set "exotic" things, that are not exposed by the BaseRestRequest.
        /// Usage: request.RequestAction(r => r.Content = ...);
        /// </summary>
        /// <param name="action">An action that takes a HttpRequestMessage as argument.</param>
        /// <returns>this.</returns>
        public new RestRequest RequestAction(Action<HttpRequestMessage> action)
        {
            return base.RequestAction(action) as RestRequest;
        }

        /// <summary>
        /// Map an action over the underlying HttpClient.
        /// Can be used to set "exotic" things, that are not exposed by the BaseRestRequest.
        /// Usage: request.ClientAction(c => c.Timeout = ...);
        /// </summary>
        /// <param name="action">An action that takes a HttpClient as argument.</param>
        /// <returns>this.</returns>
        public new RestRequest ClientAction(Action<HttpClient> action)
        {
            return base.ClientAction(action) as RestRequest;
        }

        /// <summary>
        /// Adds a Http Basic authorization header to the request. 
        /// The result string is Base64 encoded internally.
        /// </summary>
        /// <param name="username">The user name.</param>
        /// <param name="password">The user password.</param>
        /// <returns>this.</returns>
        public new RestRequest Basic(string username, string password)
        {
            return base.Basic(username, password) as RestRequest;
        }

        public new RestRequest Basic(string authentication)
        {
            return base.Basic(authentication) as RestRequest;
        }

        /// <summary>
        /// Adds a Http Bearer authorization header to the request.
        /// The given token string is Base64 encoded internally.
        /// </summary>
        /// <param name="token">The token string.</param>
        /// <returns>this.</returns>
        public new RestRequest Bearer(string token, string tokenType = "Bearer")
        {
            return base.Bearer(token, tokenType) as RestRequest;
        }

        /// <summary>
        /// Adds a parameter to the request. Can be a Query, FormUrlEncoded or Url parameter.
        /// If a value for the given name is already set, the old parameter value is overwritten silently.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value (should be convertible to string).</param>
        /// <param name="type">The ParameterType.</param>
        /// <returns>this.</returns>
        public new RestRequest Param(string name, object value, ParameterType type)
        {
            return base.Param(name, value, type) as RestRequest;
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
        public new RestRequest UrlParam(string name, object value)
        {
            return base.UrlParam(name, value) as RestRequest;
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
        public new RestRequest Param(string name, object value, bool addAsMultiple = false)
        {
            return base.Param(name, value, addAsMultiple) as RestRequest;
        }

        /// <summary>
        /// Adds a query parameter (?name=value) to the request.
        /// The parameter-value pair is added to the URL before sending the request.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value (should be convertible to string).</param>
        /// <returns>this.</returns>
        public new RestRequest QParam(string name, object value)
        {
            return base.QParam(name, value) as RestRequest;
        }

        /// <summary>
        /// Adds a header with a single value to the request.
        /// </summary>
        /// <param name="name">The header name.</param>
        /// <param name="value">The header value (should be convertible to string).</param>
        /// <returns>this.</returns>
        public new RestRequest Header(string name, string value)
        {
            return base.Header(name, value) as RestRequest;
        }

        /// <summary>
        /// Adds a header with multiple values to the request.
        /// </summary>
        /// <param name="name">The header name.</param>
        /// <param name="values">The header values (should be convertible to string).</param>
        /// <returns>this.</returns>
        public new RestRequest Header(string name, IEnumerable<string> values)
        {
            return base.Header(name, values) as RestRequest;
        }

        #endregion 

        #region Get HttpWebResponse async

        /// <summary>
        /// Sends the request and return the raw HttpResponseMessage.
        /// </summary>
        /// <returns>Task containing the HttpResponseMessage.</returns>
        public new async Task<HttpResponseMessage> GetResponseAsync()
        {
            return await base.GetResponseAsync();
        }

        /// <summary>
        /// Sends the request and returns a RestResponse with generic type IVoid.
        /// </summary>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>A Task containing the RestRespone. There will be no deserialized data, but the RestResponse.Response 
        /// (HttpResponseMessage) will be set.</returns>
        public new async Task<RestResponse<IVoid>> GetRestResponseAsync(
            Action<RestResponse<IVoid>> successAction = null,
            Action<RestResponse<IVoid>> errorAction = null)
        {
            return await base.GetRestResponseAsync(successAction, errorAction);
        }
        #endregion

        #region Fetch response

        /// <summary>
        /// Sends the request and returns the RestResponse containing deserialized data 
        /// from the HttpResponseMessage.Content if T is not IVoid.
        /// </summary>
        /// <typeparam name="T">The type of the deserialized data. Set to IVoid if no deserialization is wanted.</typeparam>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>A taks containing the RestResponse with the deserialized data if T is not IVoid and no error occured.</returns>
        public new async Task<RestResponse<T>> Fetch<T>(
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            return await base.Fetch<T>(successAction, errorAction);
        }

        /// <summary>
        /// Sends the request asynchronous and returns the RestResponse.
        /// </summary>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>A taks containing the RestResponse.</returns>
        public async Task<RestResponse<IVoid>> Fetch(
            Action<RestResponse<IVoid>> successAction = null,
            Action<RestResponse<IVoid>> errorAction = null)
        {
            return await base.Fetch(successAction, errorAction);
        }

        #endregion 

        #region Upload file binary with StreamContent

        /// <summary>
        /// Uploads a binary file using StreamContent.
        /// </summary>
        /// <typeparam name="T">The type of the deserialized data. Set to IVoid if no deserialization is wanted.</typeparam>
        /// <param name="localPath">The path to a file that will be uploaded.</param>
        /// <param name="contentType">The file content type.</param>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>A taks containing the RestResponse with the deserialized data if T is not IVoid and no error occured.</returns>
        public new async Task<RestResponse<T>> UploadFileBinary<T>(
            string localPath, string contentType,
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            return await base.UploadFileBinary<T>(localPath, contentType, successAction, errorAction);
        }


        /// <summary>
        /// Uploads a binary file stream using StreamContent.
        /// </summary>
        /// <param name="localPath">The path to the file that will be uploaded.</param>
        /// <param name="contentType">The file content type.</param>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>A taks containing the RestResponse.</returns>
        public async Task<RestResponse<IVoid>> UploadFileBinary(
            string localPath, string contentType,
            Action<RestResponse<IVoid>> successAction = null,
            Action<RestResponse<IVoid>> errorAction = null)
        {
            return await base.UploadFileBinary(localPath, contentType, successAction, errorAction);

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
        public new async Task<RestResponse<T>> UploadFileBinary<T>(
            Stream streamContent, string contentType,
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            return await base.UploadFileBinary<T>(streamContent, contentType, successAction, errorAction);
        }


        /// <summary>
        /// Uploads a binary (file) stream using StreamContent.
        /// </summary>
        /// <param name="streamContent">The (file) stream that will be uploaded.</param>
        /// <param name="contentType">The file content type.</param>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>A taks containing the RestResponse.</returns>
        public async Task<RestResponse<IVoid>> UploadFileBinary(
            Stream streamContent, string contentType,
            Action<RestResponse<IVoid>> successAction = null,
            Action<RestResponse<IVoid>> errorAction = null)
        {
            return await base.UploadFileBinary(streamContent, contentType, successAction, errorAction);
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
        public new async Task<RestResponse<T>> UploadFileFormData<T>(
            string localPath, string contentType,
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            return await base.UploadFileFormData<T>(localPath, contentType, successAction, errorAction);
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
        public new async Task<RestResponse<T>> UploadFileFormData<T>(
            Stream streamContent, string contentType, string localPath,
            Action<RestResponse<T>> successAction = null, Action<RestResponse<T>> errorAction = null)
        {
            return await base.UploadFileFormData<T>(streamContent, contentType, localPath, successAction, errorAction);
        }

        /// <summary>
        /// Uploads a binary file stream a MultipartFormDataContent and a (sub) StreamContent.
        /// AddFormUrl() is called before the StreamContent is added to the MultipartFormDataContent.
        /// AddFormUrl() will add all parameter to the request that are added with Param(..).
        /// </summary>
        /// <param name="localPath">The file that will be uploaded.</param>
        /// <param name="contentType">The file content type.</param>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>A taks containing the RestResponse.</returns>
        public async Task<RestResponse<IVoid>> UploadFileFormData(
            string localPath, string contentType,
            Action<RestResponse<IVoid>> successAction = null,
            Action<RestResponse<IVoid>> errorAction = null)
        {
            return await base.UploadFileFormData(localPath, contentType, successAction, errorAction);
        }

        /// <summary>
        /// Uploads a binary (file) stream using a MultipartFormDataContent and a (sub) StreamContent.
        /// AddFormUrl() is called before the StreamContent is added to the MultipartFormDataContent.
        /// AddFormUrl() will add all parameter to the request that are added with Param(..).
        /// </summary>
        /// <param name="streamContent">The (file) stream that will be uploaded.</param>
        /// <param name="contentType">The file content type.</param>
        /// <param name="localPath">The "path" of the (file) stream that will be uploaded.</param>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>A taks containing the RestResponse.</returns>
        public async Task<RestResponse<IVoid>> UploadFileFormData(
            Stream streamContent, string contentType, string localPath,
            Action<RestResponse<IVoid>> successAction = null,
            Action<RestResponse<IVoid>> errorAction = null)
        {
            return await base.UploadFileFormData(streamContent, contentType, localPath, successAction, errorAction);
        }

        #endregion 

    }*/
}
