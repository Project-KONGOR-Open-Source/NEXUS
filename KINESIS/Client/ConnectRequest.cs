namespace KINESIS.Client;


public class ConnectRequestData
{
    public readonly string AccountName;
    public readonly string UserId;
    public readonly int ClanIdOrZero;
    public readonly string ClanTagOrEmpty;
    public readonly string SelectedChatSymbolCode;
    public readonly string SelectedChatNameColourCode;
    public readonly string SelectedAccountIconCode;

    public ConnectRequestData(string accountName, string userId, int? clanId, string? clanTag, ICollection<string> selectedUpgradeCodes)
    {
        AccountName = accountName;
        UserId = userId;
        ClanIdOrZero = clanId ?? 0;
        ClanTagOrEmpty = clanTag ?? string.Empty;

        string? selectedChatSymbolCode = selectedUpgradeCodes.FirstOrDefault(upgrade => upgrade.StartsWith("cs."));
        SelectedChatSymbolCode = selectedChatSymbolCode != null ? selectedChatSymbolCode.Substring(3) : "";

        string? selectedChatNameColourCode = selectedUpgradeCodes.FirstOrDefault(upgrade => upgrade.StartsWith("cc."));
        SelectedChatNameColourCode = selectedChatNameColourCode != null ? selectedChatNameColourCode.Substring(3) : "white";

        string? selectedAccountIconCode = selectedUpgradeCodes.FirstOrDefault(upgrade => upgrade.StartsWith("ai."));
        SelectedAccountIconCode = selectedAccountIconCode != null ? selectedAccountIconCode.Substring(3) : "Default Icon";
    }
}

public class ConnectRequest : ProtocolRequest
{
    // Some of these are currently unused but kept for future reference.
#pragma warning disable IDE0052
    private readonly int _accountId;
    public int AccountId => _accountId;
    private readonly string _sessionCookie;
    public string SessionCookie => _sessionCookie;
    private readonly string _externalIp;
    public string ExternalIp => _externalIp;
    private readonly string _sessionAuthHash;
    public string SessionAuthHash => _sessionAuthHash;
    private readonly int _chatProtocolVersion;
    public int ChatProtocolVersion => _chatProtocolVersion;
    private readonly byte _operatingSystem;
    public byte OperatingSystem => _operatingSystem;
    private readonly byte _osMajorVersion;
    public byte OsMajorVersion => _osMajorVersion;
    private readonly byte _osMinorVersion;
    public byte OsMinorVersion => _osMinorVersion;
    private readonly byte _osMicroVersion;
    public byte OsMicroVersion => _osMicroVersion;
    private readonly string _osBuildCode;
    public string OsBuildCode => _osBuildCode;
    private readonly string _osArchitecture;
    public string OsArchitecture => _osArchitecture;
    private readonly byte _clientVersionMajor;
    public byte ClientVersionMajor => _clientVersionMajor;
    private readonly byte _clientVersionMinor;
    public byte ClientVersionMinor => _clientVersionMinor;
    private readonly byte _clientVersionMicro;
    public byte ClientVersionMicro => _clientVersionMicro;
    private readonly byte _clientVersionHotfix;
    public byte ClientVersionHotfix => _clientVersionHotfix;
    private readonly byte _lastKnownClientState;
    public byte LastKnownClientState => _lastKnownClientState;
    private readonly byte _clientChatModeState;
    public byte ClientChatModeState => _clientChatModeState;
    private readonly string _clientRegion;
    public string ClientRegion => _clientRegion;
    private readonly string _clientLanguage;
    public string ClientLanguage => _clientLanguage;
#pragma warning restore IDE0052

    public ConnectRequest(int accountId, string sessionCookie, string externalIp, string sessionAuthHash, int chatProtocolVersion, byte operatingSystem, byte osMajorVersion, byte osMinorVersion, byte osMicroVersion, string osBuildCode, string osArchitecture, byte clientVersionMajor, byte clientVersionMinor, byte clientVersionMicro, byte clientVersionHotfix, byte lastKnownClientState, byte clientChatModeState, string clientRegion, string clientLanguage)
    {
        _accountId = accountId;
        _sessionCookie = sessionCookie;
        _externalIp = externalIp;
        _sessionAuthHash = sessionAuthHash;
        _chatProtocolVersion = chatProtocolVersion;
        _operatingSystem = operatingSystem;
        _osMajorVersion = osMajorVersion;
        _osMinorVersion = osMinorVersion;
        _osMicroVersion = osMicroVersion;
        _osBuildCode = osBuildCode;
        _osArchitecture = osArchitecture;
        _clientVersionMajor = clientVersionMajor;
        _clientVersionMinor = clientVersionMinor;
        _clientVersionMicro = clientVersionMicro;
        _clientVersionHotfix = clientVersionHotfix;
        _lastKnownClientState = lastKnownClientState;
        _clientChatModeState = clientChatModeState;
        _clientRegion = clientRegion;
        _clientLanguage = clientLanguage;
    }

    public static ConnectRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        ConnectRequest message = new(
            accountId: ReadInt(data, offset, out offset),
            sessionCookie: ReadString(data, offset, out offset),
            externalIp: ReadString(data, offset, out offset),
            sessionAuthHash: ReadString(data, offset, out offset),
            chatProtocolVersion: ReadInt(data, offset, out offset),
            operatingSystem: ReadByte(data, offset, out offset),
            osMajorVersion: ReadByte(data, offset, out offset),
            osMinorVersion: ReadByte(data, offset, out offset),
            osMicroVersion: ReadByte(data, offset, out offset),
            osBuildCode: ReadString(data, offset, out offset),
            osArchitecture: ReadString(data, offset, out offset),
            clientVersionMajor: ReadByte(data, offset, out offset),
            clientVersionMinor: ReadByte(data, offset, out offset),
            clientVersionMicro: ReadByte(data, offset, out offset),
            clientVersionHotfix: ReadByte(data, offset, out offset),
            lastKnownClientState: ReadByte(data, offset, out offset),
            clientChatModeState: ReadByte(data, offset, out offset),
            clientRegion: ReadString(data, offset, out offset),
            clientLanguage: ReadString(data, offset, out offset)
        );

        updatedOffset = offset;
        return message;
    }

}

