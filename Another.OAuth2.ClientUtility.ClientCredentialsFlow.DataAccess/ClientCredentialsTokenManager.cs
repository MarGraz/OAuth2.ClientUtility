using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Another.OAuth2.ClientUtility.ClientCredentialsFlow.Common.Abstractions;
using Another.OAuth2.ClientUtility.ClientCredentialsFlow.Common.Exceptions;
using Another.OAuth2.ClientUtility.ClientCredentialsFlow.Common.Models;
using Another.OAuth2.ClientUtility.ClientCredentialsFlow.Common.Options;
using Another.OAuth2.ClientUtility.ClientCredentialsFlow.DataAccess.Abstractions;

namespace Another.OAuth2.ClientUtility.ClientCredentialsFlow.DataAccess;

internal sealed class ClientCredentialsTokenManager : IClientCredentialsTokenManager, IDisposable
{
    private readonly string _name;
    private readonly IOptionsMonitor<ClientCredentialsOptions> _clientCredentialsOptions;
    private readonly IClientCredentialsTokenProvider _tokenProvider;
    private readonly IAccessTokenCache _cache;
    private readonly ILogger<ClientCredentialsTokenManager> _logger;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    /// <summary>
    /// Provides a cached access token for a specific OAuth 2.0 client, refreshing it only when required
    /// </summary>
    public ClientCredentialsTokenManager(
        string name,
        IOptionsMonitor<ClientCredentialsOptions> clientCredentialsOptions,
        IClientCredentialsTokenProvider tokenProvider,
        IAccessTokenCache cache,
        ILogger<ClientCredentialsTokenManager> logger)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        _clientCredentialsOptions = clientCredentialsOptions ?? throw new ArgumentNullException(nameof(clientCredentialsOptions));
        _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves a valid access token for the configured client, reusing any cached token that has not yet expired
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the retrieval operation</param>
    /// <returns>The access token value</returns>
    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        AccessToken? cached = await _cache.GetAsync(_name, cancellationToken).ConfigureAwait(false);
        if (cached != null && !cached.IsExpired(now))
        {
            _logger.LogTrace("Reusing cached access token for client {ClientName}.", _name);
            return cached.Value;
        }

        // Access the semaphore to ensure only one refresh operation is on going
        await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // We do a second fetch from the cache, because between the first check and the moment we get the SemaphoreSlim, another thread may have already renewed and cached the token
            now = DateTimeOffset.UtcNow;
            cached = await _cache.GetAsync(_name, cancellationToken).ConfigureAwait(false);
            if (cached != null && !cached.IsExpired(now))
            {
                _logger.LogTrace("Reusing cached access token for client {ClientName} after waiting.", _name);
                return cached.Value;
            }

            // Token is missing or expired, need to refresh
            ClientCredentialsOptions options = _clientCredentialsOptions.Get(_name);

            _logger.LogDebug("Access token for client {ClientName} missing or expired. Requesting new.", _name);
            AccessToken accessToken = await _tokenProvider
                .RequestTokenAsync(options, cancellationToken)
                .ConfigureAwait(false);

            if (accessToken.IsExpired(DateTimeOffset.UtcNow))
            {
                throw new OAuthClientException("Token endpoint returned an already expired access token.");
            }

            // Cache the newly retrieved token
            await _cache.SetAsync(_name, accessToken, cancellationToken).ConfigureAwait(false);
            return accessToken.Value;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    public void Dispose() => _refreshLock.Dispose();
}