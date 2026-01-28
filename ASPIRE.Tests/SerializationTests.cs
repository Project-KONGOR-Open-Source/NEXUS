using KONGOR.MasterServer.Models.RequestResponse.SRP;
using KONGOR.MasterServer.Models.RequestResponse.Stats;

using OneOf;

using PhpSerializerNET;

namespace ASPIRE.Tests;

public class SerializationTests
{
    [Test]
    public async Task Serialize_ShowSimpleStatsResponse_ShouldMatchUserLogExample()
    {
        // TDD: Mimic the user's provided log data exactly.
        // "I found a working example... I can see the number values, just the icons are missing"
        // We assume the missing icons are due to:
        // 1. Incorrect slot_id (was "1", should be index of "ai." item).
        // 2. Award names having "awd_" prefix.

        ShowSimpleStatsResponse response = new ShowSimpleStatsResponse
        {
            NameWithClanTag = "[PK]GUEST-05",
            ID = "12",
            Level = 0,
            LevelExperience = 0,
            NumberOfAvatarsOwned = 0,
            NumberOfHeroesOwned = 139,
            TotalMatchesPlayed = 117, // From log
            CurrentSeason = 12,
            SeasonLevel = 100,
            CreepLevel = 100,
            SimpleSeasonStats = new SimpleSeasonStats
            {
                RankedMatchesWon = 2002,
                RankedMatchesLost = 2004,
                WinStreak = 1003,
                InPlacementPhase = 0,
                LevelsGainedThisSeason = 0
            },
            SimpleCasualSeasonStats = new SimpleSeasonStats
            {
                RankedMatchesWon = 2002,
                RankedMatchesLost = 2004,
                WinStreak = 1003,
                InPlacementPhase = 0,
                LevelsGainedThisSeason = 0
            },
            MVPAwardsCount = 1004, // From log
            // Simulating ClientRequestHelper keeping 'awd_'
            // Uses the "Working Great" configuration: Smackdown, Annihilation, Assists, Kills
            Top4AwardNames = new List<string> { "awd_msd", "awd_mann", "awd_masst", "awd_mkill" },
            Top4AwardCounts = new List<int> { 1005, 1006, 1007, 1008 },
            DiceTokens = "100",
            GameTokens = 100,
            ServerTimestamp = 1768276206, // From log
            OwnedStoreItems = new List<string>(), // Log doesn't show owned items content fully, simplified
            // Log selected_upgrades
            SelectedStoreItems = new List<string>
            {
               "ai.custom_icon:1", "av.Flamboyant", "c.cat_courier", "cc.frostburnlogo",
               "cr.Punk Creep", "cs.frostburnlogo", "m.Super-Taunt", "sc.paragon_circle_upgrade",
               "t.Dumpster_Taunt", "te.Punk TP", "w.8bit_ward"
            },
            OwnedStoreItemsData = new Dictionary<string, OneOf<object, Dictionary<string, object>>>(),
            CustomIconSlotID = "1" // Correct 1-based index of "ai.custom_icon:1".
        };

        // Populate Fake OwnedStoreItemsData to match log
        foreach (string item in response.SelectedStoreItems)
        {
            response.OwnedStoreItemsData[item] = new StoreItemData
            {
                Data = "",
                AvailableFrom = "1768276206",
                AvailableUntil = "33325185006",
                Used = 0,
                Score = "0",
                ExpirationDate = "0",
                Permanent = "1"
            };
        }

        string serialized = PhpSerialization.Serialize(response);

        Console.WriteLine(serialized); // For debug

        // Critical Assertions
        await Assert.That(serialized).Contains("s:8:\"nickname\";s:12:\"[PK]GUEST-05\";");
        // Verify slot_id is "1" (1-based index)
        await Assert.That(serialized).Contains("s:7:\"slot_id\";s:1:\"1\";");
        // Verify Awards have prefix (awd_msd) like Origin
        await Assert.That(serialized).Contains("s:7:\"awd_msd\";");
        await Assert.That(serialized).Contains("s:9:\"awd_masst\";"); // Explicitly check for allowed generic stat

        // Verify Zero property is int/bool 0/1, legacy treated bool as "b:1;".
        // New serializer with [PHPProperty(0)] might output i:0;b:1; if int key, or s:1:"0";b:1; if string key.
        // My attribute usage was [PHPProperty(0)] -> int key 0.
        // PHP output for int key is i:0;
        await Assert.That(serialized).Contains("i:0;b:1;");

        // Verify Upgrades Info populated
        await Assert.That(serialized).Contains("s:4:\"perm\";s:1:\"1\";");
        await Assert.That(serialized).Contains("s:16:\"ai.custom_icon:1\";");

        // Verify Lists are correct PHP Arrays
        // selected_upgrades should correspond to what we passed
        await Assert.That(serialized).Contains("s:16:\"ai.custom_icon:1\";");
    }
}
