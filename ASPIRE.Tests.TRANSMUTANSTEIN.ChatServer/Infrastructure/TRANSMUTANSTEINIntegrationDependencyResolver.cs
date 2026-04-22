namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

/// <summary>
///     TUnit dependency resolver for the TRANSMUTANSTEIN integration test project.
///     Activated by <c>[assembly: ClassConstructor&lt;TRANSMUTANSTEINIntegrationDependencyResolver&gt;]</c>.
/// </summary>
public sealed class TRANSMUTANSTEINIntegrationDependencyResolver : ServiceIntegrationDependencyResolver<TRANSMUTANSTEINIntegrationDependencyResolver, TRANSMUTANSTEINIntegrationWebApplicationFactory, TRANSMUTANSTEINAssemblyMarker>
{
    protected override TRANSMUTANSTEINIntegrationWebApplicationFactory BuildFactory(ServiceContainerContext containerContext)
        => new(containerContext);
}
