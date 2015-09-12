using System;
using System.IO;
using System.Threading.Tasks;

#if UNIVERSAL
using Windows.Foundation;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using Windows.Storage.Streams;
using Windows.Storage;
#else
using System.Net.Http;
#endif

namespace Nulands.Restless.Extensions
{

    public static class RestRequestExtensionsUpload
    {
        #region Upload file binary with StreamContent
        public static async Task<RestResponse<IVoid>> UploadFileBinary(
            this RestRequest request,
            string localPath,
            string contentType,
            HttpClient client = null,
            Action<RestResponse<IVoid>> successAction = null,
            Action<RestResponse<IVoid>> errorAction = null)
        {
            return await request.UploadFileBinary<IVoid>(localPath, contentType, client, successAction, errorAction);
        }

        public static async Task<RestResponse<IVoid>> UploadFileBinary(
            this RestRequest request,
#if UNIVERSAL
            IInputStream streamContent,
#else
            Stream streamContent,
#endif
            String contentType,
            HttpClient client = null,
            Action<RestResponse<IVoid>> successAction = null,
            Action<RestResponse<IVoid>> errorAction = null)
        {
            return await request.UploadFileBinary<IVoid>(streamContent, contentType, client, successAction, errorAction);
        }

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
        public static async Task<RestResponse<T>> UploadFileBinary<T>(
            this RestRequest request,
            string localPath,
            string contentType,
            HttpClient client = null,
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            // check if given file exists
            localPath.ThrowIfNotFound(true, "localPath");

            RestResponse<T> result = new RestResponse<T>();
#if UNIVERSAL
            StorageFile storageFile = await StorageFile.GetFileFromPathAsync(localPath);
            using(IInputStream fileStream = await storageFile.OpenSequentialReadAsync())
            {
                result = await request.UploadFileBinary<T>(fileStream, contentType, client, successAction, errorAction);
            }
#else
            using (Stream fileStream = Rest.File.OpenRead(localPath))
            {
                result = await request.UploadFileBinary<T>(fileStream, contentType, client, successAction, errorAction);
            }
#endif
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
        public static async Task<RestResponse<T>> UploadFileBinary<T>(
            this RestRequest request,
#if UNIVERSAL
            IInputStream streamContent,
#else
            Stream streamContent,
#endif
            String contentType,
            HttpClient client = null,
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            return await request.UploadFileBinary<T>(streamContent, contentType, "", true, client, successAction, errorAction);
        }



		// With client id for OAauth tokens from TokenManager


        public static async Task<RestResponse<IVoid>> UploadFileBinary(
            this RestRequest request,
            string localPath,
            string contentType,
            string clientId,
            bool toBase64 = true,
            HttpClient client = null,
            Action<RestResponse<IVoid>> successAction = null,
            Action<RestResponse<IVoid>> errorAction = null)
        {
            return await request.UploadFileBinary<IVoid>(localPath, contentType, clientId, toBase64, client, successAction, errorAction);
        }

        public static async Task<RestResponse<IVoid>> UploadFileBinary(
            this RestRequest request,
#if UNIVERSAL
            IInputStream streamContent,
#else
            Stream streamContent,
#endif
            String contentType,
            string clientId,
            bool toBase64 = true,
            HttpClient client = null,
            Action<RestResponse<IVoid>> successAction = null,
            Action<RestResponse<IVoid>> errorAction = null)
        {
            return await request.UploadFileBinary<IVoid>(streamContent, contentType, clientId, toBase64, client, successAction, errorAction);
        }

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
        public static async Task<RestResponse<T>> UploadFileBinary<T>(
            this RestRequest request,
            string localPath,
            string contentType,
            string clientId,
            bool toBase64 = true,
            HttpClient client = null,
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            // check if given file exists
            localPath.ThrowIfNotFound(true, "localPath");

            RestResponse<T> result = new RestResponse<T>();
#if UNIVERSAL
            StorageFile storageFile = await StorageFile.GetFileFromPathAsync(localPath);
            using(IInputStream fileStream = await storageFile.OpenSequentialReadAsync())
            {
                result = await request.UploadFileBinary<T>(fileStream, contentType, client, successAction, errorAction);
            }
#else
            using (Stream fileStream = Rest.File.OpenRead(localPath))
            {
                result = await request.UploadFileBinary<T>(fileStream, contentType, clientId, toBase64, client, successAction, errorAction);
            }
#endif
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
        public static async Task<RestResponse<T>> UploadFileBinary<T>(
            this RestRequest request,
#if UNIVERSAL
            IInputStream streamContent,
#else
            Stream streamContent,
#endif
            String contentType,
			string clientId, 
			bool toBase64 = true,
            HttpClient client = null,
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            streamContent.ThrowIfNull("fileStream");
            contentType.ThrowIfNullOrEmpty("contentType");

            request.AddStream(streamContent, contentType);

            return await request.buildAndSendRequest<T>(clientId, toBase64, successAction, errorAction, client);
        }

        // ---------------------------------------------------------------------
        

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
        public static async Task<RestResponse<T>> UploadFileFormData<T>(
            this RestRequest request,
            string localPath,
            string contentType,
            HttpClient client = null,
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            return await request.UploadFileFormData<T>(
                localPath, contentType, "", true, client, successAction, errorAction);
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
        public static async Task<RestResponse<T>> UploadFileFormData<T>(
            this RestRequest request,
#if UNIVERSAL
            IInputStream streamContent,
#else
            Stream streamContent,
#endif
            string contentType,
            string localPath,
            HttpClient client = null,
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            return await request.UploadFileFormData<T>(
				streamContent, contentType, localPath, "", true, client, successAction, errorAction);
        }



		// With client id
		
        public static async Task<RestResponse<T>> UploadFileFormData<T>(
            this RestRequest request,
            string localPath,
            string contentType,
            string clientId,
            bool toBase64 = true,
            HttpClient client = null,
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            localPath.ThrowIfNotFound(true, "localPath");
            // contenttype string is checked from call above

            RestResponse<T> result = new RestResponse<T>();
#if UNIVERSAL
            StorageFile file = await StorageFile.GetFileFromPathAsync(localPath);
            using (IInputStream fileStream = await file.OpenSequentialReadAsync())
            {
                result = await request.UploadFileFormData<T>(
					fileStream, contentType, localPath, clientId, toBase64, client, successAction, errorAction);
            }
#else
            using (Stream fileStream = Rest.File.OpenRead(localPath))
            {
                result = await request.UploadFileFormData<T>(
					fileStream, contentType, localPath, clientId, toBase64, client, successAction, errorAction);
            }
#endif
            return result;
        }

        public static async Task<RestResponse<T>> UploadFileFormData<T>(
            this RestRequest request,
#if UNIVERSAL
            IInputStream streamContent,
#else
            Stream streamContent,
#endif
            string contentType,
            string localPath,
			string clientId,
			bool toBase64 = true,
            HttpClient client = null,
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            streamContent.ThrowIfNull("fileStream");
            contentType.ThrowIfNullOrEmpty("contentType");

            // Only check for null or empty, not for existing
            // here its only used for content-disposition
            // the file is should be loaded allready, see fileStream
            localPath.ThrowIfNullOrEmpty("localPath");

            // TODO: create and add (random?) boundary
            request.AddMultipartForm();
            if (request.Params.Count > 0)
                request.AddFormUrl();           // Add form url encoded parameter to request if needed    

            request.AddStream(
                streamContent,
                contentType,
                1024,
                Path.GetFileNameWithoutExtension(localPath),
                Path.GetFileName(localPath));

            return await request.buildAndSendRequest<T>(clientId, toBase64, successAction, errorAction, client);
        }
        #endregion 
    }

}
