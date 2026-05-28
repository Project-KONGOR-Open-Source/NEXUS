namespace DAWNBRINGER.WebPortal.UI.Contracts;

public record LogInUserRequest(string Name, string Password);

public record AuthenticationTokenResponse(int UserID, string TokenType, string Token);
