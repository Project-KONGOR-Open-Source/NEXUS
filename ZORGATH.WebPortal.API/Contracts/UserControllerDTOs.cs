namespace ZORGATH.WebPortal.API.Contracts;

public record RegisterUserAndMainAccountDTO(string Token, string Name, string Password, string ConfirmPassword);

public record LogInUserDTO(string Name, string Password);

public record GetBasicUserDTO(int ID, string EmailAddress, List<GetBasicAccountDTO> Accounts);

public record GetBasicAccountDTO(int ID, string Name);

public record GetUserDTO(int ID, string EmailAddress, string Role, string CurrentAccountName, string CurrentClanName, string CurrentClanTag, DateTimeOffset TimestampCreated, int TotalLevel, int CurrentAccountLevel, List<GetUserAccountDTO> Accounts);

public record GetUserAccountDTO(int ID, string Name, bool IsMain, string AccountType, string ClanName, string ClanTag);

public record GetAuthenticationTokenDTO(int UserID, string TokenType, string Token);
