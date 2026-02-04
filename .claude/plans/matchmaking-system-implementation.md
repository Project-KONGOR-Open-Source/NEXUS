# Matchmaking System Implementation Plan

## Overview

Implement a complete matchmaking system for NEXUS based on the HON Chat Server specification documented in `.claude/specifications/matchmaking-system.md`.

**Reference Specification Sections:**
- Section 2: TCP Command Codes (protocol commands)
- Section 4: Packet Formats (message structures)
- Section 5-6: Complete Flow and Diagrams
- Section 13: Matchmaking Algorithm (ELO, combine methods, balancing)
- Section 14: Configuration Variables (CVARs)
- Section 17: Data Models
- Section 18: Leaver System
- Section 19: Match Server Allocation

---

## Current State Analysis

### Existing Infrastructure (Ready to Use)

| Component | Location | Status |
|-----------|----------|--------|
| Chat Server | `TRANSMUTANSTEIN.ChatServer` | Complete TCP server with session management |
| MatchmakingService | `Services/MatchmakingService.cs` | Framework with placeholder broker loop |
| MatchmakingGroup | `Domain/Matchmaking/MatchmakingGroup.cs` | Group lifecycle complete (create, join, leave, kick, invite) |
| MatchmakingGroupMember | `Domain/Matchmaking/MatchmakingGroupMember.cs` | Basic member model (no TMR yet) |
| MatchmakingGroupInformation | `Domain/Matchmaking/MatchmakingGroupInformation.cs` | Group settings with TeamSize calculation |
| Protocol Commands | `ASPIRE.Common/ChatProtocol.cs` | All TMM commands defined (0x0C0A-0x0F09) |
| Command Processors | `CommandProcessors/Matchmaking/` | 10 processors (GroupCreate through PopularityUpdate) |
| AccountStatistics | `MERRICK.DatabaseContext/Entities/Statistics/` | SkillRating field exists (default 1500) |
| Game Server Sessions | `Context.MatchServerChatSessions` | Tracks connected match servers |
| Server Status Handler | `CommandProcessors/Connection/ServerStatus.cs` | Receives server status updates |
| Match State Handlers | `CommandProcessors/MatchState/` | MatchComplete, MatchStatus (stubs) |
| Configuration | `KONGOR.MasterServer/Configuration/Matchmaking/` | Basic JSON structure exists |

### Missing Components (To Implement)

#### Core Algorithm
1. **Match Broker Algorithm** - Core logic to pair groups into balanced matches
2. **Rating Calculations** - ELO/TMR prediction and gain/loss formulas
3. **Team Balancing** - Two-pass group swapping for fairness
4. **TMR Spread Expansion** - Wait time increases acceptable skill range
5. **Group Combining** - 14 different team formation strategies
6. **Experience Classification** - Inexperienced, provisional, outlier detection

#### Game Server Integration
7. **Match Creation Flow** - `NET_CHAT_GS_CREATE_MATCH` → game server
8. **Match Announcement Handler** - `NET_CHAT_GS_ANNOUNCE_MATCH` ← game server
9. **Match Started Handler** - `NET_CHAT_GS_MATCH_STARTED` ← game server
10. **Match Abandoned Handler** - `NET_CHAT_GS_ABANDON_MATCH` ← game server
11. **Server Allocation** - Region-based idle server selection
12. **Connection Reminder System** - Resend connect packets to missing players

#### Client Communication
13. **Match Found Flow** - MatchFoundUpdate, QueueUpdate(type=16), AutoMatchConnect
14. **Queue Time Updates** - Periodic `TMM_GROUP_QUEUE_UPDATE (type=11)`
15. **Popularity Updates** - Real queue population data
16. **Pending Match Accept Flow** - Optional explicit acceptance (ranked)
17. **Queue Rejoin** - `TMM_GROUP_REJOIN_QUEUE` with preserved time

#### Supporting Systems
18. **Leaver System** - Strike tracking, queue bans, decay
19. **Post-Match Rating Updates** - TMR changes after match completion
20. **3v3 Support** - Grimm's Crossing with different composition scores
21. **1v1 Support** - Solo map fast matching
22. **Campaign/Seasons Mode** - Special matching and scoring rules

---

## Implementation Phases

### Phase 1: Core Data Structures

**Goal:** Create the domain models needed for the matchmaking algorithm.

#### 1.1 Create MatchmakingTeam Class

**File:** `TRANSMUTANSTEIN.ChatServer/Domain/Matchmaking/MatchmakingTeam.cs`

```csharp
public class MatchmakingTeam
{
    public Guid GUID { get; } = Guid.CreateVersion7();

    public List<MatchmakingGroup> Groups { get; set; } = [];

    public int PlayerCount => Groups.Sum(group => group.Members.Count);

    public byte TeamSize { get; set; } = 5;

    // TMR Statistics
    public double TotalTMR => Groups.Sum(group => group.TotalTMR);
    public double AverageTMR => PlayerCount > 0 ? TotalTMR / PlayerCount : 0;
    public double AdjustedTMR { get; set; } // After weighting applied
    public double HighestTMR => Groups.SelectMany(g => g.Members).Max(m => m.TMR);
    public double LowestTMR => Groups.SelectMany(g => g.Members).Min(m => m.TMR);

    // Composition
    public int GroupMakeup { get; set; } // Score: 5-stack=25, 4+1=17, etc.
    public string GroupMakeupString { get; set; } = string.Empty; // "5", "4+1", "3+2", etc.

    // Compatibility Flags (intersection of all groups)
    public uint GameModeFlags { get; set; }
    public uint RegionFlags { get; set; }

    // State
    public bool MatchedUp { get; set; }
    public bool Virtual { get; set; } // For simulation/testing

    // Methods
    public void RecalculateStatistics();
    public void CalculateGroupMakeup();
    public bool IsCompatibleWith(MatchmakingTeam other);
}
```

