namespace TRANSMUTANSTEIN.ChatServer.Contracts;

public interface ICommandProcessor
{
    public MerrickContext MerrickContext { get; set; }

    public Task Process(TCPSession session, ChatBuffer buffer);
}
