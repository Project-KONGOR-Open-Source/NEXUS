namespace KONGOR.MasterServer.Handlers.SRP;

public static class SRPHandlers
{
    public static SRPAuthenticationResponseStageTwo GenerateStageTwoResponse(StageTwoResponseParameters parameters)
    {
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
            Cookie = Guid.NewGuid().ToString(),
            IPAddress = parameters.ClientIPAddress,
            LeaverThreshold = ".05", // TODO: Set Per Partition Of Games Played (e.g. under 50 games = 20%, between 50 and 100 games = 10%, over 100 games = 5%)
            HasSubAccounts = parameters.Account.User.Accounts.Any(account => account.IsMain.Equals(false)),
            IsSubAccount = parameters.Account.IsMain.Equals(false),
            ICBURL = GetHTTPSApplicationURL(),
            AuthenticationHash = null,
            ChatServerIPAddress = null,
            ChatServerPort = null,
            ChatChannels = null,
            Accounts = null,
            GoldCoins = null,
            SilverCoins = 0,
            SlotID = null,
            CurrentSeason = null,
            MuteExpiration = 0,
            FriendAccountList = null,
            IgnoredAccountsList = null,
            BannedAccountsList = null,
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
        string urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? throw new NullReferenceException(@"""ASPNETCORE_URLS"" Environment Variable Is NULL");

        // This Works On The Assumption That Each Set Of URLs Has One HTTP Address And One HTTPS Address, Exactly In This Order
        string https = urls.Split(";").Last();

        // This Works On The Assumption That URLs In Development Are Always "localhost" And A Port Number, While URLs In Production Are Always A Public Sub-Domain With No Explicitly-Defined Port Number
        string mutated = https.Replace("localhost", "127.0.0.1");

        return mutated;
    }
}
