using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Another.OAuth2.ClientUtility.ClientCredentialsFlow.ConsoleSample;

namespace Another.OAuth2.ClientUtility.ClientCredentialsFlow.ConsoleAppSample;

internal static class Startup
{
    public static async Task Main(string[] args)
    {
        using var host = CreateHostBuilder(args).Build();

        await host.StartAsync().ConfigureAwait(false);

        try
        {
            var sample = host.Services.GetRequiredService<SampleConsoleRunner>();
            await sample.RunAsync().ConfigureAwait(false);
        }
        finally
        {
            await host.StopAsync().ConfigureAwait(false);
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(config =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
            })
            .ConfigureServices(ConfigureServices);

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddClientCredentialsConsoleSample(context.Configuration);
        services.AddTransient<SampleConsoleRunner>();
    }
}