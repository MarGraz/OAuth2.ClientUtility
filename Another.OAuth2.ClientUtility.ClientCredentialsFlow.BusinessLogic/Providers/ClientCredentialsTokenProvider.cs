using System.Text.Json;
using Microsoft.Extensions.Logging;
using Another.OAuth2.ClientUtility.ClientCredentialsFlow.BusinessLogic.Models;
using Another.OAuth2.ClientUtility.ClientCredentialsFlow.Common.Abstractions;
using Another.OAuth2.ClientUtility.ClientCredentialsFlow.Common.Options;
using Another.OAuth2.ClientUtility.ClientCredentialsFlow.Common.Models;

namespace Another.OAuth2.ClientUtility.ClientCredentialsFlow.BusinessLogic.Providers;

// Requests OAuth 2.0 access tokens from a token endpoint using the client-credentials flow
public sealed class ClientCredentialsTokenProvider : IClientCredentialsTokenProvider
{
    public const string TokenHttpClientName = "OAuth2.ClientUtility.TokenClient";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ClientCredentialsTokenProvider> _logger;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientCredentialsTokenProvider"/> class
    /// </summary>
    /// <param name="httpClientFactory">Factory used to create HTTP clients for token retrieval</param>
    /// <param name="logger">Logger used to report diagnostic information</param>
    public ClientCredentialsTokenProvider(IHttpClientFactory httpClientFactory, ILogger<ClientCredentialsTokenProvider> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AccessToken> RequestTokenAsync(ClientCredentialsOptions options, CancellationToken cancellationToken)
    {
        HttpRequestMessage request = BuildTokenRequest(options);
        HttpClient httpClient = _httpClientFactory.CreateClient(TokenHttpClientName);

        _logger.LogDebug("Requesting OAuth 2.0 token using client credentials flow at {Endpoint}", options.TokenEndpoint);

        HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        Stream contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        TokenEndpointResponse? payload = await JsonSerializer.DeserializeAsync<TokenEndpointResponse>(contentStream, _serializerOptions, cancellationToken).ConfigureAwait(false);

        DateTimeOffset expiresAt = CalculateExpiration(payload.ExpiresIn, options.RefreshBeforeExpiration);
        return new AccessToken(payload.AccessToken, expiresAt);
    }

    #region Private Methods

    /// <summary>
    /// Builds the HTTP request message used to request a token from the token endpoint adding all the possible parameters and headers
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    private static HttpRequestMessage BuildTokenRequest(ClientCredentialsOptions options)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = options.ClientId ?? throw new ArgumentNullException(nameof(options)),
            ["client_secret"] = options.ClientSecret ?? throw new ArgumentNullException(nameof(options))
        };

        if (!string.IsNullOrWhiteSpace(options.Scope))
        {
            parameters["scope"] = options.Scope!;
        }

        if (!string.IsNullOrWhiteSpace(options.Audience))
        {
            parameters["audience"] = options.Audience!;
        }

        foreach (var kvp in options.AdditionalBodyParameters)
        {
            parameters[kvp.Key] = kvp.Value;
        }

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, options.TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(parameters)
        };

        foreach (KeyValuePair<string, string> header in options.AdditionalHeaders)
        {
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return request;
    }

    private static DateTimeOffset CalculateExpiration(int? expiresInSeconds, TimeSpan refreshBeforeExpiration)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        if (expiresInSeconds is null or <= 0)
        {
            return now;
        }

        DateTimeOffset expiration = now.AddSeconds(expiresInSeconds.Value);
        if (refreshBeforeExpiration > TimeSpan.Zero && refreshBeforeExpiration < TimeSpan.FromSeconds(expiresInSeconds.Value))
        {
            expiration = expiration.Subtract(refreshBeforeExpiration);
        }

        return expiration;
    }

    #endregion
}