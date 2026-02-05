namespace KONGOR.MasterServer.Handlers.SRP;

using global::MERRICK.DatabaseContext.Entities.Core;
using global::MERRICK.DatabaseContext.Entities.Statistics;
using global::MERRICK.DatabaseContext.Enumerations;
using global::MERRICK.DatabaseContext.Extensions;

public static class SRPAuthenticationHandlers
{
    # region Chat Server Authentication Secret

    // Thank you, Denis Koroskin (aka korDen), for making this value public: https://github.com/korDen/honserver/blob/b35c23015fe915633dc5acd52db44403066cb0eb/utils.php#L6.
    // It is not clear how this magic string was obtained.
    // Project KONGOR would have not been possible without having this value in the public domain.

    private const string ChatServerAuthenticationSalt =
        "8roespiemlasToUmiuglEhOaMiaSWlesplUcOAniupr2esPOeBRiudOEphiutOuJ";

    # endregion

    public static SRPAuthenticationResponseStageTwo GenerateStageTwoResponse(StageTwoResponseParameters parameters,
        out string cookie)
    {
        cookie = Guid.CreateVersion7().ToString().Replace("-", string.Empty);

        SRPAuthenticationResponseStageTwo response = new()
        {
            ServerProof = parameters.ServerProof,
            MainAccountID =
                parameters.Account.User.Accounts.FirstOrDefault(account => account.IsMain)?.ID.ToString() ??
                parameters.Account.ID.ToString(),
            ID = parameters.Account.ID.ToString(),
            GarenaID = parameters.Account.ID.ToString(),
            Name = parameters.Account.Name,
            Email = parameters.Account.User.EmailAddress,
            AccountType = Convert.ToInt32(parameters.Account.Type).ToString(),
            SuspensionID = "0", // TODO: Implement Suspensions
            UseCloud = parameters.Account.UseCloud ? "1" : "0",
            Cookie = cookie,
            IPAddress = parameters.ClientIPAddress,
            LeaverThreshold =
                ".05", // TODO: Set Per Partition Of Games Played (e.g. under 50 games = 20%, between 50 and 100 games = 10%, over 100 games = 5%)
            HasSubAccounts = parameters.Account.User.Accounts.Any(account => account.IsMain.Equals(false)),
            IsSubAccount = parameters.Account.IsMain.Equals(false),
            ICBURL =
                Environment.GetEnvironmentVariable("APPLICATION_URL") ??
                throw new NullReferenceException("Application URL Is NULL"),
            AuthenticationHash = ComputeChatServerCookieHash(parameters.Account.ID, parameters.ClientIPAddress, cookie),
            ChatServerIPAddress = parameters.ChatServer.Address,
            ChatServerPort = parameters.ChatServer.Port.ToString(),
            ChatChannels = SetChatChannels(parameters.Account),
            Accounts =
                parameters.Account.User.Accounts.OrderBy(account => account.TimestampCreated)
                    .Select(account => new List<string> { account.Name, account.ID.ToString() }).ToList(),
            GoldCoins = parameters.Account.User.GoldCoins.ToString(),
            SilverCoins = parameters.Account.User.SilverCoins,
            CustomIconSlotID = SetCustomIconSlotID(parameters.Account),
            CurrentSeason = parameters.CurrentSeason,
            MuteExpiration = 0, // TODO: Implement Account Muting As Part Of The Karma System
            FriendAccountList = SetFriendAccountList(parameters.Account),
            IgnoredAccountsList = SetIgnoredAccountsList(parameters.Account),
            BannedAccountsList = SetBannedAccountsList(parameters.Account),
            ClanRoster = SetClanRoster(parameters.ClanRoster),
            ClanMembershipData = SetClanMembershipData(parameters.Account),
            OwnedStoreItems = parameters.Account.User.OwnedStoreItems,
            SelectedStoreItems = parameters.Account.SelectedStoreItems,
            OwnedStoreItemsData = SetOwnedStoreItemsData(parameters.Account),
            AwardsTooltips = SetAwardsTooltips(),
            DataPoints = SetDataPoints(parameters.Account),
            CloudStorageInformation = SetCloudStorageInformation(parameters.Account),
            Notifications = SetNotifications()
        };

        return response;
    }

