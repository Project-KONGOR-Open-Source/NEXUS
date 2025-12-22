namespace ZORGATH.WebPortal.API.Contracts;

public record DiscordRegistrationTokenClaims(string DiscordID, string Email, string Username, string Avatar);
