# Group Join Queue

**Command**: NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE
**Phase**: Phase 3 (Queue & Match Lobby)
**Direction**: Client → Server
**Response**: Queue confirmation + estimated wait time

## Request Message

**Structure**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE]
```

**Fields**: None (group details from session)

## Response Messages

**Success Response**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE_RESPONSE]
[int8: Success = 1]
[int32: Group ID]
[int32: Estimated Wait Time Seconds]
[int64: Queue Join Time (Unix epoch milliseconds)]
```

**Broadcast to All Group Members**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_TMM_GROUP_QUEUE_JOINED]
[int32: Group ID]
[int32: Estimated Wait Time Seconds]
[int64: Queue Join Time]
```

**Failure Response**:
```
[int8: Success = 0]
[string: Error Message]
```

## Validation Rules

| Rule | Enforcement |
|------|-------------|
| **Must be group leader** | Only leader can queue group |
| **All members ready** | All players must have ReadyStatus = true |
| **Group size valid** | 1-5 players (solo or group) |
| **Not already queued** | QueueStatus must be NotQueued |
| **No members in game** | All members must have InGame = false |

## Error Cases

| Error | Reason |
|-------|--------|
| "Not group leader" | Only leader can initiate queue |
| "Not all members ready" | One or more members ReadyStatus = false |
| "Already in queue" | Group QueueStatus != NotQueued |
| "Member in game" | One or more members currently in active match |
| "Invalid group size" | Group empty or >5 members |

## Processing Steps

1. Validate player is group leader
2. Check all members have ReadyStatus = true
3. Verify no members currently in game
4. Check group not already in queue
5. Calculate estimated wait time (based on queue depth)
6. Update group QueueStatus = Queued
7. Set QueueJoinTime = DateTime.UtcNow
8. Add group to matchmaking queue (in-memory queue list)
9. Broadcast queue confirmation to all members
10. Start matchmaking algorithm monitoring

## Estimated Wait Time Calculation

**Factors**:
- Current queue depth for game type/region
- Average match time (~30 minutes)
- Historical matchmaking time
- Time of day / player population

**Example Logic**:
```csharp
int queueDepth = MatchmakingQueue.GetQueueDepth(gameType, region);
int groupsAhead = queueDepth / 10; // Assuming 10 players per match

// Base: 30 seconds per group ahead
int estimatedSeconds = groupsAhead * 30;

// Adjust for population (peak vs off-peak)
float populationMultiplier = GetPopulationMultiplier(region, DateTime.UtcNow);
estimatedSeconds = (int)(estimatedSeconds * populationMultiplier);

// Clamp to reasonable range (10 seconds - 10 minutes)
return Math.Clamp(estimatedSeconds, 10, 600);
```

## Queue Data Structure

**In-Memory Queue**:
```csharp
public class MatchmakingQueue
{
    // Separate queues per game type + region
    private ConcurrentDictionary<(GameType, Region), ConcurrentBag<MatchmakingGroup>> Queues = new();

    public void Enqueue(MatchmakingGroup group)
    {
        var key = (group.GameType, group.Region);
        var queue = Queues.GetOrAdd(key, _ => new ConcurrentBag<MatchmakingGroup>());
        queue.Add(group);

        group.QueueStatus = QueueStatus.Queued;
        group.QueueJoinTime = DateTime.UtcNow;
    }
}
```

## Matchmaking Algorithm Trigger

**Background Task**:
```csharp
// Continuously monitor queue for matches
private async Task MatchmakingLoop()
{
    while (true)
    {
        foreach (var queue in Queues.Values)
        {
            if (queue.Count >= 10) // Minimum for 5v5 match
            {
                AttemptMatchmaking(queue);
            }
        }

        await Task.Delay(1000); // Check every second
    }
}
```

## State Management

```csharp
// Update group state
group.QueueStatus = QueueStatus.Queued;
group.QueueJoinTime = DateTime.UtcNow;

// Add to queue
var queueKey = (group.GameType, group.Region);
MatchmakingQueue.Enqueue(queueKey, group);

// Broadcast to all members
int estimatedWait = CalculateEstimatedWaitTime(group);
var queueJoinedMsg = BuildQueueJoinedMessage(group.GroupID, estimatedWait, group.QueueJoinTime);

foreach (var member in group.Members.Values)
{
    member.Session.SendAsync(queueJoinedMsg);
}
```

## Related Commands

- **group-ready-status.md**: All members must be ready first
- **group-leave-queue.md**: Leave queue
- **match-found.md**: Triggered when match found (Phase 3)
- **group-create.md**: Create group before queuing
