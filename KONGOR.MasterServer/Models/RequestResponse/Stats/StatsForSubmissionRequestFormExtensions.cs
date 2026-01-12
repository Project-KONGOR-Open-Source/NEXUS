namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

using global::KONGOR.MasterServer.Services;

public static class StatsForSubmissionRequestFormExtensions
{
    public static MatchStatistics ToMatchStatistics(this StatsForSubmissionRequestForm form, int? matchServerID = null,
        string? hostAccountName = null)
    {
        MatchStatistics statistics = new()
        {
            // A Stats Re-Submission Request Form Contains The Match Server ID While A Stats Submission Request Form Does Not
            ServerID = form.ServerID ?? matchServerID ?? throw new NullReferenceException("Server ID Is NULL"),

            // A Stats Re-Submission Request Form Contains The Host Account Name While A Stats Submission Request Form Does Not
            HostAccountName =
                form.HostAccountName ??
                hostAccountName ?? throw new NullReferenceException("Host Account Name Is NULL"),
            MatchID = int.Parse(form.MatchStats["match_id"]),
            Map = form.MatchStats["map"],
            MapVersion = form.MatchStats["map_version"],
            TimePlayed = int.Parse(form.MatchStats["time_played"]),
            FileSize = int.Parse(form.MatchStats["file_size"]),
            FileName = form.MatchStats["file_name"],
            ConnectionState = int.Parse(form.MatchStats["c_state"]),
            Version = form.MatchStats["version"],
            AveragePSR = int.Parse(form.MatchStats["avgpsr"]),
            AveragePSRTeamOne = int.Parse(form.MatchStats["avgpsr_team1"]),
            AveragePSRTeamTwo = int.Parse(form.MatchStats["avgpsr_team2"]),
            GameMode = form.MatchStats["gamemode"],
            ScoreTeam1 = form.TeamStats.First().Value.Single().Value,
            ScoreTeam2 = form.TeamStats.Last().Value.Single().Value,
            TeamScoreGoal = int.Parse(form.MatchStats["teamscoregoal"]),
            PlayerScoreGoal = int.Parse(form.MatchStats["playerscoregoal"]),
            NumberOfRounds = int.Parse(form.MatchStats["numrounds"]),
            ReleaseStage = form.MatchStats["release_stage"],
            BannedHeroes = form.MatchStats.TryGetValue("banned_heroes", out string? banned) ? banned : null,
            ScheduledEventID = form.MatchStats.TryGetValue("event_id", out string? eventId) ? int.Parse(eventId) : null,
            ScheduledMatchID =
                form.MatchStats.TryGetValue("matchup_id", out string? matchupId) ? int.Parse(matchupId) : null,
            MVPAccountID = form.MatchStats.TryGetValue("mvp", out string? mvp) ? int.Parse(mvp) : null,
            AwardMostAnnihilations = int.Parse(form.MatchStats["awd_mann"]),
            AwardMostQuadKills = int.Parse(form.MatchStats["awd_mqk"]),
            AwardLargestKillStreak = int.Parse(form.MatchStats["awd_lgks"]),
            AwardMostSmackdowns = int.Parse(form.MatchStats["awd_msd"]),
            AwardMostKills = int.Parse(form.MatchStats["awd_mkill"]),
            AwardMostAssists = int.Parse(form.MatchStats["awd_masst"]),
            AwardLeastDeaths = int.Parse(form.MatchStats["awd_ledth"]),
            AwardMostBuildingDamage = int.Parse(form.MatchStats["awd_mbdmg"]),
            AwardMostWardsKilled = int.Parse(form.MatchStats["awd_mwk"]),
            AwardMostHeroDamageDealt = int.Parse(form.MatchStats["awd_mhdd"]),
            AwardHighestCreepScore = int.Parse(form.MatchStats["awd_hcs"]),
            FragHistory = form.FragHistory?.Select(frag => new MERRICK.DatabaseContext.Entities.Statistics.FragEvent
            {
                SourceID = frag.SourceID,
                TargetID = frag.TargetID,
                GameTimeSeconds = frag.GameTimeSeconds,
                SupporterIDs = frag.SupporterIDs
            }).ToList()
        };

        return statistics;
    }

