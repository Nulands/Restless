using System;

namespace Nulands.Restless.OAuth
{
    // RFC 6749 - 2.1 Client Types
    public enum ClientType
    {
        /// <summary>
        /// Clients capable of maintaining the confidentiality of their
        /// credentials (e.g., client implemented on a secure server with
        /// restricted access to the client credentials), or capable of secure
        /// client authentication using other means.
        /// </summary>
        Confidential,

        /// <summary>
        /// Clients incapable of maintaining the confidentiality of their
        /// credentials (e.g., clients executing on the device used by the
        /// resource owner, such as an installed native application or a web
        /// browser-based application), and incapable of secure client
        /// authentication via any other means.
        /// </summary>
        Public
    }

    // RFC 6749 - 2.1 Client Types - Client Profiles
    public enum ClientProfile
    {
        Web,
        UserAgentBased,
        Native
    }

    public enum ClientAuthenticatin
    {
        Basic,
        IdAndSecret
    }

    public enum ResponseType
    {
        Code,
        Token
    }

    public class OAuth2
    {
        public const string RESPONSE_TYPE_CODE = "code";
        public const string REDIRECT_OOB = "urn:ietf:wg:oauth:2.0:oob";
        public const string REDIRECT_OOB_AUTO = "urn:ietf:wg:oauth:2.0:oob:auto";
        public const string REDIRECT_LOCALHOST = "http://localhost";

        public const string GRANT_TYPE_AUTH_CODE = "authorization_code";
        public const string GRANT_TYPE_REFRESH_TOKEN = "refresh_token";
        public const string GRANT_TYPE_PASSWORD = "password";
        public const string GRANT_TYPE_CLIENT_CREDENTIALS = "client_credentials";
    }
}
