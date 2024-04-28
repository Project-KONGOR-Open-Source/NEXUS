namespace KONGOR.MasterServer.Handlers.SRP;

public static class SRPAuthenticationHandlers
{
    public static SRPAuthenticationResponseStageTwo GenerateStageTwoResponse(StageTwoResponseParameters parameters, out string cookie)
    {
        cookie = Guid.NewGuid().ToString();

        SRPAuthenticationResponseStageTwo response = new()
        {
            ServerProof = parameters.ServerProof,
            MainAccountID = parameters.Account.User.Accounts.Single(account => account.IsMain).ID.ToString(),
            ID = parameters.Account.ID.ToString(),
            GarenaID = parameters.Account.ID.ToString(),
            Name = parameters.Account.Name,
            Email = parameters.Account.User.EmailAddress,
            AccountType = Convert.ToInt32(parameters.Account.Type).ToString(),
            SuspensionID = "0", // TODO: Implement Suspensions
            UseCloud = "0", // TODO: Implement Cloud Backups
            Cookie = cookie,
            IPAddress = parameters.ClientIPAddress,
            LeaverThreshold = ".05", // TODO: Set Per Partition Of Games Played (e.g. under 50 games = 20%, between 50 and 100 games = 10%, over 100 games = 5%)
            HasSubAccounts = parameters.Account.User.Accounts.Any(account => account.IsMain.Equals(false)),
            IsSubAccount = parameters.Account.IsMain.Equals(false),
            ICBURL = Environment.GetEnvironmentVariable("APPLICATION_URL") ?? throw new NullReferenceException("Application URL Is NULL"),
            AuthenticationHash = ComputeChatServerCookieHash(parameters.Account.ID, parameters.ClientIPAddress, cookie),
            ChatServerIPAddress = parameters.ChatServer.Address,
            ChatServerPort = parameters.ChatServer.Port.ToString(),
            ChatChannels = SetChatChannels(parameters.Account),
            Accounts = parameters.Account.User.Accounts.OrderBy(account => account.TimestampCreated).Select(account => new List<string> { account.Name, account.ID.ToString() }).ToList(),
            GoldCoins = parameters.Account.User.GoldCoins.ToString(),
            SilverCoins = parameters.Account.User.SilverCoins,
            CustomIconSlotID = SetCustomIconSlotID(parameters.Account),
            CurrentSeason = "12", // TODO: Set Season
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
            DataPoints = SetDataPoints(),
            CloudStorageInformation = SetCloudStorageInformation(parameters.Account),
            Notifications = SetNotifications()
        };

        return response;
    }

    # region Secure Remote Password Magic Strings

    // Thank you, Anton Romanov (aka Theli), for making these values public: https://github.com/theli-ua/pyHoNBot/blob/cabde31b8601c1ca55dc10fcf663ec663ec0eb71/hon/masterserver.py#L37.
    // The first magic string is also present in the k2_x64 DLL of the Windows client, between offsets 0xF2F4D0 and 0xF2F4D0.
    // It is not clear how the second magic string was obtained.
    // Project KONGOR would have not been possible without having these values in the public domain.

    private const string MagicStringOne = "[!~esTo0}";
    private const string MagicStringTwo = "taquzaph_?98phab&junaj=z=kuChusu";

    # endregion