#### 1.2 Create MatchmakingMatch Class

**File:** `TRANSMUTANSTEIN.ChatServer/Domain/Matchmaking/MatchmakingMatch.cs`

```csharp
public class MatchmakingMatch
{
    public Guid GUID { get; } = Guid.CreateVersion7();

    public int MatchIndex { get; set; } // Sequential ID for this broker cycle

    // Teams
    public required MatchmakingTeam LegionTeam { get; set; }
    public required MatchmakingTeam HellbourneTeam { get; set; }

    // Match Quality
    public double MatchupPrediction { get; set; } // 0.0-1.0 (Legion win probability)
    public double WinPercentThreshold { get; set; }
    public double LossPercentThreshold { get; set; }
    public bool MismatchedGroupMakeup { get; set; }
    public bool FirstPassBalanced { get; set; }
    public bool SecondPassBalanced { get; set; }

    // How this match was created
    public TMMCombineMethod CombineMethod { get; set; }

    // Selected Options (intersection of both teams)
    public string SelectedMap { get; set; } = string.Empty;
    public string SelectedMode { get; set; } = string.Empty;
    public string SelectedRegion { get; set; } = string.Empty;
    public bool IsRanked { get; set; }

    // Pre-calculated rating changes per player
    public Dictionary<int, (double WinValue, double LossValue)> MatchPointValues { get; set; } = [];

    // Server Assignment
    public int? AssignedServerID { get; set; }
    public string? ServerAddress { get; set; }
    public ushort? ServerPort { get; set; }

    // State
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public MatchmakingMatchState State { get; set; } = MatchmakingMatchState.Created;

    // Methods
    public IEnumerable<MatchmakingGroupMember> GetAllPlayers();
    public IEnumerable<MatchmakingGroup> GetAllGroups();
}

public enum MatchmakingMatchState
{
    Created,
    ServerAllocating,
    ServerAllocated,
    WaitingForPlayers,
    Started,
    Abandoned,
    Completed
}

public enum TMMCombineMethod
{
    FullOrTwoGroupsTimeQueued,
    FullOrTwoGroupsRandom,
    FiveRandom,
    FourPlusOneRandom,
    ThreePlusTwoRandom,
    AllOnesRandom,
    AllGroupSizesRandom,
    BruteForce,
    // 3v3 variants
    ThreeFullOrTwoGroupsTimeQueued,
    ThreeAllOnesRandom,
    ThreeAllGroupSizesRandom,
    // 1v1
    OneVsOneRandom
}
```

#### 1.3 Extend MatchmakingGroupMember

**File:** `TRANSMUTANSTEIN.ChatServer/Domain/Matchmaking/MatchmakingGroupMember.cs`

Add these properties:

```csharp
// Rating
public double TMR { get; set; } = 1500.0;

// Experience Tracking
public int TotalMatchCount { get; set; } = 0;
public int GameTypeMatchCount { get; set; } = 0; // Matches in current game type
public int RecentWins { get; set; } = 0;
public int RecentLosses { get; set; } = 0;
public double MatchHistoryAdjustment { get; set; } = 0.0;

// Statistics (for K/D ratio matching)
public int Kills { get; set; } = 0;
public int Deaths { get; set; } = 0;
public int Assists { get; set; } = 0;

// Network (for IP conflict checking)
public string IPAddress { get; set; } = string.Empty;

// Pre-calculated match values (set when match is created)
public double MatchWinValue { get; set; }
public double MatchLossValue { get; set; }
```

#### 1.4 Extend MatchmakingGroup

**File:** `TRANSMUTANSTEIN.ChatServer/Domain/Matchmaking/MatchmakingGroup.cs`

Add these properties and methods:

```csharp
// TMR Statistics (recalculated when members change)
public double TotalTMR => Members.Sum(member => member.TMR);
public double AverageTMR => Members.Count > 0 ? TotalTMR / Members.Count : 0;
public double HighestTMR => Members.Max(member => member.TMR);
public double LowestTMR => Members.Min(member => member.TMR);
public double TMRRange => HighestTMR - LowestTMR;

// Experience
public int TotalMatchCount => Members.Sum(member => member.TotalMatchCount);
public bool HasInexperiencedPlayer => Members.Any(m => m.TotalMatchCount < 50 || m.TMR < 1625);
public bool HasProvisionalPlayer => Members.Any(m => m.GameTypeMatchCount < 10 && m.TMR < 1750);
public bool HasOutlierPlayer => Members.Any(m => m.TMR < 1200 || m.TMR > 1750);

// Match State
public bool MatchedUp { get; set; }
public Guid? AssignedTeamGUID { get; set; }
public Guid? AssignedMatchGUID { get; set; }

// Methods
public void LoadMemberStatistics(MerrickContext database); // Load TMR from AccountStatistics
public void LeaveQueue(bool preserveTime = false);
public void RejoinQueue(TimeSpan previousQueueTime);
```

---

### Phase 2: Configuration System

**Goal:** Implement all matchmaking CVARs from the HON specification.

#### 2.1 Create MatchmakingSettings Class

**File:** `TRANSMUTANSTEIN.ChatServer/Configuration/MatchmakingSettings.cs`

