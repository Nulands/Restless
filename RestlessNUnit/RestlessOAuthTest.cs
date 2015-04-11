using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Nulands.Restless;
using Nulands.Restless.OAuth;

namespace RestlessNUnit
{
    [TestFixture]
    public class RestlessOAuthTest
    {
        #region Dropbox

        public static readonly string DROP_BOX_CLIENT_ID = "5rb3l2ksoc5526x";
        public static readonly string DROP_BOX_CLIENT_SECRET = "xverljep84cfemp";
        public const string DROP_BOX_TOKEN_URL = "https://api.dropbox.com/1/oauth2/token";
        public const string DROP_BOX_TEST_AUTH_CODE = "";

        #endregion

        #region Google drive

        public static readonly string GOOGLE_DRIVE_REFRESH_TOKEN = "1/w_CcYscFQQqzM7jdsu1C0CU57z1pOQw6x3xbfK5RGxM";
        public static readonly string GOOGLE_DRIVE_APPLICATION_NAME = "OwnThing";

        public static readonly string GOOGLE_DRIVE_API_KEY = "AIzaSyAaWnPT4S7WdDKTwovePaYMys20-THL48o";
        public static readonly string GOOGLE_DRIVE_CLIENT_ID = "352776068885.apps.googleusercontent.com";
        public static readonly string GOOGLE_DRIVE_CLIENT_SECRET = "qRIGL62qHhoka6C7iP1-_Dkl";

        #endregion

        #region Strato HiDrive

        public static readonly string HIDRIVE_CLIENT_ID = "f8d2012f85329e613111408274c71c23";
        public static readonly string HIDRIVE_CLIENT_SECRET = "a1236076d3c8e9fd0f2dfa64cbe336e2";
        public const string HIDRIVE_TEST_SCOPE = "admin,rw";

        public static readonly string HIDRIVE_TEST_AUTH_CODE = "YFYNlUpA";
        public static readonly string HIDRIVE_REFRESHTOKEN = "rt-xa9pt7swxwxdvbekt8m9";
        public const string STRATO_OAUTH_REFRESH_TOKEN_URL = "https://hidrive.strato.com/oauth2/token";

        #endregion

        [Test]
        public void TestHiDriveOAuth()
        {
            var authUrl = Rest.OAuth.GetAuthorizationUrl("https://hidrive.strato.com/oauth2/authorize", OAuth2.RESPONSE_TYPE_CODE, HIDRIVE_CLIENT_ID, null, HIDRIVE_TEST_SCOPE);
            System.Diagnostics.Process.Start(authUrl);

            string authCode = Nulands.Nubilu.UI.NativeUtil.PromptForAuthCode();

            RestResponse<OAuthToken> response = Rest.OAuth.TokenFromAuthorizationCode<OAuthToken>(
                STRATO_OAUTH_REFRESH_TOKEN_URL,
                authCode,
                HIDRIVE_CLIENT_ID,
                HIDRIVE_CLIENT_SECRET).Result;

            TestTokenResponse(response);

            RestResponse<OAuthToken> refreshResponse = Rest.OAuth.RefreshAccessToken(
                STRATO_OAUTH_REFRESH_TOKEN_URL, 
                response.Data.RefreshToken, 
                HIDRIVE_CLIENT_ID, 
                HIDRIVE_CLIENT_SECRET).Result;

            TestTokenResponse(refreshResponse);
        }

        [Test]
        public void TestHiDriveRefreshAccessToken()
        {
            RestResponse<OAuthToken> refreshResponse = Rest.OAuth.RefreshAccessToken(
                STRATO_OAUTH_REFRESH_TOKEN_URL,
                HIDRIVE_REFRESHTOKEN,
                HIDRIVE_CLIENT_ID,
                HIDRIVE_CLIENT_SECRET).Result;

            TestTokenResponse(refreshResponse);
        }

        [Test]
        public void TestDropBoxOAuth()
        {
            RestResponse<OAuthToken> response = Rest.OAuth.TokenFromAuthorizationCode<OAuthToken>(
                DROP_BOX_TOKEN_URL,
                DROP_BOX_TEST_AUTH_CODE,
                DROP_BOX_CLIENT_ID,
                DROP_BOX_CLIENT_SECRET).Result;

            TestTokenResponse(response);

            RestResponse<OAuthToken> refreshResponse = Rest.OAuth.RefreshAccessToken(
                DROP_BOX_TOKEN_URL,
                response.Data.RefreshToken,
                DROP_BOX_CLIENT_ID,
                DROP_BOX_CLIENT_SECRET).Result;

            TestTokenResponse(refreshResponse);
        }

        private void TestTokenResponse(RestResponse<OAuthToken> response)
        {
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Data);
            Assert.IsNotNull(response.Data.AccessToken);
            Assert.IsNotNull(response.Data.ExpiresIn);
            Assert.IsNotNull(response.Data.TokenType);
        }
    }
}
