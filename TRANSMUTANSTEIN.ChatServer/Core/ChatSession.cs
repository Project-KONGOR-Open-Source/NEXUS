namespace TRANSMUTANSTEIN.ChatServer.Core;

public class ChatSession(TCPServer server, IServiceProvider serviceProvider) : TCPSession(server)
{
    public ClientInformation ClientInformation { get; set; } = null!; // TODO: If This Is NULL Or Equivalent, Don't Process Data Other Than Client Handshake

    private IServiceProvider ServiceProvider { get; set; } = serviceProvider;

    private ILogger Logger { get; } = serviceProvider.GetRequiredService<ILogger<ChatSession>>();

    private static ConcurrentDictionary<ushort, Type> CommandToTypeMap { get; set; } = [];

    private byte[] RemainingPreviouslyReceivedData { get; set; } = [];

    protected override void OnConnected()
    {
        Logger.LogInformation("Chat Session ID {SessionID} Was Created", ID);
    }

    protected override void OnError(SocketError error)
    {
        Logger.LogInformation("Chat Session ID {SessionID} Caught A Socket Error With Code {SocketErrorCode}", ID, error);
    }

    protected override void OnDisconnected()
    {
        Logger.LogInformation("Chat Session ID {SessionID} Has Terminated", ID);
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        byte[] received = RemainingPreviouslyReceivedData.Concat(buffer[(int)offset..(int)size]).ToArray();

        List<byte[]> segments = ExtractDataSegments(received, out byte[] remaining);

        RemainingPreviouslyReceivedData = remaining;

        Parallel.ForEach(segments, ProcessDataSegment);
    }

    public void Terminate()
    {
        UpdateConnectionStatus(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_DISCONNECTED);

        // TODO: Disconnect From Chat Channels

        if (Context.ChatSessions.TryRemove(ClientInformation.Account.Name, out ChatSession? _) is false)
            Logger.LogError(@"Failed To Remove Chat Session For Account Name ""{ClientInformation.Account.Name}""", ClientInformation.Account.Name);

        Disconnect();
    }

    public void UpdateConnectionStatus(ChatProtocol.ChatClientStatus status)
    {
        if (ClientInformation.LastKnownClientState == status) return; else ClientInformation.LastKnownClientState = status;

        if (ClientInformation.ClientChatModeState is not ChatProtocol.ChatModeType.CHAT_MODE_INVISIBLE)
        {
            List<int> clanMemberIDs = [.. ClientInformation.Account.Clan?.Members.Select(clanMember => clanMember.ID) ?? []];
            List<int> friendIDs = [.. ClientInformation.Account.FriendedPeers.Select(friend => friend.Identifier)];

            List<ChatSession> onlinePeers = Context.ChatSessions
                .Where(chatSession => friendIDs.Any(friendID => friendID == chatSession.Value.ClientInformation?.Account.ID) || clanMemberIDs.Any(clanMemberID => clanMemberID == chatSession.Value.ClientInformation?.Account.ID))
                .Select(chatSession => chatSession.Value).Distinct().ToList();

            throw new NotImplementedException("ChatProtocol.Command.CHAT_CMD_UPDATE_STATUS / ChatServerProtocol.GameClientToChatServer.UpdateStatus"); // TODO: Implement Response

            Parallel.ForEach(onlinePeers, session =>
            {
                ChatBuffer update = new ();

                update.WriteCommand(ChatProtocol.Command.CHAT_CMD_UPDATE_STATUS);

                /*
                    [4] unsigned long - client's account ID
                    [1] EChatClientStatus - client's status
                    [1] unsigned char - client's flags (refer to CHAT_CLIENT_* flags)
                    [4] unsigned long - client's clan ID
                    [X] string - client's clan name
                    [X] string - client's chat symbol
                    [X] string - client's chat name color
                    [X] string - client's account icon
                    if (status > CHAT_CLIENT_STATUS_CONNECTED)
                        [X] string - server address this client is connected to, in the form of "X.X.X.X:X"
                    if (status == CHAT_CLIENT_STATUS_IN_GAME)
                        [X] string - game name this client is connected to
                        [4] unsigned long - match ID this client is in
                        [1] bool - has extended server info
                        if (has extended server info)
                            [1] EArrangedMatchType - arranged match type
                            [X] string - client's name
                            [X] string - server's region
                            [X] string - server's game mode
                            [1] unsigned char - server's team size
                            [X] string - server's map name
                            [1] unsigned char - server's tier (deprecated)
                            [1] unsigned char - server's official status (0 = unofficial (deprecated), 1 = official w/ stats, 2 = official w/o stats)
                            [1] bool - server's "no leavers" flag
                            [1] bool - server's "private" flag
                            [1] bool - server's "all heroes" flag
                            [1] bool - server's "casual mode" flag
                            [1] bool - server's "all random" flags (deprecated)
                            [1] bool - server's "auto balanced" flag
                            [1] bool - server's "advanced options" flag
                            [2] unsigned short - server's minimum PSR allowed
                            [2] unsigned short - server's maximum PSR allowed
                            [1] bool - server's "dev heroes" flag
                            [1] bool - server's "hardcore" flag
                            [1] bool - server's "verified only" flag
                            [1] bool - server's "gated" flag

                    public override CommandBuffer Encode()
                    {
                        CommandBuffer response = new();
                        response.WriteInt16(ChatServerProtocol.GameClientToChatServer.UpdateStatus);
                        response.WriteInt32(AccountId);
                        response.WriteInt8(Convert.ToByte(ChatClientStatus));
                        response.WriteInt8(Flags);

                        if (Clan == null)
                        {
                            response.WriteInt32(0);
                            response.WriteString("");
                        }
                        else
                        {
                            response.WriteInt32(Clan.ClanId);
                            response.WriteString(Clan.Name);
                        }

                        response.WriteString(SelectedChatSymbolCode);
                        response.WriteString(SelectedChatNameColourCode);
                        response.WriteString(SelectedAccountIconCode);

                        switch (ChatClientStatus)
                        {
                            case ChatServerProtocol.ChatClientStatus.JoiningGame:
                                response.WriteString(ServerAddress);
                                break;
                            case ChatServerProtocol.ChatClientStatus.InGame:
                                response.WriteString(ServerAddress);
                                response.WriteString(GameName);
                                response.WriteInt32(MatchId);

                                // 0 - done, 1 - more data about the match to follow.
                                response.WriteInt8(0);
                                break;
                        }

                        response.WriteInt32(AscensionLevel);

                        return response;
                    }
                */

                update.PrependBufferSize();

                session.SendAsync(update.Data);
            });
        }
    }

