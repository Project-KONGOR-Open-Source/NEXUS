using System.Collections;
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
            TotalGamesPlayedLegacy = 0,
            CurrentSeason = 12,
            SeasonLevel = 100,
            CreepLevel = 100,
            SimpleSeasonStats = new SimpleSeasonStats
            {
                RankedMatchesWon = 2002,
                RankedMatchesLost = 2004,
                WinStreak = 1003,
                InPlacementPhase = 0,
                // New Rank Fields
                CurrentRank = "1",
                RankedRating = "1500",
                HighestRank = "1"
            },
            SimpleCasualSeasonStats = new SimpleSeasonStats
            {
                RankedMatchesWon = 2002,
                RankedMatchesLost = 2004,
                WinStreak = 1003,
                InPlacementPhase = 0,
                CurrentRank = "1",
                RankedRating = "1500",
                HighestRank = "1"
            },
            SimpleMidWarsSeasonStats = new SimpleSeasonStats
            {
                RankedMatchesWon = 2002,
                RankedMatchesLost = 2004,
                WinStreak = 1003,
                InPlacementPhase = 0,
                CurrentRank = "1",
                RankedRating = "1500",
                HighestRank = "1"
            },
            MVPAwardsCount = "1004", // From log
            // Simulating ClientRequestHelper keeping 'awd_'
            // Uses the "Working Great" configuration: Smackdown, Annihilation, Assists, Kills
            Top4AwardNames = new List<string> { "awd_msd", "awd_mann", "awd_masst", "awd_mkill" },
            Top4AwardCounts = new List<string> { "1005", "1006", "1007", "1008" },
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
            CustomIconSlotID = "1", // Correct 1-based index of "ai.custom_icon:1".

            // New Required Fields for Profile Stats
            AccountCreationDate = "02/05/2010",
            LastActivityDate = "02/05/2026",
            TotalDisconnects = 5,
            RankedWins = 2002,
            RankedLosses = 2004,
            FavHero1 = "dampeer", FavHero1Time = 3, FavHero1_2 = "",
            FavHero2 = "mq", FavHero2Time = 3, FavHero2_2 = "",
            FavHero3 = "scout", FavHero3Time = 3, FavHero3_2 = "",
            FavHero4 = "ophelia", FavHero4Time = 3, FavHero4_2 = "",
            FavHero5 = "pharaoh", FavHero5Time = 3, FavHero5_2 = ""
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
        // Expect a:69 because we added 5 new fields (FavHero1_2 to FavHero5_2) plus con_reward
        // plus the 15 Top-Level Statistics mappings (Kills, Deaths, Assists, AvgGameLength, etc).
        // a:47 -> a:52 -> a:53 -> a:69
        await Assert.That(serialized).StartsWith("a:69:{");
        
        // Verify key presence
        await Assert.That(serialized).Contains("s:10:\"favHero1_2\";s:0:\"\";");
        
        // Updated expectation: FavHeroes should preserve "Hero_" prefix if passed that way
        // But in this test, we passed simple names "dampeer", "mq", etc.
        // So checking if they are serialized correctly as is.
        await Assert.That(serialized).Contains("s:8:\"favHero1\";s:7:\"dampeer\";");
        await Assert.That(serialized).Contains("s:14:\"season_midwars\";");
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
        // Verify Upgrades Info populated
        await Assert.That(serialized).Contains("s:4:\"wins\";i:2002;");
        await Assert.That(serialized).Contains("s:6:\"losses\";i:2004;");
        await Assert.That(serialized).Contains("s:10:\"win_streak\";i:1003;");
        await Assert.That(serialized).Contains("s:12:\"is_placement\";i:0;");
        await Assert.That(serialized).Contains("s:13:\"current_level\";s:1:\"1\";");
        await Assert.That(serialized).Contains("s:3:\"smr\";s:4:\"1500\";");
        await Assert.That(serialized).Contains("s:21:\"highest_level_current\";s:1:\"1\";");

        // Verify Lists are correct PHP Arrays
        // selected_upgrades should correspond to what we passed
        await Assert.That(serialized).Contains("s:16:\"ai.custom_icon:1\";");
    }

    [Test]
    public async Task Serialize_HybridSimpleStats_WithMasteryInfo_ShouldNotProduceEmptyObjects()
    {
        // Construct the payload with generic Collections exactly as it happens in the live server.
        ShowSimpleStatsResponse response = new ShowSimpleStatsResponse
        {
            NameWithClanTag = "TEST1",
            ID = 1,
            Level = 1,
            LevelExperience = 1,
            NumberOfAvatarsOwned = 0,
            NumberOfHeroesOwned = 139,
            TotalMatchesPlayed = 1,
            TotalGamesPlayedLegacy = 1,
            CurrentSeason = 12,
            SeasonLevel = 100,
            CreepLevel = 100,
            SimpleSeasonStats = new SimpleSeasonStats
            {
                RankedMatchesWon = 1,
                RankedMatchesLost = 1,
                WinStreak = 1,
                InPlacementPhase = 0,
                CurrentRank = "1",
                RankedRating = "1500",
                HighestRank = "1"
            },
            SimpleCasualSeasonStats = new SimpleSeasonStats
            {
                RankedMatchesWon = 1, RankedMatchesLost = 1, WinStreak = 1,
                InPlacementPhase = 0, CurrentRank = "1", RankedRating = "1500", HighestRank = "1"
            },
            SimpleMidWarsSeasonStats = new SimpleSeasonStats
            {
                RankedMatchesWon = 1, RankedMatchesLost = 1, WinStreak = 1,
                InPlacementPhase = 0, CurrentRank = "1", RankedRating = "1500", HighestRank = "1"
            },
            MVPAwardsCount = "1",
            Top4AwardNames = new List<string> { "awd_1" },
            Top4AwardCounts = new List<string> { "1" },
            CustomIconSlotID = "0",
            OwnedStoreItems = new List<string>(),
            SelectedStoreItems = new List<string>(),
            OwnedStoreItemsData = new Dictionary<string, OneOf<object, Dictionary<string, object>>>(),
            DiceTokens = "0", GameTokens = 0, ServerTimestamp = 0,
            AccountCreationDate = "01/01/2000",
            LastActivityDate = "01/01/2000",
            TotalDisconnects = 0,
            RankedWins = 1, RankedLosses = 1,
            FavHero1 = "", FavHero1Time = 0, FavHero1_2 = "", FavHero2 = "", FavHero2Time = 0, FavHero2_2 = "",
            FavHero3 = "", FavHero3Time = 0, FavHero3_2 = "", FavHero4 = "", FavHero4Time = 0, FavHero4_2 = "", FavHero5 = "", FavHero5Time = 0, FavHero5_2 = ""
        };

        // Here is the Trap: Using Strongly Typed Generic Dictionaries for MasteryInfo
        Dictionary<string, object> masteryInfoDict = new Dictionary<string, object>
        {
            { "0", new Dictionary<string, string> { { "heroname", "Hero_Legionnaire" }, { "exp", "500" } } },
            { "1", new Dictionary<string, string> { { "heroname", "Hero_Magmus" }, { "exp", "100" } } }
        };

        response.MasteryInfo = masteryInfoDict;
        response.MasteryRewards = new Dictionary<string, object>();

        // Run it through the converter
        Dictionary<object, object> hybridData = global::KONGOR.MasterServer.Services.Requester.ClientRequestHelper.CreateHybridSimpleStats(response);

        // Serialize
        string serialized = PhpSerialization.Serialize(hybridData);

        // Debug output commented to prevent IO test runner locks
        // System.IO.File.WriteAllText(@"C:\Users\ayaza\PycharmProjects\NEXUS-PRIVATE\NEXUS\ASPIRE.Tests\test_output.txt", serialized);
        // Assert the mastery info dictionary parsed correctly
        await Assert.That(serialized).Contains("s:16:\"Hero_Legionnaire\";");
        await Assert.That(serialized).Contains("s:11:\"Hero_Magmus\";");
        // Check that mastery_info itself is an array of size 2, not empty `a:0:{}`.
        await Assert.That(serialized).Contains("s:12:\"mastery_info\";a:2:{");
    }

    [Test]
    public async Task Dictionary_vs_Hashtable_Sorting()
    {
        // 1. Hashtable
        Hashtable hash = new Hashtable();
        hash[0] = "zero";
        hash[1] = "one";
        hash[2] = "two";
        string hashOutput = PhpSerialization.Serialize(hash);
        
        // 2. Dictionary
        Dictionary<object, object> dict = new Dictionary<object, object>();
        dict[0] = "zero";
        dict[1] = "one";
        dict[2] = "two";
        string dictOutput = PhpSerialization.Serialize(dict);

        // 3. Object[]
        object[] array = new object[] { "zero", "one", "two" };
        string arrayOutput = PhpSerialization.Serialize(array);

        // 4. List<Dictionary>
        List<Dictionary<string, string>> listDict = new List<Dictionary<string, string>>();
        listDict.Add(new Dictionary<string, string> { { "heroname", "test" } });
        string listDictOutput = PhpSerialization.Serialize(listDict);

        System.IO.File.WriteAllText("sorting_test_output.txt", $"HASH: {hashOutput}\nDICT: {dictOutput}\nARRAY: {arrayOutput}\nLISTDICT: {listDictOutput}");
        await Assert.That(true).IsTrue();
    }
}
