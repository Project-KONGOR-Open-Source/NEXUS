namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

/// <summary>
///     Handles user info lookup requests.
///     Returns the user's status and relevant details (channels if online, server information if in game).
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_USER_INFO)]
public class UserInformation : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        UserInfoRequestData requestData = new (buffer);

        ClientChatSession? targetSession = Context.ClientChatSessions.Values
            .SingleOrDefault(chatSession => chatSession.Account.Name.Equals(requestData.TargetName, StringComparison.OrdinalIgnoreCase));

        // Not Found Or Invisible: Treated The Same Way
        if (targetSession is null || targetSession.Metadata.ClientChatModeState is ChatProtocol.ChatModeType.CHAT_MODE_INVISIBLE)
        {
            ChatBuffer response = new ();

            response.WriteCommand(ChatProtocol.Command.CHAT_CMD_USER_INFO_NO_EXIST);
            response.WriteString(requestData.TargetName);

            session.Send(response);

            return;
        }

        switch (targetSession.Metadata.LastKnownClientState)
        {
            case ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED:
                SendOnlineInfo(session, targetSession);
                break;

            case ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_JOINING_GAME:
                SendInGameInfo(session, targetSession, gameName: string.Empty, gamePhase: string.Empty);
                break;

            case ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_IN_GAME:
                SendInGameInfo(session, targetSession,
                    gameName: targetSession.MatchInformation?.MatchName ?? string.Empty,
                    gamePhase: string.Empty);
                break;

            // Disconnected Or Unknown: Treat As Not Found
            default:
                ChatBuffer response = new ();

                response.WriteCommand(ChatProtocol.Command.CHAT_CMD_USER_INFO_NO_EXIST);
                response.WriteString(requestData.TargetName);

                session.Send(response);
                break;
        }
    }

    /// <summary>
    ///     Sends online user info with the list of shared visible channels.
    /// </summary>
    private static void SendOnlineInfo(ClientChatSession requesterSession, ClientChatSession targetSession)
    {
        // Get Channels The Target Is In That The Requester Can See (Non-Hidden, Non-Server)
        List<ChatChannel> visibleChannels = [.. Context.ChatChannels.Values
            .Where(channel => channel.Members.ContainsKey(targetSession.Account.Name))
            .Where(channel => channel.Flags.HasFlag(ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_HIDDEN) is false)
            .Where(channel => channel.Flags.HasFlag(ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_SERVER) is false)];

        ChatBuffer response = new ();

        response.WriteCommand(ChatProtocol.Command.CHAT_CMD_USER_INFO_ONLINE);
        response.WriteString(targetSession.Account.Name);
        response.WriteInt32(visibleChannels.Count);

        foreach (ChatChannel channel in visibleChannels)
            response.WriteString(channel.Name);

        requesterSession.Send(response);
    }

    /// <summary>
    ///     Sends in-game user info with server and match details.
    /// </summary>
    private static void SendInGameInfo(ClientChatSession requesterSession, ClientChatSession targetSession, string gameName, string gamePhase)
    {
        ChatBuffer response = new ();

        response.WriteCommand(ChatProtocol.Command.CHAT_CMD_USER_INFO_IN_GAME);
        response.WriteString(targetSession.Account.Name);
        response.WriteString(gameName);
        response.WriteString(gamePhase);

        requesterSession.Send(response);
    }
}

file class UserInfoRequestData
{
    public byte[] CommandBytes { get; init; }

    public string TargetName { get; init; }

    public UserInfoRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        TargetName = buffer.ReadString();
    }
}
