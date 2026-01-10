using TRANSMUTANSTEIN.ChatServer.Domain.Core;
using TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

public class GroupGameOptionUpdateRequestData
{
    public ChatProtocol.TMMGameType GameType { get; set; }
    public string MapName { get; set; }
    public string[] GameModes { get; set; }
    public string[] GameRegions { get; set; }
    public bool Ranked { get; set; }
    public byte MatchFidelity { get; set; }
    public byte BotDifficulty { get; set; }
    public bool RandomizeBots { get; set; }

    public MatchmakingGroupInformation ToGroupInformation()
    {
        return new MatchmakingGroupInformation
        {
            ClientVersion = string.Empty, // Not Sent In Update
            GroupType = ChatProtocol.TMMType.TMM_TYPE_PVP, // Placeholder, will be preserved by UpdateInformation
            GameType = GameType,
            MapName = MapName,
            GameModes = GameModes,
            GameRegions = GameRegions,
            Ranked = Ranked,
            MatchFidelity = MatchFidelity,
            BotDifficulty = BotDifficulty,
            RandomizeBots = RandomizeBots
        };
    }

    public GroupGameOptionUpdateRequestData(ChatBuffer buffer)
    {
        // Read Command Bytes first (2 bytes)
        buffer.ReadInt16();

        GameType = (ChatProtocol.TMMGameType)buffer.ReadInt8();
        MapName = buffer.ReadString();
        GameModes = buffer.ReadString().Split('|');
        GameRegions = buffer.ReadString().Split('|');
        Ranked = buffer.ReadBool();
        MatchFidelity = buffer.ReadInt8();
        BotDifficulty = buffer.ReadInt8();
        RandomizeBots = buffer.ReadBool();
    }
}
