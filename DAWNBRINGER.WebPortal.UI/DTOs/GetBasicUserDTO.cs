namespace DAWNBRINGER.WebPortal.UI.DTOs;

public record GetBasicUserDTO(
    int ID,
    string EmailAddress,
    string? ProfilePictureUrl,
    int GoldCoins,
    int SilverCoins,
    int PlinkoTickets,
    int TotalLevel,
    int TotalExperience,
    List<GetBasicAccountDTO> Accounts
);

public record GetBasicAccountDTO(int ID, string Name);
