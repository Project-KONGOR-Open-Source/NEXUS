namespace ASPIRE.Tests.KONGOR.MasterServer.Infrastructure;

/// <summary>
///     Provides Test Dependencies For KONGOR Master Server SRP Authentication Tests
/// </summary>
public sealed class SRPAuthenticationServiceProvider : IAsyncDisposable
{
    private readonly IDistributedApplicationTestingBuilder? _appHost;
    private readonly DistributedApplication? _app;
    private readonly MerrickContext _merrickContext;
    private readonly HttpClient? _httpClient;
    private readonly bool _isFullyInitialized;

    private SRPAuthenticationServiceProvider(MerrickContext merrickContext, IDistributedApplicationTestingBuilder? appHost, DistributedApplication? app, HttpClient? httpClient, bool isFullyInitialized)
    {
        _merrickContext = merrickContext;
        _appHost = appHost;
        _app = app;
        _httpClient = httpClient;
        _isFullyInitialized = isFullyInitialized;
    }

    public static async Task<SRPAuthenticationServiceProvider> CreateAsync(string? databaseIdentifier = null, bool spinUpAppHost = false)
    {
        MerrickContext merrickContext = InMemoryHelpers.GetInMemoryMerrickContext(databaseIdentifier);

        if (spinUpAppHost)
        {
            IDistributedApplicationTestingBuilder appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.ASPIRE_AppHost>();

            appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
            {
                clientBuilder.AddStandardResilienceHandler();
            });

            DistributedApplication app = await appHost.BuildAsync();
            await app.StartAsync();

            HttpClient httpClient = app.CreateHttpClient("master-server");

            return new SRPAuthenticationServiceProvider(merrickContext, appHost, app, httpClient, isFullyInitialized: true);
        }
        else
        {
            return new SRPAuthenticationServiceProvider(merrickContext, appHost: null, app: null, httpClient: null, isFullyInitialized: false);
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
