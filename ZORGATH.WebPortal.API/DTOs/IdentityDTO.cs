namespace ZORGATH.WebPortal.API.DTOs;

public record RegisterUserAndAccountDTO(string Token, string Name, string Password, string ConfirmPassword);
