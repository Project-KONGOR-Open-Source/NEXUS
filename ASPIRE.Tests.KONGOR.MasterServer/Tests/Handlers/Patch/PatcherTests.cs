namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Handlers.Patch;

/// <summary>
///     Tests for the patcher, which the game's updater asks whether a newer version exists: it answers with the distributions' version, and refuses every other version.
/// </summary>
public sealed class PatcherTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();

    [Test]
    public async Task Patcher_Answers_The_Distributions_Version_As_Current_And_Latest()
    {
        HttpResponseMessage response = await RequestPatch("patchercurrent@kongor.com", "PatcherCurrent", Distributions.Version);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        IDictionary<object, object> body = await DeserialisePHPResponse(response);

        IDictionary<object, object> latest = (IDictionary<object, object>) body.Single(entry => entry.Key.ToString() == "0").Value;

        using (Assert.Multiple())
        {
            await Assert.That((string) body["version"]).IsEqualTo(Distributions.Version);
            await Assert.That((string) body["current_version"]).IsEqualTo(Distributions.Version);
            await Assert.That((string) latest["latest_version"]).IsEqualTo(Distributions.Version);
        }
    }

    [Test]
    public async Task Patcher_Refuses_Another_Version_With_Bad_Request()
    {
        HttpResponseMessage response = await RequestPatch("patcherolder@kongor.com", "PatcherOlder", "4.10.1");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    /// <summary>
    ///     Logs a new account in, then asks the patcher about the Windows game client's distribution with the account's session cookie, as the game's updater does.
    /// </summary>
    private async Task<HttpResponseMessage> RequestPatch(string emailAddress, string accountName, string currentVersion)
    {
        SRPAuthenticationData login = await new SRPAuthenticationService(webApplicationFactory).AuthenticateWithSRP(emailAddress, accountName, "SecurePassword123!");

        HttpClient httpClient = webApplicationFactory.CreateClient();

        Dictionary<string, string> formData = new ()
        {
            { "update", "1" },
            { "version", "0.0.0.0" },
            { "current_version", currentVersion },
            { "os", "wac" },
            { "arch", "x86_64" },
            { "cookie", login.Cookie ?? throw new InvalidOperationException("The Login Returned No Session Cookie") }
        };

        return await httpClient.PostAsync("patcher/patcher.php", new FormUrlEncodedContent(formData));
    }

    private static async Task<IDictionary<object, object>> DeserialisePHPResponse(HttpResponseMessage response)
    {
        string body = await response.Content.ReadAsStringAsync();

        return PhpSerialization.Deserialize(body) as IDictionary<object, object>
            ?? throw new InvalidOperationException($@"Response Body Was Not A PHP Array: ""{body}""");
    }
}
