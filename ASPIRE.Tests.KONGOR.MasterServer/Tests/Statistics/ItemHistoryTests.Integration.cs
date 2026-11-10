namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Statistics;

/// <summary>
///     Integration tests for the persistence of the item history which a match server submits for each participant of a match.
/// </summary>
public sealed class ItemHistoryTests_Integration(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();

    [Test]
    public async Task Item_History_Is_Read_Back_In_The_Order_It_Was_Saved()
    {
        Account account = await SeedAccount("item.history@kongor.com", "ItemHistory");

        MatchParticipantStatistics participant = MatchDataHelper.BuildParticipant(account.ID, account.Name, groupNumber: 1, win: 1);

        participant.ItemHistory =
        [
            new () { ItemName = "Item_HealthPotion", GameTimeSeconds = 15, EventType = 0 },
            new () { ItemName = "Item_Marchers", GameTimeSeconds = 95, EventType = 0 },
            new () { ItemName = "Item_HealthPotion", GameTimeSeconds = 610, EventType = 1 }
        ];

        await SeedMatch(participant);

        MatchParticipantStatistics persistedParticipant = await ReadParticipant(participant.MatchID, account.ID);

        await Assert.That((persistedParticipant.ItemHistory ?? []).Select(item => (item.ItemName, item.GameTimeSeconds, item.EventType)))
            .IsEquivalentTo([("Item_HealthPotion", 15, (byte) 0), ("Item_Marchers", 95, (byte) 0), ("Item_HealthPotion", 610, (byte) 1)], TUnit.Assertions.Enums.CollectionOrdering.Matching);
    }

    [Test]
    public async Task A_Participant_Without_Item_History_Is_Read_Back_Without_Item_History()
    {
        Account account = await SeedAccount("no.item.history@kongor.com", "NoItemHistory");

        MatchParticipantStatistics participant = MatchDataHelper.BuildParticipant(account.ID, account.Name, groupNumber: 1, win: 1);

        participant.ItemHistory = null;

        await SeedMatch(participant);

        MatchParticipantStatistics persistedParticipant = await ReadParticipant(participant.MatchID, account.ID);

        await Assert.That(persistedParticipant.ItemHistory).IsNull();
    }

    private async Task<Account> SeedAccount(string emailAddress, string accountName)
    {
        SRPAuthenticationService service = new (webApplicationFactory);

        (Account account, string _) = await service.CreateAccountWithSRPCredentials(emailAddress, accountName, "DoesNotMatter123!");

        return account;
    }

    private async Task SeedMatch(MatchParticipantStatistics participant)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        await databaseContext.MatchStatistics.AddAsync(MatchDataHelper.BuildMatchStatistics(participant.MatchID));
        await databaseContext.MatchParticipantStatistics.AddAsync(participant);

        await databaseContext.SaveChangesAsync();
    }

    private async Task<MatchParticipantStatistics> ReadParticipant(int matchID, int accountID)
    {
        // Read From A Fresh Context So That The Item History Comes From The Database Rather Than From The Tracked Instance
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        return await databaseContext.MatchParticipantStatistics.SingleAsync(record => record.MatchID == matchID && record.AccountID == accountID);
    }
}
