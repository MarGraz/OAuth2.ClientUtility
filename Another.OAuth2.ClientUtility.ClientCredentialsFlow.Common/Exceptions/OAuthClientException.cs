namespace Another.OAuth2.ClientUtility.ClientCredentialsFlow.Common.Exceptions;

// Represents errors that occur while requesting OAuth 2.0 tokens
public sealed class OAuthClientException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthClientException"/> class with a specified error message.
    /// </summary>
    public OAuthClientException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthClientException"/> class with a specified error message and inner exception
    /// </summary>
    public OAuthClientException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}