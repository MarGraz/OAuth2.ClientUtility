using System.Text.Json.Serialization;

namespace Another.OAuth2.ClientUtility.ClientCredentialsFlow.BusinessLogic.Models;

/// <summary>
/// Represents the JSON payload returned by an OAuth 2.0 token endpoint
/// </summary>
internal sealed class TokenEndpointResponse
{
    [JsonPropertyName("access_token")] public string? AccessToken { get; init; }

    [JsonPropertyName("expires_in")] public int? ExpiresIn { get; init; }

    [JsonPropertyName("token_type")] public string? TokenType { get; init; }
}