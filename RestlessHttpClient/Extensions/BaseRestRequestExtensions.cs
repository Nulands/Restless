using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;

namespace Nulands.Restless.Extensions
{
    /*
    public static class BaseRestRequestExtensions
    {
        #region Set request methods GET, HEAD, POST, PUT ...

        /// <summary>
        /// Sets the HttpMethod given by string.
        /// </summary>
        /// <param name="method">The HttpMethod string. For example "GET".</param>
        /// <returns>this.</returns>
        protected T Method<T>(this T request, string method)
            where T : BaseRestRequest
        {
            request.Request.Method = new HttpMethod(method);
            return request;
        }

        /// <summary>
        /// Set the HttpMethod to GET.
        /// </summary>
        /// <returns>this.</returns>
        protected BaseRestRequest Get<T>(this T request)
            where T : BaseRestRequest
        {
            request.Request.Method = HttpMethod.Get;
            return request;
        }

        /// <summary>
        /// Set the HttpMethod to HEAD.
        /// </summary>
        /// <returns>this.</returns>
        protected BaseRestRequest Head<T>(this T request)
            where T : BaseRestRequest
        {
            request.Request.Method = new HttpMethod("HEAD");
            return request;
        }

        /// <summary>
        /// Set the HttpMethod to POST.
        /// </summary>
        /// <returns>this.</returns>
        protected BaseRestRequest Post<T>(this T request)
            where T : BaseRestRequest
        {
            request.Request.Method = new HttpMethod("POST");
            return request;
        }

        /// <summary>
        /// Set the HttpMethod to PUT.
        /// </summary>
        /// <returns>this.</returns>
        protected BaseRestRequest Put<T>(this T request)
            where T : BaseRestRequest
        {
            request.Request.Method = new HttpMethod("PUT");
            return request;
        }

        /// <summary>
        /// Set the HttpMethod to DELETE.
        /// </summary>
        /// <returns>this.</returns>
        protected BaseRestRequest Delete<T>(this T request)
            where T : BaseRestRequest
        {
            request.Request.Method = new HttpMethod("DELETE");
            return request;
        }

        /// <summary>
        /// Set the HttpMethod to TRACE.
        /// </summary>
        /// <returns>this.</returns>
        protected BaseRestRequest Trace<T>(this T request)
            where T : BaseRestRequest
        {
            request.Request.Method = new HttpMethod("TRACE");
            return request;
        }

        /// <summary>
        /// Set the HttpMethod to CONNECT.
        /// </summary>
        /// <returns>this.</returns>
        protected BaseRestRequest Connect<T>(this T request)
            where T : BaseRestRequest
        {
            request.Request.Method = new HttpMethod("CONNECT");
            return request;
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
        public T AddContent<T>(this T request, HttpContent content, string name = "", string fileName = "")
            where T : BaseRestRequest
        {
            content.ThrowIfNull("content");

            // If content is a multipart already then add it as sub content to the multipart.

            if (request.request.Content is MultipartContent)
                (request.request.Content as MultipartContent).Add(content);
            else if (request.request.Content is MultipartFormDataContent)
            {
                // For MultipartFormDataContent name and fileName must be set, so chech them first.
                name.ThrowIfNullOrEmpty("name");
                fileName.ThrowIfNullOrEmpty("fileName");
                (request.request.Content as MultipartFormDataContent).Add(content, name, fileName);
            }
            else
                request.request.Content = content;
            return request;
        }

        /// <summary>
        /// Sets the underlying HttpContent to null.
        /// </summary>
        /// <returns>this.</returns>
        public T ClearContent<T>(this T request)
            where T : BaseRestRequest
        {
            request.request.Content = null;
            return request;
        }

        /// <summary>
        /// Adds a ByteArrayContent to the request.
        /// </summary>
        /// <param name="buffer">The buffer containing data.</param>
        /// <param name="name">A name is needed if underlying HttpContent is MultipartFormDataContent. (for example multiple file uploads)</param>
        /// <param name="fileName">A file name is needed if underlying HttpContent is MultipartFormDataContent.</param>
        /// <returns>this</returns>
        public T AddByteArray<T>(this T request, byte[] buffer, string name = "", string fileName = "")
            where T : BaseRestRequest
        {
            buffer.ThrowIfNullOrEmpty("buffer");
            return request.AddContent(new ByteArrayContent(buffer), name, fileName);
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
        public T AddFormUrl<T>(this T request, params string[] kvPairs)
            where T : BaseRestRequest
        {
            List<KeyValuePair<string, string>> keyValues = new List<KeyValuePair<string, string>>();

            if (kvPairs == null || kvPairs.Length == 0)
            {
                foreach (var element in request.param)
                {
                    foreach (var value in element.Value) // we can have the parameter with element.Key set multiple times.
                        keyValues.Add(new KeyValuePair<string, string>(element.Key, value.ToString()));
                }
            }
            else
            {
                kvPairs.ThrowIf(pairs => pairs.Length % 2 != 0, "kvPairs. No value for every name given.");

                for (int i = 0; i < kvPairs.Length; i += 2)
                    keyValues.Add(new KeyValuePair<string, string>(kvPairs[i], kvPairs[i + 1]));
            }
            return request.AddContent(new FormUrlEncodedContent(keyValues));
        }

        /// <summary>
        /// Adds a MultipartContent to the request.
        /// </summary>
        /// <param name="subtype">The sub type if needed.</param>
        /// <param name="boundary">The boundary if needed.</param>
        /// <returns>this.</returns>
        public T AddMultipart<T>(this T request, string subtype = "", string boundary = "")
            where T : BaseRestRequest
        {
            return request.AddContent(new MultipartContent(subtype, boundary));
        }

        /// <summary>
        /// Adds a MultipartFormDataContent to the request.
        /// </summary>
        /// <param name="boundary">The boundary if needed.</param>
        /// <returns>this.</returns>
        public T AddMultipartForm<T>(this T request, string boundary = "")
            where T : BaseRestRequest
        {
            return request.AddContent(new MultipartFormDataContent(boundary));
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
        public T AddStream<T>(this T request, Stream stream, string mediaType, int buffersize = 1024, string name = "", string fileName = "")
            where T : BaseRestRequest
        {
            stream.ThrowIfNull("stream");
            mediaType.ThrowIfNullOrEmpty("mediaType");
            buffersize.ThrowIf(b => b <= 0, "bufferSize");

            var content = new StreamContent(stream, buffersize);
            content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            return request.AddContent(content, name, fileName);
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
        public T AddString<T>(this T request, string content, Encoding encoding, string mediaType, string name = "", string fileName = "")
            where T : BaseRestRequest
        {
            content.ThrowIfNullOrEmpty("content");
            encoding.ThrowIfNull("encoding");
            mediaType.ThrowIfNullOrEmpty("mediaType");

            return request.AddContent(new StringContent(content, encoding, mediaType), name, fileName);
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
        public T AddJson<T>(this T request, object obj, string name = "", string fileName = "")
            where T : BaseRestRequest
        {
            obj.ThrowIfNull("BaseRestRequest");
            var serializer = new Serializers.JsonSerializer();
            var jsonContent = serializer.Serialize(obj);
            jsonContent.ThrowIfNullOrEmpty("BaseRestRequest", "jsonStr");
            // .net default encoding is UTF-8
            if (!String.IsNullOrEmpty(jsonContent))
                request.AddContent(new StringContent(jsonContent, Encoding.Default, serializer.ContentType), name, fileName);
            return request;
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
        protected T AddXml<T>(this T request, object obj, string name = "", string fileName = "")
            where T : BaseRestRequest
        {
            obj.ThrowIfNull("BaseRestRequest");
            var serializer = new Serializers.DotNetXmlSerializer();
            var xmlContent = serializer.Serialize(obj);
            xmlContent.ThrowIfNullOrEmpty("BaseRestRequest", "xmlContent");
            // .net default encoding is UTF-8
            if (!String.IsNullOrEmpty(xmlContent))
                request.AddContent(new StringContent(xmlContent, Encoding.Default, serializer.ContentType), name, fileName);
            return request;
        }

        #endregion 

        #region Url, CancellationToken, parameters and headers

        /// <summary>
        /// Set the CancellationToken for this request.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>this.</returns>
        protected BaseRestRequest CancelToken<T>(this T request, CancellationToken token)
            where T: BaseRestRequest
        {
            token.ThrowIfNull("token");
            request.cancellation = token;
            return request;
        }

        /// <summary>
        /// Sets the URL string for this request.
        /// </summary>
        /// <param name="url">The URL string.</param>
        /// <returns>this.</returns>
        protected BaseRestRequest Url<T>(this T request, string url)
            where T: BaseRestRequest
        {
            url.ThrowIfNull("url");
            request.url = url;
            return request;
        }

        /// <summary>
        /// Sets the URL format parameter for this request.
        /// A test String.Format is done to verify the input objects.
        /// </summary>
        /// <param name="objects">The format parameter objects.</param>
        /// <returns>this.</returns>
        protected BaseRestRequest UrlFormat<T>(this T request, params object[] objects)
            where T: BaseRestRequest
        {
            objects.ThrowIfNullOrEmpty("objects");
            // Do a test format only to see if everything is ok otherwise it will throw an exception.
            String.Format(request.url, objects);
            // Do not save the formated test url only the format parameter.
            // Url will be build before sending the request.
            request.urlFormatParams = objects;
            return request;
        }

        /// <summary>
        /// Map an action over the underlying HttpRequestMessage.
        /// Can be used to set "exotic" things, that are not exposed by the BaseRestRequest.
        /// Usage: request.RequestAction(r => r.Content = ...);
        /// </summary>
        /// <param name="action">An action that takes a HttpRequestMessage as argument.</param>
        /// <returns>this.</returns>
        protected BaseRestRequest RequestAction<T>(this T request, Action<HttpRequestMessage> action)
            where T: BaseRestRequest
        {
            action.ThrowIfNull("action");
            action(request.request);
            return request;
        }

        /// <summary>
        /// Map an action over the underlying HttpClient.
        /// Can be used to set "exotic" things, that are not exposed by the BaseRestRequest.
        /// Usage: request.ClientAction(c => c.Timeout = ...);
        /// </summary>
        /// <param name="action">An action that takes a HttpClient as argument.</param>
        /// <returns>this.</returns>
        protected BaseRestRequest ClientAction<T>(this T request, Action<HttpClient> action)
            where T: BaseRestRequest
        {
            action.ThrowIfNull("action");
            action(request.client);
            return request;
        }

        /// <summary>
        /// Adds a Http Basic authorization header to the request. 
        /// The result string is Base64 encoded internally.
        /// </summary>
        /// <param name="username">The user name.</param>
        /// <param name="password">The user password.</param>
        /// <returns>this.</returns>
        protected BaseRestRequest Basic<T>(this T request, string username, string password)
            where T : BaseRestRequest
        {
            username.ThrowIfNullOrEmpty("username");
            password.ThrowIfNullOrEmpty("password");

            string base64authStr = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));
            request.request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64authStr);
            return request;
        }

        /// <summary>
        /// Adds a Http Bearer authorization header to the request.
        /// The given token string is Base64 encoded internally.
        /// </summary>
        /// <param name="token">The token string.</param>
        /// <returns>this.</returns>
        protected BaseRestRequest Bearer<T>(this T request, string token)
            where T : BaseRestRequest
        {
            token.ThrowIfNullOrEmpty("token");
            string base64AccessToken = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(token));
            request.request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", base64AccessToken);
            return request;
        }

        /// <summary>
        /// Adds a parameter to the request. Can be a Query, FormUrlEncoded or Url parameter.
        /// If a value for the given name is already set, the old parameter value is overwritten silently.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value (should be convertible to string).</param>
        /// <param name="type">The ParameterType.</param>
        /// <returns>this.</returns>
        protected BaseRestRequest Param<T>(this T request, string name, object value, ParameterType type)
            where T : BaseRestRequest
        {
            name.ThrowIfNullOrEmpty("name");
            value.ThrowIfNullOrToStrEmpty("value");

            if (type == ParameterType.FormUrlEncoded || type == ParameterType.NotSpecified)
                request.Param(name, value);
            //param[name] = value;
            else if (type == ParameterType.Query)
                request.query_params[name] = value;
            else // type == ParameterType.Url
            {
                if (request.url.Contains("{" + name + "}"))
                    request.url_params[name] = value;
                else
                    throw new ArgumentException("BaseRestRequest - ParameterType.Url - Url does not contain a parameter : " + name);
            }

            return request;
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
        protected BaseRestRequest UrlParam<T>(this T request, string name, object value)
            where T : BaseRestRequest
        {
            name.ThrowIfNullOrEmpty("name");
            value.ThrowIfNullOrToStrEmpty("value");
            request.url.ThrowIfNullOrEmpty("url - cannot set UrlParameter. Url is null or empty.");

            if (request.url.Contains("{" + name + "}"))
                request.url_params[name] = value;
            else
                throw new ArgumentException("BaseRestRequest - UrlParam - Url does not contain a parameter : " + name);

            return request;
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
        protected BaseRestRequest Param<T>(this T request, string name, object value, bool addAsMultiple = false)
            where T : BaseRestRequest
        {
            name.ThrowIfNullOrEmpty("name");
            value.ThrowIfNullOrToStrEmpty("value");

            List<object> paramValues = null;
            if (request.param.TryGetValue(name, out paramValues))
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
                request.param[name] = paramValues;
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
        protected BaseRestRequest QParam<T>(this T request, string name, object value)
            where T : BaseRestRequest
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
        protected BaseRestRequest Header<T>(this T request, string name, string value)
            where T : BaseRestRequest
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
        protected BaseRestRequest Header<T>(this T request, string name, IEnumerable<string> values)
            where T : BaseRestRequest
        {
            name.ThrowIfNullOrEmpty("name");
            values.ThrowIfNullOrEmpty("values");

            request.request.Headers.Add(name, values);
            return request;
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
        protected async Task<RestResponse<T>> UploadFileBinary<T>(
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
        protected async Task<RestResponse<T>> UploadFileBinary<T>(
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
        protected async Task<RestResponse<T>> UploadFileFormData<T>(
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
        protected async Task<RestResponse<T>> UploadFileFormData<T>(
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
    }*/

}
