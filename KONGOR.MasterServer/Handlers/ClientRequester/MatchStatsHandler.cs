using System.Globalization;

using KONGOR.MasterServer.Services; // For HeroDefinitionService
using KONGOR.MasterServer.Services.Requester;

using Role = MERRICK.DatabaseContext.Entities.Utility.Role;

namespace KONGOR.MasterServer.Handlers.ClientRequester;

public partial class MatchStatsHandler(
    MerrickContext databaseContext,
    IDatabase distributedCache,
    IHeroDefinitionService heroDefinitionService,
    ILogger<MatchStatsHandler> logger) : IClientRequestHandler
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private IDatabase DistributedCache { get; } = distributedCache;
    private IHeroDefinitionService HeroDefinitionService { get; } = heroDefinitionService;
    private ILogger Logger { get; } = logger;

    [LoggerMessage(Level = LogLevel.Information, Message = "[MatchStats] Request received. MatchID={MatchId}, Cookie={Cookie}")]
    private partial void LogRequestReceived(string matchId, string cookie);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[MatchStats] Missing cookie.")]
    private partial void LogMissingCookie();

    [LoggerMessage(Level = LogLevel.Error, Message = "[MatchStats] Request Failed: Missing Match ID")]
    private partial void LogMissingMatchId();

    [LoggerMessage(Level = LogLevel.Warning, Message = "[MatchStats] Invalid Match ID: {MatchId}")]
    private partial void LogInvalidMatchId(string matchId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "[MatchStats] Fetching MatchStatistics for ID {MatchId}...")]
    private partial void LogFetchingStatistics(int matchId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[MatchStats] Match Statistics Not Found For ID {MatchId}. Returning Soft Failure (Dummy Response).")]
    private partial void LogStatsNotFound(int matchId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "[MatchStats] Fetching PlayerStatistics for Match {MatchId}...")]
    private partial void LogFetchingPlayerStats(int matchId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "[MatchStats] Fetching requester account '{AccountName}'...")]
    private partial void LogFetchingRequester(string accountName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "[MatchStats] Fetching/Reconstructing MatchStartData...")]
    private partial void LogFetchingMatchStartData();

    [LoggerMessage(Level = LogLevel.Debug, Message = "[MatchStats] Fetching stats for {Count} players...")]
    private partial void LogFetchingBulkStats(int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "[MatchStats] Stats generated successfully for Match {MatchId}.")]
    private partial void LogStatsGenerated(int matchId);

    public async Task<IActionResult> HandleRequestAsync(HttpContext context)
    {
        HttpRequest Request = context.Request;
        string? cookie = ClientRequestHelper.GetCookie(Request);
        string? matchIDString = Request.Form["match_id"];

        LogRequestReceived(matchIDString ?? "NULL", cookie ?? "NULL");

        if (cookie is null)
        {
            LogMissingCookie();
            return new BadRequestObjectResult(@"Missing Value For Form Parameter ""cookie""");
        }

        if (matchIDString is null)
        {
            LogMissingMatchId();
            return new BadRequestObjectResult(@"Missing Value For Form Parameter ""match_id""");
        }

        if (!int.TryParse(matchIDString, NumberStyles.Integer, CultureInfo.InvariantCulture, out int matchID))
        {
            LogInvalidMatchId(matchIDString);
            return new BadRequestObjectResult("Invalid Match ID");
        }

        LogFetchingStatistics(matchID);
        MatchStatistics? matchStatistics =
            await MerrickContext.MatchStatistics.SingleOrDefaultAsync(matchStatistics =>
                matchStatistics.MatchID == matchID);

        if (matchStatistics is null)
        {
            LogStatsNotFound(matchID);

            MatchStatsResponse safeResponse = new()
            {
                GoldCoins = "0",
                SilverCoins = "0",
                MatchSummary = [],
                MatchPlayerStatistics = [],
                MatchPlayerInventories = [],
                MatchMastery = new MatchMastery
                {
                    HeroIdentifier = "Hero_Legionnaire",
                    HeroProductID = 1,
                    CurrentMasteryExperience = 0,
                    MatchMasteryExperience = 0,
                    MasteryExperienceBonus = 0,
                    MasteryExperienceBoost = 0,
                    MasteryExperienceSuperBoost = 0,
                    MasteryExperienceMaximumLevelHeroesCount = 0,
                    MasteryExperienceHeroesBonus = 0,
                    MasteryExperienceToBoost = 0,
                    MasteryExperienceEventBonus = 0,
                    MasteryExperienceCanBoost = false,
                    MasteryExperienceCanSuperBoost = false,
                    MasteryExperienceBoostProductIdentifier = 3609,
                    MasteryExperienceSuperBoostProductIdentifier = 4605,
                    MasteryExperienceBoostProductCount = 0,
                    MasteryExperienceSuperBoostProductCount = 0
                },
                OwnedStoreItems = [],
                SelectedStoreItems = [],
                OwnedStoreItemsData = [],
                CustomIconSlotID = "0",
                CampaignReward = new CampaignReward()
            };

            return new ContentResult { Content = PhpSerialization.Serialize(safeResponse), ContentType = "text/plain; charset=utf-8", StatusCode = 200 };
        }

        LogFetchingPlayerStats(matchID);
        List<PlayerStatistics> allPlayerStatistics = await MerrickContext.PlayerStatistics
            .Where(playerStatistics => playerStatistics.MatchID == matchStatistics.MatchID).ToListAsync();

        string? accountName = context.Items["SessionAccountName"] as string
                              ?? await DistributedCache.GetAccountNameForSessionCookie(cookie);

        Account? account = null;
        if (accountName is not null)
        {
            LogFetchingRequester(accountName);
            account = await MerrickContext.Accounts
                .Include(account => account.User)
                .Include(account => account.Clan)
                .SingleOrDefaultAsync(account => account.Name.Equals(accountName));
        }

        LogFetchingMatchStartData();
        MatchStartData? matchStartData = await DistributedCache.GetMatchStartData(matchStatistics.MatchID);

        matchStartData ??= new MatchStartData
        {
            MatchID = matchStatistics.MatchID,
            ServerID = matchStatistics.ServerID,
            MatchName = matchStatistics.FileName,
            HostAccountName = matchStatistics.HostAccountName,
            Map = matchStatistics.Map,
            MatchMode = matchStatistics.GameMode,
            Version = matchStatistics.Version,
            IsCasual = matchStatistics.GameMode == "casual",
            MatchType = (byte) (matchStatistics.GameMode.Contains("rank") ? 1 : 0),
            Options = MatchOptions.None,
            ServerName = "Unknown"
        };

        MatchSummary matchSummary = new(matchStatistics, allPlayerStatistics, matchStartData);

        Dictionary<OneOf<int, string>, MatchPlayerStatistics> matchPlayerStatistics = [];
        Dictionary<OneOf<int, string>, MatchPlayerInventory> matchPlayerInventories = [];

        List<int> playerAccountIDs = allPlayerStatistics.Select(s => s.AccountID).Distinct().ToList();

        LogFetchingBulkStats(playerAccountIDs.Count);
        List<AccountStatistics> allAccountStatistics = await MerrickContext.AccountStatistics
            .Where(stats => playerAccountIDs.Contains(stats.AccountID))
            .ToListAsync();

        List<Account> playerAccounts = await MerrickContext.Accounts
            .Include(a => a.User)
            .Include(a => a.Clan)
            .Where(a => playerAccountIDs.Contains(a.ID))
            .ToListAsync();

        Role dummyRole = new() { ID = 1, Name = "User" };

        foreach (PlayerStatistics stats in allPlayerStatistics)
        {
            Account? playerAccount = playerAccounts.SingleOrDefault(a => a.ID == stats.AccountID);

            if (playerAccount is null)
            {
                playerAccount = new Account
                {
                    ID = stats.AccountID,
                    Name = stats.AccountName,
                    IsMain = true,
                    User = new User
                    {
                        ID = stats.AccountID,
                        EmailAddress = "dummy@kongor.net",
                        Role = dummyRole,
                        SRPPasswordHash = "",
                        SRPPasswordSalt = ""
                    },
                    Clan = stats.ClanID.HasValue
                        ? new Clan { ID = stats.ClanID.Value, Tag = stats.ClanTag ?? "", Name = stats.ClanTag ?? "" }
                        : null
                };
            }

            AccountStatistics? accountStatistics =
                allAccountStatistics.SingleOrDefault(s => s.AccountID == stats.AccountID);

            accountStatistics ??= new AccountStatistics
            {
                AccountID = stats.AccountID,
                Account = playerAccount,
                Type = AccountStatisticsType.Matchmaking,
                MatchesPlayed = 0,
                MatchesWon = 0,
                MatchesLost = 0,
                MatchesConceded = 0,
                MatchesDisconnected = 0,
                MatchesKicked = 0,
                SkillRating = 1500.0,
                PerformanceScore = 0.0,
                PlacementMatchesData = ""
            };

            uint heroProductId = stats.HeroProductID ?? 0;
            uint baseHeroId = HeroDefinitionService.GetBaseHeroId(heroProductId);
            string baseHeroIdentifier = HeroDefinitionService.GetHeroIdentifier(baseHeroId);
            string productIdentifier = HeroDefinitionService.GetHeroIdentifier(heroProductId);
            string altAvatarName = (heroProductId != baseHeroId && !string.IsNullOrEmpty(productIdentifier))
                ? productIdentifier
                : "";

            matchPlayerStatistics[stats.AccountID] = new MatchPlayerStatistics(
                matchStartData,
                playerAccount,
                stats,
                accountStatistics,
                accountStatistics,
                accountStatistics
            )
            {
                HeroIdentifier = baseHeroIdentifier,
                AlternativeAvatarName = altAvatarName
            };

            List<string> inv = stats.Inventory ?? new List<string>();
            matchPlayerInventories[stats.AccountID] = new MatchPlayerInventory
            {
                AccountID = stats.AccountID,
                MatchID = matchStatistics.ID,
                Slot1 = inv.Count > 0 ? inv[0] : "",
                Slot2 = inv.Count > 1 ? inv[1] : "",
                Slot3 = inv.Count > 2 ? inv[2] : "",
                Slot4 = inv.Count > 3 ? inv[3] : "",
                Slot5 = inv.Count > 4 ? inv[4] : "",
                Slot6 = inv.Count > 5 ? inv[5] : ""
            };
        }

        string masteryHeroIdentifier = "Hero_Gauntlet";
        string masteryAltAvatarName = "";
        uint masteryHeroProductID = 0;
        int masteryHeroLevel = 1;

        if (account is not null)
        {
            PlayerStatistics? requesterStats = allPlayerStatistics.SingleOrDefault(ps => ps.AccountID == account.ID);
            if (requesterStats is not null)
            {
                uint heroProductId = requesterStats.HeroProductID ?? 0;
                masteryHeroProductID = heroProductId;
                uint baseHeroId = HeroDefinitionService.GetBaseHeroId(heroProductId);
                string resolvedIdentifier = HeroDefinitionService.GetHeroIdentifier(baseHeroId);

                if (!string.IsNullOrEmpty(resolvedIdentifier))
                {
                    masteryHeroIdentifier = resolvedIdentifier;
                }

                string productIdentifier = HeroDefinitionService.GetHeroIdentifier(heroProductId);
                if (!string.IsNullOrEmpty(productIdentifier))
                {
                    masteryAltAvatarName = productIdentifier;
                }

                masteryHeroLevel = requesterStats.HeroLevel;
            }
        }

        int matchMasteryExperience = 100; // Calculated elsewhere in future
        int bonusExperience = 10;

        MatchMastery matchMastery = new(
            masteryHeroIdentifier,
            0,
            matchMasteryExperience,
            bonusExperience)
        {
            HeroIdentifier = masteryHeroIdentifier,
            AlternativeAvatarName = masteryAltAvatarName,
            HeroProductID = (int) masteryHeroProductID,
            CurrentMasteryExperience = 0,
            MatchMasteryExperience = matchMasteryExperience,
            MasteryExperienceBonus = 0,
            MasteryExperienceBoost = 0,
            MasteryExperienceSuperBoost = 0,
            MasteryExperienceMaximumLevelHeroesCount = 0,
            MasteryExperienceHeroesBonus = bonusExperience,
            MasteryExperienceToBoost = (matchMasteryExperience + bonusExperience) * 2,
            MasteryExperienceEventBonus = 0,
            MasteryExperienceCanBoost = true,
            MasteryExperienceCanSuperBoost = true,
            MasteryExperienceBoostProductIdentifier = 3609,
            MasteryExperienceSuperBoostProductIdentifier = 4605,
            MasteryExperienceBoostProductCount = 0,
            MasteryExperienceSuperBoostProductCount = 0
        };

        MatchStatsResponse response = new()
        {
            GoldCoins = account?.User.GoldCoins.ToString(CultureInfo.InvariantCulture) ?? "0",
            SilverCoins = account?.User.SilverCoins.ToString(CultureInfo.InvariantCulture) ?? "0",
            MatchSummary = [matchSummary],
            MatchPlayerStatistics = [matchPlayerStatistics],
            MatchPlayerInventories = [matchPlayerInventories],
            MatchMastery = matchMastery,
            OwnedStoreItems = account?.User.OwnedStoreItems ?? [],
            OwnedStoreItemsData = [], // Stubbed return in helper is void, need fix in helper or logic here. Assuming empty for now as helper is void.
            SelectedStoreItems = account?.SelectedStoreItems ?? [],
            CustomIconSlotID = "0", // Stubbed return in helper is void.
            CampaignReward = new CampaignReward()
        };

        LogStatsGenerated(matchID);
        string php = PhpSerialization.Serialize(response);
        return new ContentResult { Content = php, ContentType = "text/plain; charset=utf-8", StatusCode = 200 };
    }
}
