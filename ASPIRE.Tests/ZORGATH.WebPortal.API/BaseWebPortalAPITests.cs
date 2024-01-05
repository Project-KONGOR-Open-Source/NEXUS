namespace ASPIRE.Tests.ZORGATH.WebPortal.API;

using ZORGATH = global::ZORGATH.WebPortal.API.ZORGATH;

public abstract class BaseWebPortalAPITests
{
    protected WebApplicationFactory<ZORGATH> EphemeralZorgath { get; set; } = null!;
    protected MerrickContext EphemeralMerrickContext { get; set; } = null!;
    protected HttpClient EphemeralZorgathClient { get; set; } = null!;

    protected static string EphemeralAuthenticationToken => WebPortalAPITestContext.EphemeralAuthenticationToken;

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

        EphemeralZorgathClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, EphemeralAuthenticationToken);
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
