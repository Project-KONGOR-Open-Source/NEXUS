namespace ZORGATH.WebPortal.API.Extensions;

public static class UserClaimsExtensions
{
    public static Guid GetAccountID(this IEnumerable<Claim> claims)
    {
        return Guid.Parse(claims.Single(claim => claim.Type.Equals(Claims.AccountID)).Value);
    }

    public static bool GetAccountIsMain(this IEnumerable<Claim> claims)
    {
        return bool.Parse(claims.Single(claim => claim.Type.Equals(Claims.AccountIsMain)).Value);
    }

    public static string GetAccountName(this IEnumerable<Claim> claims)
    {
        return claims.Single(claim => claim.Type.Equals(Claims.Subject) || claim.Type.Equals(Claims.NameIdentifier))
            .Value;
    }

    public static string GetAudience(this IEnumerable<Claim> claims)
    {
        return claims.Single(claim => claim.Type.Equals(Claims.Audience)).Value;
    }

    public static DateTimeOffset GetAuthenticatedAtTime(this IEnumerable<Claim> claims)
    {
        return EpochTimeToUTCTime(Convert.ToInt64(claims.Single(claim => claim.Type.Equals(Claims.AuthenticatedAtTime))
            .Value));
    }

    public static string GetClanName(this IEnumerable<Claim> claims)
    {
        return claims.Single(claim => claim.Type.Equals(Claims.ClanName)).Value;
    }

    public static string GetClanTag(this IEnumerable<Claim> claims)
    {
        return claims.Single(claim => claim.Type.Equals(Claims.ClanTag)).Value;
    }

    public static DateTimeOffset GetExpiresAtTime(this IEnumerable<Claim> claims)
    {
        return EpochTimeToUTCTime(
            Convert.ToInt64(claims.Single(claim => claim.Type.Equals(Claims.ExpiresAtTime)).Value));
    }

    public static DateTimeOffset GetIssuedAtTime(this IEnumerable<Claim> claims)
    {
        return EpochTimeToUTCTime(Convert.ToInt64(claims.Single(claim => claim.Type.Equals(Claims.IssuedAtTime))
            .Value));
    }

    public static string GetIssuer(this IEnumerable<Claim> claims)
    {
        return claims.Single(claim => claim.Type.Equals(Claims.Issuer)).Value;
    }

    public static Guid GetJWTIdentifier(this IEnumerable<Claim> claims)
    {
        return Guid.Parse(claims.Single(claim => claim.Type.Equals(Claims.JWTIdentifier)).Value);
    }

    public static Guid GetNonce(this IEnumerable<Claim> claims)
    {
        return Guid.Parse(claims.Single(claim => claim.Type.Equals(Claims.Nonce)).Value);
    }

    public static Guid GetUserID(this IEnumerable<Claim> claims)
    {
        return Guid.Parse(claims.Single(claim => claim.Type.Equals(Claims.UserID)).Value);
    }

    public static string GetUserEmailAddress(this IEnumerable<Claim> claims)
    {
        return claims.Single(claim => claim.Type.Equals(Claims.Email) || claim.Type.Equals(Claims.EmailAddress)).Value;
    }

    public static string GetUserRole(this IEnumerable<Claim> claims)
    {
        return claims.Single(claim => claim.Type.Equals(Claims.UserRole)).Value;
    }

    private static DateTimeOffset EpochTimeToUTCTime(long epochSeconds)
    {
        return DateTimeOffset.FromUnixTimeSeconds(epochSeconds);
    }
}