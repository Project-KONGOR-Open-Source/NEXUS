namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_PLAYER_READY_STATUS)]
public class GroupPlayerReadyStatus : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        GroupPlayerReadyStatusRequestData requestData = new (buffer);

        MatchmakingGroup
            .GetByMemberAccountID(session.Account.ID)
            .SendPlayerReadinessStatusUpdate(session, requestData.GameType);
    }
}

public class GroupPlayerReadyStatusRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public byte ReadyStatus = buffer.ReadInt8();

    public ChatProtocol.TMMGameType GameType = (ChatProtocol.TMMGameType) buffer.ReadInt8();
}
