using Another.OAuth2.ClientUtility.ClientCredentialsFlow.BusinessLogic.Http;
using Another.OAuth2.ClientUtility.ClientCredentialsFlow.BusinessLogic.Providers;
using Another.OAuth2.ClientUtility.ClientCredentialsFlow.Common.Abstractions;
using Another.OAuth2.ClientUtility.ClientCredentialsFlow.DataAccess.Extensions;
using Another.OAuth2.ClientUtility.ClientCredentialsFlow.Common.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Another.OAuth2.ClientUtility.ClientCredentialsFlow.BusinessLogic.Extensions;

public static class ServiceCollectionExtensions
{
    // Register token provider, token cache and named HttpClient for token requests
    public static IServiceCollection AddClientCredentialsTokenManagement(this IServiceCollection services)
    {
        services.TryAddSingleton<IClientCredentialsTokenProvider, ClientCredentialsTokenProvider>();
        services.AddAccessTokenCaching();
        services.AddHttpClient(ClientCredentialsTokenProvider.TokenHttpClientName);
        return services;
    }

    // Main extension method to register a named HttpClient that automatically adds OAuth 2.0 access tokens using client credentials flow to outgoing requests 
    // This method also registers a keyed IClientCredentialsTokenManager for the specified client name 
    // (it is assumed that AddClientCredentialsTokenManagement has been called before to register required services) 
    public static IHttpClientBuilder AddClientCredentialsHttpClient(
        this IServiceCollection services,
        string clientName,
        Action<ClientCredentialsOptions> configureOptions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientName);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.AddClientCredentialsTokenManagement();
        services.AddOptions<ClientCredentialsOptions>(clientName).Configure(configureOptions);

        // Register keyed manager internally (manager remains internal)
        services.AddKeyedClientCredentialsTokenManager(clientName);

        var builder = services.AddHttpClient(clientName);
        builder.AddHttpMessageHandler(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<ClientCredentialsDelegatingHandler>>();
            var manager = sp.GetRequiredKeyedService<IClientCredentialsTokenManager>(clientName);
            
            return new ClientCredentialsDelegatingHandler(manager, logger);
        });

        return builder;
    }

    // Overload: this make it possible to configure the HttpClient from IConfiguration section
    public static IHttpClientBuilder AddClientCredentialsHttpClient(
            this IServiceCollection services,
            string clientName,
            IConfiguration configuration)
            => services.AddClientCredentialsHttpClient(clientName, configuration.Bind);
}