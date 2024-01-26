namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason reason, string? accountName = null)
{
    /// <summary>
    ///     A string of error output in the event of an authentication failure, e.g. "Invalid Nickname Or Password.".
    /// </summary>
    [PhpProperty("auth")]
    public string AuthenticationOutcome { get; set; } = reason switch
    {
        SRPAuthenticationFailureReason.AccountIsDisabled                => "Account" + (accountName is null ? " " : $@" ""{accountName}"" ") + "Is Disabled",
        SRPAuthenticationFailureReason.AccountNotFound                  => "Account Not Found",
        SRPAuthenticationFailureReason.IncorrectPassword                => "Incorrect Password",
        SRPAuthenticationFailureReason.IncorrectSystemInformationFormat => "Incorrect System Information Format",
        // SRPAuthenticationFailureReason.InvalidCookie                    => "Invalid Cookie" + (accountName is null ? string.Empty : " " + $@"For Account Name ""{accountName}"""),
        SRPAuthenticationFailureReason.MissingCachedSRPData             => "Missing Cached SRP Data",
        SRPAuthenticationFailureReason.MissingClientPublicEphemeral     => "Missing Client Public Ephemeral",
        SRPAuthenticationFailureReason.MissingIPAddress                 => "Missing IP Address",
        SRPAuthenticationFailureReason.MissingLoginIdentifier           => "Missing Login Identifier",
        SRPAuthenticationFailureReason.MissingMajorVersion              => "Missing Major Version",
        SRPAuthenticationFailureReason.MissingMinorVersion              => "Missing Minor Version",
        SRPAuthenticationFailureReason.MissingMicroVersion              => "Missing Micro Version",
        SRPAuthenticationFailureReason.MissingOperatingSystemType       => "Missing Operating System Type",
        SRPAuthenticationFailureReason.MissingSRPClientProof            => "Missing SRP Client Proof",
        SRPAuthenticationFailureReason.MissingSystemInformation         => "Missing System Information",
        SRPAuthenticationFailureReason.SRPAuthenticationDisabled        => "SRP Authentication Is Disabled" + Environment.NewLine + "1) Open The Console (CTRL + F8)" + Environment.NewLine + @"2) Execute ""SetSave login_useSRP true""",
        SRPAuthenticationFailureReason.UnexpectedUserAgent              => "Unexpected User Agent",
        _                                                               => "Unsupported Authentication Failure Reason" + " " + $@"""{nameof(reason)}"""
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
    IncorrectPassword,
    IncorrectSystemInformationFormat,
    InvalidCookie,
    MissingCachedSRPData,
    MissingClientPublicEphemeral,
    MissingIPAddress,
    MissingLoginIdentifier,
    MissingMajorVersion,
    MissingMinorVersion,
    MissingMicroVersion,
    MissingOperatingSystemType,
    MissingSRPClientProof,
    MissingSystemInformation,
    SRPAuthenticationDisabled,
    UnexpectedUserAgent
}
