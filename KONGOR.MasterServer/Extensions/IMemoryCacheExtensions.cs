namespace KONGOR.MasterServer.Extensions;

public static class IMemoryCacheExtensions
{
    public static void SetSRPAuthenticationSessionData(this IMemoryCache cache, string accountName, SRPAuthenticationSessionData value)
        => cache.Set(ConstructSRPDataKey(accountName), value, new TimeSpan(0, 0, 30));

    public static SRPAuthenticationSessionData? GetSRPAuthenticationSessionData(this IMemoryCache cache, string accountName)
        => cache.Get<SRPAuthenticationSessionData>(ConstructSRPDataKey(accountName));

    public static void RemoveSRPAuthenticationSessionData(this IMemoryCache cache, string accountName)
        => cache.Remove(ConstructSRPDataKey(accountName));

    private static string ConstructSRPDataKey(string accountName) => $@"SRP-SESSION-DATA:[""{accountName}""]";
}
