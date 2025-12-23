using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using ZORGATH.WebPortal.API.Controllers;
using ZORGATH.WebPortal.API.Models;
using ZORGATH.WebPortal.API.Models.Configuration;

namespace ZORGATH.WebPortal.API.Services;

public class DiscordService(
    IHttpClientFactory httpClientFactory,
    IOptions<OperationalConfiguration> configuration,
    IDistributedCache cache,
    IWebHostEnvironment hostEnvironment) : IDiscordService
{
    private HttpClient HttpClient { get; } = httpClientFactory.CreateClient();
    private OperationalConfiguration Configuration { get; } = configuration.Value;
    private IDistributedCache Cache { get; } = cache;
    private IWebHostEnvironment HostEnvironment { get; } = hostEnvironment;

    private string ApiBaseUrl => Configuration.Service.PublicUrl;

    public string GetLoginUrl()
    {
        string clientId = Configuration.Discord.ClientID;
        string redirectUri = $"{ApiBaseUrl}/Auth/Discord/Callback";
        string state = Guid.NewGuid().ToString();

        // Store state in Redis with 5 minute expiration
        Cache.SetStringAsync($"discord_state_{state}", "valid", new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });
        
        return $"https://discord.com/oauth2/authorize?client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope=identify%20email&state={state}";
    }

    public async Task<DiscordTokenResponse?> ExchangeCodeForTokenAsync(string code, string state)
    {
        // Validate State
        string? storedState = await Cache.GetStringAsync($"discord_state_{state}");
        if (storedState == null)
        {
            return null; // Invalid or expired state
        }
        
        // Remove state to prevent replay
        await Cache.RemoveAsync($"discord_state_{state}");

        // Exchange Code
        var tokenRequest = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", Configuration.Discord.ClientID),
            new KeyValuePair<string, string>("client_secret", Configuration.Discord.ClientSecret),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", $"{ApiBaseUrl}/Auth/Discord/Callback")
        });

        var tokenResponse = await HttpClient.PostAsync("https://discord.com/api/oauth2/token", tokenRequest);
        if (!tokenResponse.IsSuccessStatusCode)
            return null;

        var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DiscordTokenResponse>(tokenContent);
    }

    public async Task<DiscordUserResponse?> GetUserInfoAsync(string accessToken)
    {
         var request = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/users/@me");
         request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        
         var userResponse = await HttpClient.SendAsync(request);
         if (!userResponse.IsSuccessStatusCode)
             return null;

         var userContent = await userResponse.Content.ReadAsStringAsync();
         return JsonSerializer.Deserialize<DiscordUserResponse>(userContent);
    }
}