    /// <summary>
    ///     Generates a 64-character long SHA256 hash of the account's password.
    ///     The uppercase hashes (C# default) in this method need to be lowercased, to match the lowercase hashes (C++ default) that the game client generates.
    ///     <br/>
    ///     The expectation is that the password is not hashed for SRP registration, but is hashed for SRP authentication.
    /// </summary>
    public static string ComputeSRPPasswordHash(string password, string salt, bool passwordIsHashed = true)
    {
        string passwordHash = passwordIsHashed ? password : Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(password))).ToLower();

        string magickedPasswordHash = passwordHash + salt + MagicStringOne;

        string magickedPasswordHashHash = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(magickedPasswordHash))).ToLower();

        string magickedMagickedPasswordHashHash = magickedPasswordHashHash + MagicStringTwo;

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(magickedMagickedPasswordHashHash))).ToLower();
    }

    public class StageTwoResponseParameters()
    {
        public required Account Account { get; set; }
        public required List<Account> ClanRoster { get; set; }
        public required string ServerProof { get; set; }
        public required string ClientIPAddress { get; set; }
        public required (string Address, int Port) ChatServer { get; set; }
    }

    # region Chat Server Authentication Secret

    // Thank you, Denis Koroskin (aka korDen), for making this value public: https://github.com/korDen/honserver/blob/b35c23015fe915633dc5acd52db44403066cb0eb/utils.php#L6.
    // It is not clear how this magic string was obtained.
    // Project KONGOR would have not been possible without having this value in the public domain.

    private const string ChatServerAuthenticationSalt = "8roespiemlasToUmiuglEhOaMiaSWlesplUcOAniupr2esPOeBRiudOEphiutOuJ";

    # endregion

    private static string ComputeChatServerCookieHash(int accountID, string remoteIPAddress, string cookie)
    {
        string chatServerCookie = accountID + remoteIPAddress + cookie + ChatServerAuthenticationSalt;

        // The uppercase hash (C# default) needs to be lowercased, to match the lowercase hash (C++ default) that the game client generates.
        string chatServerCookieHash = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(chatServerCookie))).ToLower();

        return chatServerCookieHash;
    }

    public static string ComputeMatchServerChatAuthenticationHash(string key, string cookie)
        => Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(key + cookie + ChatServerAuthenticationSalt))).ToLower();

    private static List<string> SetChatChannels(Account account)
    {
        List<string> channels = account.AutoConnectChatChannels;

        // "KONGOR" is a special channel name that maps to "KONGOR 1", then to "KONGOR 2" if "KONGOR 1" is full, then to "KONGOR 3" if "KONGOR 2" is full, and so on.
        const string generalChannel = "KONGOR"; // TODO: Implement Channel Load Balancing

        const string vipChannel = "VIP";
        const string streamersChannel = "Streamers";
        const string serverHostsChannel = "Server Hosts";
        const string gameMastersChannel = "Game Masters";

        // "TERMINAL" is a special channel from which chat server commands can be executed.
        const string staffChannel = "TERMINAL"; // TODO: Implement TERMINAL Command Palette

        if (account.Type is not AccountType.ServerHost)
            channels = channels.Prepend(generalChannel).ToList();

        if (account.Type is AccountType.VIP or AccountType.Staff)
            channels.Add(vipChannel);

        if (account.Type is AccountType.Streamer or AccountType.Staff)
            channels.Add(streamersChannel);

        if (account.Type is AccountType.ServerHost or AccountType.Staff)
            channels.Add(serverHostsChannel);

        if (account.Type is AccountType.GameMaster or AccountType.Staff)
            channels.Add(gameMastersChannel);

        if (account.Clan is not null)
            channels.Add($"Clan {account.Clan.Name}");

        if (account.Type is AccountType.Staff)
            channels.Add(staffChannel);

        return channels;
    }

    private static Dictionary<string, Dictionary<string, FriendAccount>> SetFriendAccountList(Account account)
        => new() { { account.ID.ToString(), account.FriendedPeers.ToDictionary(friend => friend.ID.ToString(),
            friend => new FriendAccount { ID = friend.ID.ToString(), Name = friend.Name, Group = friend.FriendGroup, ClanTag = friend.ClanTag ?? string.Empty } ) } };

    private static Dictionary<string, List<IgnoredAccount>> SetIgnoredAccountsList(Account account)
        => new() { { account.ID.ToString(), account.IgnoredPeers
            .Select(ignored => new IgnoredAccount { ID = ignored.ID.ToString(), Name = ignored.Name }).ToList() } };

    private static Dictionary<string, List<BannedAccount>> SetBannedAccountsList(Account account)
        => new() { { account.ID.ToString(), account.BannedPeers
            .Select(banned => new BannedAccount { ID = banned.ID.ToString(), Name = banned.Name, Reason = banned.BanReason }).ToList() } };

    private static Dictionary<string, ClanMemberAccount> SetClanRoster(List<Account> members)
        => members.Select(member => new KeyValuePair<string, ClanMemberAccount>(member.ID.ToString(),
                new ClanMemberAccount { ClanID = member.Clan?.ID.ToString() ?? string.Empty, ID = member.ID.ToString(),
                    JoinDate = member.TimestampJoinedClan is not null ? member.TimestampJoinedClan.GetValueOrDefault().ToString("yyyy-MM-dd HH:mm:ss") : string.Empty,
                    Name = member.Name, Rank = member.ClanTierName, Message = "TODO: Find Out What This Does", Standing = Convert.ToInt32(member.Type).ToString() }))
            .ToDictionary();

    private static OneOf<ClanMemberData, ClanMemberDataError> SetClanMembershipData(Account account)
        => account.Clan is null ? new ClanMemberDataError() : new ClanMemberData
        {
            ClanID = account.Clan?.ID.ToString() ?? string.Empty, ID = account.ID.ToString(), ClanName = account.Clan?.Name ?? string.Empty,
            ClanTag = account.Clan?.Tag ?? string.Empty, ClanOwnerAccountID = account.Clan?.Members.Single(member => member.ClanTier is ClanTier.Leader).ID.ToString() ?? string.Empty,
            JoinDate = account.TimestampJoinedClan is not null ? account.TimestampJoinedClan.GetValueOrDefault().ToString("yyyy-MM-dd HH:mm:ss") : string.Empty,
            Rank = account.ClanTierName, Message = "TODO: Find Out What This Does", Title = "TODO: Set The Clan Channel Title"
        };

    private static string SetCustomIconSlotID(Account account)
        => account.SelectedStoreItems.Any(item => item.StartsWith("ai.custom_icon"))
            ? account.SelectedStoreItems.Single(item => item.StartsWith("ai.custom_icon")).Replace("ai.custom_icon:", string.Empty) : "0";

    private static CloudStorageInformation SetCloudStorageInformation(Account account)
        => new() { AccountID = account.ID.ToString(), UseCloud = "0", AutomaticCloudUpload = "0", BackupLastUpdatedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }; // TODO: Fix These Values

    private static List<DataPoint> SetDataPoints()
    {
        List<DataPoint> dataPoints =
        [
            // TODO: Set These From Account Stats

            new DataPoint
            {
                ID = "666",
                Level = "1",
                Disconnects = "0",
                MatchesPlayed = "0",
                BotMatchesWon = "0",
                PSR = "1500.000",
                PublicMatchesPlayed = "0",
                PublicMatchesWon = "0",
                PublicMatchesLost = "0",
                PublicMatchDisconnects = "0",
                MMR = "1500.000",
                RankedMatchesPlayed = "0",
                RankedMatchesWon = "0",
                RankedMatchesLost = "0",
                RankedMatchDisconnects = "0",
                MidWarsMMR = "1500.000",
                RankedMidWarsMatchesPlayed = "0",
                RankedMidWarsMatchDisconnects = "0",
                RiftWarsMMR = "1500.000",
                RankedRiftWarsMatchesPlayed = "0",
                RankedRiftWarsMatchDisconnects = "0",
                CasualMMR = "1500.000",
                CasualRankedMatchDisconnects = "0",
                CasualRankedMatchesLost = "0",
                CasualRankedMatchesPlayed = "0",
                CasualRankedMatchesWon = "0",
                SeasonalRankedMatchesPlayed = 0,
                SeasonalRankedMatchDisconnects = 0,
                CasualSeasonalRankedMatchesPlayed = 0,
                CasualSeasonalRankedMatchDisconnects = 0,
                Experience = "666"
            }
        ];

        return dataPoints;
    }

    private static Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> SetOwnedStoreItemsData(Account account)
    {
        Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> items = account.User.OwnedStoreItems
            .Where(item => item.StartsWith("ma.").Equals(false) && item.StartsWith("cp.").Equals(false))
            .ToDictionary<string, string, OneOf<StoreItemData, StoreItemDiscountCoupon>>(upgrade => upgrade, upgrade => new StoreItemData());

        // TODO: Add Mastery Boosts And Coupons

        return items;
    }

    private static AwardsTooltips SetAwardsTooltips() => new();

    private static List<Notification> SetNotifications()
    {
        // TODO: Implement This

        return [];
    }
}
