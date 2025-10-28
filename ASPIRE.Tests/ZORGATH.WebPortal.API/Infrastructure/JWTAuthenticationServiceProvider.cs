namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Infrastructure;

/// <summary>
///     Provides Test Dependencies For ZORGATH Web Portal API Tests
/// </summary>
public sealed class JWTAuthenticationServiceProvider : IDisposable, IAsyncDisposable
{
    private readonly WebApplicationFactory<ZORGATHAssemblyMarker> _factory;
    private readonly MerrickContext _merrickContext;
    private readonly HttpClient _httpClient;

    public JWTAuthenticationServiceProvider(string? databaseIdentifier = null)
    {
        _factory = new WebApplicationFactory<ZORGATHAssemblyMarker>();
        _merrickContext = InMemoryHelpers.GetInMemoryMerrickContext(databaseIdentifier);
        _httpClient = _factory.CreateClient();
    }

    public WebApplicationFactory<ZORGATHAssemblyMarker> Factory => _factory;
    public MerrickContext MerrickContext => _merrickContext;
    public HttpClient HttpClient => _httpClient;

    public JWTAuthenticationService CreateJWTAuthenticationService()
        => new (_merrickContext, _factory);

    public void SetAuthenticationToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        _merrickContext?.Dispose();
        _factory?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        _httpClient?.Dispose();

        if (_merrickContext is not null)
            await _merrickContext.DisposeAsync();

        if (_factory is not null)
            await _factory.DisposeAsync();
    }
}
