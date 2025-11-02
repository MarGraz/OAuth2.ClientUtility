# Another OAuth2 Client Credentials Utility

A modular .NET 8 library that provide an OAuth2 client credentials token acquisition, in-memory caching and HttpClient bearer injection. Features:

- Automatic access token retrieval and refresh
- Caching of tokens (in-memory by default)
- Plug-in architecture for token storage (e.g. future Redis) by implementing the `IAccessTokenCache` interface
- Simple service registration via extension methods
- Seamless bearer injection using a delegating handler

What's the logic behind? You can reuse a named `HttpClient` across your application, the first outbound request triggers token acquisition, subsequent requests reuse the cached token until refresh is required.

## Why this library exists

This project was born from a practical limitation encountered when integrating with an OAuth 2.0 authorization server that **did not expose a valid [OpenID Connect Discovery document](https://openid.net/specs/openid-connect-discovery-1_0.html)** (the `.well-known/openid-configuration` endpoint).  

Because of this missing endpoint, it wasn’t possible to use [**MSAL.NET**](https://learn.microsoft.com/en-us/entra/msal/dotnet/#supported-platforms-and-application-architectures:~:text=While%20it%20is%20possible%20to%20use%20MSAL.NET%20with%20third%2Dparty%20IDPs%20that%20support%20OAuth%202) - Microsoft’s official library - even though the latest versions of MSAL now support the **Client Credentials** flow for non-Azure OAuth 2.0 providers. This library therefore provides a **lightweight and fully configurable alternative**, allowing you to:

- Work with any OAuth 2.0 server that supports the Client Credentials flow, even without OpenID Connect metadata  
- Plug in your own endpoints for token acquisition  
- Keep a clean separation between token management, caching, and HTTP request logic
- Is possible to extend the OAuth 2.0 

Use it when your identity provider doesn’t fully implement OpenID Connect discovery or when you simply prefer a minimal, dependency-free solution focused on server-to-server authentication.

## Extensibility

Although this library currently focuses on the **Client Credentials** flow, its architecture was designed with extensibility in mind.  
You can easily extend it to support additional OAuth 2.0 grant types or custom authentication mechanisms by:

- Implementing your own `IAccessTokenProvider` to handle alternative flows (e.g. Resource Owner Password, JWT Bearer, or Device Code)  
- Reusing the same caching and injection infrastructure already in place  
- Registering your provider through the existing dependency injection extensions  

This makes the library a flexible foundation for building modular authentication clients that can adapt to different OAuth 2.0 scenarios.

## Contents

- [NuGet Package](#NuGet-Package)
- [Projects](#projects)
- [Quick Start](#quick-start)
- [Configuration](#configuration)
- [Sample Console App](#sample-console-app)
- [How Automatic Token Injection Works](#how-automatic-token-injection-works)
- [Multiple Clients](#multiple-clients)
- [Redis / Distributed Cache Extension (For future implementation)](#redis--distributed-cache-extension-not-implemented-this-is-an-example-on-how-to-use-third-parties-caching-systems)
- [Extensibility Points](#extensibility-points)
- [Diagnostics & Logging](#diagnostics--logging)
- [Security Considerations](#security-considerations)
- [Troubleshooting](#troubleshooting)
- [Roadmap / Ideas / Contributions needed](#roadmap--ideas--contributions-needed)
- [License](#license)

## NuGet Package

Install the package via NuGet Package Manager Console:

```console
dotnet add package Another.OAuth2.ClientUtility.ClientCredentialsFlow
```

The package includes:

- **In-memory token caching** by default
- **Automatic token refresh** before expiration
- **HttpClient integration** via delegating handler
- **Extensible architecture** for custom token providers and cache implementations

## Projects in this repository

| Project | Purpose |
|--------|---------|
| `Common` | Shared abstractions, option models, token models. |
| `BusinessLogic` | Token provider + delegating handler + DI extensions. |
| `DataAccess` | Token manager (internal) + cache abstraction + in-memory cache. |
| `ConsoleAppSample` | Example usage showing a protected API call. |
| `ClientCredentialsFlow` | The NuGet package project that bundles the above. |

## Quick Start

   1. Prepare configuration (`appsettings.json`):

   ```json
   {
      "OAuthClients":{
         "SampleApi":{
            "TokenEndpoint":"https://demo.duendesoftware.com/connect/token",
            "ClientId":"m2m",
            "ClientSecret":"secret",
            "Scope":"api",
            "RefreshBeforeExpiration":"00:00:05"
         }
      },
      "ProtectedApis":{
         "SampleApi":{
            "BaseUrl":"https://demo.duendesoftware.com/"
         }
      }
   }
   ```

   2. Register services in `Program.cs`:

   ```csharp
   services.AddClientCredentialsHttpClient( "SampleApi", configuration.GetSection("OAuthClients:SampleApi"))
      .ConfigureHttpClient((sp, client) => {
         var baseUrl = configuration.GetValue<string>("ProtectedApis:SampleApi:BaseUrl");
         client.BaseAddress = new Uri(baseUrl);
      });
   ```

   3. Inject and use `HttpClient`:
   
   ```csharp	
   public class MyService { 
   
      private readonly HttpClient _httpClient; 
   
      public MyService(HttpClient httpClient) { _httpClient = httpClient; } 
      
      public async Task CallApiAsync() { 
      
          var response = await _httpClient.GetAsync("api/data"); 
          response.EnsureSuccessStatusCode(); 
      
          var content = await response.Content.ReadAsStringAsync(); 
          Console.WriteLine(content); 
       } 
   }
   ```

   4. Run your application, the first API call will automatically handle token acquisition and injection.

   ## Configuration

`ClientCredentialsOptions` (named options per client):
- `TokenEndpoint` (required) - full token endpoint URL (e.g. `https://demo.duendesoftware.com/connect/token`)
- `ClientId`, `ClientSecret` (required)
- `Scope` (optional) - space-delimited or single scope
- `Audience` (optional)
- `AdditionalBodyParameters` / `AdditionalHeaders` (optional dictionaries)
- `RefreshBeforeExpiration` - subtract time from token expiry to refresh early
- `Timeout` - per token request

Protected API settings (`SampleClientSettings` in sample) supply base URL only.

## Sample Console App

Entry point: `SampleConsoleRunner`:

```csharp
var client = _httpClientFactory.CreateClient(SampleConsoleRunner.SampleClientName); 
var json = await client.GetStringAsync("api/test"); 

_logger.LogInformation("Protected API response: {Response}", json);
```

## How automatic token injection works

1. You register a named `HttpClient` via `AddClientCredentialsHttpClient`
2. The extension:
   - Adds the token provider (for contacting the OAuth token endpoint)
   - Registers a keyed internal token manager (one per client name)
   - Attaches `ClientCredentialsDelegatingHandler`
3. On first outbound request:
   - Handler sees no `Authorization` header
   - Requests a token via the manager & provider
   - Injects `Authorization: Bearer <token>`
4. Subsequent requests reuse cached token until near expiry
5. On expiry or pre-refresh moment, a new token is fetched (single-threaded via semaphore)

## Multiple Clients

Register multiple clients, here an example:

```csharp
// First client called "CatalogApi"
services.AddClientCredentialsHttpClient("CatalogApi", config.GetSection("OAuthClients:CatalogApi"))
   .ConfigureHttpClient((_, c) =>
      c.BaseAddress = new Uri(config["ProtectedApis:CatalogApi:BaseUrl"])
   );

// Second e client called "ReportsApi"
services.AddClientCredentialsHttpClient("ReportsApi", config.GetSection("OAuthClients:ReportsApi"))
   .ConfigureHttpClient((_, c) =>
      c.BaseAddress = new Uri(config["ProtectedApis:ReportsApi:BaseUrl"])
   );
```

Use via named `HttpClient`:

```csharp
var catalogClient = _httpClientFactory.CreateClient("CatalogApi");
var reportsClient = _httpClientFactory.CreateClient("ReportsApi");
```

Each client has isolated options, cache, and refresh lifecycle.

## Redis / Distributed cache extension (Not implemented, this is an example on how to use third parties caching systems)

Introduce an abstraction already defined:

```csharp
public interface IAccessTokenCache {
   Task<AccessToken?> GetAsync(string clientName, CancellationToken ct = default);
   Task SetAsync(string clientName, AccessToken token, CancellationToken ct = default);
}
```

Create a Redis implementation (example):

```csharp
public sealed class RedisAccessTokenCache : IAccessTokenCache {

   private readonly IConnectionMultiplexer _redis;
   
   public RedisAccessTokenCache(IConnectionMultiplexer redis) => _redis = redis;

   public async Task<AccessToken?> GetAsync(string clientName, CancellationToken ct)
   {
       var db = _redis.GetDatabase();
       var raw = await db.StringGetAsync($"oauth:token:{clientName}");
       if (raw.IsNullOrEmpty) return null;
       var parts = raw.ToString().Split('|', 2);
       return parts.Length == 2 && DateTimeOffset.TryParse(parts[1], out var exp)
           ? new AccessToken(parts[0], exp)
           : null;
   }
   
   public async Task SetAsync(string clientName, AccessToken token, CancellationToken ct)
   {
       var db = _redis.GetDatabase();
       var value = $"{token.Value}|{token.ExpiresAt:O}";
       var ttl = token.ExpiresAt - DateTimeOffset.UtcNow;
       await db.StringSetAsync($"oauth:token:{clientName}", value, ttl > TimeSpan.Zero ? ttl : TimeSpan.FromMinutes(5));
   }
}
```

Wire it in place of the memory cache:

```csharp

services.AddSingleton<IAccessTokenCache, RedisAccessTokenCache>();

```

Done, whitout any change to handlers or consumers.

## Extensibility Points

| Component | Interface | Replace When |
|-----------|-----------|--------------|
| Token retrieval | `IClientCredentialsTokenProvider` | Custom auth server logic / mTLS |
| Caching | `IAccessTokenCache` | Distributed (Redis) / encryption requirements |
| Delegating handler | `ClientCredentialsDelegatingHandler` | Advanced header logic / tracing |
| Options | `ClientCredentialsOptions` | Additional OAuth parameters |

You can also add a proactive refresh background service if you need zero latency on first post-expiration call (optional).

## Diagnostics and Logging

Log levels:

- Trace: token reuse / header injection decisions
- Debug: token request boundaries
- Error: token endpoint failures

## Security Considerations

- Store `ClientSecret` securely (user secrets, environment variables, Azure Key Vault)
- Avoid logging the full token; truncate if needed
- Prefer HTTPS for both token and resource endpoints (enforced by `Uri` usage)
- Rotate client secrets periodically
- If using distributed cache, consider encrypting access tokens at rest

## Troubleshooting

| Issue | Cause | Resolution |
|-------|-------|-----------|
| 404 calling `/api/test` | Wrong BaseAddress or path | Verify `BaseUrl` ends with `/` and call relative `"api/test"` |
| 401 Unauthorized | Invalid client credentials / scope mismatch | Check `ClientId`, `ClientSecret`, `Scope` in config |
| Token not refreshed | `RefreshBeforeExpiration` zero or handler skipped | Do not set your own Authorization header; rely on handler |
| Extension method missing | Missing project reference or `using` | Add reference & `using ...BusinessLogic.Extensions` |
| Multiple token requests close together | Manager not shared | Ensure keyed registration was used, or factory exists |

## Roadmap / Ideas / Contributions needed

Here a list of nedesired features for the next releases: 

- Add Unit Test coverage
- Proactive token refresh service that refreshes before expiry in background 
- Metrics (events: token requested / reused / expired)
- Polly integration for transient token endpoint retry logic, because now it fails immediately on error responses, which may be too harsh for some scenarios, maybe is better to retry a few times
- Redis / Memory hybrid fallback cache (try Redis, fallback to memory) 
- Find a way that makes third party cache easier to integrate with minimal code (e.g. via NuGet)

## License

See [LICENSE](https://github.com/MarGraz/Another.OAuth2.ClientUtility/blob/main/LICENSE.txt) file.

## Disclaimer

The sample uses the public Duende demo (`https://demo.duendesoftware.com`), intended for development only. Do not rely on demo credentials or endpoints in production.

---

Feedback or contributions are super-welcome, feel free to open issues or pull requests. Happy coding 😊
