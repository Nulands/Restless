using System;

namespace Nulands.Restless.OAuth
{
    /// <summary>
    /// RFC 6749 5.1 Successful Response
    /// </summary>
    public class OAuthToken
    {
        /// <summary>
        /// The access token string.
        /// </summary>
        /// <remarks>
        /// Required.
        /// </remarks>
        public string AccessToken { get; set; }

        /// <summary>
        /// The token type string.
        /// </summary>
        /// <remarks>
        /// Required.
        /// </remarks>
        public string TokenType { get; set; }

        /// <summary>
        /// The lifetime in seconds of the access token.
        /// </summary>
        /// <remarks>
        /// Recommended
        /// </remarks>
        public long? ExpiresIn { get; set; }

        // Optional.
        public string Scope { get; set; }
        public string RefreshToken { get; set; }
        public string State { get; set; }

        // Optional Basic authentication
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
