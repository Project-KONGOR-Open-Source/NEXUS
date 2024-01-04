namespace ZORGATH.WebPortal.API.Extensions;

internal static class UserClaimsExtensions
{
    internal static Guid GetAccountID(this IEnumerable<Claim> claims)
        => Guid.Parse(claims.Single(claim => claim.Type.Equals(Claims.AccountID)).Value);

    internal static bool GetAccountIsMain(this IEnumerable<Claim> claims)
        => bool.Parse(claims.Single(claim => claim.Type.Equals(Claims.AccountIsMain)).Value);

    internal static string GetAccountName(this IEnumerable<Claim> claims)
        => claims.Single(claim => claim.Type.Equals(Claims.NameIdentifier)).Value;

    internal static string GetAudience(this IEnumerable<Claim> claims)
        => claims.Single(claim => claim.Type.Equals(Claims.Audience)).Value;

    internal static DateTime GetAuthenticatedAtTime(this IEnumerable<Claim> claims)
        => EpochTimeToUTCTime(Convert.ToInt64(claims.Single(claim => claim.Type.Equals(Claims.AuthenticatedAtTime)).Value));

    internal static string GetClanName(this IEnumerable<Claim> claims)
        => claims.Single(claim => claim.Type.Equals(Claims.ClanName)).Value;

    internal static string GetClanTag(this IEnumerable<Claim> claims)
        => claims.Single(claim => claim.Type.Equals(Claims.ClanTag)).Value;

    internal static DateTime GetExpiresAtTime(this IEnumerable<Claim> claims)
        => EpochTimeToUTCTime(Convert.ToInt64(claims.Single(claim => claim.Type.Equals(Claims.ExpiresAtTime)).Value));

    internal static DateTime GetIssuedAtTime(this IEnumerable<Claim> claims)
        => EpochTimeToUTCTime(Convert.ToInt64(claims.Single(claim => claim.Type.Equals(Claims.IssuedAtTime)).Value));

    internal static string GetIssuer(this IEnumerable<Claim> claims)
        => claims.Single(claim => claim.Type.Equals(Claims.Issuer)).Value;

    internal static Guid GetJWTIdentifier(this IEnumerable<Claim> claims)
        => Guid.Parse(claims.Single(claim => claim.Type.Equals(Claims.JWTIdentifier)).Value);

    internal static Guid GetNonce(this IEnumerable<Claim> claims)
        => Guid.Parse(claims.Single(claim => claim.Type.Equals(Claims.Nonce)).Value);

    internal static Guid GetUserID(this IEnumerable<Claim> claims)
        => Guid.Parse(claims.Single(claim => claim.Type.Equals(Claims.UserID)).Value);

    internal static string GetUserEmailAddress(this IEnumerable<Claim> claims)
        => claims.Single(claim => claim.Type.Equals(Claims.EmailAddress)).Value;

    internal static string GetUserRole(this IEnumerable<Claim> claims)
        => claims.Single(claim => claim.Type.Equals(Claims.UserRole)).Value;

    private static DateTime EpochTimeToUTCTime(long epochSeconds)
        => DateTimeOffset.FromUnixTimeSeconds(epochSeconds).DateTime;
}
