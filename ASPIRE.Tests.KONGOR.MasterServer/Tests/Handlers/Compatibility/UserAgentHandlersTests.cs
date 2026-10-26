namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Handlers.Compatibility;

/// <summary>
///     Covers <see cref="UserAgentHandlers"/>: a request has a version only when it carries exactly one user agent, of the game's shape.
/// </summary>
public sealed class UserAgentHandlersTests
{
    private const string GameUserAgent = $"S2 Games/Heroes Of Newerth/{Distributions.Version}/wac/x86_64";

    [Test]
    public async Task A_User_Agent_Of_The_Game_Has_Its_Version()
    {
        bool hasVersion = UserAgentHandlers.TryGetVersion(GameUserAgent, out string? version);

        using (Assert.Multiple())
        {
            await Assert.That(hasVersion).IsTrue();
            await Assert.That(version).IsEqualTo(Distributions.Version);
        }
    }

    [Test]
    public async Task Two_User_Agents_Of_The_Game_Have_No_Version()
        => await Assert.That(UserAgentHandlers.TryGetVersion(new StringValues([ GameUserAgent, GameUserAgent ]), out _)).IsFalse();
}
