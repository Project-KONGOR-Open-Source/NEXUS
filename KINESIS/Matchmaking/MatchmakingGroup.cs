namespace KINESIS.Matchmaking;

#nullable disable

public class MatchmakingGroup
{
    public enum GroupState
    {
        WaitingToStart,
        LoadingResources,
        InQueue,
    }

    public enum GroupUpdateType
    {
        GroupCreated = 0,
        Full = 1,
        Partial = 2,
        ParticipantAdded = 3,
        ParticipantRemoved = 4,
        ParticipantKicked = 5,
    }

    public class Participant
    {
        public readonly int AccountId;
        public readonly ClientInformation ClientInformation;
        public byte LoadingStatus;
        public readonly bool TopOfTheQueue; // TODO: move to ClientInformation?

    }

    public Participant[] Participants = new Participant[0];
    
    // Public Properties (DTO)
    public int GroupId { get; set; }
    public byte GroupType { get; set; }
    public GameFinder.TMMGameType GameType { get; set; }
    public string GameModes { get; set; }
    public string Regions { get; set; }
    public byte Ranked { get; set; }
    public byte MatchFidelity { get; set; }
    public byte BotDifficulty { get; set; }
    public byte RandomizeBots { get; set; }
    public GroupState State { get; set; } = GroupState.WaitingToStart;
    public byte MaxGroupSize { get; set; }
    // public Client.ChatChannel? ChatChannel { get; set; } // Unused for now

    public MatchmakingGroup(byte groupType, GameFinder.TMMGameType gameType, string gameModes, string regions, byte ranked, byte matchFidelity, byte botDifficulty, byte randomizeBots, byte maxGroupSize)
    {
        GroupType = groupType;
        GameType = gameType;
        GameModes = gameModes;
        Regions = regions;
        Ranked = ranked;
        MatchFidelity = matchFidelity;
        BotDifficulty = botDifficulty;
        RandomizeBots = randomizeBots;
        MaxGroupSize = maxGroupSize;
    }




    // Logic methods removed for Models-Only library
    /*
    public void LeaveQueue(bool initiatedByGameFinder) { ... }
    public void NotifyLoadingStatusChanged(int accountId, byte loadingStatus) { ... }
    private bool AddToGameFinderQueue(long timestampWhenJoinedQueue) { ... }
    private static void Broadcast(ProtocolResponse response, Participant[] participants) { ... }
    private void BroadcastUpdate(GroupUpdateType updateType, Participant[] participants) { ... }
    private void BroadcastUpdate(GroupUpdateType updateType, Participant[] participants, int removedOrKickedAccountId) { ... }
    */

    private static string GetMapName(GameFinder.TMMGameType gameType)
    {
        return gameType switch
        {
            GameFinder.TMMGameType.MIDWARS => "midwars",
            GameFinder.TMMGameType.RIFTWARS => "riftwars",
            GameFinder.TMMGameType.CAMPAIGN_NORMAL => "caldavar",
            GameFinder.TMMGameType.NORMAL => "caldavar",
            GameFinder.TMMGameType.CAMPAIGN_CASUAL => "caldavar_old",
            GameFinder.TMMGameType.CASUAL => "caldavar_old",
            _ => "unknown#" + gameType,
        };
    }
}
