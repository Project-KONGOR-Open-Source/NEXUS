namespace ZORGATH.WebPortal.API.Contracts;

public record GetAuthenticationTokenDTO(int UserID, string TokenType, string Token);