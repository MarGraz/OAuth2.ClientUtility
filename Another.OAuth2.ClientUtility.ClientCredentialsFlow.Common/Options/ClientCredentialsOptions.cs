using System.ComponentModel.DataAnnotations;

namespace Another.OAuth2.ClientUtility.ClientCredentialsFlow.Common.Options;

// Represents configuration values required to request an OAuth 2.0 token using the client-credentials flow
public sealed class ClientCredentialsOptions
{
    /// <summary>
    /// Gets or sets the token endpoint used to retrieve access tokens
    /// </summary>
    [Required]
    public required Uri TokenEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the client identifier registered with the authorization server
    /// </summary>
    [Required]
    public required string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the client secret registered with the authorization server
    /// </summary>
    [Required]
    public required string ClientSecret { get; set; }

    /// <summary>
    /// Gets or sets the optional scope value requested from the token endpoint
    /// </summary>
    public string? Scope { get; set; }

    /// <summary>
    /// Gets or sets an optional audience parameter if supported by the authorization server
    /// </summary>
    public string? Audience { get; set; }

    /// <summary>
    /// Gets or sets additional body parameters that will be sent to the token endpoint
    /// </summary>
    public IDictionary<string, string> AdditionalBodyParameters { get; set; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets additional headers that will be sent to the token endpoint
    /// </summary>
    public IDictionary<string, string> AdditionalHeaders { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the amount of time to subtract from the reported expiration when caching tokens
    /// </summary>
    public TimeSpan RefreshBeforeExpiration { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the timeout applied to the token request
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}