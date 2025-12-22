using System.Text.Json.Serialization;

namespace ZORGATH.WebPortal.API.Models;

public class DiscordTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = "";
}

public class DiscordUserResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
    [JsonPropertyName("username")]
    public string Username { get; set; } = "";
    [JsonPropertyName("email")]
    public string? Email { get; set; }
    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }
    [JsonPropertyName("banner")]
    public string? Banner { get; set; }
    [JsonPropertyName("verified")]
    public bool? Verified { get; set; }
    [JsonPropertyName("mfa_enabled")]
    public bool? MfaEnabled { get; set; }
}
