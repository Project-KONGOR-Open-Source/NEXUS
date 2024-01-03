namespace ZORGATH.WebPortal.API.Contracts;

public record RegisterUserAndMainAccountDTO(string Token, string Name, string Password, string ConfirmPassword);

public record GetBasicUserDTO(Guid ID, string EmailAddress, List<GetBasicAccountDTO> Accounts);

public record GetBasicAccountDTO(Guid ID, string Name);
