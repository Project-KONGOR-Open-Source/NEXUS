namespace KONGOR.MasterServer.Extensions;

public static class IMemoryCacheExtensions
{
    public static void SetSRPAuthenticationSessionData(this IMemoryCache cache, string accountName, SRPAuthenticationSessionData value)
        => cache.Set($@"SRP-SESSION-DATA:[""{accountName}""]", value, new TimeSpan(0, 0, 30));

    public static SRPAuthenticationSessionData? GetSRPAuthenticationSessionData(this IMemoryCache cache, string accountName)
        => cache.Get<SRPAuthenticationSessionData>($@"SRP-SESSION-DATA:[""{accountName}""]");
}
