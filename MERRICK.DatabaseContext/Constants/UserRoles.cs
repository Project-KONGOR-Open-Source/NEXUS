namespace MERRICK.DatabaseContext.Constants;

public static class UserRoles
{
    public const string Administrator = "ADMINISTRATOR";
    public const string Custodian = "CUSTODIAN";
    public const string User = "USER";

    public const string AllRoles = "ADMINISTRATOR,CUSTODIAN,USER";
    public const string RolesWithElevatedPrivileges = "ADMINISTRATOR,CUSTODIAN";
}
