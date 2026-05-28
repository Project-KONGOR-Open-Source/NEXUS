namespace TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;

/// <summary>
///     The pure matchmaking algorithm. Contains the broker cycle, team formation, pool size tier resolution, and team-pairing logic.
///     This type is intentionally side-effect-free. It neither reads from nor writes to <see cref="MatchmakingService.Groups"/>, performs no I/O, and dispatches no chat messages.
///     <see cref="MatchmakingService"/> consumes its results and handles all impure concerns (server allocation, packet sends, queue lifecycle updates).
/// </summary>
internal static class MatchmakingAlgorithm
{
    /// <summary>
    ///     Runs a single match broker cycle against the supplied queued groups, returning the matches that should be spawned.
    ///     The caller is responsible for filtering out co-op (bot) groups and groups that have already been matched.
    /// </summary>
    public static IReadOnlyList<MatchmakingMatch> RunMatchBrokerCycle(IReadOnlyList<MatchmakingGroup> queuedGroups, MatchmakingSettings settings)
    {
        if (queuedGroups.Count == 0)
            return [];

        int queuedPlayerCount = queuedGroups.Sum(group => group.Members.Count);

        PoolSizeParameters poolSizeParameters = ResolvePoolSizeParameters(queuedPlayerCount, settings);

        return RunMatchBrokerCycle(queuedGroups, settings, poolSizeParameters);
    }

    /// <summary>
    ///     Runs a single match broker cycle with the supplied pool size parameters, returning the matches that should be spawned.
    ///     Uses adaptive TMR spread based on queue time, pool size, and group makeup rules.
    /// </summary>
    public static IReadOnlyList<MatchmakingMatch> RunMatchBrokerCycle(IReadOnlyList<MatchmakingGroup> queuedGroups, MatchmakingSettings settings, PoolSizeParameters poolSizeParameters)
    {
        List<MatchmakingMatch> matches = [];

        int playersPerTeam = settings.PlayersPerTeam;
        double maxTMRDifference = settings.MaximumTeamTMRDifference;

        // Group By Game Type For Compatibility
        Dictionary<ChatProtocol.TMMGameType, List<MatchmakingGroup>> groupsByGameType = queuedGroups
            .GroupBy(group => group.Information.GameType)
            .ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());

