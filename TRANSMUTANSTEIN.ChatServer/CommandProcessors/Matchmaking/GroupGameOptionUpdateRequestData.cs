namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

public class GroupGameOptionUpdateRequestData
{
    public GroupGameOptionUpdateRequestData(ChatBuffer buffer)
    {
        // Legacy Protocol Quirk: Some clients verify/repeat the command ID (0x0D08) at the start of the payload.
        // Others (or modern tests) might not. We peek to check if the next 2 bytes match the command ID.
        byte[] peek = buffer.Peek(2);
        if (peek.Length == 2)
        {
            ushort potentialCb = BitConverter.ToUInt16(peek, 0);
            
            // 0x0D08 = 3336
            if (potentialCb == ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GAME_OPTION_UPDATE) 
            {
                 buffer.ReadInt16(); // Consume it
            }
        }

        GameType = (ChatProtocol.TMMGameType) buffer.ReadInt8();

        MapName = buffer.ReadString();
        GameModes = buffer.ReadString().Split('|');
        GameRegions = buffer.ReadString().Split('|');
        Ranked = buffer.ReadBool();
        MatchFidelity = buffer.ReadInt8();
        BotDifficulty = buffer.ReadInt8();
        RandomizeBots = buffer.ReadBool();
    }

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
}