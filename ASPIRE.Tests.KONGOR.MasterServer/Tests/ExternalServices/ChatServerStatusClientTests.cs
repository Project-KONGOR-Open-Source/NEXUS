namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.ExternalServices;

/// <summary>
///     Integration tests for <see cref="ChatServerStatusClient"/>.
///     Uses WireMock to stand in for the TRANSMUTANSTEIN chat server's <c>/health</c> endpoint: each test posts a mapping that drives a different scenario through the client's parsing and status-translation logic, then asserts on the resulting <see cref="ChatServerStatus"/>.
///     The factory's <c>ScopedWireMockClient</c> prepends a per-test path prefix to every mapping, and <see cref="KONGORIntegrationWebApplicationFactory.ConfigureAdditionalServices"/> points the client's <c>BaseAddress</c> at the scoped WireMock URL, so concurrent tests never collide.
/// </summary>
public sealed class ChatServerStatusClientTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithRedisContainer().WithWireMockContainer().InitialiseAsync();

    [Test]
    public async Task GetStatus_WhenChatServerReportsHealthy_ReturnsIsHealthyTrue()
    {
        await PostHealthMapping(statusCode: 200, body: """{"status":"Healthy"}""");

        IChatServerStatusClient client = webApplicationFactory.Services.GetRequiredService<IChatServerStatusClient>();

        ChatServerStatus status = await client.GetStatus();

        using (Assert.Multiple())
        {
            await Assert.That(status.IsHealthy).IsTrue();
            await Assert.That(status.RawStatus).IsEqualTo("Healthy");
        }
    }

    [Test]
    public async Task GetStatus_WhenChatServerReportsHealthyInLowerCase_StillTreatsItAsHealthy()
    {
        await PostHealthMapping(statusCode: 200, body: """{"status":"healthy"}""");

        IChatServerStatusClient client = webApplicationFactory.Services.GetRequiredService<IChatServerStatusClient>();

        ChatServerStatus status = await client.GetStatus();

        using (Assert.Multiple())
        {
            await Assert.That(status.IsHealthy).IsTrue();
            await Assert.That(status.RawStatus).IsEqualTo("healthy");
        }
    }

    [Test]
    public async Task GetStatus_WhenChatServerReportsDegraded_ReturnsIsHealthyFalse()
    {
        await PostHealthMapping(statusCode: 200, body: """{"status":"Degraded"}""");

        IChatServerStatusClient client = webApplicationFactory.Services.GetRequiredService<IChatServerStatusClient>();

        ChatServerStatus status = await client.GetStatus();

        using (Assert.Multiple())
        {
            await Assert.That(status.IsHealthy).IsFalse();
            await Assert.That(status.RawStatus).IsEqualTo("Degraded");
        }
    }

    [Test]
    public async Task GetStatus_WhenChatServerReturnsServiceUnavailable_ReturnsIsHealthyFalseWithStatusCode()
    {
        await PostHealthMapping(statusCode: 503, body: string.Empty);

        IChatServerStatusClient client = webApplicationFactory.Services.GetRequiredService<IChatServerStatusClient>();

        ChatServerStatus status = await client.GetStatus();

        using (Assert.Multiple())
        {
            await Assert.That(status.IsHealthy).IsFalse();
            await Assert.That(status.RawStatus).IsEqualTo("HTTP 503");
        }
    }

    private Task<StatusModel> PostHealthMapping(int statusCode, string body)
    {
        MappingModel mapping = new ()
        {
            Request = new RequestModel
            {
                Path = new PathModel
                {
                    Matchers =
                    [
                        new MatcherModel { Name = "WildcardMatcher", Pattern = "/health" }
                    ]
                },
                Methods = ["GET"]
            },
            Response = new ResponseModel
            {
                StatusCode = statusCode,
                Headers = new Dictionary<string, object> { ["Content-Type"] = "application/json" },
                Body = body
            }
        };

        return webApplicationFactory.WireMockClient.PostMappingAsync(mapping);
    }
}
