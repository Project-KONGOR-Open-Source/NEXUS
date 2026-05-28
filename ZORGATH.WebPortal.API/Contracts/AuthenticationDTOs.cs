namespace ZORGATH.WebPortal.API.Contracts;

public record LogInUserDTO(string Name, string Password);

public record GetAuthenticationTokenDTO(int UserID, string TokenType, string Token);
