namespace TRANSMUTANSTEIN.ChatServer.Contracts;

public interface IAsynchronousCommandProcessor<TSession> where TSession : ChatSession
{
    Task Process(TSession session, ChatBuffer buffer);
}