    private static List<byte[]> ExtractDataSegments(byte[] buffer, out byte[] remaining)
    {
        remaining = [];

        List<byte[]> segments = [];

        int offset = 0;

        while (offset < buffer.Length)
        {
            ushort size = BitConverter.ToUInt16([buffer[offset + 0], buffer[offset + 1]]);

            if (size + 2 <= buffer.Length - offset)
            {
                byte[] segment = buffer[(offset + 2)..(offset + 2 + size)];

                segments.Add(segment);

                offset += 2 + size;
            }

            else
            {
                remaining = buffer[offset..buffer.Length];

                offset = buffer.Length;
            }
        }

        return segments;
    }

    private void ProcessDataSegment(byte[] segment)
    {
        ushort command = BitConverter.ToUInt16([segment[0], segment[1]]);

        Type? commandType = GetCommandType(command);

        if (commandType is null)
        {
            string output = new StringBuilder($@"Missing Type Mapping For Command: ""0x{command:X4}""")
                .Append(Environment.NewLine).Append($"Message UTF8 Bytes: {string.Join(':', segment)}")
                .Append(Environment.NewLine).Append($"Message UTF8 Text: {Encoding.UTF8.GetString(segment)}")
                .ToString();

            Logger.LogError(output);
        }

        else
        {
            if (TRANSMUTANSTEIN.RunsInDevelopmentMode)
                Logger.LogDebug(@"Processing Command: ""0x{Command}""", command.ToString("X4"));

            if (GetCommandTypeInstance(commandType) is { } commandTypeInstance)
            {
                try
                {
                    ChatBuffer buffer = new (segment);

                    commandTypeInstance.Switch
                        (synchronous => synchronous.Process(this, buffer), async asynchronous => await asynchronous.Process(this, buffer));
                }

                catch (Exception exception)
                {
                    Logger.LogError(exception, @"[BUG] Error Processing Command: ""0x{Command}""", command.ToString("X4"));
                }
            }

            else Logger.LogError(@"[BUG] Could Not Create Command Type Instance For Command: ""0x{Command}""", command.ToString("X4"));
        }
    }

    private Type? GetCommandType(ushort command)
    {
        if (CommandToTypeMap.TryGetValue(command, out Type? type))
            return type;

        Type[] types = typeof(TRANSMUTANSTEIN).Assembly.GetTypes();

        type = types.SingleOrDefault(type => type.GetCustomAttribute<ChatCommandAttribute>() is not null
            && (type.GetCustomAttribute<ChatCommandAttribute>()?.Command.Equals(command) ?? false));

        if (type is not null)
            if (CommandToTypeMap.TryAdd(command, type) is false && CommandToTypeMap.ContainsKey(command) is false)
                Logger.LogError(@"[BUG] Could Not Add Command-To-Type Mapping For Command ""0x{Command}"" And Type ""{TypeName}""", command.ToString("X4"), type.Name);

        return type;
    }

    private OneOf<ISynchronousCommandProcessor, IAsynchronousCommandProcessor>? GetCommandTypeInstance(Type type)
    {
        object instance = ActivatorUtilities.CreateInstance(ServiceProvider, type);

        if (instance is ISynchronousCommandProcessor synchronous)
            return OneOf<ISynchronousCommandProcessor, IAsynchronousCommandProcessor>.FromT0(synchronous);

        if (instance is IAsynchronousCommandProcessor asynchronous)
            return OneOf<ISynchronousCommandProcessor, IAsynchronousCommandProcessor>.FromT1(asynchronous);

        Logger.LogError(@"[BUG] Command Type ""{TypeName}"" Does Not Implement A Supported Processor Interface", type.Name);

        return null;
    }
}
