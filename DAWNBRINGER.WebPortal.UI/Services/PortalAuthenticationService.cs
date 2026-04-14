namespace DAWNBRINGER.WebPortal.UI.Services;

/// <summary>
///     Authentication state provider that manages JWTs using Blazor's protected browser storage.
///     Extends <see cref="AuthenticationStateProvider"/> to integrate with Blazor's cascading authentication state.
/// </summary>
public class PortalAuthenticationService : AuthenticationStateProvider
{
    public const string HTTPClientName = "WebPortalAPI";

    private const string TokenStorageKey = "authentication-token";

    private string? CachedToken { get; set; }

    private IHttpClientFactory HTTPClientFactory { get; }
    private ILogger<PortalAuthenticationService> Logger { get; }
    private ProtectedLocalStorage LocalStorage { get; }

    public PortalAuthenticationService(IHttpClientFactory httpClientFactory, ILogger<PortalAuthenticationService> logger, ProtectedLocalStorage localStorage)
    {
        HTTPClientFactory = httpClientFactory;
        Logger = logger;
        LocalStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Restore The Token From Browser Storage If The In-Memory Cache Is Empty (e.g. After A Page Refresh)
        if (string.IsNullOrWhiteSpace(CachedToken))
        {
            try
            {
                ProtectedBrowserStorageResult<string> storedToken = await LocalStorage.GetAsync<string>(TokenStorageKey);

                if (storedToken.Success && string.IsNullOrWhiteSpace(storedToken.Value).Equals(false))
                {
                    CachedToken = storedToken.Value;
                }
            }

            catch (InvalidOperationException)
            {
                // JS Interop Is Not Available Yet; Return Unauthenticated For Now
            }
        }

        if (string.IsNullOrWhiteSpace(CachedToken))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
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

                await ClearTokenFromStorageAsync();

                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        ClaimsIdentity identity = new(claims, "JWT");
        ClaimsPrincipal principal = new(identity);

        return new AuthenticationState(principal);
    }

    /// <summary>
    ///     Stores the JWT in both the in-memory cache and browser storage, then notifies that the authentication state has changed.
    /// </summary>
    public async Task SetToken(string token)
    {
        CachedToken = token;

        await LocalStorage.SetAsync(TokenStorageKey, token);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    /// <summary>
    ///     Clears the JWT from both the in-memory cache and browser storage, then notifies that the authentication state has changed.
    /// </summary>
    public async Task ClearToken()
    {
        CachedToken = null;

        await ClearTokenFromStorageAsync();

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    /// <summary>
    ///     Removes the JWT from browser storage.
    /// </summary>
    private async Task ClearTokenFromStorageAsync()
    {
        try
        {
            await LocalStorage.DeleteAsync(TokenStorageKey);
        }

        catch (InvalidOperationException)
        {
            // JS Interop Is Not Available; Storage Will Be Cleared On The Next Successful Access
        }
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

        JwtSecurityToken jwt = handler.ReadJwtToken(token);

        return jwt.Claims;
    }
}