```csharp
public class MatchmakingSettings
{
    // TMR Range
    public double MinimumTMR { get; set; } = 1000.0;
    public double MaximumTMR { get; set; } = 2500.0;
    public double LowTMROutlier { get; set; } = 1200.0;
    public double HighTMROutlier { get; set; } = 1750.0;
    public double TMRMultiplier { get; set; } = 6.0;
    public double TMROutlierMultiplier { get; set; } = 40.0;

    // K-Factor (Rating Change)
    public double BaseKFactor { get; set; } = 10.0;
    public double MaxKFactor { get; set; } = 20.0;
    public double ProvisionalKFactorMultiplier { get; set; } = 2.0;
    public int ProvisionalMatchCount { get; set; } = 10;
    public double ProvisionalTMRCutoff { get; set; } = 1750.0;
    public double ReducedKFactorMultiplier { get; set; } = 0.20;
    public double ReducedKFactorTMRCutoff { get; set; } = 1600.0;

    // Experience Classification
    public int InexperiencedMatchCount { get; set; } = 50;
    public double InexperiencedTMRCutoff { get; set; } = 1625.0;

    // Wait Times
    public TimeSpan FairWaitTime { get; set; } = TimeSpan.FromMinutes(3);
    public TimeSpan LenientWaitTime { get; set; } = TimeSpan.FromMinutes(1.5);
    public TimeSpan FullTeamWaitTime { get; set; } = TimeSpan.FromMinutes(6);
    public TimeSpan BruteForceWaitTime { get; set; } = TimeSpan.FromMinutes(20);
    public TimeSpan BruteForceSoloWaitTime { get; set; } = TimeSpan.FromMinutes(10);
    public int DefaultGroupMakeupDifference { get; set; } = 2;

    // Prediction Thresholds
    public double LogisticPredictionScale { get; set; } = 225.0;
    public double StartingWinPercent { get; set; } = 0.51;
    public double StartingLossPercent { get; set; } = 0.49;
    public double WinLossMultiplier { get; set; } = 0.015;
    public double WinLossOutlierMultiplier { get; set; } = 0.240;
    public double BalanceTeamsLowPercent { get; set; } = 0.80;
    public double BalanceTeamsHighPercent { get; set; } = 0.20;

    // Match Fidelity
    public bool EnableMatchFidelity { get; set; } = true;
    public double MaxMatchFidelityTMRDifference { get; set; } = 100.0;
    public double FairLowMatchFidelityWinPercent { get; set; } = 0.47;
    public double FairHighMatchFidelityWinPercent { get; set; } = 0.53;
    public int MatchFidelityMaxWaitValue { get; set; } = 3;

    // Gamma Curve (Skill Difference Adjustment)
    public bool SkillDifferenceEnabled { get; set; } = true;
    public double GammaCurveRange { get; set; } = 175.0;
    public int GammaCurveK { get; set; } = 18;
    public double GammaCurveTheta { get; set; } = 5.0;
    public double TeamRankWeighting { get; set; } = 6.5;

    // Cycle Timing
    public TimeSpan MatchmakingCycleInterval { get; set; } = TimeSpan.FromSeconds(5);
    public int InexperiencedGroupCycleDelay { get; set; } = 4;
    public int FullOrTwoGroupCycleDelay { get; set; } = 6;
    public int AnyGroupsCycleDelay { get; set; } = 24;
    public int MaxMatchingCatchallLoopCount { get; set; } = 3;
    public int MaxMatchingTimeMs { get; set; } = 15000;

    // Feature Flags
    public bool EnableAlternateCombines { get; set; } = true;
    public bool EnableAlternateTeamBalancing { get; set; } = false;
    public bool EnableAlternateGroupSorting { get; set; } = true;
    public bool EnableAlternateTeamSorting { get; set; } = true;
    public double AlternateSortingRatio { get; set; } = 0.90;
    public bool EnableIPConflictChecking { get; set; } = false;
    public bool EnableKDRatioUse { get; set; } = false;
    public double MaxKDDifference { get; set; } = 0.75;
    public bool EnableReducedMMRLossForSmallGroups { get; set; } = true;
    public bool Fast1v1 { get; set; } = true;

    // Team Composition Scores (5v5)
    public static readonly Dictionary<string, int> CompositionScores5v5 = new()
    {
        ["5"] = 25,
        ["4+1"] = 17,
        ["3+2"] = 13,
        ["3+1+1"] = 11,
        ["2+2+1"] = 9,
        ["2+1+1+1"] = 7,
        ["1+1+1+1+1"] = 5
    };

    // Team Composition Scores (3v3)
    public static readonly Dictionary<string, int> CompositionScores3v3 = new()
    {
        ["3"] = 9,
        ["2+1"] = 5,
        ["1+1+1"] = 3
    };
}
```

#### 2.2 Create Leaver Settings Class

**File:** `TRANSMUTANSTEIN.ChatServer/Configuration/LeaverSettings.cs`

```csharp
public class LeaverSettings
{
    public double LeaverThreshold { get; set; } = 0.05; // 5% disconnect rate
    public TimeSpan StrikeDecayPeriod { get; set; } = TimeSpan.FromHours(24);
    public int GoodBehaviourMatchesForDecay { get; set; } = 5;

    // Ban Durations (indexed by strike count - 1)
    public TimeSpan[] BanDurations { get; set; } =
    [
        TimeSpan.Zero,                  // Strike 1: Warning only
        TimeSpan.FromMinutes(5),        // Strike 2
        TimeSpan.FromMinutes(15),       // Strike 3
        TimeSpan.FromMinutes(30),       // Strike 4
        TimeSpan.FromHours(1),          // Strike 5
        TimeSpan.FromHours(2),          // Strike 6+
    ];
}
```

---

### Phase 3: Rating System

