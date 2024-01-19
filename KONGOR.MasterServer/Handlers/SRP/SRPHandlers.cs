namespace KONGOR.MasterServer.Handlers.SRP;

public static class SRPHandlers
{
    public static SRPAuthenticationResponseStageTwo GenerateStageTwoResponse(StageTwoResponseParameters parameters)
    {
        SRPAuthenticationResponseStageTwo response = new()
        {
            ServerProof = null,
            MainAccountID = null,
            ID = null,
            GarenaID = null,
            Name = null,
            Email = null,
            AccountType = null,
            SuspensionID = null,
            UseCloud = null,
            Cookie = null,
            IPAddress = null,
            LeaverThreshold = null,
            HasSubAccounts = false,
            IsSubAccount = false,
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
        public required string ServerProof { get; set; }
    }
}
