namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_DISCONNECT)]
public class ServerDisconnect : ISynchronousCommandProcessor
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        ServerDisconnectRequestData requestData = new (buffer);

        // Remove Match Server
        if (Context.MatchServers.TryRemove(requestData.ServerID, out ChatSession? existingSession))
        {
            Log.Information(@"Match Server ID ""{ServerID}"" Was Removed From The Match Server Pool", requestData.ServerID);

            if (existingSession is null)
            {
                Log.Warning(@"Match Server ID ""{ServerID}"" Had A Null Session In The Match Server Pool", requestData.ServerID);

                session.Terminate();
                
                return;
            }

            if (existingSession.Metadata.SessionCookie != session.Metadata.SessionCookie)
            {
                Log.Warning(@"Match Server ID ""{ServerID}"" Had A Mismatched Session Cookie", requestData.ServerID);

                session.Terminate();
                
                return;
            }

            // TODO: Discard Any Pending Match Expected To Be Hosted By This Server

            ChatBuffer acknowledgementResponse = new ();

            acknowledgementResponse.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REMOTE_COMMAND);
            acknowledgementResponse.WriteString(session.Metadata.SessionCookie);
            acknowledgementResponse.WriteString("quit");

            // Send Disconnect Acknowledgement To Match Server
            session.Send(acknowledgementResponse);
        }

        else
        {
            Log.Warning(@"Match Server ID ""{ServerID}"" Attempted To Disconnect But Was Not Found In The Match Server Pool", requestData.ServerID);
        }

        session.Terminate();

        Log.Information(@"Match Server ID ""{ServerID}"" Has Disconnected Gracefully", requestData.ServerID);
    }
}

public class ServerDisconnectRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public int ServerID = buffer.ReadInt32();
}
