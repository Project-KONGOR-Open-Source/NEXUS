namespace KONGOR.MasterServer.Handlers.SRP;

public static class SRPHandlers
{
    public static SRPAuthenticationResponseStageTwo GenerateStageTwoResponse(StageTwoResponseParameters parameters)
    {
        string cookie = Guid.NewGuid().ToString();

        SRPAuthenticationResponseStageTwo response = new()
        {
            ServerProof = parameters.ServerProof,
            MainAccountID = parameters.Account.User.Accounts.Single(account => account.IsMain).ID.ToString(),
            ID = parameters.Account.ID.ToString(),
            GarenaID = parameters.Account.ID.ToString(),
            Name = parameters.Account.Name,
            Email = parameters.Account.User.EmailAddress,
            AccountType = parameters.Account.Type.ToString(),
            SuspensionID = string.Empty, // TODO: Implement Suspensions
            UseCloud = "0", // TODO: Implement Cloud Backups
            Cookie = cookie,
            IPAddress = parameters.ClientIPAddress,
            LeaverThreshold = ".05", // TODO: Set Per Partition Of Games Played (e.g. under 50 games = 20%, between 50 and 100 games = 10%, over 100 games = 5%)
            HasSubAccounts = parameters.Account.User.Accounts.Any(account => account.IsMain.Equals(false)),
            IsSubAccount = parameters.Account.IsMain.Equals(false),
            ICBURL = GetHTTPSApplicationURL(), // TODO: Fix This (The Port In The Environment Variable Is Different From The One In The Launch Profile)
            AuthenticationHash = ComputeChatServerCookieHash(parameters.Account.ID, parameters.ClientIPAddress, cookie),
            ChatServerIPAddress = parameters.ChatServer.Protocol + "://" + parameters.ChatServer.Host,
            ChatServerPort = parameters.ChatServer.Port.ToString(),
            ChatChannels = SetChatChannels(parameters.Account),
            Accounts = parameters.Account.User.Accounts.OrderBy(account => account.TimestampCreated).Select(account => new List<string> { account.Name, account.ID.ToString() }).ToList(),
            GoldCoins = parameters.Account.User.GoldCoins.ToString(),
            SilverCoins = parameters.Account.User.SilverCoins,
            SlotID = null,
            CurrentSeason = null,
            MuteExpiration = 0,
            FriendAccountList = SetFriendAccountList(parameters.Account),
            IgnoredAccountsList = SetIgnoredAccountsList(parameters.Account),
            BannedAccountsList = SetBannedAccountsList(parameters.Account),
            ClanRoster = null,
            ClanMembershipData = default,
            OwnedStoreItems = null,
            SelectedStoreItems = null,
            OwnedStoreItemsData = null,
            AwardsTooltip = null,
            DataPoints = null,
            CloudStorageInformation = null,
            Notifications = null
        };

        return response;
    }

    public class StageTwoResponseParameters()
    {
        public required Account Account { get; set; }
        public required string ServerProof { get; set; }
        public required string ClientIPAddress { get; set; }
        public required (string Protocol, string Host, int Port) ChatServer { get; set; }
    }

    private static string GetHTTPSApplicationURL()
    {
        string urlCollection = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? throw new NullReferenceException(@"""ASPNETCORE_URLS"" Environment Variable Is NULL");

        // This Works On The Assumption That Each Set Of URLs Has One HTTP Address And One HTTPS Address, Exactly In This Order
        string https = urlCollection.Split(";").Last();

        // This Works On The Assumption That URLs In Development Are Always "localhost" And A Port Number, While URLs In Production Are Always A Public Sub-Domain With No Explicitly-Defined Port Number
        string mutated = https.Replace("localhost", "127.0.0.1");

        return mutated;
    }

    # region Chat Server Authentication Secret

    // Thank you, Denis Koroskin (aka korDen), for making this value public: https://github.com/korDen/honserver/blob/b35c23015fe915633dc5acd52db44403066cb0eb/utils.php#L6.
    // It is not clear how this magic string was obtained.
    // Project KONGOR would have not been possible without having this value in the public domain.

    private const string ChatServerAuthenticationSalt = "8roespiemlasToUmiuglEhOaMiaSWlesplUcOAniupr2esPOeBRiudOEphiutOuJ";

    # endregion

    private static string ComputeChatServerCookieHash(Guid accountID, string remoteIPAddress, string cookie)
    {
        string chatServerCookie = accountID + remoteIPAddress + cookie + ChatServerAuthenticationSalt;

        // The uppercase hash (C# default) needs to be lowercased, to match the lowercase hash (C++ default) that the game client generates.
        string chatServerCookieHash = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(chatServerCookie))).ToLower();

        return chatServerCookieHash;
    }

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

    private static Dictionary<Guid, Dictionary<Guid, FriendAccount>> SetFriendAccountList(Account account)
        => new() { { account.ID, account.FriendAccounts.ToDictionary(friend => friend.ID,
            friend => new FriendAccount { ID = friend.ID.ToString(), Name = friend.SelfAccount.Name, Group = friend.Group, ClanTag = friend.SelfAccount.Clan?.Tag ?? string.Empty } ) } };

    private static Dictionary<Guid, List<IgnoredAccount>> SetIgnoredAccountsList(Account account)
        => new() { { account.ID, account.IgnoredAccounts
            .Select(ignored => new IgnoredAccount { ID = ignored.ID.ToString(), Name = ignored.SelfAccount.Name }).ToList() } };

    private static Dictionary<Guid, List<BannedAccount>> SetBannedAccountsList(Account account)
        => new() { { account.ID, account.BannedAccounts
            .Select(banned => new BannedAccount { ID = banned.ID.ToString(), Name = banned.SelfAccount.Name, Reason = banned.Reason }).ToList() } };
}
