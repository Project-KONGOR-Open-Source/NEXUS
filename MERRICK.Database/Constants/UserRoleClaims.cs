namespace MERRICK.Database.Constants;

public static class UserRoleClaims
{
    public static readonly List<Claim> Administrator = [new Claim("user_role", UserRoles.Administrator, ClaimValueTypes.String)];
    public static readonly List<Claim> User = [new Claim("user_role", UserRoles.User, ClaimValueTypes.String)];
}
