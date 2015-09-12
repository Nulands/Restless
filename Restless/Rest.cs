using System;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
#if UNIVERSAL
using Windows.Web.Http;
#else
using System.Net.Http;
#endif

using Nulands.Restless.Extensions;
using Nulands.Restless.OAuth;
using Nulands.Restless.Util;


namespace Nulands.Restless
{
    public static class Rest
    {
        public static bool Bootstrap()
        {
            bool result = true;
            try
            {
                // Try to get the static System.IO.File class.
                // If it is present the try to get the needed functions
                var type = Type.GetType("System.IO.File");
                if(type != null)
                {
                    var openRead = type.GetFuncStatic<string, Stream>("OpenRead");
                    if (openRead != null)
                        Rest.File.OpenRead = openRead;

                    var openWrite = type.GetFuncStatic<string, Stream>("OpenWrite");
                    if (openWrite != null)
                        Rest.File.OpenWrite = openWrite;

                    var fileExists = type.GetFuncStatic<string, bool>("Exists");
                    if (fileExists != null)
                        Rest.File.Exists = fileExists;

                    var readAllLines = type.GetFuncStatic<string, System.Text.Encoding, string[]>("ReadAllLines");
                    if (readAllLines != null)
                        Rest.File.ReadAllLines = readAllLines;
                }

                var ioDirectoryClass = Type.GetType("System.IO.Directory");
                if (ioDirectoryClass != null)
                {
                    var directoryExists = ioDirectoryClass.GetFuncStatic<string, bool>("Exists");
                    if (directoryExists != null)
                        Rest.Directory.Exists = directoryExists;
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public static class File
        {
            public static Func<string, Stream> OpenRead = null;
            public static Func<string, Stream> OpenWrite = null;
            public static Func<string, bool> Exists = null;
            public static Func<string, System.Text.Encoding, string[]> ReadAllLines = null;
        }

        public static class Directory
        {
            public static Func<string, bool> Exists = null;
        }

        public static readonly TokenManager TokenManager = new TokenManager();

        static CancellationTokenSource Cancellation = new CancellationTokenSource();
        
        public static string ClientId { get; set; }


        public static RestRequest Get(string url, HttpClient client = null)
        {
            return new RestRequest(null, client).Get(url);
        }

        public static RestRequest Post(string url, HttpClient client = null)
        {
            return new RestRequest(null, client).Post(url);
        }

        public static RestRequest Put(string url, HttpClient client = null)
        {
            return new RestRequest(null, client).Put(url);
        }

        public static RestRequest Delete(string url, HttpClient client = null)
        {
            return new RestRequest(null, client).Delete(url);
        }

        #region Get a RestRequest, and set access tokens if needed

        public static T Request<T>(string accessToken = "", bool toBase64 = true, string tokenType = "Bearer")
            where T : RestRequest, new()
        {
            T request = new T();

            if (!String.IsNullOrEmpty(accessToken))
                request.Bearer(accessToken, tokenType, toBase64);

            request.CancelToken(Cancellation.Token);

            return request;
        }

        public static RestRequest Request(string accessToken = "", bool toBase64 = true, string tokenType = "Bearer")
        {
            return Request<RestRequest>(accessToken, toBase64, tokenType);
        }

        public static async Task<T> Request<T>(string clientId = "", bool toBase64 = true)
            where T : RestRequest, new()
        {
            T request = new T();
            if (String.IsNullOrEmpty(clientId))
                clientId = ClientId;    // Set to "default" ClientId

            var token = await TokenManager.Get(clientId);

            if (token != null && !String.IsNullOrEmpty(token.AccessToken))
                request.Bearer(token.AccessToken, String.IsNullOrEmpty(token.TokenType) ? "Bearer" : token.TokenType, toBase64);
            request.CancelToken(Cancellation.Token);
            return request;
        }

        public static async Task<RestRequest> Request(string clientId, bool toBase64 = true)
        {
            return await Request<RestRequest>(clientId, toBase64);
        }

        #endregion

        #region public static OAuth class

        public static class OAuth
        {
            public static Action<string> StartAuthorization { get; set; }

            public static Action<string> PromptForAuthCode { get; set; }


            #region Get authorizatin urls

            public static async Task<RestResponse<IVoid>> CodeAuthorizationUrl(
                string authorizationUrl,
                string clientId,
                string redirect_uri = "",
                string scope = "",
                string state = "")
            {
                authorizationUrl.ThrowIfNullOrEmpty("An authorization endpoint url is needed.");
                clientId.ThrowIfNullOrEmpty("A client id is needed to start the authorization.");

                RestRequest request = new RestRequest().
                    Url(authorizationUrl).
                    Param("response_type", OAuth2.RESPONSE_TYPE_CODE, ParameterType.FormUrlEncoded).
                    Param("client_id", clientId, ParameterType.FormUrlEncoded);

                AddFormUrlParamIfNotEmpty(request, "redirect_uri", redirect_uri);
                AddFormUrlParamIfNotEmpty(request, "scope", System.Net.WebUtility.UrlEncode(scope));
                AddFormUrlParamIfNotEmpty(request, "state", state);
                return await request.GetRestResponseAsync();

                /*For example, the authorization server redirects the user-agent by
                   sending the following HTTP response:

                     HTTP/1.1 302 Found
                     Location: https://client.example.com/cb?code=SplxlOBeZQQYbYS6WxSbIA
                   &state=xyz*/
            }


            /// <summary>
            /// RFC 6749 4.1.3. Access Token Request
            /// </summary>
            /// <param name="authParameter"></param>
            /// <param name="authEndpoint"></param>
            public static string GetAuthorizationUrl(
                string authorizationUrl,
                string responseType,
                string clientId,
                string redirect_uri = "",
                string scope = "",
                string state = ""
                )
            {
                authorizationUrl.ThrowIfNullOrEmpty("A redirect uri is needed to start the authorization.");
                clientId.ThrowIfNullOrEmpty("A client id is needed to start to authorization");

                string authUrl = authorizationUrl + "?" +
                    "response_type=" + responseType + "&" +
                    "client_id=" + clientId;

                authUrl = AddIfNotEmpty(authUrl, "redirect_uri", redirect_uri);
                authUrl = AddIfNotEmpty(authUrl, "scope", System.Net.WebUtility.UrlEncode(scope));
                authUrl = AddIfNotEmpty(authUrl, "state", state);
                //authUrl = AddIfNotEmpty(authUrl, "login_hint", authParameter.LoginHint);
                //authUrl = AddIfNotEmpty(authUrl, "include_granted_scopes", authParameter.IncludeGrantedScopes.ToString().ToLower());

                //Process.Start(authUrl);
                return authUrl;
            }

            #endregion

            #region Token from authorization code, password, credentials

            // 4.1.3. Access Token Request
            public static async Task<RestResponse<T>> TokenFromAuthorizationCode<T>(
                string tokenEndpoint,
                string code,
                string clientId,
                string clientSecret = "",
                string redirectUri = "",
                string userName = "",
                string password = "",
                bool addToDefaultTokenManager = true)
                where T: OAuthToken
            {
                tokenEndpoint.ThrowIfNullOrEmpty();
                code.ThrowIfNullOrEmpty("An authorization code is needed to get the first refresh token.");
                clientId.ThrowIfNullOrEmpty("A client id code is needed to get the first refresh token.");

                RestRequest request = new RestRequest().
                    Url(tokenEndpoint).
                    Post().
                    Param("code", code).
                    Param("client_id", clientId).
                    Param("grant_type", OAuth2.GRANT_TYPE_AUTH_CODE);

                AddNonEmptyParam(request, "redirect_uri", redirectUri);
                AddNonEmptyParam(request, "client_secret", clientSecret);
                AddBasicAuthIfGiven(request, userName, password);

                var result = await request.Fetch<T>();
                if (result.HasData && addToDefaultTokenManager)
                    TokenManager.Add(clientId, clientSecret, tokenEndpoint, result.Data);
                return result;
            }

            // UNTESTED
            /*
            // 4.3. Resource Owner Password Credentials Grant
            public static async Task<RestResponse<OAuthToken>> TokenFromPassword(
                string tokenEndpoint,
                string userName,
                string password,
                string scope = "")
            {
                tokenEndpoint.ThrowIfNullOrEmpty();
                userName.ThrowIfNullOrEmpty();
                password.ThrowIfNullOrEmpty();

                RestRequest request = new RestRequest().
                    Url(tokenEndpoint).
                    Post().
                    Param("grant_type", OAuth2.GRANT_TYPE_PASSWORD).
                    Param("username", userName).
                    Param("password", password);

                AddNonEmptyParam(request, "scope", scope);
                AddBasicAuthIfGiven(request, userName, password);

                return await request.Fetch<OAuthToken>();
            }

            // 4.4. Client Credentials Grant
            public async Task<RestResponse<OAuthToken>> TokenFromCredentials(
                string tokenEndpoint,
                string userName,
                string password,
                string scope = "")
            {
                tokenEndpoint.ThrowIfNullOrEmpty();
                userName.ThrowIfNullOrEmpty();
                password.ThrowIfNullOrEmpty();

                RestRequest request = new RestRequest().
                    Url(tokenEndpoint).
                    Post().
                    Param("grant_type", OAuth2.GRANT_TYPE_PASSWORD);
                AddNonEmptyParam(request, "scope", scope);

                AddBasicAuthIfGiven(request, userName, password);

                return await request.Fetch<OAuthToken>();
            }
            */
            #endregion

            #region Refresh access token

            /// <summary>
            /// RFC 6749 6. Refreshing an Access Token
            /// </summary>
            /// <param name="refreshToken"></param>
            /// <param name="clientId"></param>
            /// 
            /// <param name="clientSecret"></param>
            /// <param name="scope"></param>
            /// <param name="tokenEndpoint"></param>
            /// <returns></returns>
            public static async Task<RestResponse<OAuthToken>> RefreshAccessToken(
                string tokenEndpoint,
                string refreshToken,
                string clientId = "",
                string clientSecret = "",
                string scope = "",
                string userName = "",
                string password = "")
            {
                tokenEndpoint.ThrowIfNullOrEmpty();
                refreshToken.ThrowIfNullOrEmpty("A refresh token is needed to get a new access token.");

                RestRequest request = new RestRequest().
                    Url(tokenEndpoint).
                    Post().
                    Param("refresh_token", refreshToken).
                    Param("grant_type", OAuth2.GRANT_TYPE_REFRESH_TOKEN);

                AddNonEmptyParam(request, "client_id", clientId);
                AddNonEmptyParam(request, "client_secret", clientSecret);
                AddNonEmptyParam(request, "scope", scope);
                AddBasicAuthIfGiven(request, userName, password);

                var result =  await request.Fetch<OAuthToken>();
                if (result.HasData)
                    TokenManager.Add(clientId, clientSecret, tokenEndpoint, result.Data);
                return result;
            }

            #endregion

            #region Private helper

            private static void AddBasicAuthIfGiven(RestRequest request, string userName, string password)
            {
                if (!String.IsNullOrEmpty(userName) && !String.IsNullOrEmpty(password))
                    request.Basic(userName, password);
            }

            private static void AddNonEmptyParam(RestRequest request, string name, string value)
            {
                if (!String.IsNullOrEmpty(value))
                    request.Param(name, value);
            }

            private static void AddFormUrlParamIfNotEmpty(RestRequest request, string name, string value)
            {
                if (!String.IsNullOrEmpty(value))
                    request.Param(name, value, ParameterType.FormUrlEncoded);
            }
            private static string AddIfNotEmpty(string current, string name, string value)
            {
                if (!string.IsNullOrEmpty(value))
                    current += "&" + name + "=" + value;
                return current;
            }

            #endregion

        }

        #endregion

    }
}
