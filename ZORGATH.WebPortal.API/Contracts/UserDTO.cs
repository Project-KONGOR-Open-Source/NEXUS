namespace ZORGATH.WebPortal.API.Contracts;

public record RegisterUserAndMainAccountDTO(string Token, string Name, string Password, string ConfirmPassword);