**Goal:** Implement ELO-based matchup prediction and rating change calculations.

#### 3.1 Create RatingCalculator Class

**File:** `TRANSMUTANSTEIN.ChatServer/Services/Matchmaking/RatingCalculator.cs`

```csharp
public static class RatingCalculator
{
    /// <summary>
    ///     Calculates the ELO-based win probability for Team A.
    ///     Formula: 1 / (1 + 10^(-(TeamA_TMR - TeamB_TMR) / 225))
    /// </summary>
    public static double GetMatchupPrediction(double teamATMR, double teamBTMR, double scale = 225.0);

    /// <summary>
    ///     Converts queue duration to a wait value tier.
    ///     0-60s→1, 60s→2, 120s→3, 180s→4, 240s→5, 300s→6, 600s→7, >600s→10
    /// </summary>
    public static int GetWaitValue(TimeSpan queueDuration);

    /// <summary>
    ///     Calculates the acceptable TMR spread based on wait time and outlier status.
    ///     Spread = WaitValue × TMRMultiplier (+ OutlierAdjustment if applicable)
    /// </summary>
    public static (double Low, double High) GetTMRSpread(
        double averageTMR,
        int waitValue,
        bool isOutlier,
        MatchmakingSettings settings);

    /// <summary>
    ///     Calculates acceptable win/loss percentage thresholds.
    ///     Expands from 51%/49% based on wait time and outlier status.
    /// </summary>
    public static (double WinPercent, double LossPercent) GetWinLossThresholds(
        int waitValue,
        bool isOutlier,
        MatchmakingSettings settings);

    /// <summary>
    ///     Calculates K-Factor for rating changes.
    ///     Accounts for provisional status and high-TMR reduction.
    /// </summary>
    public static double GetKFactor(
        double tmr,
        int matchCount,
        bool isProvisional,
        MatchmakingSettings settings);

    /// <summary>
    ///     Calculates the actual rating gain/loss values for a match.
    /// </summary>
    public static (double WinValue, double LossValue) GetMatchPointValues(
        double prediction,
        double kFactor,
        double skillDifferenceAdjustment);

    /// <summary>
    ///     Calculates the gamma distribution skill difference adjustment.
    ///     Applied when player TMR > 1750 OR team skill diff > 175.
    /// </summary>
    public static double GetSkillDifferenceAdjustment(
        double playerTMR,
        double teamAverageTMR,
        MatchmakingSettings settings);

    /// <summary>
    ///     Calculates adjusted TMR for a team using weighting.
    /// </summary>
    public static double GetAdjustedTeamTMR(
        IEnumerable<double> playerTMRs,
        double teamRankWeighting);
}
```

#### 3.2 Create ExperienceClassifier Class

**File:** `TRANSMUTANSTEIN.ChatServer/Services/Matchmaking/ExperienceClassifier.cs`

```csharp
public static class ExperienceClassifier
{
    /// <summary>
    ///     Inexperienced: Total matches &lt; 50 OR TMR &lt; 1625
    /// </summary>
    public static bool IsInexperienced(int totalMatches, double tmr, MatchmakingSettings settings);

    /// <summary>
    ///     Provisional: Matches in game type &lt; 10 AND TMR &lt; 1750
    /// </summary>
    public static bool IsProvisional(int gameTypeMatches, double tmr, MatchmakingSettings settings);

    /// <summary>
    ///     Outlier: TMR &lt; 1200 OR TMR &gt; 1750
    /// </summary>
    public static bool IsOutlier(double tmr, MatchmakingSettings settings);

    /// <summary>
    ///     Determines if a group should be delayed in matchmaking cycles.
    /// </summary>
    public static bool ShouldDelayGroup(MatchmakingGroup group, int cycleCount, MatchmakingSettings settings);
}
```

---

### Phase 4: Match Broker Algorithm

**Goal:** Implement the core matching logic.

#### 4.1 Create MatchBroker Class

**File:** `TRANSMUTANSTEIN.ChatServer/Services/Matchmaking/MatchBroker.cs`

```csharp
public class MatchBroker
{
    private readonly MatchmakingSettings _settings;
    private readonly GroupCombiner _groupCombiner;
    private readonly TeamMatcher _teamMatcher;
    private readonly TeamBalancer _teamBalancer;
    private readonly ILogger<MatchBroker> _logger;

    /// <summary>
    ///     Runs a single matchmaking cycle.
    ///     Returns a list of created matches.
    /// </summary>
    public List<MatchmakingMatch> RunCycle(
        int cycleCount,
        IEnumerable<MatchmakingGroup> queuedGroups);

    /// <summary>
    ///     Removes groups that have already been matched.
    /// </summary>
    private List<MatchmakingGroup> PruneGroups(IEnumerable<MatchmakingGroup> groups);

    /// <summary>
    ///     Groups queued groups by game type and team size.
    /// </summary>
    private Dictionary<(TMMGameType, byte), List<MatchmakingGroup>> PartitionGroups(
        IEnumerable<MatchmakingGroup> groups);

    /// <summary>
    ///     Determines which combine methods to run based on cycle count.
    /// </summary>
    private IEnumerable<TMMCombineMethod> GetCombineMethodsForCycle(
        int cycleCount,
        byte teamSize,
        bool hasInexperiencedGroups);
}
```

#### 4.2 Create GroupCombiner Class

**File:** `TRANSMUTANSTEIN.ChatServer/Services/Matchmaking/GroupCombiner.cs`

