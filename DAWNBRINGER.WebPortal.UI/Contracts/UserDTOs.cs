namespace DAWNBRINGER.WebPortal.UI.Contracts;

public record RegisterUserAndMainAccountRequest(string Token, string Name, string Password, string ConfirmPassword);

public record BasicUserResponse(int ID, string EmailAddress, List<BasicAccountResponse> Accounts);

public record BasicAccountResponse(int ID, string Name);

public record UserResponse(int ID, string EmailAddress, string Role, string CurrentAccountName, string CurrentClanName, string CurrentClanTag, DateTimeOffset TimestampCreated, int TotalLevel, int CurrentAccountLevel, List<UserAccountResponse> Accounts);

public record UserAccountResponse(int ID, string Name, bool IsMain, string AccountType, string ClanName, string ClanTag);
