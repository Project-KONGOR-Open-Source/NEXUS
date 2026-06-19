namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

/// <summary>
///     Handles a client's request for information about another player's in-progress match, used by the client to display a player's current game from their profile or context menu.
///     The match server the target player is connected to is resolved from the target's session, and its most recent status (game name, map, mode, phase, and team/player rosters) is returned to the requester.
///     When the target is not online or not in a match, no response is sent, mirroring the original chat server.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_REQUEST_GAME_INFO)]
public class RequestGameInfo : ISynchronousCommandProcessor<ClientChatSession>
{
    /// <summary>
    ///     The number of team and player information strings the response always carries: the Legion team, the Hellbourne team, and ten player slots.
    /// </summary>
    private const int TeamAndPlayerInformationCount = 12;

    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        RequestGameInfoRequestData requestData = new (buffer);

        ClientChatSession? targetSession = Context.ClientChatSessions.Values
            .SingleOrDefault(clientSession => clientSession.Account.Name.Equals(requestData.TargetName, StringComparison.OrdinalIgnoreCase));

        // The Target Must Be Online And Connected To A Match Server
        if (targetSession?.Metadata.MatchServerConnectedTo is not MatchServer matchServer)
            return;

        if (Context.MatchServerChatSessions.TryGetValue(matchServer.ID, out MatchServerChatSession? matchServerSession) is false)
            return;

        MatchServerChatSessionMetadata metadata = matchServerSession.Metadata;

        ChatBuffer response = new ();

        response.WriteCommand(ChatProtocol.Command.CHAT_CMD_REQUEST_GAME_INFO);
        response.WriteString(targetSession.Account.Name);                                        // Player Name
        response.WriteString(metadata.GameName ?? string.Empty);                                 // Game Name
        response.WriteString(metadata.MapName ?? string.Empty);                                  // Map Name
        response.WriteBool(metadata.CasualMode);                                                 // Casual
        response.WriteBool(metadata.Hardcore);                                                   // Hardcore (Deprecated)
        response.WriteBool(metadata.Gated);                                                      // Gated
        response.WriteString(metadata.GameModeName ?? string.Empty);                             // Game Mode Name
        response.WriteString(BuildGamePhase(metadata.GamePhase, metadata.GameTimeMilliseconds)); // Game Phase

        // Legion Team Information, Hellbourne Team Information, And Ten Player Information Slots
        for (int index = 0; index < TeamAndPlayerInformationCount; index++)
            response.WriteString(metadata.TeamAndPlayerInformation.ElementAtOrDefault(index) ?? string.Empty);

        session.Send(response);
    }

    /// <summary>
    ///     Builds the human-readable game phase string the client displays.
    ///     For the in-progress phases this is the elapsed match time formatted as a clock; for all other phases it is a fixed phase label.
    /// </summary>
    private static string BuildGamePhase(int gamePhase, int gameTimeMilliseconds) => gamePhase switch
    {
        0  => "Ready To Host",
        1  => "Lobby",
        2  => "Banning",
        3  => "Hero Select",
        4  => FormatMatchDuration(gameTimeMilliseconds),
        5  => FormatMatchDuration(gameTimeMilliseconds),
        6  => FormatMatchDuration(gameTimeMilliseconds),
        7  => "Finished",
        8  => "Blind Banning",
        9  => "Locking",
        10 => "Lock Picking",
        11 => "Shuffle Picking",
        _  => "0:00:00"
    };

    /// <summary>
    ///     Formats an elapsed match time, expressed in milliseconds, as an "H:MM:SS" clock; a non-positive duration is reported as a pre-match state.
    /// </summary>
    private static string FormatMatchDuration(int gameTimeMilliseconds)
    {
        if (gameTimeMilliseconds <= 0)
            return "Pre-Match";

        int hours = gameTimeMilliseconds / (60 * 60 * 1000);
        int minutes = gameTimeMilliseconds % (60 * 60 * 1000) / (60 * 1000);
        int seconds = gameTimeMilliseconds % (60 * 1000) / 1000;

        return $"{hours}:{minutes:D2}:{seconds:D2}";
    }
}

file class RequestGameInfoRequestData
{
    public byte[] CommandBytes { get; init; }

    public string TargetName { get; init; }

    public RequestGameInfoRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        TargetName = buffer.ReadString();
    }
}
