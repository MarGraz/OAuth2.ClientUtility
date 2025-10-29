namespace Another.OAuth2.ClientUtility.ClientCredentialsFlow.Common.Models;

// Represents a bearer token value together with its expiration time
public sealed class AccessToken
{
    public AccessToken(string value, DateTimeOffset expiresAtUtc)
    {
        Value = value;
        ExpiresAtUtc = expiresAtUtc;
    }

    /// <summary>
    /// Gets the raw bearer token value
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the UTC timestamp when the token expires
    /// </summary>
    public DateTimeOffset ExpiresAtUtc { get; }

    /// <summary>
    /// Determines whether the token is expired at the specified timestamp
    /// </summary>
    /// <param name="nowUtc">The time to compare against the expiration</param>
    /// <returns><c>true</c> if the token is expired, otherwise, <c>false</c></returns>
    public bool IsExpired(DateTimeOffset nowUtc)
    {
        return nowUtc >= ExpiresAtUtc;
    }
}