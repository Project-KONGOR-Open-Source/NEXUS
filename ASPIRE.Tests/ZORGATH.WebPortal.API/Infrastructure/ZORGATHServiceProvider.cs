namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Infrastructure;

/// <summary>
///     Provides Access To ZORGATH Services
/// </summary>
public sealed class ZORGATHServiceProvider
{
    public WebApplicationFactory<ZORGATHAssemblyMarker> WebApplicationFactory { get; } = new WebApplicationFactory<ZORGATHAssemblyMarker>();

    public MerrickContext DatabaseContext { get; } = InMemoryHelpers.GetInMemoryMerrickContext(Guid.CreateVersion7().ToString());
}
