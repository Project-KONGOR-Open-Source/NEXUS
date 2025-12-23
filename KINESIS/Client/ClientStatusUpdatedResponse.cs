namespace KINESIS.Client;

public class ClientStatusUpdatedResponse : ProtocolResponse
{
    private readonly int _accountId;
    public int AccountId => _accountId;
    private readonly ChatClientStatus _chatClientStatus;
    public ChatClientStatus ChatClientStatus => _chatClientStatus;
    private readonly ChatClientFlags _chatClientFlags;
    public ChatClientFlags ChatClientFlags => _chatClientFlags;
    private readonly string _serverAddress;
    public string ServerAddress => _serverAddress;
    private readonly string _gameName;
    public string GameName => _gameName;
    private readonly int _matchId;
    public int MatchId => _matchId;
    private readonly int _clanId;
    public int ClanId => _clanId;
    private readonly string _clanTag;
    public string ClanTag => _clanTag;
    private readonly string _selectedChatSymbolCode;
    public string SelectedChatSymbolCode => _selectedChatSymbolCode;
    private readonly string _selectedChatNameColourCode;
    public string SelectedChatNameColourCode => _selectedChatNameColourCode;
    private readonly string _selectedAccountIconCode;
    public string SelectedAccountIconCode => _selectedAccountIconCode;
    private readonly int _ascensionLevel;
    public int AscensionLevel => _ascensionLevel;

    public ClientStatusUpdatedResponse(int accountId, ChatClientStatus chatClientStatus, ChatClientFlags chatClientFlags, string serverAddress, string gameName, int matchId, int clanId, string clanTag, string selectedChatSymbolCode, string selectedChatNameColourCode, string selectedAccountIconCode, int ascensionLevel)
    {
        _accountId = accountId;
        _chatClientStatus = chatClientStatus;
        _chatClientFlags = chatClientFlags;
        _serverAddress = serverAddress;
        _gameName = gameName;
        _matchId = matchId;
        _clanId = clanId;
        _clanTag = clanTag;
        _selectedChatSymbolCode = selectedChatSymbolCode;
        _selectedChatNameColourCode = selectedChatNameColourCode;
        _selectedAccountIconCode = selectedAccountIconCode;
        _ascensionLevel = ascensionLevel;
    }

    public override CommandBuffer Encode()
    {
        CommandBuffer response = new();
        response.WriteInt16(ChatServerResponse.ClientStatusUpdated);
        response.WriteInt32(_accountId);
        response.WriteInt8(Convert.ToByte(_chatClientStatus));
        response.WriteInt8(Convert.ToByte(_chatClientFlags));
        response.WriteInt32(_clanId);
        response.WriteString(_clanTag);
        response.WriteString(_selectedChatSymbolCode);
        response.WriteString(_selectedChatNameColourCode);
        response.WriteString(_selectedAccountIconCode);

        switch (_chatClientStatus)
        {
            case ChatClientStatus.JoiningGame:
                response.WriteString(_serverAddress);
                break;
            case ChatClientStatus.InGame:
                response.WriteString(_serverAddress);
                response.WriteString(_gameName);
                response.WriteInt32(_matchId);

                // 0 - done, 1 - more data about the match to follow.
                response.WriteInt8(0);
                break;
        }

        response.WriteInt32(_ascensionLevel);

        return response;
    }
}

