namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Services;

/// <summary>
///     Exercises the flood prevention service's per-account token-bucket limiting and the staff exemption.
///     These run in well under one replenishment period, so no tokens are refilled mid-test and the bucket capacity is deterministic.
/// </summary>
public sealed class FloodPreventionServiceTests
{
    [Test]
    public async Task Requests_Up_To_The_Threshold_Are_Allowed()
    {
        await using FloodPreventionService service = new (NullLogger<FloodPreventionService>.Instance);

        const int accountID = 1;

        for (int request = 0; request < ChatProtocol.FLOOD_THRESHOLD; request++)
            await Assert.That(service.RequestIsAllowed(accountID, AccountType.Normal)).IsTrue();
    }

    [Test]
    public async Task A_Request_Beyond_The_Threshold_Is_Blocked()
    {
        await using FloodPreventionService service = new (NullLogger<FloodPreventionService>.Instance);

        const int accountID = 2;

        for (int request = 0; request < ChatProtocol.FLOOD_THRESHOLD; request++)
            service.RequestIsAllowed(accountID, AccountType.Normal);

        await Assert.That(service.RequestIsAllowed(accountID, AccountType.Normal)).IsFalse();
    }

    [Test]
    public async Task Staff_Accounts_Are_Exempt()
    {
        await using FloodPreventionService service = new (NullLogger<FloodPreventionService>.Instance);

        const int accountID = 3;

        for (int request = 0; request < ChatProtocol.FLOOD_THRESHOLD + 5; request++)
            await Assert.That(service.RequestIsAllowed(accountID, AccountType.Staff)).IsTrue();
    }

    [Test]
    public async Task Each_Account_Is_Limited_Independently()
    {
        await using FloodPreventionService service = new (NullLogger<FloodPreventionService>.Instance);

        const int firstAccountID = 4;
        const int secondAccountID = 5;

        // Exhaust The First Account's Tokens
        for (int request = 0; request < ChatProtocol.FLOOD_THRESHOLD; request++)
            service.RequestIsAllowed(firstAccountID, AccountType.Normal);

        using (Assert.Multiple())
        {
            await Assert.That(service.RequestIsAllowed(firstAccountID, AccountType.Normal)).IsFalse();
            await Assert.That(service.RequestIsAllowed(secondAccountID, AccountType.Normal)).IsTrue();
        }
    }
}
