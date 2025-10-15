namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.ServerManagerToChatServer.NET_CHAT_SM_CONNECT)]
public class ServerManagerHandshake : ISynchronousCommandProcessor
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        ServerManagerHandshakeRequestData requestData = new (buffer);

        // TODO: Check Cookie

        // TODO: Run Other Checks

        /*
            if (Manager is null)
            {
                Subject.Disconnect("Failed to resolve a manager with provided Id and SessionCookie.");
                return;
            }

            lock (SharedContext.ConnectedManagers)
            {
                // Disconnect previously connected manager if it's ran twice by mistake.
                if (SharedContext.ConnectedManagers.TryGetValue(ManagerId, out var conflictingManager))
                {
                    conflictingManager.Disconnect("Another manager instance has replaced this manager instance.");
                }

                SharedContext.ConnectedManagers[ManagerId] = Subject;
            }
         */

        ChatBuffer response = new ();

        response.WriteCommand(ChatProtocol.ChatServerToServerManager.NET_CHAT_SM_ACCEPT);

        session.Send(response);

        // TODO: Don't Reuse Chat Buffer Instance

        response = new ChatBuffer(); // Also Respond With NET_CHAT_SM_OPTIONS Since The Server Manager Will Not Explicitly Request It

        response.WriteCommand(ChatProtocol.ChatServerToServerManager.NET_CHAT_SM_OPTIONS);
        response.WriteInt8(Convert.ToByte(true));  // Submit Stats Enabled
        response.WriteInt8(Convert.ToByte(true));  // Upload Replays Enabled
        response.WriteInt8(Convert.ToByte(false)); // Upload To FTP On Demand Enabled
        response.WriteInt8(Convert.ToByte(true));  // Upload To HTTP On Demand Enabled
        response.WriteInt8(Convert.ToByte(true));  // Resubmit Stats Enabled
        // TODO: Investigate What This (Stats Resubmit Match ID Cut-Off) Is
        response.WriteInt32(1);                    // Stats Resubmit Match ID Cut-Off

        session.Send(response);
    }
}

public class ServerManagerHandshakeRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public int ServerManagerID = buffer.ReadInt32();

    public string SessionCookie = buffer.ReadString();

    public int ChatProtocolVersion = buffer.ReadInt32();
}
