namespace ASPIRE.Tests.ZORGATH.WebPortal.API;

using ZORGATH = global::ZORGATH.WebPortal.API.ZORGATH;

[TestFixture]
public class RegistrationAuthenticationTests
{
    private MerrickContext EphemeralMerrickContext { get; set; } = null!;
    private WebApplicationFactory<ZORGATH> EphemeralZorgath { get; set; } = null!;
    private HttpClient EphemeralZorgathClient { get; set; } = null!;

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

    [Test]
    public void Test1()
    {
        MerrickContext merrick = EphemeralMerrickContext;
        WebApplicationFactory<ZORGATH> zorgath = EphemeralZorgath;
        HttpClient client = EphemeralZorgathClient;

        Assert.Pass();
    }
}
