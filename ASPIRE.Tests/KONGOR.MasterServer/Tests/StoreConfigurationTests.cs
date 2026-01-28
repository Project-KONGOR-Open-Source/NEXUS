using ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

using KONGOR.MasterServer.Configuration;
using KONGOR.MasterServer.Configuration.Mastery;
using KONGOR.MasterServer.Configuration.Store;

namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

public class StoreConfigurationTests
{
    [Test]
    public async Task LoadConfiguration_LoadStoreConfig_ReturnsValidItems()
    {
        // Act - Trigger static constructor loading
        StoreItemConfiguration config = JSONConfiguration.StoreItemConfiguration;

        // Assert
        await Assert.That(config).IsNotNull();
        await Assert.That(config.StoreItems).IsNotNull();
        await Assert.That(config.StoreItems).IsNotEmpty();

        // Verify a known item from the json (ID 15: Female Pyromancer)
        StoreItem? pyromancer = config.StoreItems.FirstOrDefault(x => x.ID == 15);
        await Assert.That(pyromancer).IsNotNull();
        await Assert.That(pyromancer!.Name).IsEqualTo("Female Pyromancer");
        await Assert.That(pyromancer.GoldCost).IsEqualTo(90);
    }

    [Test]
    public async Task LoadConfiguration_LoadMasteryConfig_HandlesNulls()
    {
        // Act
        MasteryRewardsConfiguration config = JSONConfiguration.MasteryRewardsConfiguration;

        // Assert
        await Assert.That(config).IsNotNull();
        await Assert.That(config.MasteryRewards).IsNotNull();

        // Find a level with empty rewards (e.g. Level 3 was changed to nulls)
        MasteryReward? level3 = config.MasteryRewards.FirstOrDefault(x => x.RequiredLevel == 3);
        await Assert.That(level3).IsNotNull();

        // Verify null modernization
        await Assert.That(level3!.ProductName).IsNull();
        await Assert.That(level3.ProductCode).IsNull();
        await Assert.That(level3.ProductLocalResource).IsNull();

        // Find a level with rewards (e.g. Level 1)
        MasteryReward? level1 = config.MasteryRewards.FirstOrDefault(x => x.RequiredLevel == 1);
        await Assert.That(level1).IsNotNull();
        await Assert.That(level1!.ProductName).IsEqualTo("Mastery Boost");
    }
}
