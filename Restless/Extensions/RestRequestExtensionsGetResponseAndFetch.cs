using System;
using System.Threading.Tasks;

#if UNIVERSAL
using Windows.Foundation;
using Windows.Web.Http;
using Windows.Storage.Streams;
using Windows.Storage;
#else
using System.Net.Http;
#endif

namespace Nulands.Restless.Extensions
{
    public static class RestRequestExtensionsGetResponseAndFetch
    {
        #region Get HttpWebResponse or RestResponse async

        /// <summary>
        /// Sends the request and return the raw HttpResponseMessage.
        /// </summary>
        /// <returns>Task containing the HttpResponseMessage.</returns>
        public static async Task<HttpResponseMessage> GetResponseAsync<T>(this T request)
            where T : RestRequest
        {
            return await request.GetResponseAsync("");
        }

        /// <summary>
        /// Sends the request and returns a RestResponse with generic type IVoid.
        /// </summary>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>A Task containing the RestRespone. There will be no deserialized data, but the RestResponse.Response 
        /// (HttpResponseMessage) will be set.</returns>
        public static async Task<RestResponse<IVoid>> GetRestResponseAsync<T>(
            this T request,
            Action<RestResponse<IVoid>> successAction = null,
            Action<RestResponse<IVoid>> errorAction = null)
            where T : RestRequest
        {
            return await request.GetRestResponseAsync<T>("", false, successAction, errorAction);
        }

        public static async Task<RestResponse<IVoid>> GetRestResponseAsync<T>(
            this T request, string clientId, bool toBase64 = true,
            Action<RestResponse<IVoid>> successAction = null,
            Action<RestResponse<IVoid>> errorAction = null)
            where T : RestRequest
        {
            request.HandleFormUrlParameter();
            return await request.buildAndSendRequest<IVoid>(clientId, toBase64);
        }


        public static async Task<RestResponse<IVoid>> GetRestResponseAsync<T>(
            this T request, HttpClient client,
            Action<RestResponse<IVoid>> succ = null,
            Action<RestResponse<IVoid>> err = null) where T : RestRequest
        {
            return await request.GetRestResponseAsync(client, "", false, succ, err);
        }

        public static async Task<RestResponse<IVoid>> GetRestResponseAsync<T>(
            this T request, HttpClient client,
            string clientId, bool toBase64 = true,
            Action<RestResponse<IVoid>> succ = null,
            Action<RestResponse<IVoid>> err = null) where T : RestRequest
        {
            request.HandleFormUrlParameter();
            return await request.buildAndSendRequest<IVoid>(clientId, toBase64, succ, err, client);
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
        public static async Task<RestResponse<IVoid>> Fetch(
            this RestRequest request,
            Action<RestResponse<IVoid>> successAction = null,
            Action<RestResponse<IVoid>> errorAction = null)
        {
            return await request.Fetch("", false, successAction, errorAction);
        }

        public static async Task<RestResponse<IVoid>> Fetch(
            this RestRequest request, HttpClient client,
            Action<RestResponse<IVoid>> successAction = null,
            Action<RestResponse<IVoid>> errorAction = null)
        {
            return await request.Fetch(client, "", false, successAction, errorAction);
        }

        /// <summary>
        /// Sends the request and returns the RestResponse containing deserialized data 
        /// from the HttpResponseMessage.Content if T is not IVoid.
        /// </summary>
        /// <typeparam name="T">The type of the deserialized data. Set to IVoid if no deserialization is wanted.</typeparam>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>A taks containing the RestResponse with the deserialized data if T is not IVoid and no error occured.</returns>
        public static async Task<RestResponse<T>> Fetch<T>(
            this RestRequest request,
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            return await request.Fetch<T>("", false, successAction, errorAction);
        }


        public static async Task<RestResponse<T>> Fetch<T>(
            this RestRequest request, HttpClient client,
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            return await request.Fetch<T>(client, "", false, successAction, errorAction);
        }

        // With client id ------------------------------------------------


        /// <summary>
        /// Sends the request and returns the RestResponse containing deserialized data 
        /// from the HttpResponseMessage.Content if T is not IVoid.
        /// </summary>
        /// <typeparam name="T">The type of the deserialized data. Set to IVoid if no deserialization is wanted.</typeparam>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>A taks containing the RestResponse with the deserialized data if T is not IVoid and no error occured.</returns>
        public static async Task<RestResponse<IVoid>> Fetch(
            this RestRequest request,
            string clientId, bool toBase64 = true,
            Action<RestResponse<IVoid>> successAction = null,
            Action<RestResponse<IVoid>> errorAction = null)
        {
            request.HandleFormUrlParameter();
            return await request.buildAndSendRequest<IVoid>(clientId, toBase64, successAction, errorAction);
        }

        public static async Task<RestResponse<IVoid>> Fetch(
            this RestRequest request, HttpClient client,
            string clientId, bool toBase64 = true,
            Action<RestResponse<IVoid>> successAction = null,
            Action<RestResponse<IVoid>> errorAction = null)
        {
            request.HandleFormUrlParameter();
            return await request.buildAndSendRequest<IVoid>(clientId, toBase64, successAction, errorAction, client);
        }

        /// <summary>
        /// Sends the request and returns the RestResponse containing deserialized data 
        /// from the HttpResponseMessage.Content if T is not IVoid.
        /// </summary>
        /// <typeparam name="T">The type of the deserialized data. Set to IVoid if no deserialization is wanted.</typeparam>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>A taks containing the RestResponse with the deserialized data if T is not IVoid and no error occured.</returns>
        public static async Task<RestResponse<T>> Fetch<T>(
            this RestRequest request,
            string clientId, bool toBase64 = true,
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            request.HandleFormUrlParameter();
            return await request.buildAndSendRequest<T>(clientId, toBase64, successAction, errorAction);
        }

        public static async Task<RestResponse<T>> Fetch<T>(
            this RestRequest request, HttpClient client,
            string clientId, bool toBase64 = true,
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            request.HandleFormUrlParameter();
            return await request.buildAndSendRequest<T>(clientId, toBase64, successAction, errorAction, client);
        }

        #endregion
    }
}
