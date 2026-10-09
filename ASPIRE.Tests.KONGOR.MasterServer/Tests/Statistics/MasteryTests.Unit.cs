namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Statistics;

/// <summary>
///     Pure-logic tests for the hero mastery system covering level thresholds, match and boost experience calculation, the per-hero experience accessors, reward-tier claim tracking, consumable inventory management, and hero-name resolution.
/// </summary>
public sealed class MasteryTests_Unit
{
    private static User BuildUser(List<string>? ownedStoreItems = null) => new ()
    {
        EmailAddress = "mastery.tests@kongor.com",
        Role = new Role { Name = UserRoles.User },
        SRPPasswordSalt = "salt",
        SRPPasswordHash = "hash",
        OwnedStoreItems = ownedStoreItems ?? []
    };

    private static Mastery BuildMastery() => new ()
    {
        Account = new Account { Name = "MasteryTester", User = BuildUser(), IsMain = true }
    };

    private static MasteryRewards BuildMasteryRewards() => new ()
    {
        Account = new Account { Name = "MasteryTester", User = BuildUser(), IsMain = true }
    };

    [Test]
    [Arguments(00000, 00)]
    [Arguments(01399, 00)]
    [Arguments(01400, 01)]
    [Arguments(02999, 01)]
    [Arguments(03000, 02)]
    [Arguments(35999, 14)]
    [Arguments(36100, 15)]
    [Arguments(99999, 15)]
    public async Task GetLevelFromExperience_Returns_The_Expected_Mastery_Level(int experience, int expectedLevel)
        => await Assert.That(Mastery.GetLevelFromExperience(experience)).IsEqualTo(expectedLevel);

    [Test]
    public async Task Level_Boundaries_Bracket_The_Current_Level_And_Cap_At_The_Maximum()
    {
        using (Assert.Multiple())
        {
            await Assert.That(Mastery.GetLowerLevelBoundaryFromExperience(2000)).IsEqualTo(1400);
            await Assert.That(Mastery.GetUpperLevelBoundaryFromExperience(2000)).IsEqualTo(3000);
            await Assert.That(Mastery.GetLowerLevelBoundaryFromExperience(40000)).IsEqualTo(36100);
            await Assert.That(Mastery.GetUpperLevelBoundaryFromExperience(40000)).IsEqualTo(36100);
        }
    }

    [Test]
    [Arguments(AccountStatisticsType.Matchmaking, 20, 400)]
    [Arguments(AccountStatisticsType.MatchmakingCasual, 20, 200)]
    [Arguments(AccountStatisticsType.MidWars, 20, 200)]
    [Arguments(AccountStatisticsType.Public, 20, 0)]
    [Arguments(AccountStatisticsType.Cooperative, 20, 0)]
    public async Task CalculateMatchExperience_Scales_By_Game_Type_And_Hero_Level(AccountStatisticsType type, int heroLevel, int expected)
        => await Assert.That(Mastery.CalculateMatchExperience(type, heroLevel)).IsEqualTo(expected);

    [Test]
    public async Task CalculateRegularMasteryBoostExperience_Equals_The_Combined_Match_And_Bonus_Experience_So_That_A_Boost_Doubles_It()
        => await Assert.That(Mastery.CalculateRegularMasteryBoostExperience(400, 8)).IsEqualTo(408);

    [Test]
    [Arguments(AccountStatisticsType.Matchmaking,       500, 000, 000)]
    [Arguments(AccountStatisticsType.Matchmaking,       500, 012, 012)]
    [Arguments(AccountStatisticsType.Matchmaking,       500, 138, 138)]
    [Arguments(AccountStatisticsType.MatchmakingCasual, 250, 051, 026)]
    [Arguments(AccountStatisticsType.MatchmakingCasual, 250, 052, 026)]
    [Arguments(AccountStatisticsType.MidWars,           250, 138, 069)]
    [Arguments(AccountStatisticsType.Public,            000, 138, 000)]
    public async Task CalculateBonusExperience_Awards_One_Point_Per_Maximum_Level_Hero_Halved_And_Rounded_Up_For_Reduced_Experience_Modes(AccountStatisticsType type, int matchExperience, int heroesAtMaximumMasteryCount, int expected)
        => await Assert.That(Mastery.CalculateBonusExperience(type, matchExperience, heroesAtMaximumMasteryCount)).IsEqualTo(expected);

    [Test]
    [Arguments(AccountStatisticsType.Matchmaking,       200, 138, 100)]
    [Arguments(AccountStatisticsType.Matchmaking,       100, 138, 050)]
    [Arguments(AccountStatisticsType.Matchmaking,       020, 138, 010)]
    [Arguments(AccountStatisticsType.MatchmakingCasual, 050, 138, 025)]
    [Arguments(AccountStatisticsType.Matchmaking,       020, 010, 010)]
    public async Task CalculateBonusExperience_Never_Exceeds_Half_Of_The_Match_Experience(AccountStatisticsType type, int matchExperience, int heroesAtMaximumMasteryCount, int expected)
        => await Assert.That(Mastery.CalculateBonusExperience(type, matchExperience, heroesAtMaximumMasteryCount)).IsEqualTo(expected);

    [Test]
    public async Task SetHeroExperienceByHeroIdentifier_Caps_The_Experience_At_The_Maximum_Mastery_Level_Threshold()
    {
        Mastery mastery = BuildMastery();

        mastery.SetHeroExperienceByHeroIdentifier("Hero_Accursed", 40000);

        using (Assert.Multiple())
        {
            await Assert.That(mastery.GetHeroExperienceByHeroIdentifier("Hero_Accursed")).IsEqualTo(Mastery.MaximumMasteryExperience);
            await Assert.That(mastery.GetHeroLevelByHeroIdentifier("Hero_Accursed")).IsEqualTo(Mastery.MaximumMasteryLevel);
        }
    }