```csharp
public class GroupCombiner
{
    private readonly MatchmakingSettings _settings;

    /// <summary>
    ///     Forms teams from groups using the specified combine method.
    /// </summary>
    public List<MatchmakingTeam> CombineGroups(
        List<MatchmakingGroup> groups,
        TMMCombineMethod method,
        byte teamSize);

    // Individual combine methods
    private List<MatchmakingTeam> CombineFullOrTwoGroups(List<MatchmakingGroup> groups, byte teamSize, bool sortByTime);
    private List<MatchmakingTeam> CombineFiveRandom(List<MatchmakingGroup> groups);
    private List<MatchmakingTeam> CombineFourPlusOneRandom(List<MatchmakingGroup> groups);
    private List<MatchmakingTeam> CombineThreePlusTwoRandom(List<MatchmakingGroup> groups);
    private List<MatchmakingTeam> CombineAllOnesRandom(List<MatchmakingGroup> groups, byte teamSize);
    private List<MatchmakingTeam> CombineAllGroupSizesRandom(List<MatchmakingGroup> groups, byte teamSize);
    private List<MatchmakingTeam> CombineBruteForce(List<MatchmakingGroup> groups, byte teamSize);

    /// <summary>
    ///     Calculates the composition score for a team.
    /// </summary>
    public int CalculateGroupMakeup(MatchmakingTeam team);

    /// <summary>
    ///     Generates the composition string (e.g., "4+1", "3+1+1").
    /// </summary>
    public string GetGroupMakeupString(MatchmakingTeam team);
}
```

#### 4.3 Create TeamMatcher Class

**File:** `TRANSMUTANSTEIN.ChatServer/Services/Matchmaking/TeamMatcher.cs`

```csharp
public class TeamMatcher
{
    private readonly MatchmakingSettings _settings;
    private readonly TeamBalancer _teamBalancer;

    /// <summary>
    ///     Pairs teams into matches.
    /// </summary>
    public List<MatchmakingMatch> CreateMatches(List<MatchmakingTeam> teams);

    /// <summary>
    ///     Finds the best opposing team for a given team.
    /// </summary>
    private MatchmakingTeam? FindTeamMatchup(
        MatchmakingTeam team,
        List<MatchmakingTeam> candidates);

    /// <summary>
    ///     Calculates how fair a potential matchup would be.
    /// </summary>
    private double CalculateMatchFairness(MatchmakingTeam teamA, MatchmakingTeam teamB);

    /// <summary>
    ///     Validates that team composition scores are within acceptable range.
    /// </summary>
    public bool HasAcceptableGroupMakeup(
        MatchmakingTeam teamA,
        MatchmakingTeam teamB,
        TimeSpan longestWait);

    /// <summary>
    ///     Validates that team TMR ranges are compatible.
    /// </summary>
    public bool HasAcceptableTMRRange(
        MatchmakingTeam teamA,
        MatchmakingTeam teamB);

    /// <summary>
    ///     Selects common game options (map, mode, region) for a match.
    /// </summary>
    public (string Map, string Mode, string Region) SelectMatchOptions(
        MatchmakingTeam teamA,
        MatchmakingTeam teamB);
}
```

#### 4.4 Create TeamBalancer Class

**File:** `TRANSMUTANSTEIN.ChatServer/Services/Matchmaking/TeamBalancer.cs`

```csharp
public class TeamBalancer
{
    private readonly MatchmakingSettings _settings;

    /// <summary>
    ///     Attempts to balance teams by swapping groups.
    ///     Uses two-pass algorithm from HON specification.
    ///     Target: 49.5% - 50.5% matchup prediction.
    /// </summary>
    public (bool Balanced, int Pass) BalanceTeams(
        MatchmakingTeam teamA,
        MatchmakingTeam teamB);

    /// <summary>
    ///     First pass: Same-size group swaps (4↔4, 3↔3, 2↔2, 1↔1).
    /// </summary>
    private bool FirstPassBalance(MatchmakingTeam teamA, MatchmakingTeam teamB);

    /// <summary>
    ///     Second pass: Unequal-size swaps (4↔3+1, 4↔2+2, 3↔2+1, etc.).
    /// </summary>
    private bool SecondPassBalance(MatchmakingTeam teamA, MatchmakingTeam teamB);

    /// <summary>
    ///     Validates whether a group swap improves fairness.
    /// </summary>
    private bool IsValidGroupSwap(
        MatchmakingTeam teamA,
        MatchmakingTeam teamB,
        MatchmakingGroup groupA,
        MatchmakingGroup groupB);

    /// <summary>
    ///     Swaps groups between teams.
    /// </summary>
    private void SwapGroups(
        MatchmakingTeam teamA,
        MatchmakingTeam teamB,
        MatchmakingGroup groupA,
        MatchmakingGroup groupB);
}
```

---

### Phase 5: Server Allocation

**Goal:** Find available game servers and manage match spawning.

#### 5.1 Create ServerAllocator Class

**File:** `TRANSMUTANSTEIN.ChatServer/Services/Matchmaking/ServerAllocator.cs`

```csharp
public class ServerAllocator
{
    private readonly IDatabase _distributedCache;
    private readonly ILogger<ServerAllocator> _logger;

    /// <summary>
    ///     Finds an available server for a match.
    ///     Returns NULL if no servers available.
    /// </summary>
    public async Task<MatchServer?> FindServer(MatchmakingMatch match);

    /// <summary>
    ///     Gets idle servers in the specified region.
    /// </summary>
    private async Task<List<MatchServer>> GetIdleServersInRegion(string region);

    /// <summary>
    ///     Filters servers by map support, capacity, and version.
    /// </summary>
    private List<MatchServer> FilterCompatibleServers(
        List<MatchServer> servers,
        string mapName,
        string gameVersion);

    /// <summary>
    ///     Gets common regions from both teams, sorted by priority.
    /// </summary>
    public List<string> GetPrioritisedRegions(MatchmakingMatch match);

    /// <summary>
    ///     Marks a server as reserved for a match.
    /// </summary>
    public async Task ReserveServer(MatchServer server, MatchmakingMatch match);

    /// <summary>
    ///     Releases a server reservation.
    /// </summary>
    public async Task ReleaseServer(int serverID);
}
```

