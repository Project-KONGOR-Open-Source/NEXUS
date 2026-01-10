namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Models;

/// <summary>
///     Result Of A Complete Authentication Flow
/// </summary>
public sealed record JWTAuthenticationData(
    int UserID,
    string AccountName,
    string EmailAddress,
    string AuthenticationToken);