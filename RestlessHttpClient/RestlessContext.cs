using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Nulands.Restless.Extensions;

namespace Nulands.Restless
{
    public class RestlessContext
    {
        #region Static part

        private static RestlessContext _default = new RestlessContext();

        /// <summary>
        /// Creates and returns a concrete HiDriveRequest instance of the given type.
        /// If wanted then the Bearer access token header is set for the new request 
        /// from HiDriveContext.Default.Token.AccessToken.
        /// After request creation the HiDriveContext.Cancellation is pointed to the cancellation token of the 
        /// newly created request.
        /// Usage:
        /// 
        /// FileCopyRequest request = HiDriveContext.GetXY();
        /// 
        /// Usage of concrete Files api is easier and shorter.
        /// 
        /// FileCopyRequest request = Files.Copy();
        /// 
        /// </summary>
        /// <typeparam name="T">The concrete type of the wanted HiDriveRequest. Must have default constructor. </typeparam>
        /// <typeparam name="U">The HiDriveRequest generic parameter type. </typeparam>
        /// <param name="setAccessToken">When true and HiDriveContext.HasAccessToken() then bearer access token header is set for the new request.</param>
        /// <returns>The new HiDriveRequest of type T.</returns>
        public static T Get<T>(string accessToken = "", bool setDefaultAccessToken = true)
            where T : RestRequest, new()
        {
            T request = new T();
            if (!String.IsNullOrEmpty(accessToken))
                request.Bearer(accessToken);
           // else if (setDefaultAccessToken && String.IsNullOrEmpty(accessToken) && HiDriveContext.HasAccessToken())
           //     request.Bearer(HiDriveContext.Default.Token.AccessToken);

            RestlessContext.Cancellation = request.CancellationToken;
            return request;
        }

        /// <summary>
        /// Check if a HiDriveContext.Default.Token.AccessToken is set.
        /// </summary>
        /// <returns>True if set, false otherwise.</returns>
        public static bool HasAccessToken()
        {
            return false;
            //return Default.Token != null && !String.IsNullOrEmpty(Default.Token.AccessToken);
        }

        /// <summary>
        /// Get/Set the default HiDriveContext instance.
        /// Use like 
        ///     HiDriveContext.Default.Token = token; 
        /// for example.
        /// </summary>
        public static RestlessContext Default
        {
            get
            {
                return _default;
            }
            set
            {
                _default = value;
            }
        }

        /// <summary>
        /// Get/Set HiDriveContext.Default.Token.ClientId.
        /// </summary>
        public static string DefClientId
        {
            get
            {
                return "";
                //return HiDriveContext.Default == null || HiDriveContext.Default.Token == null ? "" : HiDriveContext.Default.Token.ClientId;
            }
            set { /*_default.Token.ClientId = value;*/ }
        }

        /// <summary>
        /// Get/Set default client secret token.
        /// </summary>
        public static string DefClientSecret
        {
            get
            {
                return _default.ClientSecret;
            }
            set { _default.ClientSecret = value; }
        }

        /// <summary>
        /// Get/Set HiDriveContext.Default.Token.AccessToken.
        /// </summary>
        public static string AccessToken
        {
            get
            {
                return "";
                //return HiDriveContext.Default == null || HiDriveContext.Default.Token == null ? "" : HiDriveContext.Default.Token.AccessToken;
            }
            set { /*_default.Token.AccessToken = value;*/ }
        }

        /// <summary>
        /// Get/Set HiDriveContext.Default.Token.RefreshToken.
        /// </summary>
        public static string RefreshToken
        {
            get
            {
                return "";
                //return HiDriveContext.Default == null || HiDriveContext.Default.Token == null ? "" : HiDriveContext.Default.Token.RefreshToken;
            }
            set { /*_default.Token.RefreshToken = value; */}
        }

        /// <summary>
        /// TODO: Make a List<CancellationToken> out of it.
        /// TODO: Or better a Dictionary with some identifier for each added CancellationToken?!
        /// </summary>
        public static CancellationToken Cancellation { get; set; }

        /// <summary>
        /// Get/Set the default authorization code to request a first HiDrive token.
        /// </summary>
        public static string DefAuthCode
        {
            get
            {
                return _default.AuthCode;
            }
            set { _default.AuthCode = value; }
        }

        #endregion

        #region class implementation

        public CancellationToken CancellationToken { get; set; }

        public string ClientSecret { get; set; }

        /// <summary>
        /// HiDrive strato api authorization code.
        /// </summary>
        public string AuthCode { get; set; }

        #endregion

    }
}
