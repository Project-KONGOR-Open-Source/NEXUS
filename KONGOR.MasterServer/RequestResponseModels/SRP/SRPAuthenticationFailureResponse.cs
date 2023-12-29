namespace KONGOR.MasterServer.RequestResponseModels.SRP;

[PhpClass]
internal class SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason reason, Account? account = null)
{
    /// <summary>
    ///     A string of error output in the event of an authentication failure, e.g. "Invalid Nickname Or Password.".
    /// </summary>
    [PhpProperty("auth")]
    internal string AuthenticationOutcome { get; set; } = reason switch
    {
        SRPAuthenticationFailureReason.AccountIsDisabled    => account is null ? "Account Is Disabled" : $@"Account ""{account.NameWithClanTag}"" Is Disabled",
        SRPAuthenticationFailureReason.AccountNotFound      => "Account Not Found",
        SRPAuthenticationFailureReason.BadRequest           => "Bad Authentication Request",
        SRPAuthenticationFailureReason.IncorrectPassword    => "Incorrect Password",
        SRPAuthenticationFailureReason.InvalidCookie        => "Invalid Cookie",
        _                                                   => $@"Unsupported Authentication Failure Reason ""{nameof(reason)}""",
    };

    /// <summary>
    ///     Unknown property which seems to be set to "true" on a successful response or "false" if an error occurs.
    ///     Since this is an error response, set to "false".
    /// </summary>
    [PhpProperty(0)]
    internal bool Zero => false;
}

internal enum SRPAuthenticationFailureReason
{
    AccountIsDisabled,
    AccountNotFound,
    BadRequest,
    IncorrectPassword,
    InvalidCookie,
}