        foreach ((ChatProtocol.TMMGameType gameType, List<MatchmakingGroup> typeGroups) in groupsByGameType)
        {
            // Form Teams From Groups (Using TMR-Aware Grouping)
            List<MatchmakingTeam> teams = [.. FormTeams(typeGroups, playersPerTeam)];

            // Sort Teams By Effective Rating For Better Matching (Power Mean + Premade Bonus)
            teams = [.. teams.OrderBy(team => team.EffectiveTeamRating)];

            // Pair Teams Into Matches (Matching Adjacent Teams By TMR)
            HashSet<int> matchedTeamIndices = [];

            for (int teamIndex = 0; teamIndex < teams.Count; teamIndex++)
            {
                if (matchedTeamIndices.Contains(teamIndex))
                    continue;

                MatchmakingTeam legionTeam = teams[teamIndex];

                MatchmakingTeam? bestOpponent = null;

                int bestOpponentIndex = -1;

                double bestTMRDifference = double.MaxValue;

                // Find The Best Opponent From Remaining Teams
                for (int opponentIndex = teamIndex + 1; opponentIndex < teams.Count; opponentIndex++)
                {
                    if (matchedTeamIndices.Contains(opponentIndex))
                        continue;

                    MatchmakingTeam candidateOpponent = teams[opponentIndex];

                    if (legionTeam.IsCompatibleWith(candidateOpponent) is false)
                        continue;

                    // Calculate TMR Difference Using Effective Rating (Power Mean + Premade Bonus)
                    double tmrDifference = Math.Abs(legionTeam.EffectiveTeamRating - candidateOpponent.EffectiveTeamRating);

                    // Get Maximum Acceptable TMR Spread Based On Queue Time And Pool Size
                    double maxAcceptableSpread = GetMaxAcceptableTMRSpread(legionTeam, candidateOpponent, maxTMRDifference, poolSizeParameters);

                    if (tmrDifference > maxAcceptableSpread)
                        continue;

                    // Check Group Makeup Difference (Pool-Size-Aware Tolerance)
                    int groupMakeupDifference = Math.Abs(legionTeam.GroupMakeup - candidateOpponent.GroupMakeup);

                    if (groupMakeupDifference > poolSizeParameters.GroupMakeupTolerance)
                    {
                        Log.Debug(@"Skipping Match Due To Group Makeup Mismatch: {Legion} vs {Hellbourne} (Diff: {Diff}, Tolerance: {Tolerance})",
                            legionTeam.GroupMakeupString, candidateOpponent.GroupMakeupString, groupMakeupDifference, poolSizeParameters.GroupMakeupTolerance);

                        continue;
                    }

                    // Check For +0/-1 Rating Outcomes (Skipped In Small Pools Where Finding Any Match Is Prioritised)
                    if (poolSizeParameters.EnforcePlusZeroMinusOneCheck && (ProducesPlusZeroMinusOne(legionTeam) || ProducesPlusZeroMinusOne(candidateOpponent)))
                    {
                        Log.Debug(@"Skipping Match Due To +0/-1 Rating Outcome Risk");

                        continue;
                    }

                    if (tmrDifference < bestTMRDifference)
                    {
                        bestOpponent = candidateOpponent;
                        bestOpponentIndex = opponentIndex;
                        bestTMRDifference = tmrDifference;
                    }
                }

                if (bestOpponent is null)
                    continue;

                // Mark Both Teams As Matched
                matchedTeamIndices.Add(teamIndex);
                matchedTeamIndices.Add(bestOpponentIndex);

                // Create The Match (Pool-Aware Group Makeup Tolerance Is Plumbed Into "MismatchedGroupMakeup")
                MatchmakingMatch match = MatchmakingMatch.FromTeams(legionTeam, bestOpponent, settings.LogisticPredictionScale, poolSizeParameters.GroupMakeupTolerance);

                // Set Match Details From First Group's Information
                MatchmakingGroupInformation information = legionTeam.Groups[0].Information;
                match.GameType = gameType;
                match.SelectedMap = information.MapName;
                match.SelectedMode = information.GameModes.Length > 0 ? information.GameModes[0] : "ap";
                match.SelectedRegion = information.GameRegions.Length > 0 ? information.GameRegions[0] : "NEWERTH";
                match.IsRanked = information.Ranked;
                match.CombineMethod = MatchmakingCombineMethod.FirstInFirstOut;

                matches.Add(match);

                if (match.HellbourneTeam is not null)
                {
                    Log.Information(@"Match Created: GUID={MatchGUID}, GameType={GameType}, Legion={LegionCount}p ({LegionMakeup}, TeamRating: {LegionTMR:F1}), Hellbourne={HellbourneCount}p ({HellbourneMakeup}, TeamRating: {HellbourneTMR:F1}), TeamRatingDifference={TMRDifference:F1}, Prediction={Prediction:P1}",
                        match.GUID, gameType, match.LegionTeam.PlayerCount, match.LegionTeam.GroupMakeupString, match.LegionTeam.EffectiveTeamRating, match.HellbourneTeam.PlayerCount, match.HellbourneTeam.GroupMakeupString, match.HellbourneTeam.EffectiveTeamRating, bestTMRDifference, match.MatchupPrediction);
                }

                else
                {
                    Log.Information(@"Bot Match Created: GUID={MatchGUID}, GameType={GameType}, Legion={LegionCount}p ({LegionMakeup}, TeamRating: {LegionTMR:F1}), TeamRatingDifference={TMRDifference:F1}",
                        match.GUID, gameType, match.LegionTeam.PlayerCount, match.LegionTeam.GroupMakeupString, match.LegionTeam.EffectiveTeamRating, bestTMRDifference);
                }
            }
        }

