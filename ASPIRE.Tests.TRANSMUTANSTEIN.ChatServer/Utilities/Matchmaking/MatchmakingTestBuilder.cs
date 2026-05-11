namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Utilities.Matchmaking;

/// <summary>
///     Builds <see cref="MatchmakingGroup"/>, <see cref="MatchmakingGroupMember"/>, and <see cref="MatchmakingGroupInformation"/> instances for matchmaking unit tests without depending on a real chat session, database, or DI container.
///     The chat-session shim is constructed via <see cref="RuntimeHelpers.GetUninitializedObject"/>, which bypasses the production constructor's <see cref="IServiceProvider"/> lookup. The matchmaking algorithm under test never invokes <c>Send</c> on the session, so the bare instance is sufficient.
/// </summary>
internal static class MatchmakingTestBuilder
{
    /// <summary>
    ///     The neutral baseline TMR used across tests for "any rating" placeholders. Matches <see cref="MatchmakingSettings.DefaultTMR"/> so it lines up with what a freshly seeded production player would have.
    /// </summary>
    public const double BaselineTMR = 1500.0;

    /// <summary>
    ///     A TMR value high enough above <see cref="BaselineTMR"/> that a five-player team containing one such player trips the production +0/-1 check (highest minus average-of-rest = 700, well above the 151 threshold).
    /// </summary>
    public const double OutlierHighTMR = 2200.0;

    private static int _nextAccountID = 0;

    static MatchmakingTestBuilder()
    {
        // The Algorithm Calls Into "TRANSMUTANSTEIN.ChatServer.Utilities.Log", Which Throws Until Initialised By The Production Host; Tests Construct The Algorithm Directly, So We Wire In A No-Op Logger Here
        Log.Initialise(NullLogger.Instance);
    }

    /// <summary>
    ///     Default group information for a normal-mode, AP, NEWERTH-region, ranked, PVP queue.
    /// </summary>
    public static MatchmakingGroupInformation NormalRankedInformation() => new ()
    {
        ClientVersion   = "4.10.1.0",
        GroupType       = ChatProtocol.TMMType.TMM_TYPE_PVP,
        GameType        = ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL,
        MapName         = "caldavar",
        GameModes       = ["ap"],
        GameRegions     = ["NEWERTH"],
        Ranked          = true,
        MatchFidelity   = 0,
        BotDifficulty   = 0,
        RandomizeBots   = false
    };

    /// <summary>
    ///     Information for a co-op (bot) match.
    /// </summary>
    public static MatchmakingGroupInformation CoopInformation() => new ()
    {
        ClientVersion   = "4.10.1.0",
        GroupType       = ChatProtocol.TMMType.TMM_TYPE_COOP,
        GameType        = ChatProtocol.TMMGameType.TMM_GAME_TYPE_CASUAL,
        MapName         = "caldavar",
        GameModes       = ["ap"],
        GameRegions     = ["NEWERTH"],
        Ranked          = false,
        MatchFidelity   = 0,
        BotDifficulty   = 2,
        RandomizeBots   = false
    };

    /// <summary>
    ///     Constructs a single-member group (a solo queuer) with the supplied TMR and queue-start offset.
    /// </summary>
    public static MatchmakingGroup BuildSoloGroup(double tmr, double queuedMinutesAgo = 0, MatchmakingGroupInformation? information = null, int totalMatchCount = 100)
    {
        return BuildGroup([tmr], queuedMinutesAgo, information, totalMatchCount);
    }

