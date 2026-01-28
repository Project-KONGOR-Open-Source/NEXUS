using KONGOR.MasterServer.Extensions.Cache;

namespace ASPIRE.Tests.KONGOR.MasterServer.Tests
{
    public sealed class DistributedCacheExtensionTests
    {
        [Test]
        public async Task PurgeSessionCookies_RemovesOnlyMatchingKeys()
        {
            // Arrange
            await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();
            using IServiceScope scope = webApplicationFactory.Services.CreateScope();
            IConnectionMultiplexer multiplexer = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
            IDatabase database = multiplexer.GetDatabase();

            // Clean up from other tests
            await multiplexer.PurgeSessionCookies();

            string matchingKey1 = "ACCOUNT-SESSION-COOKIE:123";
            string matchingKey2 = "ACCOUNT-SESSION-COOKIE:456";
            string nonMatchingKey = "SOME-OTHER-KEY:789";

            await database.StringSetAsync(matchingKey1, "value1");
            await database.StringSetAsync(matchingKey2, "value2");
            await database.StringSetAsync(nonMatchingKey, "value3");

            // Act
            await multiplexer.PurgeSessionCookies();

            // Assert
            await Assert.That(await database.KeyExistsAsync(matchingKey1)).IsFalse();
            await Assert.That(await database.KeyExistsAsync(matchingKey2)).IsFalse();
            await Assert.That(await database.KeyExistsAsync(nonMatchingKey)).IsTrue();
        }
    }
}
