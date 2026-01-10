namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_CREATE)]
public class GroupCreate : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        GroupCreateRequestData requestData = new(buffer);

        MatchmakingGroup
            .Create(session, requestData.ToGroupInformation());
    }
}