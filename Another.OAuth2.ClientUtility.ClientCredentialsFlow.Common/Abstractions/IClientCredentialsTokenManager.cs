namespace Another.OAuth2.ClientUtility.ClientCredentialsFlow.Common.Abstractions;

/// <summary>
/// Provides access to OAuth 2.0 access tokens obtained via the client-credentials flow
/// </summary>
public interface IClientCredentialsTokenManager
{
    /// <summary>
    /// Retrieves an access token, refreshing it if the cached value has expired
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the retrieval operation</param>
    /// <returns>The access token value</returns>
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
