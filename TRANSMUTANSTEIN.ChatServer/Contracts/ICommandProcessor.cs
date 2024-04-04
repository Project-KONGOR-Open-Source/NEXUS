namespace TRANSMUTANSTEIN.ChatServer.Contracts;

public interface ICommandProcessor
{
    public Task Process(TCPSession session, ChatBuffer buffer);
}
