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
    public async Task Apply_SoloWinMainAccountFirstMatch_AppliesMatchRewardAndPostSignupBonus()
    {
        Account account = await SeedMainAccount("solo.win.first@kongor.com", "SoloWinFirst");

        MatchInformation matchInformation = BuildMatchInformation(MatchType.AM_PUBLIC);
        MatchParticipantStatistics participant = BuildParticipant(account.ID, account.Name, groupNumber: 1, win: 1);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account trackedAccount = await databaseContext.Accounts.Include(candidate => candidate.User).SingleAsync(candidate => candidate.ID == account.ID);

        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, participant);
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
    public async Task Apply_SoloLossMainAccountFirstMatch_AppliesLossRewardAndPostSignupBonus()
    {
        Account account = await SeedMainAccount("solo.loss.first@kongor.com", "SoloLossFirst");

        MatchInformation matchInformation = BuildMatchInformation(MatchType.AM_PUBLIC);
        MatchParticipantStatistics participant = BuildParticipant(account.ID, account.Name, groupNumber: 1, loss: 1);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account trackedAccount = await databaseContext.Accounts.Include(candidate => candidate.User).SingleAsync(candidate => candidate.ID == account.ID);

        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, participant);
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
    public async Task Apply_AltAccount_DoesNotApplyPostSignupBonus()
    {
        Account mainAccount = await SeedMainAccount("alt.host@kongor.com", "AltHost");
        Account altAccount = await SeedAltAccount(mainAccount, "AltChild");

        MatchInformation matchInformation = BuildMatchInformation(MatchType.AM_PUBLIC);
        MatchParticipantStatistics participant = BuildParticipant(altAccount.ID, altAccount.Name, groupNumber: 1, win: 1);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account trackedAccount = await databaseContext.Accounts.Include(candidate => candidate.User).SingleAsync(candidate => candidate.ID == altAccount.ID);

        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, participant);
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
    public async Task Apply_MainAccountBeyondThreshold_DoesNotApplyPostSignupBonus()
    {
        Account account = await SeedMainAccount("veteran@kongor.com", "Veteran");

        int matchesCount = JSONConfiguration.EconomyConfiguration.EventRewards.PostSignupBonus.MatchesCount;

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account trackedAccount = await databaseContext.Accounts.Include(candidate => candidate.User).SingleAsync(candidate => candidate.ID == account.ID);

        // Seed Enough Prior Participation Rows So That "MatchesPlayedBeforeThis" Meets The Threshold
        for (int priorMatchID = 1; priorMatchID <= matchesCount; priorMatchID++)
        {
            await databaseContext.MatchParticipantStatistics.AddAsync(BuildParticipant(trackedAccount.ID, trackedAccount.Name, groupNumber: 1, win: 1, matchID: priorMatchID));
        }

        await databaseContext.SaveChangesAsync();

        MatchInformation matchInformation = BuildMatchInformation(MatchType.AM_PUBLIC);
        MatchParticipantStatistics participant = BuildParticipant(trackedAccount.ID, trackedAccount.Name, groupNumber: 1, win: 1, matchID: matchesCount + 1);

        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, participant);
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
    [Arguments(1, "Solo")]
    [Arguments(2, "TwoPersonGroup")]
    [Arguments(3, "ThreePersonGroup")]
    [Arguments(4, "FourPersonGroup")]
    [Arguments(5, "FivePersonGroup")]
    public async Task Apply_GroupNumber_SelectsMatchingRewardBucket(int groupNumber, string bucketName)
    {
        Account account = await SeedMainAccount($"group.{groupNumber}@kongor.com", $"Group{groupNumber}");

        MatchInformation matchInformation = BuildMatchInformation(MatchType.AM_PUBLIC);
        MatchParticipantStatistics participant = BuildParticipant(account.ID, account.Name, groupNumber: groupNumber, win: 1);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account trackedAccount = await databaseContext.Accounts.Include(candidate => candidate.User).SingleAsync(candidate => candidate.ID == account.ID);

        await MatchCompletionRewardsHandler.Apply(databaseContext, NullLogger.Instance, trackedAccount, matchInformation, participant);
        await databaseContext.SaveChangesAsync();

        MatchRewards matchRewards = JSONConfiguration.EconomyConfiguration.MatchRewards;
        PostSignupBonus bonus = JSONConfiguration.EconomyConfiguration.EventRewards.PostSignupBonus;

        Win expected = bucketName switch
        {
            "Solo"             => matchRewards.Solo.Win,
            "TwoPersonGroup"   => matchRewards.TwoPersonGroup.Win,
            "ThreePersonGroup" => matchRewards.ThreePersonGroup.Win,
            "FourPersonGroup"  => matchRewards.FourPersonGroup.Win,
            "FivePersonGroup"  => matchRewards.FivePersonGroup.Win,
            _                  => throw new ArgumentOutOfRangeException(nameof(bucketName), bucketName, null)
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
    [Arguments(MatchType.AM_PUBLIC,               false, AccountStatisticsType.Public)]
    [Arguments(MatchType.AM_MATCHMAKING,          false, AccountStatisticsType.Matchmaking)]
    [Arguments(MatchType.AM_MATCHMAKING,          true,  AccountStatisticsType.MatchmakingCasual)]
    [Arguments(MatchType.AM_UNRANKED_MATCHMAKING, false, AccountStatisticsType.MatchmakingCasual)]
    [Arguments(MatchType.AM_MATCHMAKING_BOTMATCH, false, AccountStatisticsType.Cooperative)]
    [Arguments(MatchType.AM_MATCHMAKING_MIDWARS,  false, AccountStatisticsType.MidWars)]
    [Arguments(MatchType.AM_MATCHMAKING_RIFTWARS, false, AccountStatisticsType.RiftWars)]
    public async Task ResolveAccountStatisticsType_MapsMatchTypeCorrectly(MatchType matchType, bool isCasual, AccountStatisticsType expected)
    {
        MatchInformation matchInformation = BuildMatchInformation(matchType, isCasual);

        AccountStatisticsType resolved = MatchCompletionRewardsHandler.ResolveAccountStatisticsType(matchInformation);

        await Assert.That(resolved).IsEqualTo(expected);
    }

    [Test]
    public async Task ResolveAccountStatisticsType_NullMatchInformation_FallsBackToPublic()
    {
        AccountStatisticsType resolved = MatchCompletionRewardsHandler.ResolveAccountStatisticsType(matchInformation: null);

        await Assert.That(resolved).IsEqualTo(AccountStatisticsType.Public);
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

    private static MatchInformation BuildMatchInformation(MatchType matchType, bool isCasual = false)
    {
        return new MatchInformation
        {
            MatchID = 1,
            MatchName = "Test Match",
            ServerID = 1,
            ServerName = "Test Server",
            HostAccountName = "TestHost",
            Map = "caldavar",
            Version = "4.10.1.0",
            IsCasual = isCasual,
            MatchType = matchType,
            MatchMode = PublicMatchMode.GAME_MODE_NORMAL
        };
    }

    private static MatchParticipantStatistics BuildParticipant(int accountID, string accountName, int groupNumber, int win = 0, int loss = 0, int matchID = 1)
    {
        return new MatchParticipantStatistics
        {
            MatchID                     = matchID,
            AccountID                   = accountID,
            AccountName                 = accountName,
            ClanID                      = null,
            ClanTag                     = null,
            Team                        = 1,
            LobbyPosition               = 0,
            GroupNumber                 = groupNumber,
            Benefit                     = 0,
            HeroProductID               = null,
            HeroIdentifier              = "Hero_Default",
            Inventory                   = [],
            Win                         = win,
            Loss                        = loss,
            Disconnected                = 0,
            Conceded                    = 0,
            Kicked                      = 0,
            PublicMatch                 = 1,
            PublicSkillRatingChange     = 0,
            RankedMatch                 = 0,
            RankedSkillRatingChange     = 0,
            SocialBonus                 = 0,
            UsedToken                   = 0,
            ConcedeVotes                = 0,
            HeroKills                   = 0,
            HeroDamage                  = 0,
            GoldFromHeroKills           = 0,
            HeroAssists                 = 0,
            HeroExperience              = 0,
            HeroDeaths                  = 0,
            Buybacks                    = 0,
            GoldLostToDeath             = 0,
            SecondsDead                 = 0,
            TeamCreepKills              = 0,
            TeamCreepDamage             = 0,
            TeamCreepGold               = 0,
            TeamCreepExperience         = 0,
            NeutralCreepKills           = 0,
            NeutralCreepDamage          = 0,
            NeutralCreepGold            = 0,
            NeutralCreepExperience      = 0,
            BuildingDamage              = 0,
            BuildingsRazed              = 0,
            ExperienceFromBuildings     = 0,
            GoldFromBuildings           = 0,
            Denies                      = 0,
            ExperienceDenied            = 0,
            Gold                        = 0,
            GoldSpent                   = 0,
            Experience                  = 0,
            Actions                     = 0,
            SecondsPlayed               = 0,
            HeroLevel                   = 0,
            ConsumablesPurchased        = 0,
            WardsPlaced                 = 0,
            FirstBlood                  = 0,
            DoubleKill                  = 0,
            TripleKill                  = 0,
            QuadKill                    = 0,
            Annihilation                = 0,
            KillStreak03                = 0,
            KillStreak04                = 0,
            KillStreak05                = 0,
            KillStreak06                = 0,
            KillStreak07                = 0,
            KillStreak08                = 0,
            KillStreak09                = 0,
            KillStreak10                = 0,
            KillStreak15                = 0,
            Smackdown                   = 0,
            Humiliation                 = 0,
            Nemesis                     = 0,
            Retribution                 = 0,
            Score                       = 0,
            GameplayStat0               = 0,
            GameplayStat1               = 0,
            GameplayStat2               = 0,
            GameplayStat3               = 0,
            GameplayStat4               = 0,
            GameplayStat5               = 0,
            GameplayStat6               = 0,
            GameplayStat7               = 0,
            GameplayStat8               = 0,
            GameplayStat9               = 0,
            TimeEarningExperience       = 0
        };
    }
}
