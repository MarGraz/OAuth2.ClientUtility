using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Another.OAuth2.ClientUtility.ClientCredentialsFlow.Common.Options;

namespace Another.OAuth2.ClientUtility.ClientCredentialsFlow
{
    /// <summary>
    /// Extension methods for registering OAuth2 client credentials flow services
    /// </summary>
    public static class OAuth2ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers a named HttpClient with automatic OAuth 2.0 client credentials token injection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="clientName">Unique name for the HttpClient and token cache</param>
        /// <param name="configureOptions">Delegate to configure <see cref="ClientCredentialsOptions"/>.</param>
        /// <returns>HttpClient builder for further configuration</returns>
        public static IHttpClientBuilder AddClientCredentialsHttpClient(
            this IServiceCollection services,
            string clientName,
            Action<ClientCredentialsOptions> configureOptions)
            => services.AddClientCredentialsHttpClient(clientName, configureOptions);

        /// <summary>
        /// Registers a named HttpClient with automatic OAuth 2.0 client credentials token injection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="clientName">Unique name for the HttpClient and token cache</param>
        /// <param name="configuration">Configuration section containing <see cref="ClientCredentialsOptions"/></param>
        /// <returns>HttpClient builder for further configuration</returns>
        public static IHttpClientBuilder AddClientCredentialsHttpClient(
            this IServiceCollection services,
            string clientName,
            IConfiguration configuration)
            => services.AddClientCredentialsHttpClient(clientName, configuration);
    }
}
