# Group Leave Queue

**Command**: NET_CHAT_CL_TMM_GROUP_LEAVE_QUEUE
**Phase**: Phase 3 (Queue & Match Lobby)
**Direction**: Client → Server
**Response**: Queue leave confirmation + broadcast to members

## Request Message

**Structure**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_TMM_GROUP_LEAVE_QUEUE]
```

**Fields**: None (group details from session)

## Response Messages

**Success Response**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_TMM_GROUP_LEAVE_QUEUE_RESPONSE]
[int8: Success = 1]
[int32: Group ID]
[int32: Time Spent in Queue Seconds]
```

**Broadcast to All Group Members**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_TMM_GROUP_QUEUE_LEFT]
[int32: Group ID]
[int32: Time Spent in Queue Seconds]
```

**Failure Response**:
```
[int8: Success = 0]
[string: Error Message]
```

## Validation Rules

| Rule | Enforcement |
|------|-------------|
| **Must be group leader** | Only leader can remove group from queue |
| **Group in queue** | QueueStatus must be Queued |
| **Not in match found state** | Cannot leave if match already found |

## Error Cases

| Error | Reason |
|-------|--------|
| "Not group leader" | Only leader can leave queue |
| "Not in queue" | Group QueueStatus != Queued |
| "Match found" | Match already found, must accept/decline instead |
| "Match starting" | Too late to leave, match is starting |

## Processing Steps

1. Validate player is group leader
2. Check group currently in queue (QueueStatus = Queued)
3. Check no match found yet (no pending match notification)
4. Calculate time spent in queue
5. Remove group from matchmaking queue
6. Update group QueueStatus = NotQueued
7. Optionally reset all member ready statuses to false
8. Broadcast queue leave notification to all members
9. Send confirmation to leader

## Time in Queue Calculation

```csharp
int timeInQueueSeconds = (int)(DateTime.UtcNow - group.QueueJoinTime).TotalSeconds;
```

## Queue Removal

**In-Memory Queue Update**:
```csharp
public void Dequeue(MatchmakingGroup group)
{
    var key = (group.GameType, group.Region);

    if (Queues.TryGetValue(key, out var queue))
    {
        // ConcurrentBag doesn't support direct removal, use filtering
        var remainingGroups = queue.Where(g => g.GroupID != group.GroupID).ToList();

        // Replace queue
        Queues[key] = new ConcurrentBag<MatchmakingGroup>(remainingGroups);
    }

    group.QueueStatus = QueueStatus.NotQueued;
    group.QueueJoinTime = null;
}
```

## Ready Status Reset (Optional)

**Behaviour**: Optionally reset all ready statuses when leaving queue

```csharp
foreach (var member in group.Members.Values)
{
    member.ReadyStatus = false; // Force re-ready before re-queuing
}
```

**Rationale**: Forces players to confirm they're still ready if re-queuing

## Match Found Edge Case

**Scenario**: Leader tries to leave queue right as match is found

**Handling**:
```csharp
if (group.QueueStatus == QueueStatus.MatchFound)
{
    SendError(session, "Match found. You must accept or decline the match.");
    return;
}

// Race condition protection: lock during match found transition
lock (group.QueueLock)
{
    if (group.QueueStatus == QueueStatus.MatchFound)
    {
        SendError(session, "Match found. You must accept or decline the match.");
        return;
    }

    // Safe to remove from queue
    MatchmakingQueue.Dequeue(group);
    group.QueueStatus = QueueStatus.NotQueued;
}
```

## State Management

```csharp
// Calculate time in queue
int timeInQueue = (int)(DateTime.UtcNow - group.QueueJoinTime).TotalSeconds;

// Remove from queue
MatchmakingQueue.Dequeue(group);
group.QueueStatus = QueueStatus.NotQueued;
group.QueueJoinTime = null;

// Optionally reset ready statuses
foreach (var member in group.Members.Values)
{
    member.ReadyStatus = false;
}

// Broadcast to all members
var queueLeftMsg = BuildQueueLeftMessage(group.GroupID, timeInQueue);
foreach (var member in group.Members.Values)
{
    member.Session.SendAsync(queueLeftMsg);
}
```

## Telemetry/Analytics

**Metrics to Log**:
- Time spent in queue (distribution analysis)
- Reason for leaving (timeout vs manual leave)
- Re-queue rate (same group re-queues within 5 minutes)
- Queue abandonment rate (% of groups that leave queue)

## Related Commands

- **group-join-queue.md**: Join queue before leaving
- **match-found.md**: Transition from queue to match found
- **group-leave.md**: Leave group entirely (requires leaving queue first)
