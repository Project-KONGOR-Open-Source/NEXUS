namespace KINESIS;

public class ClientInformation
{
    public readonly string AccountName;
    public readonly string DisplayedName;
    public readonly Client.ChatClientFlags ChatClientFlags;
    public readonly Client.ChatClientStatus ChatClientStatus;
    public readonly Client.ChatMode ChatMode;
    public readonly string ChatModeDescription;
    public readonly string SelectedChatSymbolCode;
    public readonly string SelectedChatNameColourCode;
    public readonly string SelectedAccountIconCode;
    public readonly int AscensionLevel;
    public readonly string UpperCaseClanName;
    public readonly string ServerAddress;
    public readonly string GameName;
    public readonly int MatchId;
    public readonly int ClanIdOrZero;
    public readonly string ClanTagOrEmpty;
    public readonly int[] FriendAccountIds;
    public readonly int[] ClanmateAccountIds;

    public ClientInformation(string accountName, string displayedName, Client.ChatClientFlags chatClientFlags, Client.ChatClientStatus chatClientStatus, Client.ChatMode chatMode, string chatModeDescription, string selectedChatSymbolCode, string selectedChatNameColourCode, string selectedAccountIconCode, int ascensionLevel, string upperCaseClanName, string serverAddress, string gameName, int matchId, int clanIdOrZero, string clanTagOrEmpty, int[] friendAccountIds, int[] clanmateAccountIds)
    {
        AccountName = accountName;
        DisplayedName = displayedName;
        ChatClientFlags = chatClientFlags;
        ChatClientStatus = chatClientStatus;
        ChatMode = chatMode;
        ChatModeDescription = chatModeDescription;
        SelectedChatSymbolCode = selectedChatSymbolCode;
        SelectedChatNameColourCode = selectedChatNameColourCode;
        SelectedAccountIconCode = selectedAccountIconCode;
        AscensionLevel = ascensionLevel;
        UpperCaseClanName = upperCaseClanName;
        ServerAddress = serverAddress;
        GameName = gameName;
        MatchId = matchId;
        ClanIdOrZero = clanIdOrZero;
        ClanTagOrEmpty = clanTagOrEmpty;
        FriendAccountIds = friendAccountIds;
        ClanmateAccountIds = clanmateAccountIds;
    }
}
