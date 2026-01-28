using System.Globalization;

namespace ZORGATH.WebPortal.API.Extensions;

public static class UserClaimsExtensions
{
    public static Guid GetAccountID(this IEnumerable<Claim> claims)
    {
        return Guid.Parse(claims.Single(claim => claim.Type.Equals(Claims.AccountID, StringComparison.Ordinal)).Value);
    }

    public static bool GetAccountIsMain(this IEnumerable<Claim> claims)
    {
        return bool.Parse(claims.Single(claim => claim.Type.Equals(Claims.AccountIsMain, StringComparison.Ordinal)).Value);
    }

    public static string GetAccountName(this IEnumerable<Claim> claims)
    {
        return claims.Single(claim => claim.Type.Equals(Claims.Subject, StringComparison.Ordinal) || claim.Type.Equals(Claims.NameIdentifier, StringComparison.Ordinal))
            .Value;
    }

    public static string GetAudience(this IEnumerable<Claim> claims)
    {
        return claims.Single(claim => claim.Type.Equals(Claims.Audience, StringComparison.Ordinal)).Value;
    }

    public static DateTimeOffset GetAuthenticatedAtTime(this IEnumerable<Claim> claims)
    {
        return EpochTimeToUTCTime(Convert.ToInt64(claims.Single(claim => claim.Type.Equals(Claims.AuthenticatedAtTime, StringComparison.Ordinal))
            .Value, CultureInfo.InvariantCulture));
    }

    public static string GetClanName(this IEnumerable<Claim> claims)
    {
        return claims.Single(claim => claim.Type.Equals(Claims.ClanName, StringComparison.Ordinal)).Value;
    }

    public static string GetClanTag(this IEnumerable<Claim> claims)
    {
        return claims.Single(claim => claim.Type.Equals(Claims.ClanTag, StringComparison.Ordinal)).Value;
    }

    public static DateTimeOffset GetExpiresAtTime(this IEnumerable<Claim> claims)
    {
        return EpochTimeToUTCTime(
            Convert.ToInt64(claims.Single(claim => claim.Type.Equals(Claims.ExpiresAtTime, StringComparison.Ordinal)).Value, CultureInfo.InvariantCulture));
    }

    public static DateTimeOffset GetIssuedAtTime(this IEnumerable<Claim> claims)
    {
        return EpochTimeToUTCTime(Convert.ToInt64(claims.Single(claim => claim.Type.Equals(Claims.IssuedAtTime, StringComparison.Ordinal))
            .Value, CultureInfo.InvariantCulture));
    }

    public static string GetIssuer(this IEnumerable<Claim> claims)
    {
        return claims.Single(claim => claim.Type.Equals(Claims.Issuer, StringComparison.Ordinal)).Value;
    }

    public static Guid GetJWTIdentifier(this IEnumerable<Claim> claims)
    {
        return Guid.Parse(claims.Single(claim => claim.Type.Equals(Claims.JWTIdentifier, StringComparison.Ordinal)).Value);
    }

    public static Guid GetNonce(this IEnumerable<Claim> claims)
    {
        return Guid.Parse(claims.Single(claim => claim.Type.Equals(Claims.Nonce, StringComparison.Ordinal)).Value);
    }

    public static Guid GetUserID(this IEnumerable<Claim> claims)
    {
        return Guid.Parse(claims.Single(claim => claim.Type.Equals(Claims.UserID, StringComparison.Ordinal)).Value);
    }

    public static string GetUserEmailAddress(this IEnumerable<Claim> claims)
    {
        return claims.Single(claim => claim.Type.Equals(Claims.Email, StringComparison.Ordinal) || claim.Type.Equals(Claims.EmailAddress, StringComparison.Ordinal)).Value;
    }

    public static string GetUserRole(this IEnumerable<Claim> claims)
    {
        return claims.Single(claim => claim.Type.Equals(Claims.UserRole, StringComparison.Ordinal)).Value;
    }

    private static DateTimeOffset EpochTimeToUTCTime(long epochSeconds)
    {
        return DateTimeOffset.FromUnixTimeSeconds(epochSeconds);
    }
}