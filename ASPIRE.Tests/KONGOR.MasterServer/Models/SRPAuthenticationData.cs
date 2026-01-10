namespace ASPIRE.Tests.KONGOR.MasterServer.Models;

/// <summary>
///     Result Of SRP Authentication Attempt
/// </summary>
public class SRPAuthenticationData
{
    public required Account Account { get; init; }

    public string? Name { get; init; }

    public string? Email { get; init; }

    public required bool Success { get; init; }

    public string? ServerProof { get; init; }

    public string? Cookie { get; init; }

    public string? ErrorMessage { get; init; }
}