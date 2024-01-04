namespace ZORGATH.WebPortal.API.Tests;

[TestFixture]
public class RegistrationAuthenticationTests
{
    private MerrickContext? EphemeralMerrickContext { get; set; }
    private WebApplicationFactory<ZORGATH>? EphemeralZorgath { get; set; }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        EphemeralZorgath = new WebApplicationFactory<ZORGATH>();
    }

    [SetUp]
    public void SetUp()
    {
        EphemeralMerrickContext = InMemoryHelpers.GetInMemoryMerrickContext();
    }

    [TearDown]
    public void TearDown()
    {
        EphemeralMerrickContext?.Dispose();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        EphemeralZorgath?.Dispose();
    }

    [Test]
    public void Test1()
    {
        if (EphemeralMerrickContext is null)
            Assert.Fail("Ephemeral Merrick Context Is NULL");

        if (EphemeralZorgath is null)
            Assert.Fail("Ephemeral Zorgath Is NULL");

        var asd = EphemeralMerrickContext;
        var qwe = EphemeralZorgath;

        Assert.Pass();
    }
}
