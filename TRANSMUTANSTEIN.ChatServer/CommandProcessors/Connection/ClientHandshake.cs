namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.ClientToChatServer.NET_CHAT_CL_CONNECT)]
public class ClientHandshake(MerrickContext merrick, ILogger<ClientHandshake> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;
    private ILogger<ClientHandshake> Logger { get; set; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        ClientHandshakeRequestData requestData = new (buffer);

        Account? account = await MerrickContext.Accounts.Include(account => account.Clan).SingleOrDefaultAsync(account => account.ID == requestData.AccountID);

        if (account is null)
        {
            Logger.LogError($@"[BUG] Account With ID ""{requestData.AccountID}"" Could Not Be Found");

            return;
        }

        // TODO: Check Cookie

        // TODO: Check Authentication Hash

        // Embed The Client Information In The Chat Session
        session.ClientInformation = new ClientInformation(requestData, account);

        // Add The Chat Session To The Chat Sessions Collection
        Context.ChatSessions.AddOrUpdate(account.Name, session, (key, existing) => session);

        Response.WriteCommand(ChatProtocol.ChatServerToClient.NET_CHAT_CL_ACCEPT);
        Response.PrependBufferSize();

        session.SendAsync(Response.Data);
    }
}

public class ClientHandshakeRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();
    public int AccountID = buffer.ReadInt32();
    public string SessionCookie = buffer.ReadString();
    public string RemoteIP = buffer.ReadString();
    public string SessionAuthenticationHash = buffer.ReadString();
    public int ChatProtocolVersion = buffer.ReadInt32();
    public byte OperatingSystemIdentifier = buffer.ReadInt8();
    public byte OperatingSystemVersionMajor = buffer.ReadInt8();
    public byte OperatingSystemVersionMinor = buffer.ReadInt8();
    public byte OperatingSystemVersionPatch = buffer.ReadInt8();
    public string OperatingSystemBuildCode = buffer.ReadString();
    public string OperatingSystemArchitecture = buffer.ReadString();
    public byte ClientVersionMajor = buffer.ReadInt8();
    public byte ClientVersionMinor = buffer.ReadInt8();
    public byte ClientVersionPatch = buffer.ReadInt8();
    public byte ClientVersionRevision = buffer.ReadInt8();
    public byte LastKnownClientState = buffer.ReadInt8();
    public byte ClientChatModeState = buffer.ReadInt8();
    public string ClientRegion = buffer.ReadString();
    public string ClientLanguage = buffer.ReadString();
}

public class ClientInformation(ClientHandshakeRequestData data, Account account)
{
    public Account Account = account;

    public string SessionCookie = data.SessionCookie;
    public string RemoteIP = data.RemoteIP;
    public string SessionAuthenticationHash = data.SessionAuthenticationHash;
    public int ChatProtocolVersion = data.ChatProtocolVersion;
    public byte OperatingSystemIdentifier = data.OperatingSystemIdentifier;
    public byte OperatingSystemVersionMajor = data.OperatingSystemVersionMajor;
    public byte OperatingSystemVersionMinor = data.OperatingSystemVersionMinor;
    public byte OperatingSystemVersionPatch = data.OperatingSystemVersionPatch;
    public string OperatingSystemBuildCode = data.OperatingSystemBuildCode;
    public string OperatingSystemArchitecture = data.OperatingSystemArchitecture;
    public byte ClientVersionMajor = data.ClientVersionMajor;
    public byte ClientVersionMinor = data.ClientVersionMinor;
    public byte ClientVersionPatch = data.ClientVersionPatch;
    public byte ClientVersionRevision = data.ClientVersionRevision;
    public byte LastKnownClientState = data.LastKnownClientState;
    public byte ClientChatModeState = data.ClientChatModeState;
    public string ClientRegion = data.ClientRegion;
    public string ClientLanguage = data.ClientLanguage;
}
