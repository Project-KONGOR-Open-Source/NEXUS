namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

/// <summary>
///     Relays player spectate requests and their accept/decline responses between clients and match servers.
///     A spectate request from a client is forwarded to the match server hosting the target player's match, which decides whether to allow it; the match server's response is relayed back to the requesting client.
///     This command is sent in both directions and uses the leading sub-type byte to distinguish a request (from a client) from a response (from a match server); the direction is determined here by the type of the session the command arrived on.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_PLAYER_SPECTATE_REQUEST)]
public class PlayerSpectate : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        switch (session)
        {
            // A Client Asks To Spectate Another Player; The Request Is Forwarded To The Target's Match Server
            case ClientChatSession clientSession:
                RelayRequestToMatchServer(clientSession, buffer);
                break;

            // A Match Server Has Accepted Or Declined A Spectate Request; The Response Is Relayed Back To The Requesting Client
            case MatchServerChatSession:
                RelayResponseToClient(buffer);
                break;

            default:
                Log.Error("[BUG] Received Player Spectate Command From An Unsupported Session Type {SessionType}", session.GetType().Name);
                break;
        }
    }

    /// <summary>
    ///     Forwards a client's spectate request to the match server hosting the target player's match.
    /// </summary>
    private static void RelayRequestToMatchServer(ClientChatSession session, ChatBuffer buffer)
    {
        SpectateRequestData requestData = new (buffer);

        MatchServerChatSession? matchServerSession = Context.MatchServerChatSessions.Values
            .SingleOrDefault(matchServer => string.Equals($"{matchServer.Metadata.Address}:{matchServer.Metadata.Port}", requestData.ServerInformation, StringComparison.OrdinalIgnoreCase));

        if (matchServerSession is null)
        {
            Log.Warning(@"Dropping Spectate Request From Account ""{AccountName}"" For Unknown Match Server ""{ServerInformation}""", session.Account.Name, requestData.ServerInformation);

            return;
        }

        ChatBuffer request = new ();

        request.WriteCommand(ChatProtocol.Command.CHAT_CMD_PLAYER_SPECTATE_REQUEST);
        request.WriteInt8(Convert.ToByte(ChatProtocol.PlayerSpectateRequest.PLAYER_SPECTATE_REQUEST)); // Sub-Type: Request
        request.WriteString(session.Account.Name);                                                     // Spectator Name
        request.WriteString(requestData.SpectateeName);                                                // Spectatee Name
        request.WriteInt8(requestData.MentorFlag);                                                     // Mentor Flag

        matchServerSession.Send(request);
    }

    /// <summary>
    ///     Relays a match server's accept/decline response back to the client that requested to spectate.
    /// </summary>
    private static void RelayResponseToClient(ChatBuffer buffer)
    {
        SpectateResponseData responseData = new (buffer);

        ClientChatSession? spectatorSession = Context.ClientChatSessions.Values
            .SingleOrDefault(clientSession => clientSession.Account.Name.Equals(responseData.SpectatorName, StringComparison.OrdinalIgnoreCase));

        if (spectatorSession is null)
            return;

        ChatBuffer response = new ();

        response.WriteCommand(ChatProtocol.Command.CHAT_CMD_PLAYER_SPECTATE_REQUEST);
        response.WriteInt8(Convert.ToByte(ChatProtocol.PlayerSpectateRequest.PLAYER_SPECTATE_REQUEST_RESPONSE)); // Sub-Type: Response
        response.WriteString(responseData.SpectateeName);                                                        // Spectatee Name
        response.WriteInt8(responseData.Result);                                                                 // Accept/Decline Result

        spectatorSession.Send(response);
    }
}

file class SpectateRequestData
{
    public byte[] CommandBytes { get; init; }

    public byte SubType { get; init; }

    public string SpectateeName { get; init; }

    public byte MentorFlag { get; init; }

    public string ServerInformation { get; init; }

    public SpectateRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        SubType = buffer.ReadInt8();
        SpectateeName = buffer.ReadString();
        MentorFlag = buffer.ReadInt8();
        ServerInformation = buffer.ReadString();
    }
}

file class SpectateResponseData
{
    public byte[] CommandBytes { get; init; }

    public byte SubType { get; init; }

    public string SpectatorName { get; init; }

    public string SpectateeName { get; init; }

    public byte Result { get; init; }

    public SpectateResponseData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        SubType = buffer.ReadInt8();
        SpectatorName = buffer.ReadString();
        SpectateeName = buffer.ReadString();
        Result = buffer.ReadInt8();
    }
}
