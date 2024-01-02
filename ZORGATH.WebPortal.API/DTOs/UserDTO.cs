namespace ZORGATH.WebPortal.API.DTOs;

public record RegisterUserAndMainAccountDTO(string Token, string Name, string Password, string ConfirmPassword);
