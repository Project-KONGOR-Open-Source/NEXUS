
using TRANSMUTANSTEIN.ChatServer.Domain.Core;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Gaming;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_ABANDON_MATCH)]
public class AbandonMatch : IAsynchronousCommandProcessor<MatchServerChatSession>
{
    public Task Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        // We read the command bytes to ensure the buffer is advanced, even if we don't parse the payload fully yet.
        AbandonMatchData data = new(buffer);
        
        Log.Warning(@"Match Server ID ""{ServerID}"" Sent Abandon Match (0x0504). Logic Not Fully Implemented.", session.Metadata.ServerID);

        return Task.CompletedTask;
    }
}

file class AbandonMatchData
{
    public byte[] CommandBytes { get; init; }

    public AbandonMatchData(ChatBuffer buffer)
    {
        // Consuming command bytes is standard in this codebase's processors
        CommandBytes = buffer.ReadCommandBytes();
        
        // TODO: Reverse engineer the payload for 0x0504 if needed.
        // Likely contains MatchID and Reason.
    }
}
