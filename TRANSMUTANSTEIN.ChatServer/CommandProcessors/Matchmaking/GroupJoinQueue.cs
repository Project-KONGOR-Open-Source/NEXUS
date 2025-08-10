namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE)]
public class GroupJoinQueue(ILogger<GroupJoinQueue> logger) : CommandProcessorsBase, ICommandProcessor
{
    private ILogger<GroupJoinQueue> Logger { get; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        // Client would normally request queue join; here we just ACK via bare command echo (placeholder)
        byte[] commandBytes = buffer.ReadCommandBytes(); // consume
        Response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE);
        Response.PrependBufferSize();
        session.SendAsync(Response.Data);
    }
}

/*
Placeholder Handler: GroupJoinQueue (0x0D01)

Purpose:
Accept a client's attempt to place the group into the matchmaking queue. Real logic would validate leader, group state, readiness, region & MMR constraints, then enqueue into GameFinder.

Implemented Behavior:
- Consumes inbound frame, returns a minimal acknowledgement consisting only of the command header.

Deferred Logic:
- Leader-only enforcement.
- Distinguishing fresh join vs rejoin (0x0E0C path) with saved time offset.
- Broadcasting group update type TMM_GROUP_JOINED_QUEUE (0x0D03 update frame) to all members.
- Queue time estimation updates (0x0D06) and server popularity (0x0D07).

Next Steps:
1. When group model exists, store timestamp and emit TMM_GROUP_JOINED_QUEUE update.
2. Introduce a scheduler to periodically send queue time updates.
3. Integrate with a mock GameFinder service that can later trigger MatchFoundUpdate (0x0D09).
*/
