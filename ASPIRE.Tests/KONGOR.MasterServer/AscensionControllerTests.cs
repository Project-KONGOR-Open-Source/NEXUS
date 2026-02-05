using System.Net;
using System.Text.Json;
using ASPIRE.Tests.KONGOR.MasterServer.Infrastructure;
using KONGOR.MasterServer.Internals;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ASPIRE.Tests.KONGOR.MasterServer;

public sealed class AscensionControllerTests
{
    [Test]
    public async Task CheckMatch_ReturnsSuccess_WhenMatchIdProvided()
    {
        // Arrange
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();
        using HttpClient client = factory.CreateClient();

        // Act
        // Call the endpoint with required parameters
        // route: api/match/checkmatch
        // match_id: 123 (arbitrary)
        HttpResponseMessage response = await client.GetAsync("/?r=api/match/checkmatch&match_id=123");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        string content = await response.Content.ReadAsStringAsync();
        await Assert.That(content).Contains("error_code");
        await Assert.That(content).Contains("100"); // Success code
        await Assert.That(content).Contains("is_season_match");
    }

    [Test]
    public async Task CheckMatch_ReturnsBadRequest_WhenMatchIdMissing()
    {
        // Arrange
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();
        using HttpClient client = factory.CreateClient();

        // Act
        // Missing match_id
        HttpResponseMessage response = await client.GetAsync("/?r=api/match/checkmatch");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }
}
