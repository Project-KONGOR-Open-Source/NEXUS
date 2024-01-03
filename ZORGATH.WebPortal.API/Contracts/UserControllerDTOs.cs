namespace ZORGATH.WebPortal.API.Contracts;

public record RegisterUserAndMainAccountDTO(string Token, string Name, string Password, string ConfirmPassword);

public record LogInUserDTO(string Name, string Password);

public record GetBasicUserDTO(Guid ID, string EmailAddress, List<GetBasicAccountDTO> Accounts);

public record GetBasicAccountDTO(Guid ID, string Name);

public record GetAuthenticationTokenDTO(Guid UserID, string TokenSchema, string Token);
