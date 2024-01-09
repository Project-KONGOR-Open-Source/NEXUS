namespace KONGOR.MasterServer.RequestResponseModels.SRP;

public class SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason reason, string? accountName = null)
{
    /// <summary>
    ///     A string of error output in the event of an authentication failure, e.g. "Invalid Nickname Or Password.".
    /// </summary>
    [PhpProperty("auth")]
    public string AuthenticationOutcome { get; set; } = reason switch
    {
        SRPAuthenticationFailureReason.AccountIsDisabled            => "Account" + (accountName is null ? " " : $@" ""{accountName}"" ") + "Is Disabled",
        SRPAuthenticationFailureReason.AccountNotFound              => "Account Not Found",
        SRPAuthenticationFailureReason.BadRequest                   => "Bad Authentication Request",
        SRPAuthenticationFailureReason.IncorrectPassword            => "Incorrect Password",
        SRPAuthenticationFailureReason.InvalidCookie                => "Invalid Cookie",
        SRPAuthenticationFailureReason.MissingMajorVersion          => "Missing Major Version",
        SRPAuthenticationFailureReason.MissingMinorVersion          => "Missing Minor Version",
        SRPAuthenticationFailureReason.MissingMicroVersion          => "Missing Micro Version",
        SRPAuthenticationFailureReason.MissingOperatingSystemType   => "Missing Operating System Type",
        SRPAuthenticationFailureReason.MissingSRPProof              => "Missing SRP Proof",
        SRPAuthenticationFailureReason.MissingSRPData               => "Unable To Retrieve SRP Authentication Session Data" + (accountName is null ? string.Empty : " " + $@"For Account Name ""{accountName}"""),
        SRPAuthenticationFailureReason.SRPAuthenticationDisabled    => "SRP Authentication Is Disabled" + Environment.NewLine + "1) Open The Console (CTRL + F8)" + Environment.NewLine + @"2) Execute ""SetSave login_useSRP true""",
        _                                                           => "Unsupported Authentication Failure Reason" + " " + $@"""{nameof(reason)}"""
    };

    /// <summary>
    ///     Unknown property which seems to be set to "true" on a successful response or "false" if an error occurs.
    ///     Since this is an error response, set to "false".
    /// </summary>
    [PhpProperty(0)]
    public bool Zero => false;
}

public enum SRPAuthenticationFailureReason
{
    AccountIsDisabled,
    AccountNotFound,
    BadRequest,
    IncorrectPassword,
    InvalidCookie,
    MissingMajorVersion,
    MissingMinorVersion,
    MissingMicroVersion,
    MissingOperatingSystemType,
    MissingSRPProof,
    MissingSRPData,
    SRPAuthenticationDisabled
}