    [Test]
    public async Task Per_Hero_Experience_Accessors_Round_Trip_And_Derive_The_Level()
    {
        Mastery mastery = BuildMastery();

        mastery.SetHeroExperienceByHeroIdentifier("Hero_Accursed", 5000);

        using (Assert.Multiple())
        {
            await Assert.That(mastery.GetHeroExperienceByHeroIdentifier("Hero_Accursed")).IsEqualTo(5000);

            // 5000 Experience Is Exactly The Level 3 Threshold
            await Assert.That(mastery.GetHeroLevelByHeroIdentifier("Hero_Accursed")).IsEqualTo(3);
            await Assert.That(mastery.GetHeroExperienceByHeroIdentifier("Hero_Adrenaline")).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Hero_Experiences_Are_Stored_Sparsely()
    {
        Mastery mastery = BuildMastery();

        mastery.SetHeroExperienceByHeroIdentifier("Hero_Zephyr", 1400);

        using (Assert.Multiple())
        {
            await Assert.That(mastery.HeroExperiences.Count).IsEqualTo(1);
            await Assert.That(mastery.GetHeroExperienceByHeroIdentifier("Hero_Zephyr")).IsEqualTo(1400);
            await Assert.That(mastery.GetHeroExperienceByHeroIdentifier("Hero_Accursed")).IsEqualTo(0);
        }
    }

    [Test]
    public async Task MasteryRewards_Tracks_Claimed_Tiers()
    {
        MasteryRewards rewards = BuildMasteryRewards();

        using (Assert.Multiple())
        {
            await Assert.That(rewards.HasObtained(15)).IsFalse();
            await Assert.That(rewards.MarkObtained(15)).IsTrue();
            await Assert.That(rewards.HasObtained(15)).IsTrue();

            // Claiming The Same Tier Again Is A No-Op
            await Assert.That(rewards.MarkObtained(15)).IsFalse();
            await Assert.That(rewards.HasObtained(20)).IsFalse();
        }
    }

    [Test]
    public async Task Mastery_Boost_Consumables_Add_And_Remove_Independently_Of_Super_Boosts()
    {
        User user = BuildUser();

        MasteryConsumables.AddMasteryBoost(user, 3);
        MasteryConsumables.AddMasteryBoost(user);
        MasteryConsumables.AddSuperMasteryBoost(user);

        using (Assert.Multiple())
        {
            await Assert.That(MasteryConsumables.MasteryBoostsOwned(user)).IsEqualTo(4);
            await Assert.That(MasteryConsumables.SuperMasteryBoostsOwned(user)).IsEqualTo(1);
        }

        MasteryConsumables.RemoveMasteryBoost(user, 2);

        await Assert.That(MasteryConsumables.MasteryBoostsOwned(user)).IsEqualTo(2);

        // Removing More Boosts Than Are Owned Clears The Consumable Entry Entirely
        MasteryConsumables.RemoveMasteryBoost(user, 5);

        using (Assert.Multiple())
        {
            await Assert.That(MasteryConsumables.MasteryBoostsOwned(user)).IsEqualTo(0);
            await Assert.That(MasteryConsumables.SuperMasteryBoostsOwned(user)).IsEqualTo(1);
        }
    }

    [Test]
    public async Task ResolveUnusableCoupons_Only_Considers_The_Avatars_Of_The_Coupons_Exact_Hero()
    {
        // "Hero_Chi" (Qi) Is A Prefix Of "Hero_Chipper", Whose Avatars Must Not Count Towards Qi's Coupon
        List<string> qiAvatars = MasteryCouponHelper.ApplicableAvatars("Hero_Chi");

        User user = BuildUser([.. qiAvatars, "cp.Qi Mastery Coupon * 1"]);

        MasteryConsumables.ResolveUnusableCoupons(user);

        using (Assert.Multiple())
        {
            await Assert.That(user.OwnedStoreItems.Any(item => item.StartsWith("cp.Qi Mastery Coupon * ", StringComparison.Ordinal))).IsFalse();
            await Assert.That(user.OwnedStoreItems).Contains("cp.All Avatar Mastery Coupon * 1");
        }
    }

    [Test]
    [Arguments("Blood Hunter", "Hero_Hunter")]
    [Arguments("Amun-Ra", "Hero_Ra")]
    [Arguments("amun-ra", "Hero_Ra")]
    [Arguments("Master Of Arms", "Hero_MasterOfArms")]
    [Arguments("Qi", "Hero_Chi")]
    [Arguments("Xemplar", "Hero_Mimix")]
    public async Task GetIdentifierByName_Resolves_Display_Names_To_Identifiers(string displayName, string expectedIdentifier)
        => await Assert.That(Heroes.GetIdentifierByName(displayName)).IsEqualTo(expectedIdentifier);

    [Test]
    public async Task GetIdentifierByName_Returns_Null_For_An_Unknown_Display_Name()
        => await Assert.That(Heroes.GetIdentifierByName("Not A Real Hero")).IsNull();

    [Test]
    [Arguments("Hero_Hunter", "Blood Hunter")]
    [Arguments("Hero_Ra", "Amun-Ra")]
    public async Task GetNameByIdentifier_Resolves_Identifiers_To_Display_Names(string identifier, string expectedName)
        => await Assert.That(Heroes.GetNameByIdentifier(identifier)).IsEqualTo(expectedName);
}
