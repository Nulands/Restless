using System;
using System.Threading;

namespace Nulands.Restless.OAuth
{
    public class TokenItem
    {
        public ReaderWriterLockSlim RwLock { get; private set; }
        public long CreatedTicks { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TokenEndpoint { get; set; }
        public OAuthToken Token { get; set; }

        public TokenItem()
        {
            RwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        }
    }
}
