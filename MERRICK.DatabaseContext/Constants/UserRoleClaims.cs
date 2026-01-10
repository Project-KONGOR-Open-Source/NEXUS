namespace MERRICK.DatabaseContext.Constants;

public static class UserRoleClaims
{
    public static readonly List<Claim> Administrator =
        [new(Claims.UserRole, UserRoles.Administrator, ClaimValueTypes.String)];

    public static readonly List<Claim> User = [new(Claims.UserRole, UserRoles.User, ClaimValueTypes.String)];
}