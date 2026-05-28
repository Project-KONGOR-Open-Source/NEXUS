namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Internals;

/// <summary>
///     Assembly-level hooks for the TRANSMUTANSTEIN integration test project.
/// </summary>
public class AssemblyHooks
{
    /// <summary>
    ///     Pre-pulls the Docker images required by the integration test suite before any test in the assembly runs.
    /// </summary>
    [Before(HookType.Assembly)]
    public static Task Before_Assembly(AssemblyHookContext _)
        => IntegrationAssemblyHooks.EnsureContainerImagesArePulled();
}
