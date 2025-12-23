namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_LEFT_GAME)]
public class LeftMatch : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        session.LeaveMatch();
    }
}
