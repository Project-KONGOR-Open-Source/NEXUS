namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_PLAYER_LOADING_STATUS)]
public class GroupPlayerLoadingStatus(ILogger<GroupPlayerLoadingStatus> logger) : CommandProcessorsBase, ICommandProcessor
{
    private ILogger<GroupPlayerLoadingStatus> Logger { get; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        GroupPlayerLoadingStatusRequestData requestData = new (buffer);

        MatchmakingGroup group = MatchmakingService.GetMatchmakingGroup(session.ClientInformation.Account.ID)
            ?? throw new NullReferenceException($@"No Matchmaking Group Found For Invite Issuer ID ""{session.ClientInformation.Account.ID}""");

        MatchmakingGroupMember groupMember = group.Members.Single(member => member.Account.ID == session.ClientInformation.Account.ID);

        groupMember.LoadingPercent = requestData.LoadingPercent;

        bool loaded = group.Members.All(member => member.LoadingPercent is 100);

        /*
            // Register the Group with the GameFinder.
            if (!AddToGameFinderQueue(timestampWhenJoinedQueue: Stopwatch.GetTimestamp()))
            {
                // Failed to join the queue, go back to WaitingToStart state.
                State = GroupState.WaitingToStart;
                BroadcastUpdate(GroupUpdateType.Partial);
                return;
            }

            // If we want to broadcast expected time in queue, we can:
            Broadcast(new MatchmakingGroupQueueUpdateResponse(
                updateType: 11,
                averageTimeInQueueInSeconds: 42
            ));

            // Don't trigger Timer again, for now. But before we do so, send an update.
            BroadcastUpdate(GroupUpdateType.Partial);
            Timer.Change(Timeout.Infinite, Timeout.Infinite);

            ---------------------------------------------------------------------------------------------------------

            internal bool AddToGameFinderQueue(long timestampWhenJoinedQueue)
            {
                Dictionary<int, int>? pingInformation;
                if (Regions == "AUTO" || Regions == "AUTO|")
                {
                    // If the region is auto, obtain ping information for the group.
                    pingInformation = CombinePingInformation(PlayersInfo.Select(info => info.PingInformation).ToList());
                    if (pingInformation.Count == 0)
                    {
                        // No ping information is available.
                        KongorContext.ConnectedClients[Participants[0].AccountId].SendResponse(
                            new ErrorMessageResponse("Not all players have reported server pings. Please toggle AUTO region on/off and try again.")
                        );
                        return false;
                    }
                }
                else
                {
                    pingInformation = null;
                }

                if (Participants.Count != PlayersInfo.Count())
                {
                    KongorContext.ConnectedClients[Participants[0].AccountId].SendResponse(
                        new ErrorMessageResponse("Internal error: couldn't obtain MMR of all players.")
                    );
                    return false;
                }

                ProtocolResponse response;
                ConnectedClient leader = KongorContext.ConnectedClients[Participants[0].AccountId];
                if (leader.TimestampWhenTimePreviouslySpentInQueueExpire > timestampWhenJoinedQueue)
                {
                    long timePreviouslySpentInQueue = leader.TimePreviouslySpentInQueue;
                    timestampWhenJoinedQueue -= timePreviouslySpentInQueue;
                    response = new MatchmakingGroupRejoinQueueResponse(Convert.ToInt32(timePreviouslySpentInQueue / Stopwatch.Frequency));
            
                }
                else
                {
                    response = new MatchmakingGroupJoinQueueResponse();
                }
        
                // Create an immutable snapshot of our MatchmakingGroup.
                TMMGroup = ToTMMGroup(timestampWhenJoinedQueue, pingInformation);
                if (TMMGroup == null) return false;

                Broadcast(response);

                // Register it with the GameFinder.
                GameFinder.AddToQueue(TMMGroup);

                // We are now in the queue.
                State = GroupState.InQueue;

                return true;
            }
         */

        if (loaded)
        {
            ChatBuffer load = new();

            load.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_QUEUE_UPDATE);
            load.WriteInt8(Convert.ToByte(ChatProtocol.TMMUpdateType.TMM_GROUP_QUEUE_UPDATE));
            // TODO: Get Actual Average Time In Queue (In Seconds)
            load.WriteInt32(666);

            load.PrependBufferSize();

            Parallel.ForEach(group.Members, member => member.Session.SendAsync(load.Data));
        }