    /// <summary>
    ///     Constructs a group with the supplied member TMRs. The first TMR becomes the leader.
    /// </summary>
    public static MatchmakingGroup BuildGroup(IReadOnlyList<double> memberTMRs, double queuedMinutesAgo = 0, MatchmakingGroupInformation? information = null, int totalMatchCount = 100)
    {
        information ??= NormalRankedInformation();

        List<MatchmakingGroupMember> members = [];

        for (int index = 0; index < memberTMRs.Count; index++)
        {
            int accountID = Interlocked.Increment(ref _nextAccountID);

            string accountName = $"P{accountID}";

            ClientChatSession session = CreateNullSession(accountID, accountName);

            MatchmakingGroupMember member = new (session)
            {
                Slot                     = Convert.ToByte(index + 1),
                IsLeader                 = index == 0,
                IsReady                  = true,
                IsInGame                 = false,
                IsEligibleForMatchmaking = true,
                LoadingPercent           = 100,
                GameModeAccess           = string.Empty,
                TMR                      = memberTMRs[index],
                CasualTMR                = memberTMRs[index],
                TotalMatchCount          = totalMatchCount
            };

            members.Add(member);
        }

        DateTimeOffset? queueStartTime = queuedMinutesAgo >= 0
            ? DateTimeOffset.UtcNow - TimeSpan.FromMinutes(queuedMinutesAgo)
            : null;

        MatchmakingGroup group = (MatchmakingGroup)Activator.CreateInstance(typeof(MatchmakingGroup), nonPublic: true)!;

        group.Members        = members;
        group.Information    = information;
        group.QueueStartTime = queueStartTime;

        return group;
    }

    /// <summary>
    ///     Constructs a clone of the default normal-ranked information with selected fields overridden.
    /// </summary>
    public static MatchmakingGroupInformation Information
    (
        ChatProtocol.TMMGameType? gameType = null,
        string[]? gameModes = null,
        string[]? gameRegions = null,
        bool? ranked = null,
        ChatProtocol.TMMType? groupType = null,
        byte? botDifficulty = null
    )
    {
        MatchmakingGroupInformation baseInformation = NormalRankedInformation();

        return new MatchmakingGroupInformation
        {
            ClientVersion   = baseInformation.ClientVersion,
            GroupType       = groupType     ?? baseInformation.GroupType,
            GameType        = gameType      ?? baseInformation.GameType,
            MapName         = baseInformation.MapName,
            GameModes       = gameModes     ?? baseInformation.GameModes,
            GameRegions     = gameRegions   ?? baseInformation.GameRegions,
            Ranked          = ranked        ?? baseInformation.Ranked,
            MatchFidelity   = baseInformation.MatchFidelity,
            BotDifficulty   = botDifficulty ?? baseInformation.BotDifficulty,
            RandomizeBots   = baseInformation.RandomizeBots
        };
    }

    /// <summary>
    ///     Builds a <see cref="MatchmakingSettings"/> instance using the production defaults from <see cref="MatchmakingSettings"/>'s property initialisers.
    ///     Tests can mutate the returned object before passing it to the algorithm.
    /// </summary>
    public static MatchmakingSettings DefaultSettings() => new ();

    /// <summary>
    ///     Constructs a stand-in <see cref="ClientChatSession"/> that bypasses the production constructor (which requires a TCP server and a service provider resolving <see cref="Microsoft.AspNetCore.Hosting.IWebHostEnvironment"/>).
    ///     Only the <see cref="ClientChatSession.Account"/> property is populated; the matchmaking algorithm under test never invokes <c>Send</c> or any other socket-bound member, so the rest of the session graph is left at default.
    /// </summary>
    private static ClientChatSession CreateNullSession(int accountID, string accountName)
    {
        ClientChatSession session = (ClientChatSession)RuntimeHelpers.GetUninitializedObject(typeof(ClientChatSession));

        session.Account = CreateAccount(accountID, accountName);

        return session;
    }

    private static Account CreateAccount(int id, string name)
    {
        MERRICK.DatabaseContext.Entities.Utility.Role role = new () { Name = "Player" };

        User user = new ()
        {
            EmailAddress    = $"{name}@test.local",
            Role            = role,
            SRPPasswordSalt = string.Empty,
            SRPPasswordHash = string.Empty
        };

        return new Account
        {
            ID      = id,
            Name    = name,
            User    = user,
            IsMain  = true
        };
    }
}
