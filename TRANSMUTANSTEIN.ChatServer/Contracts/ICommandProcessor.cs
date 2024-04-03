namespace TRANSMUTANSTEIN.ChatServer.Contracts;

public interface ICommandProcessor
{
    public void Process(TCPSession session, ChatBuffer buffer);
}
