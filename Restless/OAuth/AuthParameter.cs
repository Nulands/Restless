using System;
using System.Collections.Generic;

namespace Nulands.Restless.OAuth
{
    public class AuthorizationRequest
    {
        public string Endpoint { get; set; }
        public string ResponseType { get; set; }
        public string ClientId { get; set; }
        public string RedirectUri { get; set; }
        public string Scope { get; set; }
        public string State { get; set; }

        public List<Tuple<string, object>> CustomParameter { get; set; }
        
        // Goolge specific
        //public string LoginHint { get; set; }
        //public bool IncludeGrantedScopes { get; set; }

        public AuthorizationRequest()
        {
            CustomParameter = new List<Tuple<string, object>>();
        }

        public static AuthParameterGenerator Generate()
        {
            return new AuthParameterGenerator();
        }
    }

    public class AuthParameterGenerator
    {
        public AuthorizationRequest AuthParameter { get; set; }

        public AuthParameterGenerator()
        {
            AuthParameter = new AuthorizationRequest();
            AuthParameter.ResponseType = OAuth2.RESPONSE_TYPE_CODE;
        }

        public AuthParameterGenerator ResponseType(string responseType)
        {
            AuthParameter.ResponseType = responseType;
            return this;
        }

        public AuthParameterGenerator ClientId(string clientId)
        {
            AuthParameter.ClientId = clientId;
            return this;
        }

        public AuthParameterGenerator RedirectUri(string redirectUri)
        {
            AuthParameter.RedirectUri = redirectUri;
            return this;
        }

        public AuthParameterGenerator Scope(string scope)
        {
            AuthParameter.Scope = scope;
            return this;
        }

        public AuthParameterGenerator State(string state)
        {
            AuthParameter.State = state;
            return this;
        }
    }
}
