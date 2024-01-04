namespace ZORGATH.WebPortal.API.Extensions;

internal static class UserClaimsExtensions
{
    internal static Guid GetAccountID(this IEnumerable<Claim> claims)
        => Guid.Parse(claims.Single(claim => claim.Type.Equals("account_id")).Value);

    internal static string GetAccountName(this IEnumerable<Claim> claims)
        => claims.Single(claim => claim.Type.EndsWith("claims/nameidentifier")).Value;

    internal static bool GetAccountIsMain(this IEnumerable<Claim> claims)
        => bool.Parse(claims.Single(claim => claim.Type.Equals("account_is_main")).Value);

    internal static string GetClanName(this IEnumerable<Claim> claims)
        => claims.Single(claim => claim.Type.Equals("clan_name")).Value;

    internal static string GetClanTag(this IEnumerable<Claim> claims)
        => claims.Single(claim => claim.Type.Equals("clan_tag")).Value;

    internal static Guid GetUserID(this IEnumerable<Claim> claims)
        => Guid.Parse(claims.Single(claim => claim.Type.Equals("user_id")).Value);

    internal static string GetUserEmailAddress(this IEnumerable<Claim> claims)
        => claims.Single(claim => claim.Type.EndsWith("claims/emailaddress")).Value;

    internal static string GetUserRole(this IEnumerable<Claim> claims)
        => claims.Single(claim => claim.Type.Equals("user_role")).Value;
}
