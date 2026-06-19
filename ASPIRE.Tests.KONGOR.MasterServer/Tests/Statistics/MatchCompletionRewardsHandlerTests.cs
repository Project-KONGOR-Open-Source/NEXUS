namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Statistics;

/// <summary>
///     Tests for <see cref="MatchCompletionRewardsHandler"/> covering match rewards, the additive post-signup bonus, and <see cref="AccountStatistics"/> counter maintenance.
/// </summary>
public sealed class MatchCompletionRewardsHandlerTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithRedisContainer().InitialiseAsync();

    [Test]
    public async Task Apply_Solo_Win_Main_Account_First_Match_Applies_Match_Reward_And_Post_Signup_Bonus()
    {
        Account account = await SeedMainAccount("solo.win.first@kongor.com", "SoloWinFirst");

        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(MatchType.AM_PUBLIC);
        MatchParticipantStatistics participant = MatchDataHelper.BuildParticipant(account.ID, account.Name, groupNumber: 1, win: 1);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account trackedAccount = await databaseContext.Accounts.Include(candidate => candidate.User).SingleAsync(candidate => candidate.ID == account.ID);

        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, MatchDataHelper.BuildMatchStatistics(), participant);
        await databaseContext.SaveChangesAsync();

        Win winReward = JSONConfiguration.EconomyConfiguration.MatchRewards.Solo.Win;
        PostSignupBonus bonus = JSONConfiguration.EconomyConfiguration.EventRewards.PostSignupBonus;

        User user = await databaseContext.Users.SingleAsync(record => record.ID == trackedAccount.User.ID);
        AccountStatistics statistics = await databaseContext.AccountStatistics.SingleAsync(record => record.AccountID == trackedAccount.ID && record.Type == AccountStatisticsType.Public);

        using (Assert.Multiple())
        {
            await Assert.That(user.GoldCoins).IsEqualTo(winReward.GoldCoins + bonus.GoldCoins);
            await Assert.That(user.SilverCoins).IsEqualTo(winReward.SilverCoins + bonus.SilverCoins);
            await Assert.That(user.PlinkoTickets).IsEqualTo(winReward.PlinkoTickets + bonus.PlinkoTickets);

            await Assert.That(statistics.MatchesPlayed).IsEqualTo(1);
            await Assert.That(statistics.MatchesWon).IsEqualTo(1);
            await Assert.That(statistics.MatchesLost).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Apply_Solo_Loss_Main_Account_First_Match_Applies_Loss_Reward_And_Post_Signup_Bonus()
    {
        Account account = await SeedMainAccount("solo.loss.first@kongor.com", "SoloLossFirst");

        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(MatchType.AM_PUBLIC);
        MatchParticipantStatistics participant = MatchDataHelper.BuildParticipant(account.ID, account.Name, groupNumber: 1, loss: 1);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account trackedAccount = await databaseContext.Accounts.Include(candidate => candidate.User).SingleAsync(candidate => candidate.ID == account.ID);

        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, MatchDataHelper.BuildMatchStatistics(), participant);
        await databaseContext.SaveChangesAsync();

        Loss lossReward = JSONConfiguration.EconomyConfiguration.MatchRewards.Solo.Loss;
        PostSignupBonus bonus = JSONConfiguration.EconomyConfiguration.EventRewards.PostSignupBonus;

        User user = await databaseContext.Users.SingleAsync(record => record.ID == trackedAccount.User.ID);
        AccountStatistics statistics = await databaseContext.AccountStatistics.SingleAsync(record => record.AccountID == trackedAccount.ID && record.Type == AccountStatisticsType.Public);

        using (Assert.Multiple())
        {
            await Assert.That(user.GoldCoins).IsEqualTo(lossReward.GoldCoins + bonus.GoldCoins);
            await Assert.That(user.SilverCoins).IsEqualTo(lossReward.SilverCoins + bonus.SilverCoins);
            await Assert.That(user.PlinkoTickets).IsEqualTo(lossReward.PlinkoTickets + bonus.PlinkoTickets);

            await Assert.That(statistics.MatchesPlayed).IsEqualTo(1);
            await Assert.That(statistics.MatchesWon).IsEqualTo(0);
            await Assert.That(statistics.MatchesLost).IsEqualTo(1);
        }
    }

    [Test]
    public async Task Apply_Alt_Account_Does_Not_Apply_Post_Signup_Bonus()
    {
        Account mainAccount = await SeedMainAccount("alt.host@kongor.com", "AltHost");
        Account altAccount = await SeedAltAccount(mainAccount, "AltChild");

        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(MatchType.AM_PUBLIC);
        MatchParticipantStatistics participant = MatchDataHelper.BuildParticipant(altAccount.ID, altAccount.Name, groupNumber: 1, win: 1);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account trackedAccount = await databaseContext.Accounts.Include(candidate => candidate.User).SingleAsync(candidate => candidate.ID == altAccount.ID);

        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, MatchDataHelper.BuildMatchStatistics(), participant);
        await databaseContext.SaveChangesAsync();

        Win winReward = JSONConfiguration.EconomyConfiguration.MatchRewards.Solo.Win;

        User user = await databaseContext.Users.SingleAsync(record => record.ID == trackedAccount.User.ID);

        using (Assert.Multiple())
        {
            await Assert.That(user.GoldCoins).IsEqualTo(winReward.GoldCoins);
            await Assert.That(user.SilverCoins).IsEqualTo(winReward.SilverCoins);
            await Assert.That(user.PlinkoTickets).IsEqualTo(winReward.PlinkoTickets);
        }
    }

    [Test]
    public async Task Apply_Main_Account_Beyond_Threshold_Does_Not_Apply_Post_Signup_Bonus()
    {
        Account account = await SeedMainAccount("veteran@kongor.com", "Veteran");

        int matchesCount = JSONConfiguration.EconomyConfiguration.EventRewards.PostSignupBonus.MatchesCount;

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account trackedAccount = await databaseContext.Accounts.Include(candidate => candidate.User).SingleAsync(candidate => candidate.ID == account.ID);

        // Seed Enough Prior Participation Rows So That "MatchesPlayedBeforeThis" Meets The Threshold
        for (int priorMatchID = 1; priorMatchID <= matchesCount; priorMatchID++)
        {
            await databaseContext.MatchParticipantStatistics.AddAsync(MatchDataHelper.BuildParticipant(trackedAccount.ID, trackedAccount.Name, groupNumber: 1, win: 1, matchID: priorMatchID));
        }

        await databaseContext.SaveChangesAsync();

        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(MatchType.AM_PUBLIC);
        MatchParticipantStatistics participant = MatchDataHelper.BuildParticipant(trackedAccount.ID, trackedAccount.Name, groupNumber: 1, win: 1, matchID: matchesCount + 1);

        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, MatchDataHelper.BuildMatchStatistics(), participant);
        await databaseContext.SaveChangesAsync();

        Win winReward = JSONConfiguration.EconomyConfiguration.MatchRewards.Solo.Win;

        User user = await databaseContext.Users.SingleAsync(record => record.ID == trackedAccount.User.ID);

        using (Assert.Multiple())
        {
            await Assert.That(user.GoldCoins).IsEqualTo(winReward.GoldCoins);
            await Assert.That(user.SilverCoins).IsEqualTo(winReward.SilverCoins);
            await Assert.That(user.PlinkoTickets).IsEqualTo(winReward.PlinkoTickets);
        }
    }

    [Test]
    [Arguments(-1, "Solo")]
    [Arguments(+1, "Solo")]
    [Arguments(+2, "TwoPersonGroup")]
    [Arguments(+3, "ThreePersonGroup")]
    [Arguments(+4, "FourPersonGroup")]
    [Arguments(+5, "FivePersonGroup")]
    public async Task Apply_Group_Number_Selects_Matching_Reward_Partition(int groupNumber, string partitionName)
    {
        Account account = await SeedMainAccount($"group.{groupNumber}@kongor.com", $"Group{groupNumber}");

        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(MatchType.AM_PUBLIC);
        MatchParticipantStatistics participant = MatchDataHelper.BuildParticipant(account.ID, account.Name, groupNumber: groupNumber, win: 1);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account trackedAccount = await databaseContext.Accounts.Include(candidate => candidate.User).SingleAsync(candidate => candidate.ID == account.ID);

        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, MatchDataHelper.BuildMatchStatistics(), participant);
        await databaseContext.SaveChangesAsync();

        MatchRewards matchRewards = JSONConfiguration.EconomyConfiguration.MatchRewards;
        PostSignupBonus bonus = JSONConfiguration.EconomyConfiguration.EventRewards.PostSignupBonus;

        Win expected = partitionName switch
        {
            "Solo"             => matchRewards.Solo.Win,
            "TwoPersonGroup"   => matchRewards.TwoPersonGroup.Win,
            "ThreePersonGroup" => matchRewards.ThreePersonGroup.Win,
            "FourPersonGroup"  => matchRewards.FourPersonGroup.Win,
            "FivePersonGroup"  => matchRewards.FivePersonGroup.Win,
            _                  => throw new ArgumentOutOfRangeException(nameof(partitionName), partitionName, null)
        };

        User user = await databaseContext.Users.SingleAsync(record => record.ID == trackedAccount.User.ID);

        using (Assert.Multiple())
        {
            await Assert.That(user.GoldCoins).IsEqualTo(expected.GoldCoins + bonus.GoldCoins);
            await Assert.That(user.SilverCoins).IsEqualTo(expected.SilverCoins + bonus.SilverCoins);
            await Assert.That(user.PlinkoTickets).IsEqualTo(expected.PlinkoTickets + bonus.PlinkoTickets);
        }
    }

    [Test]
    public async Task Apply_Ranked_Rating_Change_Adjusts_The_Matchmaking_Skill_Rating()
    {
        Account account = await SeedMainAccount("ranked.gain@kongor.com", "RankedGain");

        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(MatchType.AM_MATCHMAKING);
        MatchParticipantStatistics participant = MatchDataHelper.BuildParticipant(account.ID, account.Name, groupNumber: 1, win: 1, publicMatch: 0, rankedMatch: 1, rankedSkillRatingChange: 5.5);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account trackedAccount = await databaseContext.Accounts.Include(candidate => candidate.User).SingleAsync(candidate => candidate.ID == account.ID);

        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, MatchDataHelper.BuildMatchStatistics(), participant);
        await databaseContext.SaveChangesAsync();

        AccountStatistics statistics = await databaseContext.AccountStatistics.SingleAsync(record => record.AccountID == trackedAccount.ID && record.Type == AccountStatisticsType.Matchmaking);

        await Assert.That(statistics.SkillRating).IsEqualTo(1505.5);
    }

    [Test]
    public async Task Apply_Ranked_Rating_Loss_Does_Not_Drop_The_Skill_Rating_Below_The_Minimum()
    {
        Account account = await SeedMainAccount("ranked.floor@kongor.com", "RankedFloor");

        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(MatchType.AM_MATCHMAKING);
        MatchParticipantStatistics participant = MatchDataHelper.BuildParticipant(account.ID, account.Name, groupNumber: 1, loss: 1, publicMatch: 0, rankedMatch: 1, rankedSkillRatingChange: -501.0);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account trackedAccount = await databaseContext.Accounts.Include(candidate => candidate.User).SingleAsync(candidate => candidate.ID == account.ID);

        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, MatchDataHelper.BuildMatchStatistics(), participant);
        await databaseContext.SaveChangesAsync();

        AccountStatistics statistics = await databaseContext.AccountStatistics.SingleAsync(record => record.AccountID == trackedAccount.ID && record.Type == AccountStatisticsType.Matchmaking);

        // A Default Rating Of 1500 Minus 501 Would Fall Below The Floor Of 1000, So The Rating Is Clamped
        await Assert.That(statistics.SkillRating).IsEqualTo(1000.0);
    }

    [Test]
    public async Task Apply_Public_Rating_Change_Adjusts_The_Public_Skill_Rating()
    {
        Account account = await SeedMainAccount("public.gain@kongor.com", "PublicGain");

        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(MatchType.AM_PUBLIC);
        MatchParticipantStatistics participant = MatchDataHelper.BuildParticipant(account.ID, account.Name, groupNumber: -1, win: 1, publicSkillRatingChange: 3.25);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account trackedAccount = await databaseContext.Accounts.Include(candidate => candidate.User).SingleAsync(candidate => candidate.ID == account.ID);

        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, MatchDataHelper.BuildMatchStatistics(), participant);
        await databaseContext.SaveChangesAsync();

        AccountStatistics statistics = await databaseContext.AccountStatistics.SingleAsync(record => record.AccountID == trackedAccount.ID && record.Type == AccountStatisticsType.Public);

        await Assert.That(statistics.SkillRating).IsEqualTo(1503.25);
    }

    [Test]
    public async Task Apply_Ranked_Resubmission_Routes_To_The_Matchmaking_Row_And_Leaves_The_Public_Row_Untouched()
    {
        Account account = await SeedMainAccount("ranked.fallback@kongor.com", "RankedFallback");

        // A Resubmission Has No Match Information Snapshot, So The Type Is Derived From The Submitted Flags And Map: A Ranked Match On "caldavar" Resolves To The Matchmaking Row, Not The Public Row
        MatchParticipantStatistics participant = MatchDataHelper.BuildParticipant(account.ID, account.Name, groupNumber: 1, win: 1, publicMatch: 0, rankedMatch: 1, rankedSkillRatingChange: 7.0);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account trackedAccount = await databaseContext.Accounts.Include(candidate => candidate.User).SingleAsync(candidate => candidate.ID == account.ID);

        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation: null, MatchDataHelper.BuildMatchStatistics(map: "caldavar"), participant);
        await databaseContext.SaveChangesAsync();

        AccountStatistics matchmakingStatistics = await databaseContext.AccountStatistics.SingleAsync(record => record.AccountID == trackedAccount.ID && record.Type == AccountStatisticsType.Matchmaking);
        AccountStatistics publicStatistics = await databaseContext.AccountStatistics.SingleAsync(record => record.AccountID == trackedAccount.ID && record.Type == AccountStatisticsType.Public);

        using (Assert.Multiple())
        {
            await Assert.That(matchmakingStatistics.SkillRating).IsEqualTo(1507.0);
            await Assert.That(matchmakingStatistics.MatchesPlayed).IsEqualTo(1);
            await Assert.That(publicStatistics.SkillRating).IsEqualTo(1500.0);
            await Assert.That(publicStatistics.MatchesPlayed).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Apply_Records_A_Placement_Result_During_The_Placement_Phase()
    {
        Account account = await SeedMainAccount("placement.record@kongor.com", "PlacementRecord");

        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(MatchType.AM_MATCHMAKING);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account trackedAccount = await databaseContext.Accounts.Include(candidate => candidate.User).SingleAsync(candidate => candidate.ID == account.ID);

        MatchParticipantStatistics winParticipant  = MatchDataHelper.BuildParticipant(account.ID, account.Name, groupNumber: 1, win: 1, matchID: 1, publicMatch: 0, rankedMatch: 1);
        MatchParticipantStatistics lossParticipant = MatchDataHelper.BuildParticipant(account.ID, account.Name, groupNumber: 1, loss: 1, matchID: 2, publicMatch: 0, rankedMatch: 1);

        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, MatchDataHelper.BuildMatchStatistics(), winParticipant);
        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, MatchDataHelper.BuildMatchStatistics(), lossParticipant);
        await databaseContext.SaveChangesAsync();

        AccountStatistics statistics = await databaseContext.AccountStatistics.SingleAsync(record => record.AccountID == trackedAccount.ID && record.Type == AccountStatisticsType.Matchmaking);

        await Assert.That(statistics.PlacementMatchesData).IsEqualTo("10");
    }

    [Test]
    public async Task Apply_Does_Not_Record_A_Placement_Result_Beyond_The_Placement_Phase()
    {
        Account account = await SeedMainAccount("placement.complete@kongor.com", "PlacementDone");

        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(MatchType.AM_MATCHMAKING);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account trackedAccount = await databaseContext.Accounts.Include(candidate => candidate.User).SingleAsync(candidate => candidate.ID == account.ID);

        AccountStatistics statistics = await databaseContext.AccountStatistics.SingleAsync(record => record.AccountID == trackedAccount.ID && record.Type == AccountStatisticsType.Matchmaking);

        statistics.PlacementMatchesData = "110100";

        await databaseContext.SaveChangesAsync();

        MatchParticipantStatistics participant = MatchDataHelper.BuildParticipant(account.ID, account.Name, groupNumber: 1, win: 1, publicMatch: 0, rankedMatch: 1);

        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, MatchDataHelper.BuildMatchStatistics(), participant);
        await databaseContext.SaveChangesAsync();

        await Assert.That(statistics.PlacementMatchesData).IsEqualTo("110100");
    }

    [Test]
    public async Task Apply_Does_Not_Record_A_Placement_Result_For_A_Disconnected_Participant()
    {
        Account account = await SeedMainAccount("placement.leaver@kongor.com", "PlacementLeaver");

        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(MatchType.AM_MATCHMAKING);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account trackedAccount = await databaseContext.Accounts.Include(candidate => candidate.User).SingleAsync(candidate => candidate.ID == account.ID);

        MatchParticipantStatistics participant = MatchDataHelper.BuildParticipant(account.ID, account.Name, groupNumber: 1, loss: 1, publicMatch: 0, rankedMatch: 1, disconnected: 1);

        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, MatchDataHelper.BuildMatchStatistics(), participant);
        await databaseContext.SaveChangesAsync();

        AccountStatistics statistics = await databaseContext.AccountStatistics.SingleAsync(record => record.AccountID == trackedAccount.ID && record.Type == AccountStatisticsType.Matchmaking);

        await Assert.That(statistics.PlacementMatchesData).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task Apply_Does_Not_Record_Placement_Data_For_An_Untracked_Queue()
    {
        Account account = await SeedMainAccount("placement.untracked@kongor.com", "PlacementNone");

        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(MatchType.AM_MATCHMAKING_MIDWARS, map: "midwars");

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account trackedAccount = await databaseContext.Accounts.Include(candidate => candidate.User).SingleAsync(candidate => candidate.ID == account.ID);

        MatchParticipantStatistics participant = MatchDataHelper.BuildParticipant(account.ID, account.Name, groupNumber: 1, win: 1, publicMatch: 0, rankedMatch: 1);

        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, MatchDataHelper.BuildMatchStatistics(), participant);
        await databaseContext.SaveChangesAsync();

        AccountStatistics statistics = await databaseContext.AccountStatistics.SingleAsync(record => record.AccountID == trackedAccount.ID && record.Type == AccountStatisticsType.MidWars);

        await Assert.That(statistics.PlacementMatchesData).IsNull();
    }

    [Test]
    [Arguments(MatchType.AM_PUBLIC,               false, "caldavar",        AccountStatisticsType.Public)]
    [Arguments(MatchType.AM_MATCHMAKING,          false, "caldavar",        AccountStatisticsType.Matchmaking)]
    [Arguments(MatchType.AM_MATCHMAKING,          true,  "caldavar",        AccountStatisticsType.MatchmakingCasual)]
    [Arguments(MatchType.AM_UNRANKED_MATCHMAKING, false, "caldavar",        AccountStatisticsType.MatchmakingCasual)]
    [Arguments(MatchType.AM_MATCHMAKING_BOTMATCH, false, "caldavar",        AccountStatisticsType.Cooperative)]
    [Arguments(MatchType.AM_MATCHMAKING_MIDWARS,  false, "midwars",         AccountStatisticsType.MidWars)]
    [Arguments(MatchType.AM_MATCHMAKING_MIDWARS,  false, "caldavar_reborn", AccountStatisticsType.Matchmaking)]
    [Arguments(MatchType.AM_MATCHMAKING_MIDWARS,  true,  "caldavar_reborn", AccountStatisticsType.MatchmakingCasual)]
    [Arguments(MatchType.AM_MATCHMAKING_RIFTWARS, false, "riftwars",        AccountStatisticsType.RiftWars)]
    public async Task Resolve_Account_Statistics_Type_Maps_Match_Type_Correctly(MatchType matchType, bool isCasual, string map, AccountStatisticsType expected)
    {
        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(matchType, isCasual, map);

        AccountStatisticsType resolved = MatchCompletionRewardsHandler.ResolveAccountStatisticsType(matchInformation);

        await Assert.That(resolved).IsEqualTo(expected);
    }

    [Test]
    public async Task Resolve_Account_Statistics_Type_NULL_Match_Information_Falls_Back_To_Public()
    {
        AccountStatisticsType resolved = MatchCompletionRewardsHandler.ResolveAccountStatisticsType(matchInformation: null);

        await Assert.That(resolved).IsEqualTo(AccountStatisticsType.Public);
    }

    [Test]
    public async Task Apply_Accumulates_Primitive_Statistics_Into_Resolved_Account_Statistics()
    {
        Account account = await SeedMainAccount("primitive.stats@kongor.com", "PrimitiveStats");

        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(MatchType.AM_PUBLIC);
        MatchParticipantStatistics participant = MatchDataHelper.BuildParticipant(account.ID, account.Name, groupNumber: 1, win: 1);
        participant.HeroKills = 10;
        participant.HeroAssists = 12;
        participant.HeroDeaths = 5;
        participant.WardsPlaced = 4;
        participant.Smackdown = 2;

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account trackedAccount = await databaseContext.Accounts.Include(candidate => candidate.User).SingleAsync(candidate => candidate.ID == account.ID);

        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, MatchDataHelper.BuildMatchStatistics(), participant);
        await databaseContext.SaveChangesAsync();

        AccountStatistics statistics = await databaseContext.AccountStatistics.SingleAsync(record => record.AccountID == trackedAccount.ID && record.Type == AccountStatisticsType.Public);

        using (Assert.Multiple())
        {
            await Assert.That(statistics.HeroKills).IsEqualTo(10);
            await Assert.That(statistics.HeroAssists).IsEqualTo(12);
            await Assert.That(statistics.HeroDeaths).IsEqualTo(5);
            await Assert.That(statistics.WardsPlaced).IsEqualTo(4);
            await Assert.That(statistics.Smackdowns).IsEqualTo(2);
        }
    }

    [Test]
    public async Task Apply_Accumulates_Per_Hero_And_Award_Statistics_Into_Resolved_Row()
    {
        Account account = await SeedMainAccount("hero.award.stats@kongor.com", "HeroAwardStats");

        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(MatchType.AM_PUBLIC);

        MatchParticipantStatistics participant = MatchDataHelper.BuildParticipant(account.ID, account.Name, groupNumber: 1, win: 1);
        participant.HeroIdentifier = "Hero_Engineer";
        participant.HeroKills = 7;
        participant.HeroAssists = 9;
        participant.HeroDeaths = 3;
        participant.TeamCreepKills = 120;
        participant.Denies = 11;

        MatchStatistics matchStatistics = MatchDataHelper.BuildMatchStatistics();
        matchStatistics.MVPAccountID = account.ID;
        matchStatistics.AwardMostKills = account.ID;

        using (IServiceScope writeScope = webApplicationFactory.Services.CreateScope())
        {
            MerrickContext writeContext = writeScope.ServiceProvider.GetRequiredService<MerrickContext>();

            Account trackedAccount = await writeContext.Accounts.Include(candidate => candidate.User).SingleAsync(candidate => candidate.ID == account.ID);

            await MatchCompletionRewardsHandler.Apply(writeContext, NullLogger.Instance, trackedAccount, matchInformation, matchStatistics, participant);
            await writeContext.SaveChangesAsync();
        }

        // Re-read From A Fresh Context To Confirm The Owned JSON Columns Were Actually Persisted
        using IServiceScope readScope = webApplicationFactory.Services.CreateScope();

        MerrickContext readContext = readScope.ServiceProvider.GetRequiredService<MerrickContext>();

        AccountStatistics statistics = await readContext.AccountStatistics.SingleAsync(record => record.AccountID == account.ID && record.Type == AccountStatisticsType.Public);

        HeroStats heroStats = statistics.HeroStatistics.Heroes.Single(hero => hero.HeroIdentifier == "Hero_Engineer");

        using (Assert.Multiple())
        {
            await Assert.That(heroStats.GamesPlayed).IsEqualTo(1);
            await Assert.That(heroStats.Wins).IsEqualTo(1);
            await Assert.That(heroStats.HeroKills).IsEqualTo(7);
            await Assert.That(heroStats.HeroAssists).IsEqualTo(9);
            await Assert.That(heroStats.HeroDeaths).IsEqualTo(3);
            await Assert.That(heroStats.TeamCreepKills).IsEqualTo(120);
            await Assert.That(heroStats.Denies).IsEqualTo(11);

            await Assert.That(statistics.AwardStatistics.MVPAwards).IsEqualTo(1);
            await Assert.That(statistics.AwardStatistics.MostKillsAwards).IsEqualTo(1);
            await Assert.That(statistics.AwardStatistics.AnnihilationAwards).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Apply_Routes_Statistics_To_The_Matchmaking_Row_For_A_Ranked_Match()
    {
        Account account = await SeedMainAccount("ranked.routing@kongor.com", "RankedRouting");

        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(MatchType.AM_MATCHMAKING);

        MatchParticipantStatistics participant = MatchDataHelper.BuildParticipant(account.ID, account.Name, groupNumber: 1, win: 1, publicMatch: 0, rankedMatch: 1);
        participant.HeroIdentifier = "Hero_Engineer";
        participant.HeroKills = 5;

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account trackedAccount = await databaseContext.Accounts.Include(candidate => candidate.User).SingleAsync(candidate => candidate.ID == account.ID);

        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, MatchDataHelper.BuildMatchStatistics(), participant);
        await databaseContext.SaveChangesAsync();

        AccountStatistics matchmakingStatistics = await databaseContext.AccountStatistics.SingleAsync(record => record.AccountID == account.ID && record.Type == AccountStatisticsType.Matchmaking);
        AccountStatistics publicStatistics = await databaseContext.AccountStatistics.SingleAsync(record => record.AccountID == account.ID && record.Type == AccountStatisticsType.Public);

        using (Assert.Multiple())
        {
            await Assert.That(matchmakingStatistics.MatchesPlayed).IsEqualTo(1);
            await Assert.That(matchmakingStatistics.HeroKills).IsEqualTo(5);
            await Assert.That(matchmakingStatistics.HeroStatistics.Heroes.Count).IsEqualTo(1);

            await Assert.That(publicStatistics.MatchesPlayed).IsEqualTo(0);
            await Assert.That(publicStatistics.HeroKills).IsEqualTo(0);
            await Assert.That(publicStatistics.HeroStatistics.Heroes.Count).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Apply_Accumulates_Statistics_Across_Multiple_Matches()
    {
        Account account = await SeedMainAccount("cumulative.stats@kongor.com", "CumulativeStats");

        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(MatchType.AM_PUBLIC);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account trackedAccount = await databaseContext.Accounts.Include(candidate => candidate.User).SingleAsync(candidate => candidate.ID == account.ID);

        MatchParticipantStatistics firstParticipant = MatchDataHelper.BuildParticipant(account.ID, account.Name, groupNumber: 1, win: 1, matchID: 1);
        firstParticipant.HeroIdentifier = "Hero_Engineer";
        firstParticipant.HeroKills = 4;

        MatchParticipantStatistics secondParticipant = MatchDataHelper.BuildParticipant(account.ID, account.Name, groupNumber: 1, loss: 1, matchID: 2);
        secondParticipant.HeroIdentifier = "Hero_Engineer";
        secondParticipant.HeroKills = 6;

        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, MatchDataHelper.BuildMatchStatistics(matchID: 1), firstParticipant);
        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, MatchDataHelper.BuildMatchStatistics(matchID: 2), secondParticipant);
        await databaseContext.SaveChangesAsync();

        AccountStatistics statistics = await databaseContext.AccountStatistics.SingleAsync(record => record.AccountID == account.ID && record.Type == AccountStatisticsType.Public);

        HeroStats heroStats = statistics.HeroStatistics.Heroes.Single(hero => hero.HeroIdentifier == "Hero_Engineer");

        using (Assert.Multiple())
        {
            await Assert.That(statistics.MatchesPlayed).IsEqualTo(2);
            await Assert.That(statistics.MatchesWon).IsEqualTo(1);
            await Assert.That(statistics.MatchesLost).IsEqualTo(1);
            await Assert.That(statistics.HeroKills).IsEqualTo(10);
            await Assert.That(heroStats.GamesPlayed).IsEqualTo(2);
            await Assert.That(heroStats.HeroKills).IsEqualTo(10);
        }
    }

    [Test]
    public async Task Apply_Routes_Statistics_To_The_Cooperative_Row_For_A_Bot_Match()
    {
        Account account = await SeedMainAccount("coop.routing@kongor.com", "CoopRouting");

        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(MatchType.AM_MATCHMAKING_BOTMATCH);

        MatchParticipantStatistics participant = MatchDataHelper.BuildParticipant(account.ID, account.Name, groupNumber: 1, win: 1);
        participant.HeroIdentifier = "Hero_Engineer";
        participant.HeroKills = 8;

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account trackedAccount = await databaseContext.Accounts.Include(candidate => candidate.User).SingleAsync(candidate => candidate.ID == account.ID);

        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, MatchDataHelper.BuildMatchStatistics(), participant);
        await databaseContext.SaveChangesAsync();

        AccountStatistics cooperativeStatistics = await databaseContext.AccountStatistics.SingleAsync(record => record.AccountID == account.ID && record.Type == AccountStatisticsType.Cooperative);
        AccountStatistics publicStatistics = await databaseContext.AccountStatistics.SingleAsync(record => record.AccountID == account.ID && record.Type == AccountStatisticsType.Public);

        using (Assert.Multiple())
        {
            await Assert.That(cooperativeStatistics.MatchesPlayed).IsEqualTo(1);
            await Assert.That(cooperativeStatistics.HeroKills).IsEqualTo(8);
            await Assert.That(cooperativeStatistics.HeroStatistics.Heroes.Count).IsEqualTo(1);

            await Assert.That(publicStatistics.MatchesPlayed).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Campaign_Response_Surfaces_Aggregated_Detailed_Statistics()
    {
        Account account = await SeedMainAccount("aggregated.detail@kongor.com", "AggDetail");

        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(MatchType.AM_MATCHMAKING);

        MatchParticipantStatistics participant = MatchDataHelper.BuildParticipant(account.ID, account.Name, groupNumber: 1, win: 1, publicMatch: 0, rankedMatch: 1);

        participant.HeroIdentifier = "Hero_Engineer";
        participant.HeroDamage = 1234;
        participant.Denies = 17;
        participant.BuildingDamage = 5000;
        participant.Gold = 20000;
        participant.QuadKill = 1;

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account trackedAccount = await databaseContext.Accounts
            .Include(candidate => candidate.User).ThenInclude(user => user.Accounts)
            .Include(candidate => candidate.Clan)
            .SingleAsync(candidate => candidate.ID == account.ID);

        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, MatchDataHelper.BuildMatchStatistics(), participant);
        await databaseContext.SaveChangesAsync();

        Dictionary<AccountStatisticsType, AccountStatistics> statisticsByType = await databaseContext.AccountStatistics
            .Where(record => record.AccountID == account.ID)
            .ToDictionaryAsync(record => record.Type);

        AggregateStatistics aggregates = AggregateStatistics.FromStatistics(statisticsByType);

        CampaignStatisticsResponse response = new (trackedAccount, statisticsByType[AccountStatisticsType.Matchmaking], aggregates);

        using (Assert.Multiple())
        {
            await Assert.That(response.HeroDamage).IsEqualTo("1234");
            await Assert.That(response.Denies).IsEqualTo("17");
            await Assert.That(response.BuildingDamage).IsEqualTo("5000");
            await Assert.That(response.Gold).IsEqualTo("20000");
            await Assert.That(response.QuadKills).IsEqualTo("1");
        }
    }

    [Test]
    public async Task Apply_Ranked_Matchmaking_Match_Accrues_Mastery_Experience()
    {
        Account account = await SeedMainAccount("mastery.accrual@kongor.com", "MasteryAccrue");

        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(MatchType.AM_MATCHMAKING);

        MatchParticipantStatistics participant = MatchDataHelper.BuildParticipant(account.ID, account.Name, groupNumber: 1, win: 1, publicMatch: 0, rankedMatch: 1);

        participant.HeroIdentifier = "Hero_Accursed";
        participant.HeroLevel = 20;

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account trackedAccount = await databaseContext.Accounts.Include(candidate => candidate.User).SingleAsync(candidate => candidate.ID == account.ID);

        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, MatchDataHelper.BuildMatchStatistics(), participant);

        await databaseContext.SaveChangesAsync();

        Mastery mastery = await databaseContext.Masteries.SingleAsync(record => record.AccountID == trackedAccount.ID);

        // Ranked Normal Matchmaking Awards (Hero Level * 20); A Fresh Account Has No Maximum-Level Heroes, So The Bonus Experience Is Zero
        await Assert.That(mastery.GetHeroExperienceByHeroIdentifier("Hero_Accursed")).IsEqualTo(20 * 20);
    }

    [Test]
    public async Task Apply_Match_That_Crosses_A_Mastery_Level_Issues_The_Hero_Level_Reward()
    {
        Account account = await SeedMainAccount("mastery.levelup@kongor.com", "MasteryLevelUp");

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account trackedAccount = await databaseContext.Accounts.Include(candidate => candidate.User).SingleAsync(candidate => candidate.ID == account.ID);

        Mastery seededMastery = await databaseContext.Masteries.SingleAsync(record => record.AccountID == trackedAccount.ID);

        // Pre-Set The Hero Just Below The Level 1 Threshold (1400) So That A Single Match Crosses It
        seededMastery.SetHeroExperienceByHeroIdentifier("Hero_Accursed", 1399);

        await databaseContext.SaveChangesAsync();

        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(MatchType.AM_MATCHMAKING);

        MatchParticipantStatistics participant = MatchDataHelper.BuildParticipant(account.ID, account.Name, groupNumber: 1, win: 1, publicMatch: 0, rankedMatch: 1);

        participant.HeroIdentifier = "Hero_Accursed";
        participant.HeroLevel = 1;

        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, MatchDataHelper.BuildMatchStatistics(), participant);

        await databaseContext.SaveChangesAsync();

        Mastery mastery = await databaseContext.Masteries.SingleAsync(record => record.AccountID == trackedAccount.ID);

        User user = await databaseContext.Users.SingleAsync(record => record.ID == trackedAccount.User.ID);

        using (Assert.Multiple())
        {
            await Assert.That(mastery.GetHeroLevelByHeroIdentifier("Hero_Accursed")).IsEqualTo(1);

            // The Level 1 Per-Hero Reward Is The Hero's Mastery Account Icon
            await Assert.That(user.OwnedStoreItems.Any(item => item.Contains("Mastery Icon - Accursed"))).IsTrue();
        }
    }

    private async Task<Account> SeedMainAccount(string emailAddress, string accountName)
    {
        SRPAuthenticationService service = new (webApplicationFactory);

        (Account account, string _) = await service.CreateAccountWithSRPCredentials(emailAddress, accountName, "DoesNotMatter123!");

        return account;
    }

    private async Task<Account> SeedAltAccount(Account mainAccount, string altAccountName)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        User user = await databaseContext.Users.SingleAsync(candidate => candidate.ID == mainAccount.User.ID);

        Account altAccount = new ()
        {
            Name = altAccountName,
            User = user,
            IsMain = false
        };

        await databaseContext.Accounts.AddAsync(altAccount);
        await databaseContext.SaveChangesAsync();

        return altAccount;
    }

}