    public static PlayerStatistics ToPlayerStatistics(this StatsForSubmissionRequestForm form, int playerIndex,
        int accountID, string accountName, int? clanID, string? clanTag, IHeroDefinitionService heroDefinitionService)
    {
        string hero = form.PlayerStats[playerIndex].Keys.Single();

        Dictionary<string, string> player = form.PlayerStats[playerIndex][hero];

        // 2026-01-11: Public Games (and others) may send 0 or uint.MaxValue for hero_id.
        // We must attempt to resolve the ID from the `hero` string key if the payload ID is invalid.
        uint parsedHeroId = uint.Parse(player["hero_id"]);
        uint heroProductId;
        
        if (parsedHeroId == 0 || parsedHeroId == uint.MaxValue)
        {
             // Resolve from identifier (e.g. "Hero_Valkyrie")
             heroProductId = heroDefinitionService.GetBaseHeroId(hero);
        }
        else
        {
             heroProductId = parsedHeroId;
        }

        PlayerStatistics statistics = new()
        {
            MatchID = int.Parse(form.MatchStats["match_id"]),
            AccountID = accountID,
            AccountName = accountName,
            ClanID = clanID,
            ClanTag = clanTag,
            Team = int.Parse(player["team"]),
            LobbyPosition = int.Parse(player["position"]),
            GroupNumber = int.Parse(player["group_num"]),
            Benefit = int.Parse(player["benefit"]),
            HeroProductID = heroProductId,
            AlternativeAvatarName =
                player.TryGetValue("alt_avatar_name", out string? altAvatarName) ? altAvatarName : null,
            AlternativeAvatarProductID =
                player.TryGetValue("alt_avatar_pid", out string? altAvatarPid) &&
                uint.TryParse(altAvatarPid, out uint pid) && pid != uint.MaxValue
                    ? pid
                    : null,
            WardProductName = player.TryGetValue("ward_name", out string? wardName) ? wardName : null,
            WardProductID =
                player.TryGetValue("ward_pid", out string? wardPid) && uint.TryParse(wardPid, out uint wpid) &&
                wpid != uint.MaxValue
                    ? wpid
                    : null,
            TauntProductName = player.TryGetValue("taunt_name", out string? tauntName) ? tauntName : null,
            TauntProductID =
                player.TryGetValue("taunt_pid", out string? tauntPid) && uint.TryParse(tauntPid, out uint tpid) &&
                tpid != uint.MaxValue
                    ? tpid
                    : null,
            AnnouncerProductName =
                player.TryGetValue("announcer_name", out string? announcerName) ? announcerName : null,
            AnnouncerProductID =
                player.TryGetValue("announcer_pid", out string? announcerPid) &&
                uint.TryParse(announcerPid, out uint apid) && apid != uint.MaxValue
                    ? apid
                    : null,
            CourierProductName = player.TryGetValue("courier_name", out string? courierName) ? courierName : null,
            CourierProductID =
                player.TryGetValue("courier_pid", out string? courierPid) && uint.TryParse(courierPid, out uint cpid) &&
                cpid != uint.MaxValue
                    ? cpid
                    : null,
            AccountIconProductName = player.TryGetValue("account_icon_name", out string? iconName) ? iconName : null,
            AccountIconProductID =
                player.TryGetValue("account_icon_pid", out string? iconPid) && uint.TryParse(iconPid, out uint ipid) &&
                ipid != uint.MaxValue
                    ? ipid
                    : null,
            ChatColourProductName = player.TryGetValue("chat_color_name", out string? chatName) ? chatName : null,
            ChatColourProductID =
                player.TryGetValue("chat_color_pid", out string? chatPid) && uint.TryParse(chatPid, out uint chpid) &&
                chpid != uint.MaxValue
                    ? chpid
                    : null,
            Inventory = form.PlayerInventory is not null &&
                        form.PlayerInventory.TryGetValue(playerIndex, out Dictionary<int, string>? inventory)
                ? [.. inventory.Values]
                : [],
            Win = int.Parse(player["wins"]),
            Loss = int.Parse(player["losses"]),
            Disconnected = int.Parse(player["discos"]),
            Conceded = int.Parse(player["concedes"]),
            Kicked = int.Parse(player["kicked"]),
            PublicMatch = player.TryGetValue("pub_count", out string? pubCount) ? int.Parse(pubCount) : 0,
            PublicSkillRatingChange =
                player.TryGetValue("pub_skill", out string? pubSkill) ? double.Parse(pubSkill) : 0,
            RankedMatch = player.TryGetValue("amm_team_count", out string? rankCount) ? int.Parse(rankCount) : 0,
            RankedSkillRatingChange =
                player.TryGetValue("amm_team_rating", out string? rankSkill) ? double.Parse(rankSkill) : 0,
            SocialBonus = int.Parse(player["social_bonus"]),
            UsedToken = int.Parse(player["used_token"]),
            ConcedeVotes = int.Parse(player["concedevotes"]),
            HeroKills = int.Parse(player["herokills"]),
            HeroDamage = int.Parse(player["herodmg"]),
            GoldFromHeroKills = int.Parse(player["herokillsgold"]),
            HeroAssists = int.Parse(player["heroassists"]),
            HeroExperience = int.Parse(player["heroexp"]),
            HeroDeaths = int.Parse(player["deaths"]),
            Buybacks = int.Parse(player["buybacks"]),
            GoldLostToDeath = int.Parse(player["goldlost2death"]),
            SecondsDead = int.Parse(player["secs_dead"]),
            TeamCreepKills = int.Parse(player["teamcreepkills"]),
            TeamCreepDamage = int.Parse(player["teamcreepdmg"]),
            TeamCreepGold = int.Parse(player["teamcreepgold"]),
            TeamCreepExperience = int.Parse(player["teamcreepexp"]),
            NeutralCreepKills = int.Parse(player["neutralcreepkills"]),
            NeutralCreepDamage = int.Parse(player["neutralcreepdmg"]),
            NeutralCreepGold = int.Parse(player["neutralcreepgold"]),
            NeutralCreepExperience = int.Parse(player["neutralcreepexp"]),
            BuildingDamage = int.Parse(player["bdmg"]),
            BuildingsRazed = int.Parse(player["razed"]),
            ExperienceFromBuildings = int.Parse(player["bdmgexp"]),
            GoldFromBuildings = int.Parse(player["bgold"]),
            Denies = int.Parse(player["denies"]),
            ExperienceDenied = int.Parse(player["exp_denied"]),
            Gold = int.Parse(player["gold"]),
            GoldSpent = int.Parse(player["gold_spent"]),
            Experience = int.Parse(player["exp"]),
            Actions = int.Parse(player["actions"]),
            SecondsPlayed = int.Parse(player["secs"]),
            HeroLevel = int.Parse(player["level"]),
            ConsumablesPurchased = int.Parse(player["consumables"]),
            WardsPlaced = int.Parse(player["wards"]),
            FirstBlood = int.Parse(player["bloodlust"]),
            DoubleKill = int.Parse(player["doublekill"]),
            TripleKill = int.Parse(player["triplekill"]),
            QuadKill = int.Parse(player["quadkill"]),
            Annihilation = int.Parse(player["annihilation"]),
            KillStreak03 = int.Parse(player["ks3"]),
            KillStreak04 = int.Parse(player["ks4"]),
            KillStreak05 = int.Parse(player["ks5"]),
            KillStreak06 = int.Parse(player["ks6"]),
            KillStreak07 = int.Parse(player["ks7"]),
            KillStreak08 = int.Parse(player["ks8"]),
            KillStreak09 = int.Parse(player["ks9"]),
            KillStreak10 = int.Parse(player["ks10"]),
            KillStreak15 = int.Parse(player["ks15"]),
            Smackdown = int.Parse(player["smackdown"]),
            Humiliation = int.Parse(player["humiliation"]),
            Nemesis = int.Parse(player["nemesis"]),
            Retribution = int.Parse(player["retribution"]),
            Score = int.Parse(player["score"]),
            GameplayStat0 = double.Parse(player["gameplaystat0"]),
            GameplayStat1 = double.Parse(player["gameplaystat1"]),
            GameplayStat2 = double.Parse(player["gameplaystat2"]),
            GameplayStat3 = double.Parse(player["gameplaystat3"]),
            GameplayStat4 = double.Parse(player["gameplaystat4"]),
            GameplayStat5 = double.Parse(player["gameplaystat5"]),
            GameplayStat6 = double.Parse(player["gameplaystat6"]),
            GameplayStat7 = double.Parse(player["gameplaystat7"]),
            GameplayStat8 = double.Parse(player["gameplaystat8"]),
            GameplayStat9 = double.Parse(player["gameplaystat9"]),
            TimeEarningExperience = int.Parse(player["time_earning_exp"]),
            ItemHistory = form.ItemHistory?.Where(item => item.AccountID == accountID).Select(item =>
                new MERRICK.DatabaseContext.Entities.Statistics.ItemEvent
                {
                    ItemName = item.ItemName, GameTimeSeconds = item.GameTimeSeconds, EventType = item.EventType
                }).ToList(),
            AbilityHistory = form.AbilityHistory is not null &&
                             form.AbilityHistory.TryGetValue(accountID, out List<AbilityEvent>? abilities)
                ?
                [
                    .. abilities.Select(ability => new MERRICK.DatabaseContext.Entities.Statistics.AbilityEvent
                    {
                        HeroName = ability.HeroName,
                        AbilityName = ability.AbilityName,
                        GameTimeSeconds = ability.GameTimeSeconds,
                        SlotIndex = ability.SlotIndex
                    })
                ]
                : null
        };

        return statistics;
    }
}