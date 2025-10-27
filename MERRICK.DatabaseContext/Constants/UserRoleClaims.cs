namespace MERRICK.DatabaseContext.Constants;

public static class UserRoleClaims
{
    public static readonly List<Claim> Administrator = [new Claim(Claims.UserRole, UserRoles.Administrator, ClaimValueTypes.String)];
    public static readonly List<Claim> User = [new Claim(Claims.UserRole, UserRoles.User, ClaimValueTypes.String)];
}
