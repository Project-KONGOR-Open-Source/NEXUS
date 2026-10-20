namespace KONGOR.MasterServer.Models.ServerManagement;

/// <summary>
///     The game cookie which the client connection request issues to an account for a match server.
///     A match server's duplicate session check compares the game cookie which it holds for an account with the account's stored one, so that a player who has since joined another game is removed from the match which they left.
/// </summary>
public class GameCookie
{
    public required string Cookie { get; set; }

    /// <summary>
    ///     The ID of the match server which the game cookie was issued for, or <see langword="null"/> when the session of the match server which asked was not a registered match server's.
    /// </summary>
    public required int? ServerID { get; set; }
}
