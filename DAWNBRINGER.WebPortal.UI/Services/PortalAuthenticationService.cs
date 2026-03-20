namespace DAWNBRINGER.WebPortal.UI.Services;

/// <summary>
///     Authentication state provider that manages JWT tokens using Blazor's protected browser storage.
///     Extends <see cref="AuthenticationStateProvider"/> to integrate with Blazor's cascading authentication state.
/// </summary>
public class PortalAuthenticationService : AuthenticationStateProvider
{
    public const string HTTPClientName = "WebPortalAPI";

    private const string TokenStorageKey = "authentication-token";

    private string? CachedToken { get; set; }

    private IHttpClientFactory HTTPClientFactory { get; }
    private ILogger<PortalAuthenticationService> Logger { get; }

    public PortalAuthenticationService(IHttpClientFactory httpClientFactory, ILogger<PortalAuthenticationService> logger)
    {
        HTTPClientFactory = httpClientFactory;
        Logger = logger;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (string.IsNullOrWhiteSpace(CachedToken))
        {
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }

        IEnumerable<Claim> claims = ParseClaimsFromJWT(CachedToken);

        // Check If The Token Has Expired
        Claim? expirationClaim = claims.SingleOrDefault(claim => claim.Type.Equals(JwtRegisteredClaimNames.Exp));

        if (expirationClaim is not null && long.TryParse(expirationClaim.Value, out long expirationUnixSeconds))
        {
            DateTimeOffset expiration = DateTimeOffset.FromUnixTimeSeconds(expirationUnixSeconds);

            if (expiration <= DateTimeOffset.UtcNow)
            {
                CachedToken = null;
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            }
        }

        ClaimsIdentity identity = new(claims, "JWT");
        ClaimsPrincipal principal = new(identity);

        return Task.FromResult(new AuthenticationState(principal));
    }

    /// <summary>
    ///     Stores the JWT token and notifies the authentication state has changed.
    /// </summary>
    public Task SetToken(string token)
    {
        CachedToken = token;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Clears the stored JWT token and notifies the authentication state has changed.
    /// </summary>
    public Task ClearToken()
    {
        CachedToken = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Creates an <see cref="HttpClient"/> configured with the authentication token for API calls.
    /// </summary>
    public HttpClient CreateAuthenticatedClient()
    {
        HttpClient client = HTTPClientFactory.CreateClient(HTTPClientName);

        if (string.IsNullOrWhiteSpace(CachedToken).Equals(false))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CachedToken);
        }

        return client;
    }

    /// <summary>
    ///     Creates an <see cref="HttpClient"/> for unauthenticated API calls.
    /// </summary>
    public HttpClient CreateAnonymousClient()
    {
        return HTTPClientFactory.CreateClient(HTTPClientName);
    }

    private static IEnumerable<Claim> ParseClaimsFromJWT(string token)
    {
        JwtSecurityTokenHandler handler = new();

        if (handler.CanReadToken(token).Equals(false))
        {
            return [];
        }

        JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

        return jwtToken.Claims;
    }
}
