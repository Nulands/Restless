using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Security;

using System.Net.Http;
using Nulands.Restless.OAuth;
using Nulands.Restless.Extensions;

namespace Nulands.Restless
{
    /*
    public class TwitterToken : OAuthToken
    {

    }

    public class TwitterClient
    {
        /// <summary>
        /// OAuth Parameters key names to include in the Authorization header.
        /// </summary>
        private static readonly string[] OAuthParametersToIncludeInHeader = new[]
                                                          {
                                                              "oauth_version",
                                                              "oauth_nonce",
                                                              "oauth_timestamp",
                                                              "oauth_signature_method",
                                                              "oauth_consumer_key",
                                                              "oauth_token",
                                                              "oauth_verifier"
                                                              // Leave signature omitted from the list, it is added manually
                                                              // "oauth_signature",
                                                          };

        /// <summary>
        /// Parameters that may appear in the list, but should never be included in the header or the request.
        /// </summary>
        private static readonly string[] SecretParameters = new[]
                                                                {
                                                                    "oauth_consumer_secret",
                                                                    "oauth_token_secret",
                                                                    "oauth_signature"
                                                                };

        public const string ApiUrl = "https://api.twitter.com";

        public const string OAuthAuthorizationUrl       = "/oauth/authorize";
        public const string OAuthAccessTokenUrl         = "/oauth/access_token";
        public const string OAuthRequestTokenUrl        = "/oauth/request_token";
        public const string OAuthInvalidateTokenUrl     = "/oauth/invalidate_token";
        public const string OAuthTokenUrl               = "/oauth/token";

        HttpClient client = new HttpClient();

        public string ClientSecret { get; set; }
        public string ClientId { get; set; }

        public void OAuthAuthenticate(bool forceLogin, string screenName)
        {
            Rest.OAuth.GetAuthorizationUrl()
        }

        public void RequestToken()
        {
            RestRequest request = new RestRequest()
                .Header("oauth_nonce", "")
                .Header("oauth_callback", "")
                .Header("oauth_signature_method", "HMAC-SHA1")
                .Header("oauth_timestamp", "")
                .Header("oauth_consumer_key", "")
                .Header("oauth_signature", "")
                .Header("oauth_version", "");
        }

        /// <summary>
        /// Sets up the OAuth request details.
        /// </summary>
        private void SetupOAuth(Action<Dictionary<string, object>> configurator)
        {
            // We only sign oauth requests
            if (!this.UseOAuth)
            {
                return;
            }
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            configurator(parameters); 
            // Add the OAuth parameters
            parameters["oauth_version"] = "1.0";
            parameters["oauth_nonce"] = GenerateNonce();
            parameters["oauth_timestamp"] = GenerateTimeStamp();
            parameters["oauth_signature_method"] = "HMAC-SHA1";
            parameters["oauth_consumer_key"] = ConsumerKey;
            parameters["oauth_consumer_secret"] = ConsumerSecret;

            if (!string.IsNullOrEmpty(this.Tokens.AccessToken))
            {
                this.Parameters.Add("oauth_token", this.Tokens.AccessToken);
            }

            if (!string.IsNullOrEmpty(this.Tokens.AccessTokenSecret))
            {
                this.Parameters.Add("oauth_token_secret", this.Tokens.AccessTokenSecret);
            }

            string signature = GenerateSignature();

            // Add the signature to the oauth parameters
            this.Parameters.Add("oauth_signature", signature);
        }

        /// <summary>
        /// Generates the signature.
        /// </summary>
        /// <returns></returns>
        public string GenerateSignature()
        {
            IEnumerable<KeyValuePair<string, object>> nonSecretParameters;

            if (Multipart)
            {
                nonSecretParameters = (from p in this.Parameters
                                       where (!SecretParameters.Contains(p.Key) && p.Key.StartsWith("oauth_"))
                                       select p);
            }
            else
            {
                nonSecretParameters = (from p in this.Parameters
                                       where (!SecretParameters.Contains(p.Key))
                                       select p);
            }

            Uri urlForSigning = this.RequestUri;

            // Create the base string. This is the string that will be hashed for the signature.
            string signatureBaseString = string.Format(
                CultureInfo.InvariantCulture,
                "{0}&{1}&{2}",
                this.Verb.ToString().ToUpper(CultureInfo.InvariantCulture),
                UrlEncode(NormalizeUrl(urlForSigning)),
                UrlEncode(nonSecretParameters));

            // Create our hash key (you might say this is a password)
            string key = string.Format(
                CultureInfo.InvariantCulture,
                "{0}&{1}",
                UrlEncode(this.Tokens.ConsumerSecret),
                UrlEncode(this.Tokens.AccessTokenSecret));


            // Generate the hash
            HMACSHA1 hmacsha1 = new HMACSHA1(Encoding.UTF8.GetBytes(key));
            byte[] signatureBytes = hmacsha1.ComputeHash(Encoding.UTF8.GetBytes(signatureBaseString));
            return Convert.ToBase64String(signatureBytes);
        }

        /// <summary>
        /// Generate the timestamp for the signature        
        /// </summary>
        /// <returns>A timestamp value in a string.</returns>
        public static string GenerateTimeStamp()
        {
            // Default implementation of UNIX time of the current UTC time
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds, CultureInfo.CurrentCulture).ToString(CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Generate a nonce
        /// </summary>
        /// <returns>A random number between 123400 and 9999999 in a string.</returns>
        public static string GenerateNonce()
        {
            // Just a simple implementation of a random number between 123400 and 9999999
            return new Random()
                .Next(123400, int.MaxValue)
                .ToString("X", CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Generates the authorization header.
        /// </summary>
        /// <returns>The string value of the HTTP header to be included for OAuth requests.</returns>
        public string GenerateAuthorizationHeader(RestRequest request, string realm)
        {
            StringBuilder authHeaderBuilder = new StringBuilder();
            authHeaderBuilder.AppendFormat("OAuth realm=\"{0}\"", realm);

            var sortedParameters = from p in request.Params
                                   where OAuthParametersToIncludeInHeader.Contains(p.Key)
                                   let value = p.Value.First().As<string>()
                                   orderby p.Key, UrlEncode(value)
                                   select p;

            foreach (var item in sortedParameters)
            {
                authHeaderBuilder.AppendFormat(
                    ",{0}=\"{1}\"",
                    UrlEncode(item.Key),
                    UrlEncode(item.Value.First() as string));
            }

            authHeaderBuilder.AppendFormat(",oauth_signature=\"{0}\"", UrlEncode(request.Params["oauth_signature"].First() as string));

            return authHeaderBuilder.ToString();
        }
    }*/
}
