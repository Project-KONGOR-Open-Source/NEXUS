namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests;

/// <summary>
///     Tests for the reconciliation logic that decides which cached hosts the stale host reaper removes.
/// </summary>
public sealed class StaleHostReaperTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UnixEpoch.AddDays(1);

    private static readonly TimeSpan GracePeriod = TimeSpan.FromMinutes(2);

    [Test]
    public async Task A_Host_With_A_Live_Session_Is_Never_Reaped()
    {
        (List<int> toReap, Dictionary<int, DateTimeOffset> nextFirstObservedMissing) =
            StaleHostReaper.Reconcile([1, 2], [1, 2], new Dictionary<int, DateTimeOffset>(), Now, GracePeriod);

        using (Assert.Multiple())
        {
            await Assert.That(toReap.Count).IsEqualTo(0);
            await Assert.That(nextFirstObservedMissing.Count).IsEqualTo(0);
        }
    }

    [Test]
    public async Task A_Missing_Host_Is_Marked_On_First_Observation_And_Not_Reaped()
    {
        (List<int> toReap, Dictionary<int, DateTimeOffset> nextFirstObservedMissing) =
            StaleHostReaper.Reconcile([1], [], new Dictionary<int, DateTimeOffset>(), Now, GracePeriod);

        using (Assert.Multiple())
        {
            await Assert.That(toReap.Count).IsEqualTo(0);
            await Assert.That(nextFirstObservedMissing.ContainsKey(1)).IsTrue();
            await Assert.That(nextFirstObservedMissing[1]).IsEqualTo(Now);
        }
    }

    [Test]
    public async Task A_Missing_Host_Within_The_Grace_Period_Is_Not_Reaped_And_Retains_Its_Original_Mark()
    {
        DateTimeOffset firstObserved = Now - TimeSpan.FromSeconds(30);

        (List<int> toReap, Dictionary<int, DateTimeOffset> nextFirstObservedMissing) =
            StaleHostReaper.Reconcile([1], [], new Dictionary<int, DateTimeOffset> { [1] = firstObserved }, Now, GracePeriod);

        using (Assert.Multiple())
        {
            await Assert.That(toReap.Count).IsEqualTo(0);
            await Assert.That(nextFirstObservedMissing[1]).IsEqualTo(firstObserved);
        }
    }

    [Test]
    public async Task A_Missing_Host_Beyond_The_Grace_Period_Is_Reaped()
    {
        (List<int> toReap, Dictionary<int, DateTimeOffset> nextFirstObservedMissing) =
            StaleHostReaper.Reconcile([1], [], new Dictionary<int, DateTimeOffset> { [1] = Now - TimeSpan.FromMinutes(3) }, Now, GracePeriod);

        using (Assert.Multiple())
        {
            await Assert.That(toReap.Contains(1)).IsTrue();
            await Assert.That(nextFirstObservedMissing.ContainsKey(1)).IsFalse();
        }
    }

    [Test]
    public async Task A_Host_That_Reconnected_Within_The_Grace_Period_Clears_Its_Mark()
    {
        (List<int> toReap, Dictionary<int, DateTimeOffset> nextFirstObservedMissing) =
            StaleHostReaper.Reconcile([1], [1], new Dictionary<int, DateTimeOffset> { [1] = Now - TimeSpan.FromSeconds(90) }, Now, GracePeriod);

        using (Assert.Multiple())
        {
            await Assert.That(toReap.Count).IsEqualTo(0);
            await Assert.That(nextFirstObservedMissing.ContainsKey(1)).IsFalse();
        }
    }

    [Test]
    public async Task A_Host_No_Longer_In_The_Cache_Is_Dropped_From_The_Pending_Marks()
    {
        (List<int> toReap, Dictionary<int, DateTimeOffset> nextFirstObservedMissing) =
            StaleHostReaper.Reconcile([], [], new Dictionary<int, DateTimeOffset> { [1] = Now - TimeSpan.FromSeconds(30) }, Now, GracePeriod);

        using (Assert.Multiple())
        {
            await Assert.That(toReap.Count).IsEqualTo(0);
            await Assert.That(nextFirstObservedMissing.Count).IsEqualTo(0);
        }
    }
}