        return matches;
    }

    /// <summary>
    ///     Gets the maximum acceptable TMR spread for matching two teams.
    ///     Uses the adaptive spread based on the longest-waiting group and the current pool size tier.
    /// </summary>
    public static double GetMaxAcceptableTMRSpread(MatchmakingTeam team1, MatchmakingTeam team2, double baseMaxDifference, PoolSizeParameters poolSizeParameters)
    {
        // Get The Longest Queue Time From Either Team
        double longestQueueMinutes = Math.Max(team1.Groups.Max(group => group.QueuedTimeInMinutes), team2.Groups.Max(group => group.QueuedTimeInMinutes));

        double spread = baseMaxDifference;

        // Expand TMR Spread After The Pool-Size-Aware Delay Has Elapsed
        if (longestQueueMinutes > poolSizeParameters.ExpansionDelayMinutes)
        {
            double minutesBeyondDelay = longestQueueMinutes - poolSizeParameters.ExpansionDelayMinutes;

            spread += minutesBeyondDelay * poolSizeParameters.ExpansionRatePerMinute;
        }

        return Math.Min(spread, poolSizeParameters.MaximumTMRSpread);
    }

    /// <summary>
    ///     Checks if a team would produce "+0/-1" rating outcomes.
    ///     This happens when the highest-rated player is so far above their teammates that they would gain 0 MMR for winning but lose MMR for losing.
    /// </summary>
    /// <param name="team">The team to check.</param>
    /// <returns><see langword="true"/> if the team would produce +0/-1 outcomes, <see langword="false"/> otherwise.</returns>
    public static bool ProducesPlusZeroMinusOne(MatchmakingTeam team)
    {
        int teamSize = team.TeamSize;
        int playerCount = team.PlayerCount;

        // Only Applies To Full Teams With More Than 1 Player
        if (playerCount != teamSize || teamSize <= 1)
            return false;

        double totalTMR = team.TotalTMR;
        double highestTMR = team.HighestTMR;

        // Calculate The Combined TMR Of The Bottom N-1 Players
        double bottomMembersTMR = totalTMR - highestTMR;

        // If The Highest Player Is More Than 151 Above The Average Of The Rest, Reject
        // A Difference Of 200 Produces +0/-1 Outcomes, So We Use 151 As The Threshold
        double averageOfOthers = bottomMembersTMR / (teamSize - 1);
        double difference = highestTMR - averageOfOthers;

        return difference > 151.0;
    }

    /// <summary>
    ///     Forms teams from groups using a phased combine method.
    ///     Prioritises full teams first, then larger group combinations, then smaller.
    /// </summary>
    public static IReadOnlyList<MatchmakingTeam> FormTeams(IReadOnlyList<MatchmakingGroup> groups, int playersPerTeam)
    {
        List<MatchmakingTeam> teams = [];
        List<MatchmakingGroup> availableGroups = [.. groups.Where(group => group.MatchedUp is false)];

        // Phase 1: Full Teams (5-Stacks For 5v5)
        teams.AddRange(FormTeamsWithPattern(availableGroups, playersPerTeam, [playersPerTeam]));

        // Phase 2: Two-Group Combinations (Largest First)
        if (playersPerTeam == 5)
        {
            teams.AddRange(FormTeamsWithPattern(availableGroups, playersPerTeam, [4, 1])); // 4+1
            teams.AddRange(FormTeamsWithPattern(availableGroups, playersPerTeam, [3, 2])); // 3+2
        }

        else if (playersPerTeam == 3)
        {
            teams.AddRange(FormTeamsWithPattern(availableGroups, playersPerTeam, [2, 1])); // 2+1
        }

        // Phase 3: Three-Group Combinations
        if (playersPerTeam == 5)
        {
            teams.AddRange(FormTeamsWithPattern(availableGroups, playersPerTeam, [3, 1, 1])); // 3+1+1
            teams.AddRange(FormTeamsWithPattern(availableGroups, playersPerTeam, [2, 2, 1])); // 2+2+1
        }

        // Phase 4: Four-Group Combinations
        if (playersPerTeam == 5)
        {
            teams.AddRange(FormTeamsWithPattern(availableGroups, playersPerTeam, [2, 1, 1, 1])); // 2+1+1+1
        }

        // Phase 5: All Solo Players
        teams.AddRange(FormTeamsWithPattern(availableGroups, playersPerTeam, [.. Enumerable.Repeat(1, playersPerTeam)]));

        return teams;
    }

    /// <summary>
    ///     Forms teams matching a specific group size pattern.
    ///     For example, pattern [4, 1] looks for a 4-player group and a 1-player group.
    /// </summary>
    private static List<MatchmakingTeam> FormTeamsWithPattern(List<MatchmakingGroup> availableGroups, int playersPerTeam, int[] pattern)
    {
        List<MatchmakingTeam> teams = [];

        // Sort Pattern Descending For Greedy Matching
        int[] sortedPattern = [.. pattern.OrderByDescending(size => size)];

        while (true)
        {
            List<MatchmakingGroup> teamGroups = [];
            List<MatchmakingGroup> usedGroups = [];

            // Try To Find Groups Matching Each Size In The Pattern
            foreach (int requiredSize in sortedPattern)
            {
                // Find A Group With Exactly This Size (That Hasn't Been Used Yet And Is Compatible With Existing Team Groups)
                MatchmakingGroup? matchingGroup = availableGroups
                    .Except(usedGroups)
                    .Where(group => group.Members.Count == requiredSize)
                    .Where(group => teamGroups.Count == 0 || HasCompatibleQueuePreferences(teamGroups.First(), group))
                    .OrderBy(group => group.QueueStartTime) // FIFO Within Same Size
                    .FirstOrDefault();

                if (matchingGroup is null)
                    break; // Can't Complete This Pattern

                teamGroups.Add(matchingGroup);
                usedGroups.Add(matchingGroup);
            }

            // Check If We Found A Complete Pattern
            if (teamGroups.Count == sortedPattern.Length && teamGroups.Sum(group => group.Members.Count) == playersPerTeam)
            {
                // Create The Team
                MatchmakingTeam team = MatchmakingTeam.FromGroups(teamGroups, playersPerTeam);
                teams.Add(team);

                // Remove Used Groups From Available Pool
                foreach (MatchmakingGroup group in teamGroups)
                    availableGroups.Remove(group);

                // Track Combine Method
                string patternString = string.Join("+", sortedPattern);

                Log.Debug(@"Formed Team With Pattern {Pattern}: {GroupCount} Groups, {PlayerCount} Players",
                    patternString, teamGroups.Count, team.PlayerCount);
            }

            else
            {
                // Can't Form More Teams With This Pattern
                break;
            }
        }

        return teams;
    }

    /// <summary>
    ///     Checks if a candidate group has compatible queue preferences with a reference group for team formation.
    ///     Verifies overlapping game modes, overlapping regions, and matching ranked status.
    ///     This is a subset of <see cref="MatchmakingGroup.IsCompatibleWith"/> that excludes size and TMR checks.
    /// </summary>
    public static bool HasCompatibleQueuePreferences(MatchmakingGroup reference, MatchmakingGroup candidate)
    {
        // Must Have Overlapping Game Modes
        if (reference.Information.GameModes.Intersect(candidate.Information.GameModes).Any() is false)
            return false;

        // Must Have Overlapping Regions (NEWERTH Is A Wildcard That Matches All Regions)
        bool eitherHasAutoRegion = reference.Information.GameRegions.Contains("NEWERTH", StringComparer.OrdinalIgnoreCase)
            || candidate.Information.GameRegions.Contains("NEWERTH", StringComparer.OrdinalIgnoreCase);

        if (eitherHasAutoRegion is false && reference.Information.GameRegions.Intersect(candidate.Information.GameRegions).Any() is false)
            return false;

        // Must Be Same Ranked Status
        if (reference.Information.Ranked != candidate.Information.Ranked)
            return false;

        return true;
    }

    /// <summary>
    ///     Resolves the pool-size-aware matchmaking parameters for the current broker cycle.
    ///     The player pool size determines how aggressively the algorithm relaxes constraints to find matches.
    /// </summary>
    public static PoolSizeParameters ResolvePoolSizeParameters(int queuedPlayerCount, MatchmakingSettings settings)
    {
        if (queuedPlayerCount < settings.SmallPoolThreshold)
        {
            return new PoolSizeParameters
            (
                Tier:                         PoolSizeTier.Micro,
                ExpansionDelayMinutes:         settings.MicroPoolExpansionDelayMinutes,
                ExpansionRatePerMinute:        settings.MicroPoolExpansionRatePerMinute,
                MaximumTMRSpread:              settings.MicroPoolMaximumTMRSpread,
                GroupMakeupTolerance:          settings.MicroPoolGroupMakeupTolerance,
                EnforcePlusZeroMinusOneCheck:  settings.MicroPoolEnforcePlusZeroMinusOneCheck
            );
        }

        else if (queuedPlayerCount < settings.MediumPoolThreshold)
        {
            return new PoolSizeParameters
            (
                Tier:                         PoolSizeTier.Small,
                ExpansionDelayMinutes:         settings.SmallPoolExpansionDelayMinutes,
                ExpansionRatePerMinute:        settings.SmallPoolExpansionRatePerMinute,
                MaximumTMRSpread:              settings.SmallPoolMaximumTMRSpread,
                GroupMakeupTolerance:          settings.SmallPoolGroupMakeupTolerance,
                EnforcePlusZeroMinusOneCheck:  settings.SmallPoolEnforcePlusZeroMinusOneCheck
            );
        }

        else if (queuedPlayerCount < settings.LargePoolThreshold)
        {
            return new PoolSizeParameters
            (
                Tier:                         PoolSizeTier.Medium,
                ExpansionDelayMinutes:         settings.MediumPoolExpansionDelayMinutes,
                ExpansionRatePerMinute:        settings.MediumPoolExpansionRatePerMinute,
                MaximumTMRSpread:              settings.MediumPoolMaximumTMRSpread,
                GroupMakeupTolerance:          settings.MediumPoolGroupMakeupTolerance,
                EnforcePlusZeroMinusOneCheck:  settings.MediumPoolEnforcePlusZeroMinusOneCheck
            );
        }

        else if (queuedPlayerCount < settings.MacroPoolThreshold)
        {
            return new PoolSizeParameters
            (
                Tier:                         PoolSizeTier.Large,
                ExpansionDelayMinutes:         settings.LargePoolExpansionDelayMinutes,
                ExpansionRatePerMinute:        settings.LargePoolExpansionRatePerMinute,
                MaximumTMRSpread:              settings.LargePoolMaximumTMRSpread,
                GroupMakeupTolerance:          settings.LargePoolGroupMakeupTolerance,
                EnforcePlusZeroMinusOneCheck:  settings.LargePoolEnforcePlusZeroMinusOneCheck
            );
        }

        else
        {
            return new PoolSizeParameters
            (
                Tier:                         PoolSizeTier.Macro,
                ExpansionDelayMinutes:         settings.MacroPoolExpansionDelayMinutes,
                ExpansionRatePerMinute:        settings.MacroPoolExpansionRatePerMinute,
                MaximumTMRSpread:              settings.MacroPoolMaximumTMRSpread,
                GroupMakeupTolerance:          settings.MacroPoolGroupMakeupTolerance,
                EnforcePlusZeroMinusOneCheck:  settings.MacroPoolEnforcePlusZeroMinusOneCheck
            );
        }
    }
}

