using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Another.OAuth2.ClientUtility.ClientCredentialsFlow.ConsoleAppSample
{

    internal sealed class SampleConsoleRunner
    {
        internal const string SampleClientName = "SampleApi";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SampleConsoleRunner> _logger;

        public SampleConsoleRunner(
            IHttpClientFactory httpClientFactory,
            ILogger<SampleConsoleRunner> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Calling protected endpoint...");

            // Create an HTTP client configured to call the protected API with automatic token injection 
            HttpClient client = _httpClientFactory.CreateClient(SampleClientName);

            // Call the protected API endpoint (the bearer token is injected automatically)
            HttpResponseMessage response = await client.GetAsync("api/test", cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Bearer Token: {Token}", response.RequestMessage.Headers.Authorization.Parameter);
            Console.WriteLine($"Token: {response.RequestMessage.Headers.Authorization.Parameter}");

            string json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("`Response obtained contacting the protected API, using the Bearer token:");

            Console.WriteLine(FormatJson(json));
        }

        #region Private Methods
        private static string FormatJson(string json)
        {
            try
            {
                JsonElement jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
                return JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
            }
            catch
            {
                // In case of error, return the original string
                return json;
            }
        }
        #endregion
    }
}
