using ZORGATH.WebPortal.API.Controllers;
using ZORGATH.WebPortal.API.Models;

namespace ZORGATH.WebPortal.API.Services;

public interface IDiscordService
{
    string GetLoginUrl();
    Task<DiscordTokenResponse?> ExchangeCodeForTokenAsync(string code, string state);
    Task<DiscordUserResponse?> GetUserInfoAsync(string accessToken);
}
