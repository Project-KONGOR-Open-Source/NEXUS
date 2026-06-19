namespace ASPIRE.Tests.Infrastructure.Container;

/// <summary>
///     Manages the lifecycle of a distributed cache container for integration tests.
///     Uses the official Garnet image, matching the Aspire AppHost configuration. The Testcontainers Redis builder's default <c>redis-cli</c> readiness probe is replaced with a log-based wait because the Garnet image does not ship <c>redis-cli</c>.
///     Per-test keyspace isolation is achieved by wrapping <see cref="IDatabase"/> with <c>WithKeyPrefix</c> at registration time; see <see cref="ServiceIntegrationWebApplicationFactory{TSelf, TAssemblyMarker}"/>.
/// </summary>
public sealed class DistributedCacheContainer : IAsyncDisposable
{
    /// <summary>
    ///     The distributed cache container image used by the test suite.
    /// </summary>
    public const string Image = "ghcr.io/microsoft/garnet:latest";

    /// <summary>
    ///     Display name used in error messages.
    /// </summary>
    public const string DisplayName = "Distributed Cache";

    private AsynchronousLock Lock { get; } = new();

    private DistributedCacheTestContainer? Self { get; set; }

    /// <summary>
    ///     The host:port connection string for the shared container, suitable for <see cref="StackExchange.Redis.ConnectionMultiplexer.Connect(string)"/>.
    /// </summary>
    public string ConnectionString
        => Self?.GetConnectionString() ?? throw new NullReferenceException($"{DisplayName} Container Connection String Is NULL");

    /// <summary>
    ///     Starts the distributed cache container if it has not already been started.
    ///     Idempotent and thread-safe: concurrent callers will await the same startup work.
    /// </summary>
    public async Task StartAsync()
    {
        using (await Lock.EnterScope())
        {
            if (Self is not null)
            {
                return;
            }

            DistributedCacheTestContainerBuilder builder = new DistributedCacheTestContainerBuilder(image: Image)
                .WithDockerEndpoint(DockerEndpointResolver.GetDockerEndpoint())
                // The Garnet Image Does Not Ship "redis-cli", So The Default Readiness Probe Is Replaced With A Wait On Garnet's Startup Log Line
                .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Ready to accept connections"));

            Self = builder.Build();

            await Self.StartAsync();
        }
    }

    /// <summary>
    ///     Disposes the underlying distributed cache container.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (Self is not null)
        {
            await Self.DisposeAsync();
        }
    }
}
