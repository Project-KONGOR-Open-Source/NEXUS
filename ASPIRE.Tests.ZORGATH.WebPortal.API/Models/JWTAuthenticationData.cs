namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Models;

/// <summary>
///     Result of a complete authentication flow.
/// </summary>
public sealed record JWTAuthenticationData(int UserID, string AccountName, string EmailAddress, string AuthenticationToken);