#### 5.2 Create Game Server Command Processors

**File:** `TRANSMUTANSTEIN.ChatServer/CommandProcessors/MatchState/MatchAnnounced.cs`

```csharp
[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_ANNOUNCE_MATCH)]
public class MatchAnnounced : IAsynchronousCommandProcessor<MatchServerChatSession>
{
    // Handle game server announcing it's ready to accept players
    // Send AutoMatchConnect to all players
}
```

**File:** `TRANSMUTANSTEIN.ChatServer/CommandProcessors/MatchState/MatchStarted.cs`

```csharp
[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_MATCH_STARTED)]
public class MatchStarted : IAsynchronousCommandProcessor<MatchServerChatSession>
{
    // Handle match starting (all players connected, banning/picking begins)
    // Update player availability states
    // Clean up matchmaking groups
}
```

**File:** `TRANSMUTANSTEIN.ChatServer/CommandProcessors/MatchState/MatchAbandoned.cs`

```csharp
[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_ABANDON_MATCH)]
public class MatchAbandoned : IAsynchronousCommandProcessor<MatchServerChatSession>
{
    // Handle match failing to start
    // Return groups to queue with preserved time
    // Track players who failed to connect
}
```

**File:** `TRANSMUTANSTEIN.ChatServer/CommandProcessors/MatchState/PlayerReminder.cs`

```csharp
[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_REMIND_PLAYER)]
public class PlayerReminder : IAsynchronousCommandProcessor<MatchServerChatSession>
{
    // Handle reminder for missing player
    // Resend AutoMatchConnect with reminder flag (0xFFFFFFFF)
}
```

---

### Phase 6: Match Spawn Flow

**Goal:** Implement match creation and client notification.

#### 6.1 Create MatchSpawner Class

**File:** `TRANSMUTANSTEIN.ChatServer/Services/Matchmaking/MatchSpawner.cs`

```csharp
public class MatchSpawner
{
    private readonly ServerAllocator _serverAllocator;
    private readonly ILogger<MatchSpawner> _logger;

    // Pending matches awaiting server response
    private readonly ConcurrentDictionary<Guid, MatchmakingMatch> _pendingMatches = [];

    /// <summary>
    ///     Spawns a match by allocating a server and notifying players.
    /// </summary>
    public async Task<bool> SpawnMatch(MatchmakingMatch match);

    /// <summary>
    ///     Sends NET_CHAT_GS_CREATE_MATCH to the game server.
    /// </summary>
    private void SendCreateMatchToServer(MatchServer server, MatchmakingMatch match);

    /// <summary>
    ///     Called when game server sends NET_CHAT_GS_ANNOUNCE_MATCH.
    ///     Notifies all players that the match is ready.
    /// </summary>
    public void OnMatchAnnounced(int serverID, int matchID);

    /// <summary>
    ///     Sends match found packets to all players.
    /// </summary>
    private void NotifyPlayersMatchFound(MatchmakingMatch match);

    /// <summary>
    ///     Sends MatchFoundUpdate (0x0D09) to a player.
    /// </summary>
    private void SendMatchFoundUpdate(MatchmakingGroupMember member, MatchmakingMatch match);

    /// <summary>
    ///     Sends QueueUpdate type=16 (TMM_GROUP_FOUND_SERVER) to a player.
    ///     This triggers "Sound The Horn!" in the client.
    /// </summary>
    private void SendFoundServerUpdate(MatchmakingGroupMember member);

    /// <summary>
    ///     Sends AutoMatchConnect (0x0062) to a player.
    /// </summary>
    private void SendAutoMatchConnect(
        MatchmakingGroupMember member,
        MatchmakingMatch match,
        bool isReminder = false);

    /// <summary>
    ///     Handles no servers available - sends TMM_GROUP_NO_SERVERS_FOUND.
    /// </summary>
    public void SendNoServersFound(MatchmakingMatch match);
}
```

#### 6.2 Update MatchmakingService

**File:** `TRANSMUTANSTEIN.ChatServer/Services/MatchmakingService.cs`

Replace the placeholder `RunMatchBroker()` method:

```csharp
private async Task RunMatchBroker(CancellationToken cancellationToken)
{
    int cycleCount = 0;

    while (cancellationToken.IsCancellationRequested is false)
    {
        await Task.Delay(_settings.MatchmakingCycleInterval, cancellationToken);

        cycleCount++;

        // Get all queued groups
        List<MatchmakingGroup> queuedGroups = Groups.Values
            .Where(group => group.QueueStartTime is not null && group.MatchedUp is false)
            .ToList();

        if (queuedGroups.Count == 0)
            continue;

        // Run the broker cycle
        List<MatchmakingMatch> matches = _matchBroker.RunCycle(cycleCount, queuedGroups);

        // Spawn each match
        foreach (MatchmakingMatch match in matches)
        {
            bool spawned = await _matchSpawner.SpawnMatch(match);

            if (spawned is false)
            {
                _matchSpawner.SendNoServersFound(match);

                // Return groups to queue
                foreach (MatchmakingGroup group in match.GetAllGroups())
                    group.MatchedUp = false;
            }
        }

        // Send periodic queue time updates (every 10 seconds)
        if (cycleCount % 2 == 0)
            BroadcastQueueTimeUpdates(queuedGroups);
    }
}

private void BroadcastQueueTimeUpdates(List<MatchmakingGroup> queuedGroups)
{
    // Calculate average queue time
    int averageQueueTimeSeconds = queuedGroups.Count > 0
        ? (int)queuedGroups.Average(g => g.QueueDuration.TotalSeconds)
        : 0;

    foreach (MatchmakingGroup group in queuedGroups)
    {
        ChatBuffer update = new();
        update.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_QUEUE_UPDATE);
        update.WriteInt8(Convert.ToByte(ChatProtocol.TMMUpdateType.TMM_GROUP_QUEUE_UPDATE));
        update.WriteInt32(averageQueueTimeSeconds);

        foreach (MatchmakingGroupMember member in group.Members)
            member.Session.Send(update);
    }
}
```

