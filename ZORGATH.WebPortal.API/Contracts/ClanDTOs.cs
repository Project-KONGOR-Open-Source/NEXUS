namespace ZORGATH.WebPortal.API.Contracts;

/// <summary>
///     The request payload for updating the title and logo of the authenticated account's clan.
/// </summary>
public record SetClanDetailsDTO(string Title, string Logo);

/// <summary>
///     The clan details returned after a successful update.
/// </summary>
public record ClanDetailsDTO(string Name, string Tag, string Title, string Logo);
