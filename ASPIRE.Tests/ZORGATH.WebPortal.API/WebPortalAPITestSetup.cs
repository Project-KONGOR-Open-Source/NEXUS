namespace ASPIRE.Tests.ZORGATH.WebPortal.API;

using ZORGATH = global::ZORGATH.WebPortal.API.ZORGATH;

public abstract class WebPortalAPITestSetup
{
    protected WebApplicationFactory<ZORGATH> EphemeralZorgath { get; set; } = null!;
    protected MerrickContext EphemeralMerrickContext { get; set; } = null!;
    protected HttpClient EphemeralZorgathClient { get; set; } = null!;

    [OneTimeSetUp]
    public static void OneTimeSetUp() { }

    [SetUp]
    public virtual async Task SetUp()
    {
        while (WebPortalAPITestContext.AuthenticationFlowHasExecuted.Equals(false))
            await Task.Delay(250);

        EphemeralZorgath = new WebApplicationFactory<ZORGATH>();
        EphemeralZorgathClient = EphemeralZorgath.CreateClient();
        EphemeralMerrickContext = InMemoryHelpers.GetInMemoryMerrickContext();

        if (string.IsNullOrWhiteSpace(WebPortalAPITestContext.EphemeralAuthenticationToken).Equals(false))
            EphemeralZorgathClient.DefaultRequestHeaders.Authorization
                = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, WebPortalAPITestContext.EphemeralAuthenticationToken);
    }

    [TearDown]
    public virtual void TearDown()
    {
        EphemeralMerrickContext.Dispose();
        EphemeralZorgath.Dispose();
        EphemeralZorgathClient.Dispose();
    }

    [OneTimeTearDown]
    public static void OneTimeTearDown() { }
}
