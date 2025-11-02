using Another.OAuth2.ClientUtility.ClientCredentialsFlow.Common.Models;

namespace Another.OAuth2.ClientUtility.ClientCredentialsFlow.DataAccess.Abstractions
{
    /// <summary>
    /// Abstraction for caching access tokens (e.g. in-memory, distributed cache, etc.)
    /// In the future, additional caching implementations may be added (e.g. distributed cache)
    /// </summary>
    public interface IAccessTokenCache
    {
        /// <summary>
        /// get the cached access token for the specified client name, or null if not found
        /// </summary>
        /// <param name="clientName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<AccessToken?> GetAsync(string clientName, CancellationToken cancellationToken = default);

        /// <summary>
        /// set the cached access token for the specified client name
        /// </summary>
        /// <param name="clientName"></param>
        /// <param name="token"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SetAsync(string clientName, AccessToken token, CancellationToken cancellationToken = default);
    }
}
