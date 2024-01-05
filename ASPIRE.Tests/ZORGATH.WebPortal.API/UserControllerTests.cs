namespace ASPIRE.Tests.ZORGATH.WebPortal.API;

[TestFixture]
public sealed class UserControllerTests : BaseWebPortalAPITests
{
    // TODO: Add Some Tests

    [Test]
    public void VerifyAuthenticationToken()
        => Assert.That(string.IsNullOrWhiteSpace(EphemeralAuthenticationToken), Is.False);
}
