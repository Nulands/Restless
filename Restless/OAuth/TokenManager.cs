using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Nulands.Restless.OAuth
{
    /// <summary>
    /// OAuth2 token life time manager class.
    /// </summary>
    public class TokenManager
    {
        public const int REFRESH_DELTA = 120;
        
        ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        Dictionary<string, TokenItem> tokens = new Dictionary<string, TokenItem>();

        /// <summary>
        /// Add all given token items to the manager.
        /// </summary>
        /// <param name="tokens">The token items to be added.</param>
        public void Load(params TokenItem[] tokens)
        {
            Write(() => tokens.ForEach(t => this.tokens[t.ClientId] = t));
        }

        public void Save(Action<IEnumerable<TokenItem>> saveAction)
        {
            Read(() =>saveAction(tokens.Values));
        }

        public void Save(Action<TokenItem> saveAction)
        {
            Read(() => tokens.Values.ForEach(t => saveAction(t)));
        }

        /// <summary>
        /// Add a token to the token manager.
        /// </summary>
        /// <remarks>
        /// For each added token a <see cref="TokenItem"/> is created.
        /// The token item CreatedTicks is set to DateTime.Now.Ticks and is used
        /// in the Get functions to check if a tokens needs a refresh before it is returned.
        /// Client id and secret are needed to refresh the access token.
        /// </remarks>
        /// <param name="clientId">The client id this token referes to.</param>
        /// <param name="clientSecret">The client secret this token referes to.</param>
        /// <param name="tokenEndpoint">The token endpoint where to refresh the access token.</param>
        /// <param name="token">The token that is added to the manager.</param>
        public void Add(string clientId, string clientSecret, string tokenEndpoint, OAuthToken token)
        {
            if (!String.IsNullOrEmpty(clientId))
            {
                TokenItem item = new TokenItem()
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    TokenEndpoint = tokenEndpoint,
                    Token = token,
                    CreatedTicks = DateTime.Now.Ticks
                };
                Write(() => tokens[clientId] = item);
            }
        }

        /// <summary>
        /// Retreive a token for a given client id.
        /// </summary>
        /// <remarks>
        /// If the OAuth access token is already expired it is refreshed before returned.
        /// </remarks>
        /// <param name="clientId">The client id of the token that will be returned.</param>
        /// <returns>The OAuthToken or null if not found.</returns>
        public async Task<OAuthToken> Get(string clientId)
        {
            TokenItem token = null;
            OAuthToken result = null;
            Read(() => tokens.TryGetValue(clientId, out token));

            if (token != null)
            {
                await Read(token.RwLock, () => Refresh(token));  // get new access token if needed
                result = token.Token;
            }
            return result;
        }

        /// <summary>
        /// Retreive a token for a given client id.
        /// </summary>
        /// <remarks>
        /// If the OAuth access token is already expired it is refreshed before returned.
        /// </remarks>
        /// <param name="clientId">The client id of the token that will be returned.</param>
        /// <returns>The OAuthToken or null if not found.</returns>
        public async Task<T> Get<T>(string clientId)
            where T : OAuthToken
        {
            return (await Get(clientId)) as T;
        }

        private async Task Refresh(TokenItem item)
        {
            var elapsedTime = new TimeSpan(DateTime.Now.Ticks - item.CreatedTicks);
            if (item.Token.ExpiresIn.HasValue && elapsedTime.TotalSeconds >= item.Token.ExpiresIn - REFRESH_DELTA)
            {
                var tokenResponse = await Rest.OAuth.RefreshAccessToken(item.TokenEndpoint, item.Token.RefreshToken, item.ClientId, item.ClientSecret);
                if(tokenResponse.HasData)
                {
                    item.Token.AccessToken = tokenResponse.Data.AccessToken;
                    item.Token.ExpiresIn = tokenResponse.Data.ExpiresIn;
                    item.Token.Scope = tokenResponse.Data.Scope;
                }
            }
        }

        public bool Remove(string clientId)
        {
            bool result = false;
            if(!String.IsNullOrEmpty(clientId))
                Write(() => result = tokens.Remove(clientId));
            return result;
        }

        #region private read wirte inside rwlock helper methods

        async Task Read(ReaderWriterLockSlim rwLock, Func<Task> action)
        {
            rwLock.EnterReadLock();
            try
            {
                await action();
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }

        async Task Write(ReaderWriterLockSlim rwLock, Func<Task> action)
        {
            rwLock.EnterWriteLock();
            try
            {
                await action();
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        void Read(ReaderWriterLockSlim rwLock, Action action)
        {
            rwLock.EnterReadLock();
            try
            {
                action();
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }

        void Write(ReaderWriterLockSlim rwLock, Action action)
        {
            rwLock.EnterWriteLock();
            try
            {
                action();
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        // Thread locking helper
        void Read(Action action)
        {
            Read(rwLock, action);
        }

        void Write(Action action)
        {
            Write(rwLock, action);
        }

        async Task Read(Func<Task> action)
        {
            await Read(rwLock, action);
        }

        async Task Write(Func<Task> action)
        {
            await Write(rwLock, action);
        }

        #endregion

    }
}
