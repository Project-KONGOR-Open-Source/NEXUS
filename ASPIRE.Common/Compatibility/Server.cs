namespace ASPIRE.Common.Compatibility;

public static class Server
{
    # region The Server Version, As Defined By "shell_common.h"
    public const int MAJOR_VERSION = 4;
    public const int MINOR_VERSION = 10;
    public const int MICRO_VERSION = 1;
    public const int HOTFIX_VERSION = 0;
    # endregion

    public static string GetVersion()
    # pragma warning disable CS8520
        => HOTFIX_VERSION is 0
            ? $"{MAJOR_VERSION}.{MINOR_VERSION}.{MICRO_VERSION}"
            : $"{MAJOR_VERSION}.{MINOR_VERSION}.{MICRO_VERSION}.{HOTFIX_VERSION}";
    # pragma warning restore CS8520
}
