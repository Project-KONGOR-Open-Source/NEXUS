namespace ASPIRE.Tests.ZORGATH.WebPortal.API;

using ZORGATH = global::ZORGATH.WebPortal.API.ZORGATH;

public abstract class WebPortalAPITestSetup
{
    protected WebApplicationFactory<ZORGATH> EphemeralZorgath { get; set; } = null!;
    protected MerrickContext EphemeralMerrickContext { get; set; } = null!;
    protected HttpClient EphemeralZorgathClient { get; set; } = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        EphemeralZorgath = new WebApplicationFactory<ZORGATH>();
        EphemeralZorgathClient = EphemeralZorgath.CreateClient();
    }

    [SetUp]
    public void SetUp()
    {
        EphemeralMerrickContext = InMemoryHelpers.GetInMemoryMerrickContext();

        if (string.IsNullOrWhiteSpace(WebPortalAPITestContext.EphemeralAuthenticationToken).Equals(false))
            EphemeralZorgathClient.DefaultRequestHeaders.Authorization
                = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, WebPortalAPITestContext.EphemeralAuthenticationToken);
    }

    [TearDown]
    public void TearDown()
    {
        EphemeralMerrickContext.Dispose();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        EphemeralZorgathClient.Dispose();
        EphemeralZorgath.Dispose();
    }
}
