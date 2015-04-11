using System;


namespace Nulands.Restless.OAuth
{
    /// <summary>
    /// RFC 6749 5.2 Error Response
    /// </summary>
    public class OAuthTokenError
    {
        public string Error { get; set; }
        public string ErrorDescription { get; set; }
        public string ErrorUri { get; set; }
    }
}
