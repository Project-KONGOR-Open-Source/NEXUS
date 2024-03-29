﻿namespace ASPIRE.Tests.ZORGATH.WebPortal.API;

public abstract class WebPortalAPITestSetup
{
    protected WebApplicationFactory<IZorgathAssemblyMarker> TransientZorgath { get; set; } = null!;
    protected MerrickContext TransientMerrickContext { get; set; } = null!;
    protected HttpClient TransientZorgathClient { get; set; } = null!;

    [OneTimeSetUp]
    public static void OneTimeSetUp() { }

    [SetUp]
    public virtual async Task SetUp()
    {
        while (WebPortalAPITestContext.AuthenticationFlowHasExecuted.Equals(false))
            await Task.Delay(250);

        TransientZorgath = new WebApplicationFactory<IZorgathAssemblyMarker>();
        TransientZorgathClient = TransientZorgath.CreateClient();
        TransientMerrickContext = InMemoryHelpers.GetInMemoryMerrickContext();

        if (string.IsNullOrWhiteSpace(WebPortalAPITestContext.TransientAuthenticationToken).Equals(false))
            TransientZorgathClient.DefaultRequestHeaders.Authorization
                = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, WebPortalAPITestContext.TransientAuthenticationToken);
    }

    [TearDown]
    public virtual void TearDown()
    {
        TransientMerrickContext.Dispose();
        TransientZorgath.Dispose();
        TransientZorgathClient.Dispose();
    }

    [OneTimeTearDown]
    public static void OneTimeTearDown() { }
}
