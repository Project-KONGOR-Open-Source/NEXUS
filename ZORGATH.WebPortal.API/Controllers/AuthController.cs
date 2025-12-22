using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
// Actually, using Newtonsoft or System.Text.Json for Discord response parsing inside controller for brevity.
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("Auth")]
public class AuthController(
    MerrickContext databaseContext,
    IOptions<OperationalConfiguration> configuration,
    ILogger<AuthController> logger,
    IHttpClientFactory httpClientFactory,
    IWebHostEnvironment hostEnvironment) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private OperationalConfiguration Configuration { get; } = configuration.Value;
    private ILogger Logger { get; } = logger;
    private HttpClient HttpClient { get; } = httpClientFactory.CreateClient();
    private IWebHostEnvironment HostEnvironment { get; } = hostEnvironment;

    private string FrontendBaseUrl => HostEnvironment.IsDevelopment() ? "https://localhost:55510" : "https://portal.ui.kongor.net";
    private string ApiBaseUrl => HostEnvironment.IsDevelopment() ? "https://localhost:55556" : "https://portal.api.kongor.net";

    [HttpGet("Discord/Login")]
    public IActionResult KeycloakLogin()
    {
        string clientId = Configuration.Discord.ClientID;
        string redirectUri = $"{ApiBaseUrl}/Auth/Discord/Callback";
        string state = Guid.NewGuid().ToString(); // Should be stored/verified for security, skipping for simplicity now.
        
        string url = $"https://discord.com/oauth2/authorize?client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope=identify%20email&state={state}";
        
        return Redirect(url);
    }

    [HttpGet("Discord/Callback")]
    public async Task<IActionResult> DiscordCallback(string code, string state)
    {
        if (string.IsNullOrEmpty(code))
            return BadRequest("Authorization Code Missing");

        // Exchange Code for Token
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
            return Unauthorized("Failed to exchange code for token from Discord.");

        var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
        var tokenData = JsonSerializer.Deserialize<DiscordTokenResponse>(tokenContent);
        
        if (tokenData is null || string.IsNullOrEmpty(tokenData.AccessToken))
             return Unauthorized("Invalid Token Response from Discord.");

        // Get User Info
        var request = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/users/@me");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenData.AccessToken);
        
        var userResponse = await HttpClient.SendAsync(request);
        if (!userResponse.IsSuccessStatusCode)
             return Unauthorized("Failed to get user info from Discord.");

        var userContent = await userResponse.Content.ReadAsStringAsync();
        var userData = JsonSerializer.Deserialize<DiscordUserResponse>(userContent);

        if (userData is null || string.IsNullOrEmpty(userData.Id))
            return Unauthorized("Invalid User Info Response from Discord.");

        // Check DB
        var user = await MerrickContext.Users
            .Include(u => u.Role)
            .Include(u => u.Accounts).ThenInclude(a => a.Clan)
            .SingleOrDefaultAsync(u => u.DiscordID == userData.Id);

        if (user != null)
        {
            // Sync Profile Data
            bool hasChanges = false;
            if (user.DiscordUsername != userData.Username) { user.DiscordUsername = userData.Username; hasChanges = true; }
            if (user.DiscordAvatar != userData.Avatar) { user.DiscordAvatar = userData.Avatar; hasChanges = true; }
            // Assuming Banner is added to DiscordUserResponse in previous steps, if not I need to make sure DTO has it.
            if (user.DiscordBanner != userData.Banner) { user.DiscordBanner = userData.Banner; hasChanges = true; }

            if (hasChanges)
            {
                await MerrickContext.SaveChangesAsync();
            }

            // Login
            string jwt = GenerateJwtToken(user, user.Accounts.FirstOrDefault(a => a.IsMain));
            // Redirect to frontend with token
            // WARNING: ID Token in URL fragment is standard for Implicit Flow, but here we prefer secure cookie or intermediate page.
            // For simplicity/speed: Redirect with token in query param (Development) or fragment.
            return Redirect($"{FrontendBaseUrl}/login-callback?token={jwt}&userid={user.ID}");
        }
        else
        {
            // Registration Token
             string registrationToken = GenerateRegistrationToken(userData);
             return Redirect($"{FrontendBaseUrl}/register/{registrationToken}");
        }
    }

    private string GenerateJwtToken(User user, Account? account)
    {
        // ... Reuse Logic from UserController (Should be refactored to a service, duplicating for speed now) ...
        var roleClaims = user.Role.Name == UserRoles.Administrator ? UserRoleClaims.Administrator : UserRoleClaims.User;
        var openIdClaims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Sub, account?.Name ?? user.EmailAddress, ClaimValueTypes.String), // Prefer Account Name
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.Email, user.EmailAddress)
        };
        var customClaims = new List<Claim>
        {
             new (Claims.UserID, user.ID.ToString()),
             new (Claims.AccountID, account?.ID.ToString() ?? "0"),
             // etc...
        };

        var allClaims = roleClaims.Union(openIdClaims).Union(customClaims);

         var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.JWT.SigningKey));
         var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
         var token = new JwtSecurityToken(
             Configuration.JWT.Issuer,
             Configuration.JWT.Audience,
             allClaims,
             expires: DateTime.UtcNow.AddHours(Configuration.JWT.DurationInHours),
             signingCredentials: creds
         );

         return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRegistrationToken(DiscordUserResponse discordUser)
    {
        // Generate a short-lived token containing Discord ID and Email
        // Verify this token on "Complete Registration"
        var claims = new List<Claim>
        {
            new ("discord_id", discordUser.Id),
            new ("email", discordUser.Email ?? ""),
            new ("username", discordUser.Username),
            new ("avatar", discordUser.Avatar ?? ""),
            new ("banner", discordUser.Banner ?? "")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.JWT.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            Configuration.JWT.Issuer,
            Configuration.JWT.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(30), // Short expiry for registration
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

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
}
