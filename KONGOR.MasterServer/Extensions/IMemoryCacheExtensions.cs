namespace KONGOR.MasterServer.Extensions;

public static class IMemoryCacheExtensions
{
    private static string ConstructSRPAuthenticationSessionDataKey(string accountName) => $@"SRP-SESSION-DATA:[""{accountName}""]";

    public static void SetSRPAuthenticationSessionData(this IMemoryCache cache, string accountName, SRPAuthenticationSessionDataStageOne data)
        => cache.Set(ConstructSRPAuthenticationSessionDataKey(accountName), data, new TimeSpan(0, 0, 30));

    public static SRPAuthenticationSessionDataStageOne? GetSRPAuthenticationSessionData(this IMemoryCache cache, string accountName)
        => cache.Get<SRPAuthenticationSessionDataStageOne>(ConstructSRPAuthenticationSessionDataKey(accountName));

    public static void RemoveSRPAuthenticationSessionData(this IMemoryCache cache, string accountName)
        => cache.Remove(ConstructSRPAuthenticationSessionDataKey(accountName));

    private static string ConstructSRPAuthenticationSystemInformationKey(string accountName) => $@"SYSTEM-INFORMATION:[""{accountName}""]";

    public static void SetSRPAuthenticationSystemInformation(this IMemoryCache cache, string accountName, string systemInformation)
        => cache.Set(ConstructSRPAuthenticationSystemInformationKey(accountName), systemInformation, new TimeSpan(0, 0, 30));

    public static string? GetSRPAuthenticationSystemInformation(this IMemoryCache cache, string accountName)
        => cache.Get<string>(ConstructSRPAuthenticationSystemInformationKey(accountName));

    public static void RemoveSRPAuthenticationSystemInformation(this IMemoryCache cache, string accountName)
        => cache.Remove(ConstructSRPAuthenticationSystemInformationKey(accountName));

    // The Cookie Is Stored In The Key Because Most Requests From HoN Are Made With Just A Cookie But No Account Information
    // Additionally, Cached Values Cannot Be Retrieved Without Their Respective Key, So Iterating Just The Values Is Not Possible Without Complex Reflection
    private static string ConstructAccountSessionCookieKey(string cookie) => $@"ACCOUNT-SESSION-COOKIE:[""{cookie}""]";

    public static void SetAccountSessionCookie(this IMemoryCache cache, string cookie, string accountName)
        => cache.Set(ConstructAccountSessionCookieKey(cookie), accountName, new TimeSpan(24, 0, 0));

    public static string? GetAccountSessionCookie(this IMemoryCache cache, string cookie)
        => cache.Get<string>(ConstructAccountSessionCookieKey(cookie));

    public static void RemoveAccountSessionCookie(this IMemoryCache cache, string cookie)
        => cache.Remove(ConstructAccountSessionCookieKey(cookie));

    public static bool ValidateAccountSessionCookie(this IMemoryCache cache, string cookie, out string? accountName)
    {
        accountName = cache.GetAccountSessionCookie(cookie);

        return accountName is not null;
    }
}
