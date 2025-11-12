namespace ASPIRE.Tests.KONGOR.MasterServer.Infrastructure;

/// <summary>
///     Provides Test Dependencies For KONGOR Master Server SRP Authentication Tests
/// </summary>
public sealed class KONGORServiceProvider(MerrickContext merrickContext, DistributedApplication? app, HttpClient? httpClient, bool isFullyInitialized) : IAsyncDisposable
{



    // TODO: Make ZORGATH Use Aspire Resource Too





    public static async Task<KONGORServiceProvider> CreateAsync(string? databaseIdentifier = null, bool spinUpAppHost = false)
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

            return new KONGORServiceProvider(merrickContext, app, httpClient, isFullyInitialized: true);
        }

        else
        {
            return new KONGORServiceProvider(merrickContext, app: null, httpClient: null, isFullyInitialized: false);
        }
    }

    public MerrickContext MerrickContext => merrickContext;
    public HttpClient HttpClient => httpClient ?? throw new InvalidOperationException("HTTP Client Is NULL");
    public bool IsFullyInitialized => isFullyInitialized;

    public async ValueTask DisposeAsync()
    {
        httpClient?.Dispose();

        await merrickContext.DisposeAsync();

        if (app is not null)
            await app.DisposeAsync();
    }
}
