namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

/// <summary>
///     Handles admin broadcast messages to connected clients.
///     The message type byte determines the broadcast scope:
///     <list type="bullet">
///         <item>
///             <term>Type 0</term>
///             <description>
///                 Global broadcast to all connected clients.
///                 Requires the sender to be <see cref="AccountType.Staff"/>.
///                 The <see cref="MessageAllRequestData.Value"/> field is unused.
///             </description>
///         </item>
///         <item>
///             <term>Type 1</term>
///             <description>
///                 Targeted broadcast to scheduled match participants for a specific event.
///                 Requires the sender to be <see cref="AccountType.Staff"/> or <see cref="AccountType.MatchModerator"/>.
///                 The <see cref="MessageAllRequestData.Value"/> field carries the event ID, or <c>-1</c> to broadcast to participants across all events.
///                 The broadcast is also delivered to staff and match moderators.
///             </description>
///         </item>
///     </list>
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL)]
public class MessageAll : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        MessageAllRequestData requestData = new (buffer);

        // Only Staff Or Match Moderators Can Send Broadcast Messages
        if (session.Account.Type is not AccountType.Staff and not AccountType.MatchModerator)
            return;

        switch (requestData.MessageType)
        {
            // Type 0: Global Broadcast To All Connected Clients (Staff Only)
            case 0:
            {
                ChatBuffer broadcast = new ();

                broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL);
                broadcast.WriteString(session.Account.Name); // Sender's Name
                broadcast.WriteString(requestData.Message);  // Message Content

                foreach (ClientChatSession clientSession in Context.ClientChatSessions.Values)
                    clientSession.Send(broadcast);

                break;
            }

            // Type 1: Targeted Broadcast To Scheduled Match Participants For A Specific Event
            case 1:
            {
                // TODO: Implement Scheduled Match Participant Broadcast
                // TODO: Filter Recipients By Event ID (requestData.Value), Or All Events If Value Is -1
                // TODO: Include Staff And Match Moderators In The Broadcast

                Log.Warning("Type 1 (Scheduled Match) Broadcast Requested By {AccountName} For Event {EventID}, But Scheduled Match Broadcasts Are Not Yet Implemented",
                    session.Account.Name, requestData.Value);

                break;
            }
        }
    }
}

file class MessageAllRequestData
{
    public byte[] CommandBytes { get; init; }

    /// <summary>
    ///     The broadcast type: 0 for global broadcast to all clients, 1 for targeted broadcast to scheduled match participants.
    /// </summary>
    public byte MessageType { get; init; }

    /// <summary>
    ///     The event ID for type 1 (scheduled match) broadcasts.
    ///     A value of <c>-1</c> targets participants across all events.
    ///     Unused for type 0 (global) broadcasts.
    /// </summary>
    public int Value { get; init; }

    /// <summary>
    ///     The message content to broadcast.
    /// </summary>
    public string Message { get; init; }

    public MessageAllRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        MessageType = buffer.ReadInt8();
        Value = buffer.ReadInt32();
        Message = buffer.ReadString();
    }
}
