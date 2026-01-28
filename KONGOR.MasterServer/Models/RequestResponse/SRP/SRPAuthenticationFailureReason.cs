namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public enum SRPAuthenticationFailureReason
{
    AccountIsDisabled,
    AccountNotFound,
    IncorrectPassword,
    IncorrectSystemInformationFormat,
    IsServerHostingAccount,
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