        if (group is not null)
        {
            MatchmakingGroupMember member = group.Members.Single(m => m.Account.ID == session.ClientInformation.Account.ID);
            member.LoadingPercent = requestData.LoadingPercent;
        }

        // Broadcast a partial group update reflecting current per-member loading
        ChatBuffer update = new ();
        update.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE); // TODO: Make This DRY To Eliminate The Duplication
        update.WriteInt8(Convert.ToByte(ChatProtocol.TMMUpdateType.TMM_PARTIAL_GROUP_UPDATE));
        update.WriteInt32(session.ClientInformation.Account.ID); // Actor

        if (group is null)
        {
            // Solo fallback
            update.WriteInt8(1);
            update.WriteInt16(1500);
            update.WriteInt32(session.ClientInformation.Account.ID);
            update.WriteInt8(Convert.ToByte(ChatProtocol.ArrangedMatchType.AM_MATCHMAKING));
            update.WriteInt8(Convert.ToByte(ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL));
            update.WriteString("caldavar");
            update.WriteString("ap|ar|sd");
            update.WriteString("EU|USE|USW");
            update.WriteBool(true);
            update.WriteInt8(1);
            update.WriteInt8(1);
            update.WriteBool(false);
            update.WriteString(string.Empty);
            update.WriteString(string.Empty);
            update.WriteInt8(5);
            update.WriteInt8(Convert.ToByte(ChatProtocol.TMMType.TMM_TYPE_CAMPAIGN));
            update.WriteInt8(requestData.LoadingPercent);
            update.WriteInt8(0);
            update.WriteBool(false);
            update.PrependBufferSize();
            session.SendAsync(update.Data);
        }
        else
        {
            update.WriteInt8(Convert.ToByte(group.Members.Count));
            update.WriteInt16(1500);
            update.WriteInt32(group.Leader.Account.ID);
            update.WriteInt8(Convert.ToByte(ChatProtocol.ArrangedMatchType.AM_MATCHMAKING));
            update.WriteInt8(Convert.ToByte(ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL));
            update.WriteString("caldavar");
            update.WriteString("ap|ar|sd");
            update.WriteString("EU|USE|USW");
            update.WriteBool(true);
            update.WriteInt8(1);
            update.WriteInt8(1);
            update.WriteBool(false);
            update.WriteString(string.Empty);
            update.WriteString(string.Empty);
            update.WriteInt8(5);
            update.WriteInt8(Convert.ToByte(ChatProtocol.TMMType.TMM_TYPE_CAMPAIGN));

            foreach (MatchmakingGroupMember m in group.Members)
            {
                update.WriteInt8(m.LoadingPercent);
                update.WriteInt8(Convert.ToByte(m.IsReady));
                update.WriteBool(m.IsInGame);
            }

            update.PrependBufferSize();
            Parallel.ForEach(group.Members, m => m.Session.SendAsync(update.Data));
        }

        if (group is not null)
        {
            bool allLoaded = group.Members.All(m => m.LoadingPercent >= 100);
            if (allLoaded)
            {
                // Mirror KONGOR minimal flow when all are loaded:
                // 1) Join queue (or rejoin)
                await BroadcastBare(group, ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE);

                // 2) Optional queue update (avg time); we’ll omit payload and keep it minimal for now
                //    In KONGOR this is MatchmakingGroupQueueUpdateResponse(updateType: 11)

                // 3) For a thin placeholder, jump to match found and lobby join
                await BroadcastBare(group, ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_MATCH_FOUND_UPDATE);
                await BroadcastBare(group, ChatProtocol.ChatServerToClient.NET_CHAT_CL_GAME_LOBBY_JOINED);
            }
        }
    }

    private async Task BroadcastBare(MatchmakingGroup group, ushort command)
    {
        ChatBuffer buf = new ();
        buf.WriteCommand(command);
        buf.PrependBufferSize();
        Parallel.ForEach(group.Members, m => m.Session.SendAsync(buf.Data));
    }
}

public class GroupPlayerLoadingStatusRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();
    public byte LoadingPercent = buffer.ReadInt8();
}
