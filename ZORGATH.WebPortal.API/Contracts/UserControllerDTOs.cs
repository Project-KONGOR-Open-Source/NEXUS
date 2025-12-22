namespace ZORGATH.WebPortal.API.Contracts;

public record RegisterUserAndMainAccountDTO(string Token, string Name, string Password, string ConfirmPassword);

public record RegisterUserWithDiscordDTO(string RegistrationToken, string Name, string Password, string ConfirmPassword);

public record LogInUserDTO(string Name, string Password);

public record GetBasicUserDTO(
    int ID, 
    string EmailAddress,
    string? ProfilePictureUrl,
    int GoldCoins,
    int SilverCoins,
    int PlinkoTickets,
    int TotalLevel,
    int TotalExperience,
    bool IsEmailVerified,
    bool IsMfaEnabled,
    List<GetBasicAccountDTO> Accounts
);

public record GetBasicAccountDTO(int ID, string Name);

public record GetAuthenticationTokenDTO(int UserID, string TokenType, string Token);
