using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MERRICK.DatabaseContext.Enumerations;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZORGATH.WebPortal.API.Services;
using ZORGATH.WebPortal.API.Models;

namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("Auth")]
public class AuthController(
    MerrickContext databaseContext,
    IOptions<OperationalConfiguration> configuration,
    ILogger<AuthController> logger,
    IDiscordService discordService,
    IWebHostEnvironment hostEnvironment) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private OperationalConfiguration Configuration { get; } = configuration.Value;
    private ILogger Logger { get; } = logger;
    private IDiscordService DiscordService { get; } = discordService;
    private IWebHostEnvironment HostEnvironment { get; } = hostEnvironment;

    private string FrontendBaseUrl => Configuration.Service.FrontendUrl;

    [HttpGet("Discord/Login")]
    public IActionResult KeycloakLogin()
    {
        return Redirect(DiscordService.GetLoginUrl());
    }

    [HttpGet("Discord/Callback")]
    public async Task<IActionResult> DiscordCallback(string code, string state)
    {
        if (string.IsNullOrEmpty(code))
            return BadRequest("Authorization Code Missing");

        // Exchange Code for Token (State validation happens inside ExchangeCodeForTokenAsync)
        var tokenData = await DiscordService.ExchangeCodeForTokenAsync(code, state);
        
        if (tokenData is null || string.IsNullOrEmpty(tokenData.AccessToken))
             return Unauthorized("Failed to exchange code for token or invalid state.");

        // Get User Info
        var userData = await DiscordService.GetUserInfoAsync(tokenData.AccessToken);

        if (userData is null || string.IsNullOrEmpty(userData.Id))
            return Unauthorized("Failed to get user info from Discord.");

        // Check DB
        var user = await MerrickContext.Users
            .Include(u => u.Role)
            .Include(u => u.DiscordProfile) // Include Discord Profile
            .Include(u => u.Accounts).ThenInclude(a => a.Clan)
            .SingleOrDefaultAsync(u => u.DiscordProfile != null && u.DiscordProfile.DiscordID == userData.Id);

        if (user != null)
        {
            // Sync Profile Data
            bool hasChanges = false;
            
            // Ensure DiscordProfile exists (sanity check, though query handles nulls gracefully usually)
            if (user.DiscordProfile == null)
            {
                user.DiscordProfile = new MERRICK.DatabaseContext.Entities.Core.UserDiscordProfile 
                { 
                    DiscordID = userData.Id, 
                    Username = userData.Username 
                };
                hasChanges = true;
            }

            if (user.DiscordProfile.Username != userData.Username) { user.DiscordProfile.Username = userData.Username; hasChanges = true; }
            if (user.DiscordProfile.Avatar != userData.Avatar) { user.DiscordProfile.Avatar = userData.Avatar; hasChanges = true; }
            if (user.DiscordProfile.Banner != userData.Banner) { user.DiscordProfile.Banner = userData.Banner; hasChanges = true; }
            
            // Map nullable booleans to bool, defaulting to false if null
            bool newVerified = userData.Verified ?? false;
            if (user.DiscordProfile.EmailVerified != newVerified) { user.DiscordProfile.EmailVerified = newVerified; hasChanges = true; }
            
            bool newMfa = userData.MfaEnabled ?? false;
            if (user.DiscordProfile.MfaEnabled != newMfa) { user.DiscordProfile.MfaEnabled = newMfa; hasChanges = true; }

            // Sync Account Type logic
            AccountType targetType = AccountType.Disabled;
            if (newMfa) targetType = AccountType.Legacy;
            else if (newVerified) targetType = AccountType.Normal;

            // Update all accounts for this user
            foreach (var account in user.Accounts)
            {
                // Only upgrade/downgrade if it matches one of our auto-managed types (or Trial if any exist)
                // Using Disabled ensures unverified users cannot play
                if (account.Type == AccountType.Disabled || account.Type == AccountType.Trial || account.Type == AccountType.Normal || account.Type == AccountType.Legacy)
                {
                    if (account.Type != targetType)
                    {
                        account.Type = targetType;
                        hasChanges = true;
                    }
                }
            }

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
            new ("banner", discordUser.Banner ?? ""),
            new ("verified", (discordUser.Verified ?? false).ToString()),
            new ("mfa_enabled", (discordUser.MfaEnabled ?? false).ToString())
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