---

### Phase 7: Leaver System

**Goal:** Track leaver strikes and enforce queue bans.

#### 7.1 Create LeaverTracker Class

**File:** `TRANSMUTANSTEIN.ChatServer/Services/Matchmaking/LeaverTracker.cs`

```csharp
public class LeaverTracker
{
    private readonly MerrickContext _database;
    private readonly LeaverSettings _settings;
    private readonly ILogger<LeaverTracker> _logger;

    /// <summary>
    ///     Checks if a player is currently a "leaver" based on disconnect rate.
    ///     Leaver = Disconnects / TotalMatches > 5%
    /// </summary>
    public async Task<bool> IsLeaver(int accountID);

    /// <summary>
    ///     Checks if a player is currently banned from matchmaking.
    /// </summary>
    public async Task<(bool IsBanned, TimeSpan? RemainingTime)> IsBanned(int accountID);

    /// <summary>
    ///     Records a leaver strike for a player.
    /// </summary>
    public async Task RecordStrike(int accountID);

    /// <summary>
    ///     Gets the ban duration for a given strike count.
    /// </summary>
    public TimeSpan GetBanDuration(int strikeCount);

    /// <summary>
    ///     Decays old strikes based on time passed.
    /// </summary>
    public async Task DecayStrikes(int accountID);

    /// <summary>
    ///     Decays strikes for good behaviour (completed matches).
    /// </summary>
    public async Task RecordGoodBehaviour(int accountID);

    /// <summary>
    ///     Sends leaver packets to a player who fails to join due to leaver status.
    /// </summary>
    public void SendLeaverInfo(ClientChatSession session, AccountStatistics stats);

    /// <summary>
    ///     Sends leaver strike warning popup.
    /// </summary>
    public void SendLeaverStrikeWarning(ClientChatSession session, int strikeCount);
}
```

#### 7.2 Create LeaverStrike Database Entity

**File:** `MERRICK.DatabaseContext/Entities/LeaverStrike.cs`

```csharp
[Index(nameof(AccountID), IsUnique = true)]
public class LeaverStrike
{
    [Key]
    public int ID { get; set; }

    public int AccountID { get; set; }

    [ForeignKey(nameof(AccountID))]
    public required Account Account { get; set; }

    public int StrikeCount { get; set; } = 0;

    public DateTime? BanExpiresAt { get; set; }

    public DateTime LastStrikeAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastDecayAt { get; set; }

    public int GoodBehaviourMatches { get; set; } = 0;
}
```

#### 7.3 Update MerrickContext

**File:** `MERRICK.DatabaseContext/MerrickContext.cs`

Add DbSet:

```csharp
public DbSet<LeaverStrike> LeaverStrikes { get; set; }
```

---

### Phase 8: Post-Match Rating Updates

**Goal:** Update player ratings after match completion.

#### 8.1 Extend ServerRequesterController

**File:** `KONGOR.MasterServer/Controllers/ServerRequesterController/ServerRequesterController.Match.cs`

Add rating update logic after match statistics are submitted:

```csharp
/// <summary>
///     Updates player ratings based on match outcome.
/// </summary>
private async Task UpdatePlayerRatings(
    MatchStatistics match,
    List<MatchParticipantStatistics> participants,
    byte winningTeam)
{
    // Group participants by team
    var legionPlayers = participants.Where(p => p.Team == 1).ToList();
    var hellbournePlayers = participants.Where(p => p.Team == 2).ToList();

    // Calculate team TMRs
    double legionTMR = legionPlayers.Average(p => p.PreMatchRating);
    double hellbourneTMR = hellbournePlayers.Average(p => p.PreMatchRating);

    // Calculate prediction
    double prediction = RatingCalculator.GetMatchupPrediction(legionTMR, hellbourneTMR);

    // Determine if reduced loss applies (small groups vs large)
    bool reducedLossApplies = ShouldApplyReducedLoss(match);

    foreach (MatchParticipantStatistics participant in participants)
    {
        bool won = participant.Team == winningTeam;

        // Get pre-calculated match point values OR calculate now
        double ratingChange = won
            ? participant.WinValue
            : participant.LossValue;

        // Apply reduced loss for small groups
        if (won is false && reducedLossApplies && IsInSmallGroup(participant))
            ratingChange *= 0.5;

        // Update AccountStatistics
        AccountStatistics? stats = await _database.AccountStatistics
            .FirstOrDefaultAsync(s =>
                s.AccountID == participant.AccountID &&
                s.Type == GetStatisticsType(match.GameType));

        if (stats is not null)
        {
            stats.SkillRating = Math.Clamp(
                stats.SkillRating + ratingChange,
                1000.0,
                2500.0);

            if (won)
                stats.MatchesWon++;
            else
                stats.MatchesLost++;

            stats.MatchesPlayed++;
        }
    }

    await _database.SaveChangesAsync();
}
```

