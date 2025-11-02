using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Another.OAuth2.ClientUtility.ClientCredentialsFlow.BusinessLogic.Extensions;
using Another.OAuth2.ClientUtility.ClientCredentialsFlow.ConsoleAppSample;
using Another.OAuth2.ClientUtility.ClientCredentialsFlow.ConsoleAppSample.Configurations;

namespace Another.OAuth2.ClientUtility.ClientCredentialsFlow.ConsoleSample;

internal static class ConfigureServices
{
    public static IServiceCollection AddClientCredentialsConsoleSample(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddLogging(builder =>
        {
            // To better visualize log output in the console, here the console is configured with timestamps and colors, etc.
            builder.AddSimpleConsole(options =>
            {
                options.SingleLine = false;
                options.TimestampFormat = "HH:mm:ss ";
                options.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Enabled;
                options.IncludeScopes = true;
            });
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // SampleApi protected settings
        services.AddOptions<SampleClientSettings>()
            .Bind(configuration.GetSection($"ProtectedApis:{SampleConsoleRunner.SampleClientName}"))
            .Validate(s => !string.IsNullOrWhiteSpace(s.BaseUrl), "BaseUrl required.")
            .ValidateOnStart();

        // SampleApi HttpClient (token options from config)
        var sampleBuilder = services.AddClientCredentialsHttpClient(
            SampleConsoleRunner.SampleClientName,
            configuration.GetSection($"OAuthClients:{SampleConsoleRunner.SampleClientName}"));

        sampleBuilder.ConfigureHttpClient((sp, client) =>
        {
            var api = sp.GetRequiredService<IOptions<SampleClientSettings>>().Value;
            client.BaseAddress = new Uri(api.BaseUrl, UriKind.Absolute);
        });

        // Example to create a second client (ReportsApi)
        //
        //services.AddClientCredentialsHttpClient(
        //    "ReportsApi",
        //    configuration.GetSection("OAuthClients:ReportsApi"))
        //        .ConfigureHttpClient((sp, client) =>
        //        {
        //            // If you have a separate settings class, bind it similarly
        //
        //           var baseUrl = configuration.GetValue<string>("ProtectedApis:ReportsApi:BaseUrl");
        //            client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
        //        });

        services.AddSingleton<SampleConsoleRunner>();
        return services;
    }
}