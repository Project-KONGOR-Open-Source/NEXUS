namespace ASPIRE.Tests.ZORGATH.WebPortal.API;

[TestFixture]
public sealed class EmailAddressControllerTests : WebPortalAPITestSetup
{
    // TODO: Add Some Tests

    [Test]
    public void VerifyAuthenticationToken()
        => Assert.That(string.IsNullOrWhiteSpace(TransientZorgathClient.DefaultRequestHeaders.Authorization?.ToString()), Is.False);
}
