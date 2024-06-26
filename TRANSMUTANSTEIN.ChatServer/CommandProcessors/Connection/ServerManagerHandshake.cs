﻿namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.ServerManagerToChatServer.NET_CHAT_SM_CONNECT)]
public class ServerManagerHandshake(MerrickContext merrick, ILogger<ServerManagerHandshake> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;
    private ILogger<ServerManagerHandshake> Logger { get; set; } = logger;

    public async Task Process(TCPSession session, ChatBuffer buffer)
    {
        byte[] _ = buffer.ReadCommandBytes();
        int serverManagerID = buffer.ReadInt32();
        string sessionCookie = buffer.ReadString();
        int chatProtocolVersion = buffer.ReadInt32();

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

        Response.WriteCommand(ChatProtocol.ChatServerToServerManager.NET_CHAT_SM_ACCEPT);
        Response.PrependBufferSize();

        session.SendAsync(Response.Data);

        Response = new ChatBuffer(); // Also Respond With NET_CHAT_SM_OPTIONS Since The Server Manager Will Not Explicitly Request It

        Response.WriteCommand(ChatProtocol.ChatServerToServerManager.NET_CHAT_SM_OPTIONS);
        Response.WriteInt8(Convert.ToByte(true));   // Submit Stats Enabled
        Response.WriteInt8(Convert.ToByte(true));   // Upload Replays Enabled
        Response.WriteInt8(Convert.ToByte(false));  // Upload To FTP On Demand Enabled
        Response.WriteInt8(Convert.ToByte(true));   // Upload To HTTP On Demand Enabled
        Response.WriteInt8(Convert.ToByte(true));   // Resubmit Stats Enabled
        Response.WriteInt32(1);                     // Stats Resubmit Match ID Cut-Off // TODO: Investigate What This Is
        Response.PrependBufferSize();

        session.SendAsync(Response.Data);
    }
}
