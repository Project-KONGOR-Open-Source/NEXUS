namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Locks down the queue duration estimation logic, ensuring calculations are isolated by queue type partition and correctly handle fallbacks.
/// </summary>
public sealed class QueueDurationTests
{
    [Before(HookType.Test)]
    public Task Before_Each_Test()
    {
        MatchmakingService.Groups.Clear();
        MatchmakingService.RecentQueueDurationSecondsSamplesPerPartition[QueueType.COOP].Clear();
        MatchmakingService.RecentQueueDurationSecondsSamplesPerPartition[QueueType.Caldavar].Clear();
        MatchmakingService.RecentQueueDurationSecondsSamplesPerPartition[QueueType.MidWars].Clear();
        MatchmakingService.RecentQueueDurationSecondsSamplesPerPartition[QueueType.RiftWars].Clear();

        return Task.CompletedTask;
    }

    [After(HookType.Test)]
    public Task After_Each_Test()
    {
        MatchmakingService.Groups.Clear();
        MatchmakingService.RecentQueueDurationSecondsSamplesPerPartition[QueueType.COOP].Clear();
        MatchmakingService.RecentQueueDurationSecondsSamplesPerPartition[QueueType.Caldavar].Clear();
        MatchmakingService.RecentQueueDurationSecondsSamplesPerPartition[QueueType.MidWars].Clear();
        MatchmakingService.RecentQueueDurationSecondsSamplesPerPartition[QueueType.RiftWars].Clear();

        return Task.CompletedTask;
    }

    [Test]
    [NotInParallel]
    public async Task When_No_Samples_Exist_GetEstimatedQueueDurationSeconds_Falls_Back_To_Active_Queue_Durations()
    {
        MatchmakingGroup group = MatchmakingTestBuilder.BuildSoloGroup(tmr: 1500.0, queuedMinutesAgo: 5.0, information: MatchmakingTestBuilder.Information(gameType: ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL));

        MatchmakingService.Groups.TryAdd(group.Leader.Account.ID, group);

        int estimatedDurationSeconds = MatchmakingService.GetEstimatedQueueDurationSeconds(QueueType.Caldavar);

        await Assert.That(estimatedDurationSeconds).IsEqualTo(300);
    }

    [Test]
    [NotInParallel]
    public async Task When_Samples_Exist_GetEstimatedQueueDurationSeconds_Uses_Recent_Queue_Durations()
    {
        MatchmakingGroup group = MatchmakingTestBuilder.BuildSoloGroup(tmr: 1500.0, queuedMinutesAgo: 5.0);
        MatchmakingTeam team1 = MatchmakingTeam.FromGroups([group], teamSize: 5);
        MatchmakingTeam team2 = MatchmakingTeam.FromGroups([MatchmakingTestBuilder.BuildSoloGroup(tmr: 1500.0, queuedMinutesAgo: 10.0)], teamSize: 5);
        MatchmakingMatch match = MatchmakingMatch.FromTeams(team1, team2);

        match.GameType = ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL;

        MatchmakingService.RecordQueueDuration(match);

        int estimatedDurationSeconds = MatchmakingService.GetEstimatedQueueDurationSeconds(QueueType.Caldavar);

        // Average of 300 and 600 is 450
        await Assert.That(estimatedDurationSeconds).IsEqualTo(450);
    }

    [Test]
    [NotInParallel]
    public async Task Samples_Are_Isolated_By_Queue_Type_Partition()
    {
        // COOP queue duration
        MatchmakingGroup coopGroup = MatchmakingTestBuilder.BuildSoloGroup(tmr: 1500.0, queuedMinutesAgo: 2.0, information: MatchmakingTestBuilder.CoopInformation());
        MatchmakingMatch coopMatch = MatchmakingMatch.FromBotGroup(coopGroup);
        MatchmakingService.RecordQueueDuration(coopMatch);

        // Caldavar queue duration
        MatchmakingGroup caldavarGroup = MatchmakingTestBuilder.BuildSoloGroup(tmr: 1500.0, queuedMinutesAgo: 5.0);
        MatchmakingTeam caldavarTeam1 = MatchmakingTeam.FromGroups([caldavarGroup], teamSize: 5);
        MatchmakingTeam caldavarTeam2 = MatchmakingTeam.FromGroups([MatchmakingTestBuilder.BuildSoloGroup(tmr: 1500.0, queuedMinutesAgo: 5.0)], teamSize: 5);
        MatchmakingMatch caldavarMatch = MatchmakingMatch.FromTeams(caldavarTeam1, caldavarTeam2);

        caldavarMatch.GameType = ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL;

        MatchmakingService.RecordQueueDuration(caldavarMatch);

        int coopEstimatedDurationSeconds = MatchmakingService.GetEstimatedQueueDurationSeconds(QueueType.COOP);
        int caldavarEstimatedDurationSeconds = MatchmakingService.GetEstimatedQueueDurationSeconds(QueueType.Caldavar);

        using (Assert.Multiple())
        {
            await Assert.That(coopEstimatedDurationSeconds).IsEqualTo(120);
            await Assert.That(caldavarEstimatedDurationSeconds).IsEqualTo(300);
        }
    }

    [Test]
    [NotInParallel]
    public async Task Queue_Retains_At_Most_20_Samples()
    {
        for (int iteration = 1; iteration <= 25; iteration++)
        {
            MatchmakingGroup group = MatchmakingTestBuilder.BuildSoloGroup(tmr: 1500.0, queuedMinutesAgo: iteration, information: MatchmakingTestBuilder.CoopInformation());
            MatchmakingMatch match = MatchmakingMatch.FromBotGroup(group);

            MatchmakingService.RecordQueueDuration(match);
        }

        int estimatedDurationSeconds = MatchmakingService.GetEstimatedQueueDurationSeconds(QueueType.COOP);

        // The last 20 matches had queued durations from 6 to 25 minutes. Average of 6..25 is 15.5 minutes = 930 seconds
        await Assert.That(estimatedDurationSeconds).IsEqualTo(930);
    }

    [Test]
    [NotInParallel]
    public async Task GetQueueTypePartition_Throws_For_A_Non_Queueable_Game_Type()
    {
        await Assert.That(() => MatchmakingService.GetQueueTypePartition(ChatProtocol.TMMType.TMM_TYPE_PVP, ChatProtocol.TMMGameType.TMM_GAME_TYPE_CUSTOM))
            .Throws<ArgumentOutOfRangeException>();
    }
}
