using Another.OAuth2.ClientUtility.ClientCredentialsFlow.Common.Models;
using Another.OAuth2.ClientUtility.ClientCredentialsFlow.DataAccess.Abstractions;
using System.Collections.Concurrent;

namespace Another.OAuth2.ClientUtility.ClientCredentialsFlow.DataAccess.Caching
{
    internal sealed class InMemoryAccessTokenCache : IAccessTokenCache
    {
        private readonly ConcurrentDictionary<string, AccessToken> _inMemoryStorage = new(StringComparer.Ordinal);

        public Task<AccessToken?> GetAsync(string clientName, CancellationToken cancellationToken = default)
        {
            _inMemoryStorage.TryGetValue(clientName, out var token);
            return Task.FromResult(token);
        }

        public Task SetAsync(string clientName, AccessToken token, CancellationToken cancellationToken = default)
        {
            _inMemoryStorage[clientName] = token;
            return Task.CompletedTask;
        }
    }
}
