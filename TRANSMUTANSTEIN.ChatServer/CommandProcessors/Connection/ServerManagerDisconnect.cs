namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.ServerManagerToChatServer.NET_CHAT_SM_DISCONNECT)]
public class ServerManagerDisconnect : ISynchronousCommandProcessor
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        ServerManagerDisconnectRequestData requestData = new (buffer);

        // Remove Match Server Manager
        if (Context.MatchServerManagers.TryRemove(requestData.ServerManagerID, out ChatSession? existingSession))
        {
            Log.Information(@"Match Server Manager ID ""{ServerManagerID}"" Was Removed From The Match Server Pool", requestData.ServerManagerID);

            if (existingSession is null)
            {
                Log.Warning(@"Match Server Manager ID ""{ServerManagerID}"" Had A Null Session In The Match Server Pool", requestData.ServerManagerID);

                session.Terminate();

                return;
            }

            if (existingSession.Metadata.SessionCookie != session.Metadata.SessionCookie)
            {
                Log.Warning(@"Match Server Manager ID ""{ServerManagerID}"" Had A Mismatched Session Cookie", requestData.ServerManagerID);

                session.Terminate();

                return;
            }

            // TODO: Check For Orphaned Match Servers And Handle Appropriately

            ChatBuffer acknowledgementResponse = new ();

            acknowledgementResponse.WriteCommand(ChatProtocol.ChatServerToServerManager.NET_CHAT_SM_REMOTE_COMMAND);
            acknowledgementResponse.WriteString(session.Metadata.SessionCookie);
            acknowledgementResponse.WriteString("quit");

            // Send Disconnect Acknowledgement To Match Server Manager
            session.Send(acknowledgementResponse);
        }

        else
        {
            Log.Warning(@"Match Server Manager ID ""{ServerManagerID}"" Attempted To Disconnect But Was Not Found In The Match Server Pool", requestData.ServerManagerID);
        }

        session.Terminate();

        Log.Information(@"Match Server Manager ID ""{ServerManagerID}"" Has Disconnected Gracefully", requestData.ServerManagerID);
    }
}

public class ServerManagerDisconnectRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public int ServerManagerID = buffer.ReadInt32();
}