---

## Infrastructure Requirements

### Dependency Injection Registration

**File:** `TRANSMUTANSTEIN.ChatServer/Program.cs`

Add service registrations:

```csharp
// Configuration
builder.Services.Configure<MatchmakingSettings>(
    builder.Configuration.GetSection("Matchmaking"));
builder.Services.Configure<LeaverSettings>(
    builder.Configuration.GetSection("Leaver"));

// Services
builder.Services.AddSingleton<MatchBroker>();
builder.Services.AddSingleton<GroupCombiner>();
builder.Services.AddSingleton<TeamMatcher>();
builder.Services.AddSingleton<TeamBalancer>();
builder.Services.AddSingleton<ServerAllocator>();
builder.Services.AddSingleton<MatchSpawner>();
builder.Services.AddScoped<LeaverTracker>();

builder.Services.AddHostedService<MatchmakingService>();
```

### Global Using Directives

**File:** `TRANSMUTANSTEIN.ChatServer/Internals/UsingDirectives.cs`

Add new usings for matchmaking services:

```csharp
global using TRANSMUTANSTEIN.ChatServer.Services.Matchmaking;
global using TRANSMUTANSTEIN.ChatServer.Configuration;
```

### Database Migration

Create EF Core migration for `LeaverStrike` entity:

```bash
dotnet ef migrations add AddLeaverStrikeTable --project MERRICK.DatabaseContext
dotnet ef database update --project MERRICK.DatabaseContext
```

---

## Files Summary

### New Files to Create

| File | Purpose |
|------|---------|
| `Domain/Matchmaking/MatchmakingTeam.cs` | Team formed from groups |
| `Domain/Matchmaking/MatchmakingMatch.cs` | Match between two teams |
| `Services/Matchmaking/MatchBroker.cs` | Main broker algorithm |
| `Services/Matchmaking/GroupCombiner.cs` | Team formation strategies |
| `Services/Matchmaking/TeamMatcher.cs` | Team pairing logic |
| `Services/Matchmaking/TeamBalancer.cs` | Two-pass balancing |
| `Services/Matchmaking/RatingCalculator.cs` | ELO formulas |
| `Services/Matchmaking/ExperienceClassifier.cs` | Player classification |
| `Services/Matchmaking/ServerAllocator.cs` | Game server selection |
| `Services/Matchmaking/MatchSpawner.cs` | Match creation and notification |
| `Services/Matchmaking/LeaverTracker.cs` | Strike tracking |
| `Configuration/MatchmakingSettings.cs` | Algorithm CVARs |
| `Configuration/LeaverSettings.cs` | Leaver system CVARs |
| `CommandProcessors/MatchState/MatchAnnounced.cs` | Handle GS_ANNOUNCE_MATCH |
| `CommandProcessors/MatchState/MatchStarted.cs` | Handle GS_MATCH_STARTED |
| `CommandProcessors/MatchState/MatchAbandoned.cs` | Handle GS_ABANDON_MATCH |
| `CommandProcessors/MatchState/PlayerReminder.cs` | Handle GS_REMIND_PLAYER |
| `Entities/LeaverStrike.cs` | Database model |

### Files to Modify

| File | Changes |
|------|---------|
| `Services/MatchmakingService.cs` | Replace placeholder broker, add DI |
| `Domain/Matchmaking/MatchmakingGroup.cs` | Add TMR caching, queue methods |
| `Domain/Matchmaking/MatchmakingGroupMember.cs` | Add TMR, MatchCount, IPAddress |
| `MerrickContext.cs` | Add LeaverStrike DbSet |
| `Program.cs` | Register new services |
| `Internals/UsingDirectives.cs` | Add new global usings |
| `CommandProcessors/Matchmaking/GroupCreate.cs` | Add leaver check |
| `CommandProcessors/MatchState/MatchComplete.cs` | Trigger rating updates |

---

## Implementation Order

1. **Phase 1** - Data structures (MatchmakingTeam, MatchmakingMatch, extend GroupMember/Group)
2. **Phase 2** - Configuration (MatchmakingSettings, LeaverSettings)
3. **Phase 3** - Rating system (RatingCalculator, ExperienceClassifier)
4. **Phase 4** - Match broker algorithm (MatchBroker, GroupCombiner, TeamMatcher, TeamBalancer)
5. **Phase 5** - Server allocation (ServerAllocator, game server command processors)
6. **Phase 6** - Match spawn flow (MatchSpawner, update MatchmakingService)
7. **Phase 7** - Leaver system (LeaverTracker, LeaverStrike entity, migration)
8. **Phase 8** - Post-match rating updates (ServerRequesterController extension)

---

## Verification

### Unit Tests

1. `RatingCalculator` - Test ELO prediction formula, K-Factor, wait value tiers
2. `GroupCombiner` - Test all 14 combine methods
3. `TeamMatcher` - Test matchup selection and fairness validation
4. `TeamBalancer` - Test group swapping improves fairness
5. `ExperienceClassifier` - Test player classification thresholds

### Integration Tests

1. Create groups, join queue, verify match creation
2. Verify rating changes after match completion
3. Verify leaver system bans and strikes
4. Verify server allocation and fallback regions

### Manual Testing

1. Start two clients, create groups, queue for match
2. Verify `TMM_GROUP_FOUND_SERVER` packet received
3. Verify `CHAT_CMD_AUTO_MATCH_CONNECT` with server details
4. Complete match, verify rating changes in database
5. Test leaver detection and strike escalation
