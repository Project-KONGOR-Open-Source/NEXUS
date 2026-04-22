namespace ASPIRE.Tests.Infrastructure.Container;

/// <summary>
///     Manages the lifecycle of a Redis container for integration tests.
///     Uses the official <c>redis:latest</c> image, matching the Aspire AppHost configuration.
///     Per-test keyspace isolation is achieved by wrapping <see cref="IDatabase"/> with <c>WithKeyPrefix</c> at registration time; see <see cref="ServiceIntegrationWebApplicationFactory{TSelf, TAssemblyMarker}"/>.
/// </summary>
public sealed class RedisContainer : IAsyncDisposable
{
    /// <summary>
    ///     The Redis container image used by the test suite.
    /// </summary>
    public const string Image = "redis:latest";

    /// <summary>
    ///     Display name used in error messages.
    /// </summary>
    public const string DisplayName = "Redis";

    private AsynchronousLock Lock { get; } = new();

    private RedisTestContainer? Self { get; set; }

    /// <summary>
    ///     The host:port connection string for the shared container, suitable for <see cref="StackExchange.Redis.ConnectionMultiplexer.Connect(string)"/>.
    /// </summary>
    public string ConnectionString
        => Self?.GetConnectionString() ?? throw new NullReferenceException($"{DisplayName} Container Connection String Is NULL");

    /// <summary>
    ///     Starts the Redis container if it has not already been started.
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

            RedisTestContainerBuilder builder = new RedisTestContainerBuilder(image: Image)
                .WithDockerEndpoint(DockerEndpointResolver.GetDockerEndpoint());

            Self = builder.Build();

            await Self.StartAsync();
        }
    }

    /// <summary>
    ///     Disposes the underlying Redis container.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (Self is not null)
        {
            await Self.DisposeAsync();
        }
    }
}
