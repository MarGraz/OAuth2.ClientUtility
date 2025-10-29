using System.Net.Http.Headers;
using Another.OAuth2.ClientUtility.ClientCredentialsFlow.Common.Abstractions;
using Microsoft.Extensions.Logging;

namespace Another.OAuth2.ClientUtility.ClientCredentialsFlow.BusinessLogic.Http;

// Delegating handler that injects a bearer token obtained via the client-credentials flow into outgoing HTTP requests
public sealed class ClientCredentialsDelegatingHandler : DelegatingHandler
{
    private readonly IClientCredentialsTokenManager _tokenManager;
    private readonly ILogger<ClientCredentialsDelegatingHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientCredentialsDelegatingHandler"/> class
    /// </summary>
    /// <param name="tokenManager">Token manager used to retrieve access tokens</param>
    /// <param name="logger">Logger used for tracing token usage</param>
    public ClientCredentialsDelegatingHandler(
        IClientCredentialsTokenManager tokenManager,
        ILogger<ClientCredentialsDelegatingHandler> logger)
    {
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization is null)
        {
            string token = await _tokenManager.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            _logger.LogTrace("Added bearer token to outgoing HTTP request.");
        }
        else
        {
            _logger.LogTrace("Outgoing HTTP request already contains an Authorization header. Skipping token injection.");
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}