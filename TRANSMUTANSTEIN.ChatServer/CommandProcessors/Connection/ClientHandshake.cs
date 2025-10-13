namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.ClientToChatServer.NET_CHAT_CL_CONNECT)]
public class ClientHandshake(MerrickContext merrick, ILogger<ClientHandshake> logger) : IAsynchronousCommandProcessor
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        ClientHandshakeRequestData requestData = new (buffer);

        Account? account = await merrick.Accounts
            .Include(account => account.FriendedPeers)
            .Include(account => account.Clan).ThenInclude(clan => clan!.Members)
            .SingleOrDefaultAsync(account => account.ID == requestData.AccountID);

        // TODO: Check Session Cookie And/Or Session Authentication Hash And Fail With ECR_AUTH_FAILED

        if (account is null)
        {
            logger.LogError(@"[BUG] Account With ID ""{RequestData.AccountID}"" Could Not Be Found", requestData.AccountID);

            ChatBuffer failure = new ();

            failure.WriteCommand(ChatProtocol.ChatServerToClient.NET_CHAT_CL_REJECT);
            failure.WriteInt8(Convert.ToByte(ChatProtocol.ChatRejectReason.ECR_UNKNOWN));
            failure.PrependBufferSize();

            session.SendAsync(failure.Data);

            session.Terminate();

            return;
        }

        // TODO: Check For Account Sharing And Concurrent Sub-Account Connections

        if ($"{requestData.ClientVersionMajor}.{requestData.ClientVersionMinor}.{requestData.ClientVersionPatch}.{requestData.ClientVersionRevision}" is not "4.10.1.0")
        {
            ChatBuffer failure = new ();

            failure.WriteCommand(ChatProtocol.ChatServerToClient.NET_CHAT_CL_REJECT);
            failure.WriteInt8(Convert.ToByte(ChatProtocol.ChatRejectReason.ECR_BAD_VERSION));
            failure.PrependBufferSize();

            session.SendAsync(failure.Data);

            session.Terminate();

            return;
        }

        // Embed The Client Information In The Chat Session
        session.Metadata = new ChatSessionMetadata(requestData);

        // Link The Account To The Chat Session
        session.Account = account;

        // Add The Chat Session To The Chat Sessions Collection
        Context.ChatSessions.AddOrUpdate(account.Name, session, (key, existing) => session);

        ChatBuffer response = new ();

        response.WriteCommand(ChatProtocol.ChatServerToClient.NET_CHAT_CL_ACCEPT);
        response.PrependBufferSize();

        session.SendAsync(response.Data);

        // Notify Self, Clan Members, And Friends That This Client Is Now Connected
        session.UpdateConnectionStatus(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED);

        // TODO: Update Chat Channel Members

        // TODO: Send Initial Update
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

    public ChatProtocol.ChatClientStatus LastKnownClientState = (ChatProtocol.ChatClientStatus) buffer.ReadInt8();

    public ChatProtocol.ChatModeType ClientChatModeState = (ChatProtocol.ChatModeType) buffer.ReadInt8();

    public string ClientRegion = buffer.ReadString();

    public string ClientLanguage = buffer.ReadString();
}
