namespace ASPIRE.Tests.Infrastructure.Container;

/// <summary>
///     Manages the lifecycle of a WireMock container for integration tests.
///     Provides HTTP mocking for outbound requests; per-test isolation is achieved through unique path prefixes applied by <see cref="ScopedWireMockClient"/>.
/// </summary>
public sealed class WireMockContainer : IAsyncDisposable
{
    /// <summary>
    ///     The WireMock container image used by the test suite.
    /// </summary>
    public const string Image = "wiremock/wiremock:latest";

    /// <summary>
    ///     Display name used in error messages.
    /// </summary>
    public const string DisplayName = "WireMock";

    private AsynchronousLock Lock { get; } = new();

    private WireMockTestContainer? Self { get; set; }

    private IWireMockAdminApi? CachedAdministrativeClient { get; set; }

    /// <summary>
    ///     The public URL of the running WireMock container (e.g. <c>http://localhost:32768/</c>).
    ///     Per-test path scoping is composed by the consuming factory rather than this container so that both the mapping prefix and the client base URL have a single source of truth.
    /// </summary>
    public string PublicURL
        => Self?.GetPublicUrl() ?? throw new NullReferenceException($"{DisplayName} Container Public URL Is NULL");

    /// <summary>
    ///     The administrative client used for registering, reading, and removing WireMock mappings.
    ///     Populated inside <see cref="StartAsync"/>, so callers that access it before the container has started observe a clear failure rather than a partially-initialised client.
    /// </summary>
    public IWireMockAdminApi AdministrativeClient
        => CachedAdministrativeClient ?? throw new InvalidOperationException($@"{DisplayName} Container Has Not Been Started; Call ""{nameof(StartAsync)}"" Before Accessing ""{nameof(AdministrativeClient)}""");

    /// <summary>
    ///     Starts the WireMock container if it has not already been started.
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

            WireMockTestContainerBuilder builder = new WireMockTestContainerBuilder()
                .WithDockerEndpoint(DockerEndpointResolver.GetDockerEndpoint());

            Self = builder.Build();

            await Self.StartAsync();

            CachedAdministrativeClient = RestClient.For<IWireMockAdminApi>(PublicURL);
        }
    }

    /// <summary>
    ///     Disposes the underlying WireMock container.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (Self is not null)
        {
            await Self.DisposeAsync();
        }
    }
}
