namespace MERRICK.Database.Constants;

public static class UserRoleClaims
{
    public static readonly List<Claim> Administrator = [new Claim("UserRole", UserRoles.Administrator)];
    public static readonly List<Claim> User = [new Claim("UserRole", UserRoles.User)];
}
