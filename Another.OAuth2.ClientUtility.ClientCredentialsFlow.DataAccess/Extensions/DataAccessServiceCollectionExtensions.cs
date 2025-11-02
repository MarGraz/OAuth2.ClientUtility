using Another.OAuth2.ClientUtility.ClientCredentialsFlow.Common.Abstractions;
using Another.OAuth2.ClientUtility.ClientCredentialsFlow.Common.Options;
using Another.OAuth2.ClientUtility.ClientCredentialsFlow.DataAccess.Abstractions;
using Another.OAuth2.ClientUtility.ClientCredentialsFlow.DataAccess.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Another.OAuth2.ClientUtility.ClientCredentialsFlow.DataAccess.Extensions;

public static class DataAccessServiceCollectionExtensions
{
    /// <summary>
    /// Register a singleton in-memory IAccessTokenCache implementation for caching access tokens in memory
    /// In the future, additional caching implementations may be added (e.g. distributed cache)
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddAccessTokenCaching(this IServiceCollection services)
    {
        services.AddSingleton<IAccessTokenCache, InMemoryAccessTokenCache>();
        return services;
    }     

    /// <summary>
    /// It is assumed that AddAccessTokenCaching has been called before this method to register IAccessTokenCache
    /// Register a keyed singleton IClientCredentialsTokenManager for the specified client name
    /// </summary>
    /// <param name="services"></param>
    /// <param name="clientName"></param>
    /// <returns></returns>
    public static IServiceCollection AddKeyedClientCredentialsTokenManager(
        this IServiceCollection services,
        string clientName)
    {
        services.AddKeyedSingleton<IClientCredentialsTokenManager>(clientName, (sp, key) =>
        {
            string name = (string)key!;
            var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<ClientCredentialsOptions>>();
            var provider = sp.GetRequiredService<IClientCredentialsTokenProvider>();
            var cache = sp.GetRequiredService<IAccessTokenCache>();
            var logger = sp.GetRequiredService<ILogger<ClientCredentialsTokenManager>>();

            return new ClientCredentialsTokenManager(name, optionsMonitor, provider, cache, logger);
        });
        return services;
    }
}