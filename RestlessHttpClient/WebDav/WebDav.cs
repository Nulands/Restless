using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

using Nulands.Restless;
using Nulands.Restless.Extensions;

namespace Nulands.WebDav
{
    public class WebDav : BaseRestRequest
    {
        private const string URI = "{host}/{resource}";

        public static string UserName { get; set; }
        public static string Password { get; set; }
        public static string HostUrl { get; set; }

        #region Set Host, Resource (remote path) and Auth (username and password) methods

        public WebDav Host(string host)
        {
            return Param("host", host, ParameterType.Url) as WebDav;
        }

        public WebDav Resource(string resource)
        {
            return Param("resource", resource, ParameterType.Url) as WebDav;
        }

        public WebDav Auth(string username, string password)
        {
            return Basic(username, password) as WebDav;
        }

        #endregion


        #region Static part, Upload, Download, CreateDir, Delete,

        private static void SetDefaults(WebDav request, string username = "", string password = "")
        {
            request.Url(HostUrl);

            SetAuthIfAvailable(request, username, password);

            if (!String.IsNullOrEmpty(HostUrl))
                request.Host(HostUrl);
        }

        private static void SetAuthIfAvailable(WebDav request, string username, string password)
        {
            if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
                request.Auth(username, password);
            else if (!String.IsNullOrEmpty(UserName) && !String.IsNullOrEmpty(Password))
                request.Auth(UserName, Password);
        }

        public static async Task<RestResponse<IVoid>> Upload(
            string localPath, 
            string remotePath,
            string username = "",
            string password = "")
        {
            WebDav request = new WebDav();

            request.Put();
            // File must exist, throw exception otherwise.
            localPath.ThrowIfNotFound();
            SetDefaults(request, username, password);
            request.Resource(remotePath);
            return await request.UploadFileBinary<IVoid>(localPath, "application/octet-stream");
        }

        public static async Task<HttpResponseMessage> Download(
            string remotePath,
            string username = "",
            string password = "")
        {
            remotePath.ThrowIfNullOrEmpty("remotePath");
            WebDav request = new WebDav();
            request.Get();
            SetDefaults(request, username, password);
            return await request.GetResponseAsync();
        }

        public static async Task<RestResponse<IVoid>> CreateDir(
            string remotePath,
            string username = "",
            string password = "")
        {
            remotePath.ThrowIfNullOrEmpty("remotePath");
            WebDav request = new WebDav();
            request.Method("MKCOL");
            SetDefaults(request, username, password);
            request.Resource(remotePath);
            return await request.Fetch();
        }

        public static async Task<RestResponse<IVoid>> Delete(
            string remotePath,
            string username = "",
            string password = "")
        {
            remotePath.ThrowIfNullOrEmpty("remotePath");
            WebDav request = new WebDav();
            request.Delete();
            SetDefaults(request, username, password);
            request.Resource(remotePath);
            return await request.Fetch();
        }

        #endregion

    }

}
