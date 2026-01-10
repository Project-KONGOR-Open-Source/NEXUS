using KONGOR.MasterServer.Configuration;
using KONGOR.MasterServer.Configuration.Matchmaking;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_POPULARITY_UPDATE)]
public class PopularityUpdate : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        PopularityUpdateRequestData requestData = new(buffer);

        SendMatchmakingPopularity(session);
    }

    public static void SendMatchmakingPopularity(ChatSession session)
    {
        // Legacy Logic Hardcoded for Stability
        // Dynamic Logic via Configuration
        MatchmakingConfiguration config = JSONConfiguration.MatchmakingConfiguration;

        HashSet<string> maps = [];
        HashSet<string> types = ["1", "5", "6"]; // Default: Normal(1), Custom(5), Campaign(6)
        HashSet<string> modes = [];
        HashSet<string> regions = [];

        // Ranked
        if (config.Ranked is not null)
        {
            maps.Add(config.Ranked.Map);
            foreach (string m in config.Ranked.Modes)
            {
                modes.Add(m);
            }

            foreach (string r in config.Ranked.Regions)
            {
                regions.Add(r);
            }
        }

        // Unranked (Casual - Type 2)
        if (config.Unranked is not null)
        {
            maps.Add(config.Unranked.Map);
            // TODO: Fix UI Duplication Glitch for Casual Mode
            // types.Add("2"); 
            foreach (string m in config.Unranked.Modes)
            {
                modes.Add(m);
            }

            foreach (string r in config.Unranked.Regions)
            {
                regions.Add(r);
            }
        }

        // MidWars (Type 3)
        if (config.MidWars is not null)
        {
            maps.Add(config.MidWars.Map);
            types.Add("3");
            foreach (string m in config.MidWars.Modes)
            {
                modes.Add(m);
            }

            foreach (string r in config.MidWars.Regions)
            {
                regions.Add(r);
            }
        }

        // RiftWars (Type 4)
        if (config.RiftWars is not null)
        {
            maps.Add(config.RiftWars.Map);
            types.Add("4");
            foreach (string m in config.RiftWars.Modes)
            {
                modes.Add(m);
            }

            foreach (string r in config.RiftWars.Regions)
            {
                regions.Add(r);
            }
        }

        List<string> enabledMaps = maps.ToList();
        List<string> enabledGameTypes = types.OrderBy(t => t).ToList();
        List<string> enabledGameModes = modes.ToList();
        List<string> enabledRegions = regions.ToList();

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

        ChatBuffer response = new();

        response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_POPULARITY_UPDATE);

        response.WriteInt8(1); // TMM Availability
        response.WriteString(availableMaps); // Maps
        response.WriteString(gameTypes); // Types
        response.WriteString(gameModes); // Modes
        response.WriteString(availableRegions); // Regions
        response.WriteString(disabledGameModesByGameType);
        response.WriteString(disabledGameModesByRankType);
        response.WriteString(disabledGameModesByMap);
        response.WriteString(restrictedRegions);
        response.WriteString(clientCountryCode);
        response.WriteString(legend);

        // Popularity By Game Map (Count = Maps)
        foreach (string _ in enabledMaps)
        {
            response.WriteInt8(10);
        }

        // Popularity By Game Type (Count = Types * Maps) - CRITICAL: Legacy logic creates array of size Types * Maps
        for (int i = 0; i < enabledGameTypes.Count * enabledMaps.Count; i++)
        {
            response.WriteInt8(10);
        }

        // Popularity By Game Mode (Count = Modes)
        foreach (string _ in enabledGameModes)
        {
            response.WriteInt8(10);
        }

        // Popularity By Region (Count = Regions)
        foreach (string _ in enabledRegions)
        {
            response.WriteInt8(10);
        }

        // Custom Map Rotation Time
        response.WriteInt32(0);

        session.Send(response);
    }
}