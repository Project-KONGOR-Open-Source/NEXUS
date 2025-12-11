namespace TRANSMUTANSTEIN.ChatServer.Contracts;

public interface ISynchronousCommandProcessor<TSession> where TSession : ChatSession
{
    void Process(TSession session, ChatBuffer buffer);
}