    /// <summary>
    ///     Generates a 64-character long SHA256 hash of the account's password.
    ///     The uppercase hashes (C# default) in this method need to be lowercased, to match the lowercase hashes (C++ default)
    ///     that the game client generates.
    ///     <br />
    ///     The expectation is that the password is not hashed for SRP registration, but is hashed for SRP authentication.
    /// </summary>
    public static string ComputeSRPPasswordHash(string password, string salt, bool passwordIsHashed = true)
    {
        string passwordHash = passwordIsHashed
            ? password
            : Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(password))).ToLower();

        string magickedPasswordHash = passwordHash + salt + MagicStringOne;

        string magickedPasswordHashHash =
            Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(magickedPasswordHash))).ToLower();

        string magickedMagickedPasswordHashHash = magickedPasswordHashHash + MagicStringTwo;

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(magickedMagickedPasswordHashHash))).ToLower();
    }

    public static string ComputeChatServerCookieHash(int accountID, string remoteIPAddress, string cookie)
    {
        string chatServerCookie = accountID + remoteIPAddress + cookie + ChatServerAuthenticationSalt;

        // The uppercase hash (C# default) needs to be lowercased, to match the lowercase hash (C++ default) that the game client generates.
        string chatServerCookieHash =
            Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(chatServerCookie))).ToLower();

        return chatServerCookieHash;
    }

    public static string ComputeMatchServerChatAuthenticationHash(string key, string cookie)
    {
        return Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(key + cookie + ChatServerAuthenticationSalt)))
            .ToLower();
    }

    private static List<string> SetChatChannels(Account account)
    {
        List<string> channels = account.AutoConnectChatChannels;

        if (account.Type is not AccountType.ServerHost)
        {
            channels = channels.Prepend(ChatChannels.GeneralChannel).ToList();
        }

        if (account.Clan is not null)
        {
            channels.Add(account.Clan.GetChatChannelName());
        }

        if (account.Type is AccountType.GameMaster or AccountType.Staff)
        {
            channels.Add(ChatChannels.GameMastersChannel);
        }

        if (account.Type is AccountType.Guest or AccountType.Staff)
        {
            channels.Add(ChatChannels.GuestsChannel);
        }

        if (account.Type is AccountType.ServerHost or AccountType.Staff)
        {
            channels.Add(ChatChannels.ServerHostsChannel);
        }

        if (account.Type is AccountType.Streamer or AccountType.Staff)
        {
            channels.Add(ChatChannels.StreamersChannel);
        }

        if (account.Type is AccountType.VIP or AccountType.Staff)
        {
            channels.Add(ChatChannels.VIPChannel);
        }

        if (account.Type is AccountType.Staff)
        {
            channels.Add(ChatChannels.StaffChannel);
        }

        channels = channels.Distinct().Order().ToList();

        return channels;
    }

    private static Dictionary<string, Dictionary<string, FriendAccount>> SetFriendAccountList(Account account)
    {
        return new Dictionary<string, Dictionary<string, FriendAccount>>
        {
            {
                account.ID.ToString(), account.FriendedPeers.ToDictionary(friend => friend.ID.ToString(),
                    friend => new FriendAccount
                    {
                        ID = friend.ID.ToString(),
                        Name = friend.Name,
                        Group = friend.FriendGroup,
                        ClanTag = friend.ClanTag ?? string.Empty
                    })
            }
        };
    }

    private static Dictionary<string, List<IgnoredAccount>> SetIgnoredAccountsList(Account account)
    {
        return new Dictionary<string, List<IgnoredAccount>>
        {
            {
                account.ID.ToString(), account.IgnoredPeers
                    .Select(ignored => new IgnoredAccount { ID = ignored.ID.ToString(), Name = ignored.Name }).ToList()
            }
        };
    }

    private static Dictionary<string, List<BannedAccount>> SetBannedAccountsList(Account account)
    {
        return new Dictionary<string, List<BannedAccount>>
        {
            {
                account.ID.ToString(), account.BannedPeers
                    .Select(banned =>
                        new BannedAccount { ID = banned.ID.ToString(), Name = banned.Name, Reason = banned.BanReason })
                    .ToList()
            }
        };
    }

    private static Dictionary<string, ClanMemberAccount> SetClanRoster(List<Account> members)
    {
        return members.Select(member => new KeyValuePair<string, ClanMemberAccount>(member.ID.ToString(),
                new ClanMemberAccount
                {
                    ClanID = member.Clan?.ID.ToString() ?? string.Empty,
                    ID = member.ID.ToString(),
                    JoinDate =
                        member.TimestampJoinedClan is not null
                            ? member.TimestampJoinedClan.GetValueOrDefault().ToString("yyyy-MM-dd HH:mm:ss")
                            : string.Empty,
                    Name = member.GetNameWithClanTag(),
                    Rank = member.GetClanTierName(),
                    Message = "TODO: Find Out What This Does",
                    Standing = Convert.ToInt32(member.Type).ToString()
                }))
            .ToDictionary();
    }

    private static OneOf<ClanMemberData, ClanMemberDataError> SetClanMembershipData(Account account)
    {
        return account.Clan is null
            ? new ClanMemberDataError()
            : new ClanMemberData
            {
                ClanID = account.Clan?.ID.ToString() ?? string.Empty,
                ID = account.ID.ToString(),
                ClanName = account.Clan?.Name ?? string.Empty,
                ClanTag = account.Clan?.Tag ?? string.Empty,
                ClanOwnerAccountID =
                    account.Clan?.Members.FirstOrDefault(member => member.ClanTier is ClanTier.Leader)?.ID
                        .ToString() ?? string.Empty,
                JoinDate =
                    account.TimestampJoinedClan is not null
                        ? account.TimestampJoinedClan.GetValueOrDefault().ToString("yyyy-MM-dd HH:mm:ss")
                        : string.Empty,
                Rank = account.GetClanTierName(),
                Message = "TODO: Find Out What This Does",
                Title = "TODO: Set The Clan Channel Title"
            };
    }

    private static string SetCustomIconSlotID(Account account)
    {
        return account.SelectedStoreItems.Any(item => item.StartsWith("ai.custom_icon"))
            ? account.SelectedStoreItems.FirstOrDefault(item => item.StartsWith("ai.custom_icon"))
                ?.Replace("ai.custom_icon:", string.Empty) ?? "0"
            : "0";
    }

    private static CloudStorageInformation SetCloudStorageInformation(Account account)
    {
        return new CloudStorageInformation
        {
            AccountID = account.ID.ToString(),
            UseCloud = account.UseCloud ? "1" : "0",
            AutomaticCloudUpload = account.AutomaticCloudUpload ? "1" : "0",
            BackupLastUpdatedTime = account.BackupLastUpdatedTime?.ToString("yyyy-MM-dd HH:mm:ss") ??
                                    DateTimeOffset.MinValue.ToString("yyyy-MM-dd HH:mm:ss")
        };
    }

    private static List<DataPoint> SetDataPoints(Account account)
    {
        AccountStatistics? publicStats = account.Statistics.FirstOrDefault(s => s.Type == AccountStatisticsType.Public);
        AccountStatistics? matchmakingStats = account.Statistics.FirstOrDefault(s => s.Type == AccountStatisticsType.Matchmaking);
        AccountStatistics? casualStats = account.Statistics.FirstOrDefault(s => s.Type == AccountStatisticsType.MatchmakingCasual);
        AccountStatistics? midWarsStats = account.Statistics.FirstOrDefault(s => s.Type == AccountStatisticsType.MidWars);
        AccountStatistics? riftWarsStats = account.Statistics.FirstOrDefault(s => s.Type == AccountStatisticsType.RiftWars);
        AccountStatistics? cooperativeStats = account.Statistics.FirstOrDefault(s => s.Type == AccountStatisticsType.Cooperative);

        int totalDisconnects = account.Statistics.Sum(s => s.MatchesDisconnected);
        int totalMatchesPlayed = account.Statistics.Sum(s => s.MatchesPlayed);

        List<DataPoint> dataPoints =
        [
            new()
            {
                ID = account.ID.ToString(),
                Level = account.AscensionLevel.ToString(),
                Disconnects = totalDisconnects.ToString(),
                MatchesPlayed = totalMatchesPlayed.ToString(),
                BotMatchesWon = cooperativeStats?.MatchesWon.ToString() ?? "0",
                PSR = publicStats?.SkillRating.ToString("F3") ?? "1500.000",
                PublicMatchesPlayed = publicStats?.MatchesPlayed.ToString() ?? "0",
                PublicMatchesWon = publicStats?.MatchesWon.ToString() ?? "0",
                PublicMatchesLost = publicStats?.MatchesLost.ToString() ?? "0",
                PublicMatchDisconnects = publicStats?.MatchesDisconnected.ToString() ?? "0",
                MMR = matchmakingStats?.SkillRating.ToString("F3") ?? "1500.000",
                RankedMatchesPlayed = matchmakingStats?.MatchesPlayed.ToString() ?? "0",
                RankedMatchesWon = matchmakingStats?.MatchesWon.ToString() ?? "0",
                RankedMatchesLost = matchmakingStats?.MatchesLost.ToString() ?? "0",
                RankedMatchDisconnects = matchmakingStats?.MatchesDisconnected.ToString() ?? "0",
                MidWarsMMR = midWarsStats?.SkillRating.ToString("F3") ?? "1500.000",
                RankedMidWarsMatchesPlayed = midWarsStats?.MatchesPlayed.ToString() ?? "0",
                RankedMidWarsMatchDisconnects = midWarsStats?.MatchesDisconnected.ToString() ?? "0",
                RiftWarsMMR = riftWarsStats?.SkillRating.ToString("F3") ?? "1500.000",
                RankedRiftWarsMatchesPlayed = riftWarsStats?.MatchesPlayed.ToString() ?? "0",
                RankedRiftWarsMatchDisconnects = riftWarsStats?.MatchesDisconnected.ToString() ?? "0",
                CasualMMR = casualStats?.SkillRating.ToString("F3") ?? "1500.000",
                CasualRankedMatchDisconnects = casualStats?.MatchesDisconnected.ToString() ?? "0",
                CasualRankedMatchesLost = casualStats?.MatchesLost.ToString() ?? "0",
                CasualRankedMatchesPlayed = casualStats?.MatchesPlayed.ToString() ?? "0",
                CasualRankedMatchesWon = casualStats?.MatchesWon.ToString() ?? "0",
                SeasonalRankedMatchesPlayed = 0,
                SeasonalRankedMatchDisconnects = 0,
                CasualSeasonalRankedMatchesPlayed = 0,
                CasualSeasonalRankedMatchDisconnects = 0,
                Experience = account.User.TotalExperience.ToString()
            }
        ];

        return dataPoints;
    }

    private static Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> SetOwnedStoreItemsData(
        Account account)
    {
        Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> items = account.User.OwnedStoreItems
            .Where(item => item.StartsWith("ma.").Equals(false) && item.StartsWith("cp.").Equals(false))
            .ToDictionary<string, string, OneOf<StoreItemData, StoreItemDiscountCoupon>>(upgrade => upgrade,
                upgrade => new StoreItemData());

        // TODO: Add Mastery Boosts And Coupons

        return items;
    }

    private static AwardsTooltips SetAwardsTooltips()
    {
        return new AwardsTooltips();
    }

    private static List<Notification> SetNotifications()
    {
        // TODO: Implement This

        return [];
    }

    public class StageTwoResponseParameters
    {
        public required Account Account { get; set; }
        public required List<Account> ClanRoster { get; set; }
        public required string ServerProof { get; set; }
        public required string ClientIPAddress { get; set; }
        public required (string Address, int Port) ChatServer { get; set; }
        public required string CurrentSeason { get; set; }
    }

    # region Secure Remote Password Magic Strings

    // Thank you, Anton Romanov (aka Theli), for making these values public: https://github.com/theli-ua/pyHoNBot/blob/cabde31b8601c1ca55dc10fcf663ec663ec0eb71/hon/masterserver.py#L37.
    // The first magic string is also present in the k2_x64 DLL of the Windows client, between offsets 0xF2F4D0 and 0xF2F4D0.
    // It is not clear how the second magic string was obtained.
    // Project KONGOR would have not been possible without having these values in the public domain.

    private const string MagicStringOne = "[!~esTo0}";
    private const string MagicStringTwo = "taquzaph_?98phab&junaj=z=kuChusu";

    # endregion
}