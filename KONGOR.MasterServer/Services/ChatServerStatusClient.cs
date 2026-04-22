namespace KONGOR.MasterServer.Services;

/// <summary>
///     Default <see cref="IChatServerStatusClient"/> implementation that probes the chat server's <c>/health</c> endpoint.
///     Fetches the JSON payload, reads the <c>status</c> field case-insensitively, and translates it into a boolean verdict.
///     Any transport failure (non-2xx status, malformed body, transport exception) is swallowed and surfaced as an unhealthy result. The client should never throw.
/// </summary>
public sealed class ChatServerStatusClient(HttpClient httpClient, ILogger<ChatServerStatusClient> logger) : IChatServerStatusClient
{
    private const string HealthyStatus = "Healthy";

    private HttpClient HttpClient { get; } = httpClient;

    private ILogger Logger { get; } = logger;

    public async Task<ChatServerStatus> GetStatus(CancellationToken cancellationToken = default)
    {
        DateTimeOffset checkedAt = DateTimeOffset.UtcNow;

        try
        {
            HttpResponseMessage response = await HttpClient.GetAsync("health", cancellationToken);

            if (response.IsSuccessStatusCode is false)
            {
                return new ChatServerStatus(IsHealthy: false, RawStatus: $"HTTP {(int)response.StatusCode}", CheckedAt: checkedAt);
            }

            await using Stream body = await response.Content.ReadAsStreamAsync(cancellationToken);

            HealthProbeResponse? parsed = await JsonSerializer.DeserializeAsync<HealthProbeResponse>(body, SerializerOptions, cancellationToken);

            string rawStatus = parsed?.Status ?? string.Empty;

            bool isHealthy = rawStatus.Equals(HealthyStatus, StringComparison.OrdinalIgnoreCase);

            return new ChatServerStatus(IsHealthy: isHealthy, RawStatus: rawStatus, CheckedAt: checkedAt);
        }

        catch (Exception exception)
        {
            Logger.LogWarning(exception, "Chat Server Health Probe Failed");

            return new ChatServerStatus(IsHealthy: false, RawStatus: exception.GetType().Name, CheckedAt: checkedAt);
        }
    }

    private static JsonSerializerOptions SerializerOptions { get; } = new(JsonSerializerDefaults.Web);

    private sealed record HealthProbeResponse(string? Status);
}