/// <summary>
///     Classifies the player pool into size tiers that drive adaptive matchmaking behaviour.
/// </summary>
internal enum PoolSizeTier
{
    /// <summary>
    ///     A micro pool uses the most aggressive TMR expansion and the most relaxed fairness constraints.
    ///     Finding any match at all is prioritised over match quality.
    /// </summary>
    Micro,

    /// <summary>
    ///     A small pool uses aggressive TMR expansion and relaxed fairness constraints to minimise queue times.
    /// </summary>
    Small,

    /// <summary>
    ///     A medium pool uses balanced TMR expansion and standard fairness constraints.
    /// </summary>
    Medium,

    /// <summary>
    ///     A large pool uses conservative TMR expansion and strict fairness constraints to maximise match quality.
    /// </summary>
    Large,

    /// <summary>
    ///     A macro pool uses the most conservative TMR expansion and the strictest fairness constraints.
    ///     Match quality is prioritised over queue times.
    /// </summary>
    Macro
}

/// <summary>
///     The resolved matchmaking parameters for a single match broker cycle, determined by the current player pool size tier.
/// </summary>
internal record PoolSizeParameters
(
    PoolSizeTier Tier, double ExpansionDelayMinutes, double ExpansionRatePerMinute, double MaximumTMRSpread, int GroupMakeupTolerance, bool EnforcePlusZeroMinusOneCheck
);
