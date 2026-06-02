namespace ASPIRE.Common.Constants;

public static class OpenPasswords
{
    /// <summary>
    ///     The publicly-known open password for the built-in GUEST accounts with game client authentication/authorisation permissions.
    ///     These public accounts cannot be used to host match servers.
    /// </summary>
    public const string GuestUserPassword = "KONGOR";

    /// <summary>
    ///     The publicly-known open password for the built-in OPERATOR account with match server hosting permissions and CUSTODIAN role on the user portal.
    ///     This public account cannot be used to authenticate/authorise into the game client.
    /// </summary>
    public const string OperatorPassword = "KONGOR";
}
