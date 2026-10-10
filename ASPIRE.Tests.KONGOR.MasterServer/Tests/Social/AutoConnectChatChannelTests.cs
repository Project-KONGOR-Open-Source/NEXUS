namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Social;

/// <summary>
///     Integration tests for the "add_room", "remove_room", and "clear_rooms" client requester commands, which manage the account's auto-connect chat channel list.
/// </summary>
public sealed class AutoConnectChatChannelTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();

    [Test]
    public async Task Adding_A_Channel_Saves_It_To_The_Auto_Connect_List()
    {
        (string cookie, int accountID) = await SeedSession("channel.add@kongor.com", "ChannelAdd");

        IDictionary<object, object> body = await PostAndDeserialise("add_room", cookie, "Auto Connect Channel");

        using (Assert.Multiple())
        {
            await Assert.That(body["add_room"]).IsEqualTo("OK");
            await Assert.That(await LoadAutoConnectChatChannels(accountID)).IsEquivalentTo(["Auto Connect Channel"]);
        }
    }

    [Test]
    public async Task Adding_A_Channel_Already_Saved_In_A_Different_Casing_Succeeds_Without_Duplicating_It()
    {
        (string cookie, int accountID) = await SeedSession("channel.casing@kongor.com", "ChannelCasing");

        await UpdateAccount(accountID, account => account.AutoConnectChatChannels = ["Auto Connect Channel"]);

        IDictionary<object, object> body = await PostAndDeserialise("add_room", cookie, "AUTO CONNECT CHANNEL");

        using (Assert.Multiple())
        {
            await Assert.That(body["add_room"]).IsEqualTo("OK");
            await Assert.That(await LoadAutoConnectChatChannels(accountID)).IsEquivalentTo(["Auto Connect Channel"]);
        }
    }

    [Test]
    [Arguments("Clan Other Clan")]
    [Arguments("TERMINAL")]
    [Arguments("Match 123")]
    [Arguments("TMM Group 456")]
    public async Task Adding_A_Reserved_Channel_Is_Rejected_With_A_Failure_The_Client_Reports(string channelName)
    {
        (string cookie, int accountID) = await SeedSession("channel.reserved@kongor.com", "ChannelReserved");

        IDictionary<object, object> body = await PostAndDeserialise("add_room", cookie, channelName);

        using (Assert.Multiple())
        {
            await Assert.That(body["add_room"]).IsEqualTo(false);
            await Assert.That(body.ContainsKey("error")).IsTrue();
            await Assert.That(await LoadAutoConnectChatChannels(accountID)).IsEmpty();
        }
    }

    [Test]
    public async Task Adding_A_Channel_Named_After_The_General_Channel_Without_A_Number_Saves_It()
    {
        (string cookie, int accountID) = await SeedSession("channel.general.name@kongor.com", "ChannelGeneral");

        IDictionary<object, object> body = await PostAndDeserialise("add_room", cookie, "KONGOR Fans");

        using (Assert.Multiple())
        {
            await Assert.That(body["add_room"]).IsEqualTo("OK");
            await Assert.That(await LoadAutoConnectChatChannels(accountID)).IsEquivalentTo(["KONGOR Fans"]);
        }
    }

    [Test]
    [Arguments("kongor")]
    [Arguments("KONGOR 2")]
    [Arguments("Clan Own Clan")]
    public async Task Adding_A_Default_Channel_Succeeds_Without_Saving_It(string channelName)
    {
        (string cookie, int accountID) = await SeedSession("channel.default.add@kongor.com", "ChannelDefault");

        await UpdateAccount(accountID, account => account.Clan = new Clan { Name = "Own Clan", Tag = "OWN" });

        IDictionary<object, object> body = await PostAndDeserialise("add_room", cookie, channelName);

        using (Assert.Multiple())
        {
            await Assert.That(body["add_room"]).IsEqualTo("OK");
            await Assert.That(await LoadAutoConnectChatChannels(accountID)).IsEmpty();
        }
    }

    [Test]
    public async Task Adding_A_Channel_When_The_Default_And_Saved_Channels_Reach_The_Channel_Limit_Is_Rejected_With_A_Failure_The_Client_Reports()
    {
        (string cookie, int accountID) = await SeedSession("channel.full@kongor.com", "ChannelFull");

        // The General Channel Is A Default Channel Of Every Player Account, So It Takes Up One Of The Channel Slots
        List<string> savedChannels = Enumerable.Range(1, (int) ChatProtocol.MAX_CHANNELS_PER_CLIENT - 1).Select(index => $"Saved Channel {index}").ToList();

        await UpdateAccount(accountID, account => account.AutoConnectChatChannels = [.. savedChannels]);

        IDictionary<object, object> body = await PostAndDeserialise("add_room", cookie, "One Channel Too Many");

        using (Assert.Multiple())
        {
            await Assert.That(body["add_room"]).IsEqualTo(false);
            await Assert.That(body.ContainsKey("error")).IsTrue();
            await Assert.That(await LoadAutoConnectChatChannels(accountID)).IsEquivalentTo(savedChannels);
        }
    }

    [Test]
    public async Task Staff_Accounts_Are_Exempt_From_The_Auto_Connect_List_Limit()
    {
        (string cookie, int accountID) = await SeedSession("channel.staff@kongor.com", "ChannelStaff");

        List<string> savedChannels = Enumerable.Range(1, (int) ChatProtocol.MAX_CHANNELS_PER_CLIENT).Select(index => $"Saved Channel {index}").ToList();

        await UpdateAccount(accountID, account =>
        {
            account.Type = AccountType.Staff;
            account.AutoConnectChatChannels = [.. savedChannels];
        });

        IDictionary<object, object> body = await PostAndDeserialise("add_room", cookie, "One Channel Too Many");

        using (Assert.Multiple())
        {
            await Assert.That(body["add_room"]).IsEqualTo("OK");
            await Assert.That(await LoadAutoConnectChatChannels(accountID)).Contains("One Channel Too Many");
        }
    }

    [Test]
    public async Task Adding_A_Channel_Name_Longer_Than_The_Protocol_Maximum_Is_Rejected()
    {
        (string cookie, int accountID) = await SeedSession("channel.length@kongor.com", "ChannelLength");

        HttpResponseMessage response = await Post("add_room", cookie, new string('A', ChatProtocol.CHAT_CHANNEL_MAX_LENGTH + 1));

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
            await Assert.That(await LoadAutoConnectChatChannels(accountID)).IsEmpty();
        }
    }

    [Test]
    public async Task Removing_A_Channel_Matches_The_Saved_Channel_Case_Insensitively()
    {
        (string cookie, int accountID) = await SeedSession("channel.remove@kongor.com", "ChannelRemove");

        await UpdateAccount(accountID, account => account.AutoConnectChatChannels = ["Auto Connect Channel", "Other Channel"]);

        IDictionary<object, object> body = await PostAndDeserialise("remove_room", cookie, "AUTO CONNECT CHANNEL");

        using (Assert.Multiple())
        {
            await Assert.That(body["remove_room"]).IsEqualTo("OK");
            await Assert.That(await LoadAutoConnectChatChannels(accountID)).IsEquivalentTo(["Other Channel"]);
        }
    }

    [Test]
    public async Task Removing_A_Channel_That_Is_Not_Saved_Still_Succeeds()
    {
        (string cookie, int accountID) = await SeedSession("channel.absent@kongor.com", "ChannelAbsent");

        await UpdateAccount(accountID, account => account.AutoConnectChatChannels = ["Other Channel"]);

        IDictionary<object, object> body = await PostAndDeserialise("remove_room", cookie, "Unsaved Channel");

        using (Assert.Multiple())
        {
            await Assert.That(body["remove_room"]).IsEqualTo("OK");
            await Assert.That(await LoadAutoConnectChatChannels(accountID)).IsEquivalentTo(["Other Channel"]);
        }
    }

    [Test]
    [Arguments("KONGOR")]
    [Arguments("kongor 3")]
    [Arguments("clan own clan")]
    public async Task Removing_A_Default_Channel_Is_Rejected_With_A_Failure_The_Client_Reports(string channelName)
    {
        (string cookie, int accountID) = await SeedSession("channel.default.remove@kongor.com", "ChannelKeep");

        await UpdateAccount(accountID, account => account.Clan = new Clan { Name = "Own Clan", Tag = "OWN" });

        IDictionary<object, object> body = await PostAndDeserialise("remove_room", cookie, channelName);

        using (Assert.Multiple())
        {
            await Assert.That(body["remove_room"]).IsEqualTo(false);
            await Assert.That(body.ContainsKey("error")).IsTrue();
        }
    }

    [Test]
    public async Task Removing_The_Channel_Of_A_Clan_The_Account_Has_Left_Succeeds()
    {
        (string cookie, int accountID) = await SeedSession("channel.former.clan@kongor.com", "ChannelFormer");

        await UpdateAccount(accountID, account => account.AutoConnectChatChannels = ["Clan Former Clan", "Other Channel"]);

        IDictionary<object, object> body = await PostAndDeserialise("remove_room", cookie, "Clan Former Clan");

        using (Assert.Multiple())
        {
            await Assert.That(body["remove_room"]).IsEqualTo("OK");
            await Assert.That(await LoadAutoConnectChatChannels(accountID)).IsEquivalentTo(["Other Channel"]);
        }
    }

    [Test]
    public async Task Clearing_The_Auto_Connect_List_Removes_Every_Saved_Channel()
    {
        (string cookie, int accountID) = await SeedSession("channel.clear@kongor.com", "ChannelClear");

        await UpdateAccount(accountID, account => account.AutoConnectChatChannels = ["Auto Connect Channel", "Other Channel"]);

        IDictionary<object, object> body = await PostAndDeserialise("clear_rooms", cookie);

        using (Assert.Multiple())
        {
            await Assert.That(body["clear_rooms"]).IsEqualTo("OK");
            await Assert.That(await LoadAutoConnectChatChannels(accountID)).IsEmpty();
        }
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

    private async Task UpdateAccount(int accountID, Action<Account> update)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account account = await databaseContext.Accounts.SingleAsync(record => record.ID == accountID);

        update(account);

        await databaseContext.SaveChangesAsync();
    }

    private async Task<HttpResponseMessage> Post(string function, string cookie, string? channelName = null)
    {
        Dictionary<string, string> fields = new () { ["cookie"] = cookie };

        if (channelName is not null)
            fields["chatroom_name"] = channelName;

        return await PlinkoTestsHelper.PostForm(webApplicationFactory, $"/client_requester.php?f={function}", fields);
    }

    private async Task<IDictionary<object, object>> PostAndDeserialise(string function, string cookie, string? channelName = null)
        => await PlinkoTestsHelper.DeserialisePhpResponse(await Post(function, cookie, channelName));

    private async Task<List<string>> LoadAutoConnectChatChannels(int accountID)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account account = await databaseContext.Accounts.AsNoTracking().SingleAsync(record => record.ID == accountID);

        return account.AutoConnectChatChannels;
    }
}
