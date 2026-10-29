namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Handlers.Ascension;

/// <summary>
///     Tests for the Ascension controller, which the game reaches at the address it is given with "-ascensionserver": the match server checks each new match there, and the game client asks for the player's role.
/// </summary>
public sealed class AscensionTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();

    [Test]
    public async Task A_Match_Check_Answers_That_The_Match_Is_Not_A_Season_Match()
    {
        using HttpClient client = webApplicationFactory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/?r=api/match/checkmatch&match_id=1");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        JsonElement data = body.GetProperty("data");

        using (Assert.Multiple())
        {
            await Assert.That(body.GetProperty("error_code").GetInt32()).IsEqualTo(100);
            await Assert.That(data.GetProperty("is_season_match").GetBoolean()).IsFalse();

            // The Match Server Reads Both Arrays Without Checking That They Exist
            await Assert.That(data.GetProperty("comment").GetArrayLength()).IsEqualTo(0);
            await Assert.That(data.GetProperty("referee").GetArrayLength()).IsEqualTo(0);
        }
    }

    [Test]
    public async Task A_User_Role_Check_Answers_No_Role()
    {
        using HttpClient client = webApplicationFactory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/?r=api/match/checkuserrole&account_id=1&region=naeu");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        using (Assert.Multiple())
        {
            await Assert.That(body.GetProperty("error_code").GetInt32()).IsEqualTo(100);
            await Assert.That(body.GetProperty("role").GetString()).IsEqualTo("0");
        }
    }
}
