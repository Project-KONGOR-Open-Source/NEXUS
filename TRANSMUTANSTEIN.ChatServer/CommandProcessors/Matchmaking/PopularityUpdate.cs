namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_POPULARITY_UPDATE)]
public class PopularityUpdate : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        PopularityUpdateRequestData requestData = new (buffer);

        SendMatchmakingPopularity(session);
    }

    public static void SendMatchmakingPopularity(ChatSession session)
    {
        // Legacy Logic Hardcoded for Stability
        List<string> enabledMaps = ["caldavar", "midwars"];
        List<string> enabledGameTypes = ["1", "3", "4", "5", "6"]; // Matching legacy TMMGameTypes
        List<string> enabledGameModes = ["ap", "sd", "ar", "bd", "bp"];
        List<string> enabledRegions = ["USE", "USW", "EU"];

        // Construct Pipe Strings
        string availableMaps = string.Join("|", enabledMaps);
        string gameTypes = string.Join("|", enabledGameTypes);
        string gameModes = string.Join("|", enabledGameModes);
        string availableRegions = string.Join("|", enabledRegions);

        // Legacy Expects Empty Strings for Disabled Lists
        string disabledGameModesByGameType = "";
        string disabledGameModesByRankType = "";
        string disabledGameModesByMap = "";
        string restrictedRegions = "";
        string clientCountryCode = "";
        
        // Legacy Legend String
        string legend = "maps:modes:|regions:";
        
        ChatBuffer response = new ();

        response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_POPULARITY_UPDATE);

        response.WriteInt8(1);                                  // TMM Availability
        response.WriteString(availableMaps);                    // Maps
        response.WriteString(gameTypes);                        // Types
        response.WriteString(gameModes);                        // Modes
        response.WriteString(availableRegions);                 // Regions
        response.WriteString(disabledGameModesByGameType);
        response.WriteString(disabledGameModesByRankType);
        response.WriteString(disabledGameModesByMap);
        response.WriteString(restrictedRegions);
        response.WriteString(clientCountryCode);
        response.WriteString(legend);

        // Popularity By Game Map (Count = Maps)
        foreach (string _ in enabledMaps) response.WriteInt8(10);

        // Popularity By Game Type (Count = Types * Maps) - CRITICAL: Legacy logic creates array of size Types * Maps
        for (int i = 0; i < enabledGameTypes.Count * enabledMaps.Count; i++) response.WriteInt8(10);

        // Popularity By Game Mode (Count = Modes)
        foreach (string _ in enabledGameModes) response.WriteInt8(10);

        // Popularity By Region (Count = Regions)
        foreach (string _ in enabledRegions) response.WriteInt8(10);

        // Custom Map Rotation Time
        response.WriteInt32(0);

        session.Send(response);
    }
}

file class PopularityUpdateRequestData
{
    public byte[] CommandBytes { get; init; }

    public PopularityUpdateRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
    }
}

