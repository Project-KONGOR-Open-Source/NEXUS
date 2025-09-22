namespace TRANSMUTANSTEIN.ChatServer.Contracts;

public interface IAsynchronousCommandProcessor
{
    public Task Process(ChatSession session, ChatBuffer buffer);
}
