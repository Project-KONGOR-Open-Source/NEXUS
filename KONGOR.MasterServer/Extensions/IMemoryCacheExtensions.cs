namespace KONGOR.MasterServer.Extensions;

public static class IMemoryCacheExtensions
{
    private static string ConstructSRPDataKey(string accountName) => $@"SRP-SESSION-DATA:[""{accountName}""]";

    public static void SetSRPAuthenticationSessionData(this IMemoryCache cache, string accountName, SRPAuthenticationSessionDataStageOne data)
        => cache.Set(ConstructSRPDataKey(accountName), data, new TimeSpan(0, 0, 30));

    public static SRPAuthenticationSessionDataStageOne? GetSRPAuthenticationSessionData(this IMemoryCache cache, string accountName)
        => cache.Get<SRPAuthenticationSessionDataStageOne>(ConstructSRPDataKey(accountName));

    public static void RemoveSRPAuthenticationSessionData(this IMemoryCache cache, string accountName)
        => cache.Remove(ConstructSRPDataKey(accountName));

    private static string ConstructSRPSystemInformationKey(string accountName) => $@"SYSTEM-INFORMATION:[""{accountName}""]";

    public static void SetSRPAuthenticationSystemInformation(this IMemoryCache cache, string accountName, string systemInformation)
        => cache.Set(ConstructSRPSystemInformationKey(accountName), systemInformation, new TimeSpan(0, 0, 30));

    public static string? GetSRPAuthenticationSystemInformation(this IMemoryCache cache, string accountName)
        => cache.Get<string>(ConstructSRPSystemInformationKey(accountName));

    public static void RemoveSRPAuthenticationSystemInformation(this IMemoryCache cache, string accountName)
        => cache.Remove(ConstructSRPSystemInformationKey(accountName));
}
