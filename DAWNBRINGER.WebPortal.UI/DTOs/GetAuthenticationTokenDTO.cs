namespace DAWNBRINGER.WebPortal.UI.DTOs;

public record GetAuthenticationTokenDTO(int UserID, string TokenType, string Token);
