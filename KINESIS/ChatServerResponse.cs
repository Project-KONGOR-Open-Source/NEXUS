namespace KINESIS;

public class ChatServerResponse
{
    public const short ReceivedChatChannelMessage = 0x0003;
    public const short FullChannelUpdate = 0x0004;
    public const short JoinedChatChannel = 0x0005;
    public const short LeftChatChannel = 0x0006;
    public const short WhisperedToPlayer = 0x0008;
    public const short WhisperFailed = 0x0009;
    public const short ClientStatusUpdated = 0x000C;
    public const short WhisperedToClanmates = 0x0013;
    public const short ClanWhisperFailed = 0x0014;
    public const short WhisperedToBuddies = 0x0020;
    public const short ChatModeAutoResponse = 0x0067;
    public const short PlayerCount = 0x0068;

    public const short EnteredMatchmakingQueue = 0x0D01;
    public const short LeftMatchmakingQueue = 0x0D02;
    public const short MatchmakingGroupUpdated = 0x0D03;
    public const short GroupQueueStateChanged = 0x0D06;
    public const short MatchmakingSettingsResponse = 0x0D07;

    public const short FailedToJoinMatchmakingGroup = 0x0E0A;

    public const short StartedLoadingResources = 0x0F03;
    public const short RefreshMatchmakingStatsResponse = 0x0F07;

    public const short ServerConnectionAccepted = 0x1500;
    public const short ServerConnectionRejected = 0x1501;

    public const short ManagerConnectionAccepted = 0x1700;
    public const short UpdateManagerOptions = 0x1703;

    public const short ConnectionAccepted = 0x1C00;
    public const short ConnectionRejected = 0x1C01;

    public const short PingReceived = 0x2A01;
}
