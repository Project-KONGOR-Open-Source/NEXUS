namespace TRANSMUTANSTEIN.ChatServer.Contracts;

public interface ICommandProcessor
{
    public Task Process(ChatSession session, ChatBuffer buffer);
}
