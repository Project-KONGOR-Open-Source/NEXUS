namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Infrastructure;

/// <summary>
///     TUnit dependency resolver for the ZORGATH integration test project.
///     Activated by <c>[assembly: ClassConstructor&lt;ZORGATHIntegrationDependencyResolver&gt;]</c>.
/// </summary>
public sealed class ZORGATHIntegrationDependencyResolver : ServiceIntegrationDependencyResolver<ZORGATHIntegrationDependencyResolver, ZORGATHIntegrationWebApplicationFactory, ZORGATHAssemblyMarker>
{
    protected override ZORGATHIntegrationWebApplicationFactory BuildFactory(ServiceContainerContext containerContext)
        => new(containerContext);
}
