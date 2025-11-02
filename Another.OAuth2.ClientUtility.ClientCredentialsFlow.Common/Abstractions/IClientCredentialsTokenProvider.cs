using Another.OAuth2.ClientUtility.ClientCredentialsFlow.Common.Models;
using Another.OAuth2.ClientUtility.ClientCredentialsFlow.Common.Options;

namespace Another.OAuth2.ClientUtility.ClientCredentialsFlow.Common.Abstractions;

/// <summary>
/// Represents a component capable of requesting OAuth 2.0 access tokens using client credentials
/// </summary>
public interface IClientCredentialsTokenProvider
{
    /// <summary>
    /// Requests a new access token from the configured authorization server
    /// </summary>
    /// <param name="options">Options describing the client and token endpoint</param>
    /// <param name="cancellationToken">Token used to cancel the request</param>
    /// <returns>The retrieved access token</returns>
    Task<AccessToken> RequestTokenAsync(ClientCredentialsOptions options, CancellationToken cancellationToken);
}