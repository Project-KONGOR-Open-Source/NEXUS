namespace ASPIRE.Tests.ZORGATH.WebPortal.API;

public static class WebPortalAPITestContext
{
    public static bool AuthenticationFlowHasExecuted { get; set; } = false;

    public static string TransientAuthenticationToken { get; set; } = string.Empty;
}
