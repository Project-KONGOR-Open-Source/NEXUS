namespace KINESIS;

public class ChatServerRequest
{
    public const short SendChatChannelMessage = 0x0003;
    public const short WhisperToPlayer = 0x0008;
    public const short NotifyJoiningGame = 0x000F;
    public const short NotifyJoinedGame = 0x0010;
    public const short NotifyLeftGame = 0x0011;
    public const short WhisperToClanmates = 0x0013;
    public const short JoinChatChannel = 0x001E;
    public const short WhisperToBuddies = 0x0020;
    public const short LeaveChatChannel = 0x0022;
    public const short TrackPlayerAction = 0x00B9;
    public const short ConnectServer = 0x0500;
    public const short DisconnectServer = 0x0501;
    public const short UpdateServerStatus = 0x0502;
    public const short ConnectClient = 0x0C00;
    public const short CreateMatchmakingGroup = 0x0C0A;
    public const short LeaveMatchmakingGroup = 0x0C0C;
    public const short LeaveMatchmakingQueue = 0x0D02;
    public const short LoadingStatusChanged = 0x0D04;
    public const short EnterMatchmakingQueue = 0x0D05;
    public const short RefreshMatchmakingSettings = 0x0D07;
    public const short RefreshMatchmakingStats = 0x0F07;
    public const short ConnectManager = 0x1600;
    public const short DisconnectManager = 0x1601;
    public const short UpdateManagerStatus = 0x1602;
    public const short Ping = 0x2A00;
}
