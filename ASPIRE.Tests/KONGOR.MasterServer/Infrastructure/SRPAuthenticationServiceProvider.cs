namespace ASPIRE.Tests.KONGOR.MasterServer.Infrastructure;

/// <summary>
///     Provides Test Dependencies For KONGOR Master Server SRP Authentication Tests
/// </summary>
public sealed class SRPAuthenticationServiceProvider : IAsyncDisposable
{
    private readonly IDistributedApplicationTestingBuilder _appHost;
    private readonly DistributedApplication? _app;
    private readonly MerrickContext _merrickContext;
    private readonly HttpClient? _httpClient;
    private readonly bool _isFullyInitialized;

    public SRPAuthenticationServiceProvider(string? databaseIdentifier = null, bool spinUpAppHost = false)
    {
        _merrickContext = InMemoryHelpers.GetInMemoryMerrickContext(databaseIdentifier);

        if (spinUpAppHost)
        {
            // Spin Up Aspire AppHost For Integration Tests
            _appHost = DistributedApplicationTestingBuilder.CreateAsync<Projects.ASPIRE_AppHost>().GetAwaiter().GetResult();

            _appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
            {
                clientBuilder.AddStandardResilienceHandler();
            });

            _app = _appHost.BuildAsync().GetAwaiter().GetResult();

            _app.StartAsync().GetAwaiter().GetResult();

            _httpClient = _app.CreateHttpClient("kongor-masterserver");
            _isFullyInitialized = true;
        }
        else
        {
            _appHost = null!;
            _app = null;
            _httpClient = null;
            _isFullyInitialized = false;
        }
    }

    public MerrickContext MerrickContext => _merrickContext;
    public HttpClient HttpClient => _httpClient ?? throw new InvalidOperationException("HTTP Client Is NULL - AppHost Was Not Initialized");
    public bool IsFullyInitialized => _isFullyInitialized;

    public async ValueTask DisposeAsync()
    {
        _httpClient?.Dispose();

        await _merrickContext.DisposeAsync();

        if (_app is not null)
            await _app.DisposeAsync();
    }
}
