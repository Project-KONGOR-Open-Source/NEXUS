namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Handlers.Compatibility;

/// <summary>
///     Covers <see cref="UserAgentHandlers"/>: a request has a version only when it carries exactly one user agent of the game's shape.
/// </summary>
public sealed class UserAgentHandlersTests
{
    private const string GameUserAgent = $"S2 Games | Heroes Of Newerth | {Distributions.Version} | WAC";

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
    public async Task A_User_Agent_Of_A_Linux_Match_Server_Has_Its_Version()
    {
        bool hasVersion = UserAgentHandlers.TryGetVersion($"S2 Games | Heroes Of Newerth | {Distributions.Version} | LAS", out string? version);

        using (Assert.Multiple())
        {
            await Assert.That(hasVersion).IsTrue();
            await Assert.That(version).IsEqualTo(Distributions.Version);
        }
    }

    [Test]
    [Arguments("S2 Games | Heroes Of Newerth | 5.26.10.31 | wac")]
    [Arguments("S2 Games | Heroes Of Newerth | 5.26.10.31 | WAC | x86_64")]
    [Arguments("S2 Games | Heroes of Newerth | 5.26.10.31 | WAC")]
    [Arguments("Project KONGOR | Heroes Of Newerth | 5.26.10.31 | WAC")]
    [Arguments("S2 Games | Heroes Of Newerth | 5.26.10.31 | MAC")]
    [Arguments("S2 Games/Heroes of Newerth/4.10.1.0/wac/x86_64")]
    public async Task A_User_Agent_Of_Another_Shape_Has_No_Version(string userAgent)
        => await Assert.That(UserAgentHandlers.TryGetVersion(userAgent, out _)).IsFalse();

    [Test]
    public async Task Two_User_Agents_Of_The_Game_Have_No_Version()
        => await Assert.That(UserAgentHandlers.TryGetVersion(new StringValues([ GameUserAgent, GameUserAgent ]), out _)).IsFalse();
}
