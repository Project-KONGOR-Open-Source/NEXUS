namespace KONGOR.MasterServer.Models.ServerManagement;

/// <summary>
///     Records that a client logged out of an account with its session cookie.
///     The master server publishes it for the chat server, which ends only the chat session which used that session cookie, because the newer login of the same account must survive the old machine's logout.
/// </summary>
public class AccountLogout
{
    public required string AccountName { get; set; }

    public required string SessionCookie { get; set; }
}
