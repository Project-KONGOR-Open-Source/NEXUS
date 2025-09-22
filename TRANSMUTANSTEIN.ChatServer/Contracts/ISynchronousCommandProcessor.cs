namespace TRANSMUTANSTEIN.ChatServer.Contracts;

public interface ISynchronousCommandProcessor
{
    public void Process(ChatSession session, ChatBuffer buffer);
}
