namespace ASPIRE.Tests.ZORGATH.WebPortal.API;

using ZORGATH = global::ZORGATH.WebPortal.API.ZORGATH;

public abstract class BaseWebPortalAPITests
{
    protected MerrickContext EphemeralMerrickContext { get; set; } = null!;
    protected WebApplicationFactory<ZORGATH> EphemeralZorgath { get; set; } = null!;
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
    }

    [TearDown]
    public void TearDown()
    {
        EphemeralMerrickContext.Dispose();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        EphemeralZorgath.Dispose();
        EphemeralZorgathClient.Dispose();
    }
}
