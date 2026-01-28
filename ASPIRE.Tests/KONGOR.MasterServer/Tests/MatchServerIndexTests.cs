using KONGOR.MasterServer.Extensions.Cache;
using KONGOR.MasterServer.Models.ServerManagement;

namespace ASPIRE.Tests.KONGOR.MasterServer.Tests
{
    public sealed class MatchServerIndexTests
    {
        [Test]
        public async Task MatchServerIndex_Lifecycle_WorksCorrectly()
        {
            // Arrange
            await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();
            using IServiceScope scope = webApplicationFactory.Services.CreateScope();
            IConnectionMultiplexer multiplexer = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
            IDatabase database = multiplexer.GetDatabase();

            // Cleanup
            string matchServersKey = "MATCH-SERVERS";
            string matchServerIndexKey = "MATCH-SERVER-INDEX";
            await database.KeyDeleteAsync(matchServersKey);
            await database.KeyDeleteAsync(matchServerIndexKey);

            MatchServer server = new MatchServer
            {
                ID = 12345,
                HostAccountID = 1,
                HostAccountName = "TestHost",
                Instance = 1,
                IPAddress = "127.0.0.1",
                Port = 10000,
                Location = "USE",
                Name = "TestServer",
                Description = "Unit Test Server"
            };

            // Act 1: Set (Should populate Index)
            await database.SetMatchServer(server.HostAccountName, server);

            // Assert 1
            await Assert.That(await database.HashExistsAsync(matchServerIndexKey, server.ID)).IsTrue();
            await Assert.That(await database.HashExistsAsync(matchServersKey, $"{server.HostAccountName}:{server.Instance}")).IsTrue();

            // Act 2: Get (Should use Index - implicit verify by result)
            MatchServer? retrieved = await database.GetMatchServerByID(server.ID);
            await Assert.That(retrieved).IsNotNull();
            await Assert.That(retrieved!.ID).IsEqualTo(server.ID);

            // Act 3: Fallback & Repair (Delete Index, Get should still work and repair index)
            await database.HashDeleteAsync(matchServerIndexKey, server.ID);
            await Assert.That(await database.HashExistsAsync(matchServerIndexKey, server.ID)).IsFalse();

            MatchServer? retrievedFallback = await database.GetMatchServerByID(server.ID);
            await Assert.That(retrievedFallback).IsNotNull();
            await Assert.That(retrievedFallback!.ID).IsEqualTo(server.ID);

            // Allow small delay for fire-and-forget or async ops if any (though logic seems awaited)
            // The repair logic is in the same method: await distributedCacheStore.HashSetAsync(MatchServerIndexKey...)
            await Assert.That(await database.HashExistsAsync(matchServerIndexKey, server.ID)).IsTrue();

            // Act 4: Remove (Should remove Index)
            await database.RemoveMatchServerByID(server.ID);

            await Assert.That(await database.HashExistsAsync(matchServerIndexKey, server.ID)).IsFalse();
            await Assert.That(await database.HashExistsAsync(matchServersKey, $"{server.HostAccountName}:{server.Instance}")).IsFalse();
        }
    }
}
