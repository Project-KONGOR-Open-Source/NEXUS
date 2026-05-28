namespace ASPIRE.Tests.KONGOR.MasterServer.Infrastructure;

/// <summary>
///     TUnit dependency resolver for the KONGOR integration test project.
///     Activated by <c>[assembly: ClassConstructor&lt;KONGORIntegrationDependencyResolver&gt;]</c>.
/// </summary>
public sealed class KONGORIntegrationDependencyResolver : ServiceIntegrationDependencyResolver<KONGORIntegrationDependencyResolver, KONGORIntegrationWebApplicationFactory, KONGORAssemblyMarker>
{
    protected override KONGORIntegrationWebApplicationFactory BuildFactory(ServiceContainerContext containerContext)
        => new(containerContext);
}
