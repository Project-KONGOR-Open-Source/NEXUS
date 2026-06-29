namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Social;

/// <summary>
///     Integration tests for the "set_rank" client requester command, which is the authoritative persistence point for clan promotions, demotions, and removals.
/// </summary>
public sealed class SetClanRankTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    private const string SetClanRankRoute = "/client_requester.php?f=set_rank";

    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();

    [Test]
    public async Task Leader_Can_Promote_A_Member_To_Officer()
    {
        (string leaderCookie, int leaderID) = await SeedSession("promote.leader@kongor.com", "PromoteLeader");
        (string _, int memberID) = await SeedSession("promote.member@kongor.com", "PromoteMember");

        int clanID = await CreateClan("Promote Clan", "PRMO", (leaderID, ClanTier.Leader), (memberID, ClanTier.Member));

        IDictionary<object, object> body = await SetClanRank(leaderCookie, memberID, clanID, "Officer");

        await Assert.That(body["set_rank"]).IsEqualTo("Member updated.");

        Account member = await LoadAccountWithClan(memberID);

        await Assert.That(member.ClanTier).IsEqualTo(ClanTier.Officer);
    }

    [Test]
    public async Task Leader_Can_Remove_A_Member()
    {
        (string leaderCookie, int leaderID) = await SeedSession("remove.leader@kongor.com", "RemoveLeader");
        (string _, int memberID) = await SeedSession("remove.member@kongor.com", "RemoveMember");

        int clanID = await CreateClan("Remove Clan", "RMVE", (leaderID, ClanTier.Leader), (memberID, ClanTier.Member));

        IDictionary<object, object> body = await SetClanRank(leaderCookie, memberID, clanID, "Remove");

        await Assert.That(body["set_rank"]).IsEqualTo("Member updated.");

        Account member = await LoadAccountWithClan(memberID);

        using (Assert.Multiple())
        {
            await Assert.That(member.Clan).IsNull();
            await Assert.That(member.ClanTier).IsEqualTo(ClanTier.None);
        }
    }

    [Test]
    public async Task A_Member_Cannot_Promote_And_The_Database_Is_Unchanged()
    {
        (string _, int leaderID) = await SeedSession("perm.leader@kongor.com", "PermLeader");
        (string memberCookie, int memberID) = await SeedSession("perm.member@kongor.com", "PermMember");

        int clanID = await CreateClan("Perm Clan", "PERM", (leaderID, ClanTier.Leader), (memberID, ClanTier.Member));

        // The Member Attempts To Promote The Leader, Which They Have No Permission To Do
        IDictionary<object, object> body = await SetClanRank(memberCookie, leaderID, clanID, "Officer");

        await Assert.That(body).ContainsKey("error");

        Account leader = await LoadAccountWithClan(leaderID);

        await Assert.That(leader.ClanTier).IsEqualTo(ClanTier.Leader);
    }

    [Test]
    public async Task A_Member_Can_Remove_Themselves()
    {
        (string memberCookie, int memberID) = await SeedSession("selfremove.member@kongor.com", "SelfRemMember");
        (string _, int leaderID) = await SeedSession("selfremove.leader@kongor.com", "SelfRemLeader");

        int clanID = await CreateClan("Self Remove Clan", "SELF", (leaderID, ClanTier.Leader), (memberID, ClanTier.Member));

        IDictionary<object, object> body = await SetClanRank(memberCookie, memberID, clanID, "Remove");

        await Assert.That(body["set_rank"]).IsEqualTo("Member updated.");

        Account member = await LoadAccountWithClan(memberID);

        await Assert.That(member.Clan).IsNull();
    }

    [Test]
    public async Task Leader_Self_Removal_Transfers_Ownership_To_The_Longest_Tenured_Officer()
    {
        (string leaderCookie, int leaderID) = await SeedSession("transfer.leader@kongor.com", "XferLeader");
        (string _, int seniorOfficerID) = await SeedSession("transfer.senior@kongor.com", "XferSenior");
        (string _, int juniorOfficerID) = await SeedSession("transfer.junior@kongor.com", "XferJunior");

        // The Senior Officer Joined Before The Junior Officer, So They Have The Longest Clan Membership Among The Officers
        int clanID = await CreateClanWithJoinDates("Transfer Clan", "XFER",
            (leaderID, ClanTier.Leader, new DateTimeOffset(2020, 01, 01, 0, 0, 0, TimeSpan.Zero)),
            (seniorOfficerID, ClanTier.Officer, new DateTimeOffset(2021, 01, 01, 0, 0, 0, TimeSpan.Zero)),
            (juniorOfficerID, ClanTier.Officer, new DateTimeOffset(2022, 01, 01, 0, 0, 0, TimeSpan.Zero)));

        IDictionary<object, object> body = await SetClanRank(leaderCookie, leaderID, clanID, "Remove");

        await Assert.That(body["set_rank"]).IsEqualTo("Member updated.");

        Account formerLeader = await LoadAccountWithClan(leaderID);
        Account seniorOfficer = await LoadAccountWithClan(seniorOfficerID);
        Account juniorOfficer = await LoadAccountWithClan(juniorOfficerID);

        using (Assert.Multiple())
        {
            await Assert.That(formerLeader.Clan).IsNull();
            await Assert.That(seniorOfficer.ClanTier).IsEqualTo(ClanTier.Leader);
            await Assert.That(juniorOfficer.ClanTier).IsEqualTo(ClanTier.Officer);
        }
    }

    [Test]
    public async Task Leader_Self_Removal_With_No_Officers_Promotes_The_Longest_Tenured_Member()
    {
        (string leaderCookie, int leaderID) = await SeedSession("nomf.leader@kongor.com", "NoMfLeader");
        (string _, int seniorMemberID) = await SeedSession("nomf.senior@kongor.com", "NoMfSenior");
        (string _, int juniorMemberID) = await SeedSession("nomf.junior@kongor.com", "NoMfJunior");

        // There Are No Officers, So Ownership Falls To The Longest-Tenured Member
        int clanID = await CreateClanWithJoinDates("No Officers Clan", "NOMF",
            (leaderID, ClanTier.Leader, new DateTimeOffset(2020, 01, 01, 0, 0, 0, TimeSpan.Zero)),
            (seniorMemberID, ClanTier.Member, new DateTimeOffset(2021, 01, 01, 0, 0, 0, TimeSpan.Zero)),
            (juniorMemberID, ClanTier.Member, new DateTimeOffset(2022, 01, 01, 0, 0, 0, TimeSpan.Zero)));

        IDictionary<object, object> body = await SetClanRank(leaderCookie, leaderID, clanID, "Remove");

        await Assert.That(body["set_rank"]).IsEqualTo("Member updated.");

        Account seniorMember = await LoadAccountWithClan(seniorMemberID);
        Account juniorMember = await LoadAccountWithClan(juniorMemberID);

        using (Assert.Multiple())
        {
            await Assert.That(seniorMember.ClanTier).IsEqualTo(ClanTier.Leader);
            await Assert.That(juniorMember.ClanTier).IsEqualTo(ClanTier.Member);
        }
    }

    [Test]
    public async Task Leader_Self_Removal_As_The_Sole_Member_Disbands_The_Clan()
    {
        (string leaderCookie, int leaderID) = await SeedSession("disband.leader@kongor.com", "DisbandLeader");

        int clanID = await CreateClan("Disband Clan", "DSBD", (leaderID, ClanTier.Leader));

        IDictionary<object, object> body = await SetClanRank(leaderCookie, leaderID, clanID, "Remove");

        await Assert.That(body["set_rank"]).IsEqualTo("Member updated.");

        Account formerLeader = await LoadAccountWithClan(leaderID);

        using (Assert.Multiple())
        {
            await Assert.That(formerLeader.Clan).IsNull();
            await Assert.That(await ClanExists(clanID)).IsFalse();
        }
    }

    [Test]
    public async Task New_Clan_Leader_Receives_A_Leadership_Message()
    {
        (string leaderCookie, int leaderID) = await SeedSession("ldrmsg.leader@kongor.com", "LdrMsgLeader");
        (string _, int officerID) = await SeedSession("ldrmsg.officer@kongor.com", "LdrMsgOfficer");

        int clanID = await CreateClan("Leader Msg Clan", "LMSG", (leaderID, ClanTier.Leader), (officerID, ClanTier.Officer));

        await SetClanRank(leaderCookie, leaderID, clanID, "Remove");

        List<Message> messages = await GetMessages(officerID);

        await Assert.That(messages.Any(message => message.Subject.Equals("You Are Now The Clan Leader"))).IsTrue();
    }

    private async Task<(string Cookie, int AccountID)> SeedSession(string emailAddress, string accountName)
    {
        SRPAuthenticationService service = new (webApplicationFactory);

        (Account account, string _) = await service.CreateAccountWithSRPCredentials(emailAddress, accountName, "DoesNotMatter123!");

        string cookie = Guid.NewGuid().ToString("N");

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        await distributedCache.SetAccountNameForSessionCookie(cookie, accountName);

        return (cookie, account.ID);
    }

    private async Task<int> CreateClan(string clanName, string clanTag, params (int AccountID, ClanTier Tier)[] members)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Clan clan = new () { Name = clanName, Tag = clanTag };

        await databaseContext.Clans.AddAsync(clan);

        foreach ((int accountID, ClanTier tier) in members)
        {
            Account account = await databaseContext.Accounts.SingleAsync(record => record.ID == accountID);

            account.Clan = clan;
            account.ClanTier = tier;
            account.TimestampJoinedClan = DateTimeOffset.UtcNow;
        }

        await databaseContext.SaveChangesAsync();

        return clan.ID;
    }

    private async Task<int> CreateClanWithJoinDates(string clanName, string clanTag, params (int AccountID, ClanTier Tier, DateTimeOffset JoinedClan)[] members)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Clan clan = new () { Name = clanName, Tag = clanTag };

        await databaseContext.Clans.AddAsync(clan);

        foreach ((int accountID, ClanTier tier, DateTimeOffset joinedClan) in members)
        {
            Account account = await databaseContext.Accounts.SingleAsync(record => record.ID == accountID);

            account.Clan = clan;
            account.ClanTier = tier;
            account.TimestampJoinedClan = joinedClan;
        }

        await databaseContext.SaveChangesAsync();

        return clan.ID;
    }

    private async Task<IDictionary<object, object>> SetClanRank(string cookie, int targetID, int clanID, string rank)
    {
        HttpResponseMessage response = await PlinkoTestsHelper.PostForm(webApplicationFactory, SetClanRankRoute, new Dictionary<string, string>
        {
            ["cookie"]    = cookie,
            ["target_id"] = targetID.ToString(),
            ["clan_id"]   = clanID.ToString(),
            ["rank"]      = rank
        });

        return await PlinkoTestsHelper.DeserialisePhpResponse(response);
    }

    private async Task<Account> LoadAccountWithClan(int accountID)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        return await databaseContext.Accounts.AsNoTracking().Include(account => account.Clan).SingleAsync(account => account.ID == accountID);
    }

    private async Task<bool> ClanExists(int clanID)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        return await databaseContext.Clans.AnyAsync(clan => clan.ID == clanID);
    }

    private async Task<List<Message>> GetMessages(int accountID)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        return await databaseContext.Messages.AsNoTracking().Where(message => message.AccountID == accountID).ToListAsync();
    }
}
