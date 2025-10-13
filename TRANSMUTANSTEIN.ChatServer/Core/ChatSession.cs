namespace TRANSMUTANSTEIN.ChatServer.Core;

public class ChatSession(TCPServer server, IServiceProvider serviceProvider) : TCPSession(server)
{
    /// <summary>
    ///     Gets Set After A Successful Client Handshake, And Contains Metadata About The Client Connected To This Chat Session
    /// </summary>
    public ChatSessionMetadata Metadata { get; set; } = null!;

    /// <summary>
    ///     Gets Set After A Successful Client Handshake, And Contains The Account Information Of The Client Connected To This Chat Session
    /// </summary>
    public Account Account { get; set; } = null!;

    public ILogger Logger { get; } = serviceProvider.GetRequiredService<ILogger<ChatSession>>();

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
        // TODO: Reconsider This Logic, Because The Channels Don't Get Removed Without A Client Action ... Or Find A Way To Remove Channels Just From The Server Side

        List<ChatChannel> channels = [.. Context.ChatChannels.Values.Where(channel => channel.Members.ContainsKey(ClientInformation.Account.Name))];

        Parallel.ForEach(channels, channel => channel.Kick(ClientInformation.Account.ID, ClientInformation.Account.ID));

        UpdateConnectionStatus(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_DISCONNECTED);

        Disconnect();

        if (Context.ChatSessions.TryRemove(Account.Name, out ChatSession? _) is false)
            Logger.LogError(@"Failed To Remove Chat Session For Account Name ""{ClientInformation.Account.Name}""", Account.Name);

        // TODO: Send Disconnect Notification To Friends And Clan Members
    }

    public void UpdateConnectionStatus(ChatProtocol.ChatClientStatus status, MatchServer? matchServer = null)
    {
        if (Metadata.LastKnownClientState == status) return; else Metadata.LastKnownClientState = status;

        if (Metadata.ClientChatModeState is not ChatProtocol.ChatModeType.CHAT_MODE_INVISIBLE)
        {
            List<int> clanMemberIDs = [.. Account.Clan?.Members.Select(clanMember => clanMember.ID) ?? []];
            List<int> friendIDs = [.. Account.FriendedPeers.Select(friend => friend.Identifier)];

            List<ChatSession> onlinePeers = Context.ChatSessions
                .Where(chatSession => friendIDs.Any(friendID => friendID == chatSession.Value.Account.ID) || clanMemberIDs.Any(clanMemberID => clanMemberID == chatSession.Value.Account.ID))
                .Select(chatSession => chatSession.Value).Distinct().ToList();

            Parallel.ForEach(onlinePeers, session =>
            {
                ChatBuffer update = new ();

                update.WriteCommand(ChatProtocol.Command.CHAT_CMD_UPDATE_STATUS);

                update.WriteInt32(Account.ID);                          // Client's Account ID
                update.WriteInt8(Convert.ToByte(status));               // Client's Status
                update.WriteInt8(Account.GetChatClientFlags());         // Client's Flags (Chat Client Type)
                update.WriteInt32(Account.Clan?.ID ?? 0);               // Client's Clan ID
                update.WriteString(Account.Clan?.Name ?? string.Empty); // Client's Clan Name
                update.WriteString(Account.ChatSymbolNoPrefixCode);     // Chat Symbol
                update.WriteString(Account.NameColourNoPrefixCode);     // Name Colour
                update.WriteString(Account.IconNoPrefixCode);           // Account Icon

                if (status is ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_JOINING_GAME || status is ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_IN_GAME)
                {
                    if (matchServer is null)
                    {
                        Logger.LogError(@"[BUG] A Connection Status Update Was Requested For Account Name ""{ClientInformation.Account.Name}"" While Connected To A Match Server, But The Match Server Is NULL", Account.Name);

                        return;
                    }

                    update.WriteString($"{matchServer.IPAddress}:{matchServer.Port}"); // Server Address This Client Is Connected To, In The Form Of "X.X.X.X:P"

                    if (status is ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_IN_GAME)
                    {
                        // TODO: Populate With Real Match Name
                        update.WriteString(string.Empty);                              // Match Name

                        // TODO: Populate With Real Match ID
                        update.WriteInt32(default(int));                               // Match ID

                        update.WriteBool(false);                                       // Has Extended Server Info

                        // TODO: Set Extended Server Info To True And Populate The Following Fields

                        /*
                            [1] EArrangedMatchType - arranged match type
                            [X] string - client's name
                            [X] string - server's region
                            [X] string - server's game mode
                            [1] unsigned char - server's team size
                            [X] string - server's map name
                            [1] unsigned char - server's tier (deprecated)
                            [1] unsigned char - server's official status (0 = unofficial (deprecated), 1 = official with stats, 2 = official without stats)
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
                        */
                    }
                }

                update.WriteInt32(Account.AscensionLevel);                             // Client's Ascension Level

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
        object instance = ActivatorUtilities.CreateInstance(serviceProvider, type);

        if (instance is ISynchronousCommandProcessor synchronous)
            return OneOf<ISynchronousCommandProcessor, IAsynchronousCommandProcessor>.FromT0(synchronous);

        if (instance is IAsynchronousCommandProcessor asynchronous)
            return OneOf<ISynchronousCommandProcessor, IAsynchronousCommandProcessor>.FromT1(asynchronous);

        Logger.LogError(@"[BUG] Command Type ""{TypeName}"" Does Not Implement A Supported Processor Interface", type.Name);

        return null;
    }
}
