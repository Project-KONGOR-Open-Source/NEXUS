namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

using PlayerEntity = MERRICK.DatabaseContext.Entities.Statistics.PlayerStatistics;

// Properties Common To Both "submit_stats" And "resubmit_stats" Requests
public partial class StatsForSubmissionRequestForm
{
    [FromForm(Name = "f")]
    public required string Function { get; set; }

    [FromForm(Name = "match_stats")]
    public required Dictionary<string, string> MatchStats { get; set; }

    [FromForm(Name = "team_stats")]
    public required Dictionary<int, Dictionary<string, int>> TeamStats { get; set; }

    [FromForm(Name = "player_stats")]
    public required Dictionary<int, Dictionary<string, Dictionary<string, string>>> PlayerStats { get; set; }

    [FromForm(Name = "inventory")]
    public Dictionary<int, Dictionary<int, string>>? PlayerInventory { get; set; }
}

// Properties Specific To "submit_stats" Requests
public partial class StatsForSubmissionRequestForm
{
    [FromForm(Name = "session")]
    public string? Session { get; set; }
}

// Properties Specific To "resubmit_stats" Requests
public partial class StatsForSubmissionRequestForm
{
    [FromForm(Name = "login")]
    public string? HostAccountName { get; set; }

    [FromForm(Name = "pass")]
    public string? HostAccountPasswordHash { get; set; }

    [FromForm(Name = "resubmission_key")]
    public string? StatsResubmissionKey { get; set; }

    [FromForm(Name = "server_id")]
    public int? ServerID { get; set; }
}

// Conditional Properties
public partial class StatsForSubmissionRequestForm
{
    /// <summary>
    ///     Item purchase, sell, and drop history.
    ///     Only submitted if server CVAR "svr_submitMatchStatItems" is enabled.
    /// </summary>
    [FromForm(Name = "items")]
    public List<ItemEvent>? ItemHistory { get; set; }

    /// <summary>
    ///     Ability upgrade timeline for each player.
    ///     Only submitted if server CVAR "svr_submitMatchStatAbilities" is enabled.
    /// </summary>
    [FromForm(Name = "abilities")]
    public Dictionary<int, List<AbilityEvent>>? AbilityHistory { get; set; }

    /// <summary>
    ///     Kill/Death event details with assists.
    ///     Only submitted if server CVAR "svr_submitMatchStatFrags" is enabled.
    /// </summary>
    [FromForm(Name = "frags")]
    public List<FragEvent>? FragHistory { get; set; }
}

public class MatchStats
{
    [FromForm(Name = "server_id")]
    public required int ServerID { get; set; }

    [FromForm(Name = "match_id")]
    public required int MatchID { get; set; }

    [FromForm(Name = "map")]
    public required string Map { get; set; }

    [FromForm(Name = "map_version")]
    public required string MapVersion { get; set; }

    [FromForm(Name = "time_played")]
    public required int TimePlayed { get; set; }

    [FromForm(Name = "file_size")]
    public required int FileSize { get; set; }

    [FromForm(Name = "file_name")]
    public required string FileName { get; set; }

    [FromForm(Name = "c_state")]
    public required int ConnectionState { get; set; }

    [FromForm(Name = "version")]
    public required string Version { get; set; }

    [FromForm(Name = "avgpsr")]
    public required int AveragePSR { get; set; }

    [FromForm(Name = "avgpsr_team1")]
    public required int AveragePSRTeamOne { get; set; }

    [FromForm(Name = "avgpsr_team2")]
    public required int AveragePSRTeamTwo { get; set; }

    [FromForm(Name = "gamemode")]
    public required string GameMode { get; set; }

    [FromForm(Name = "teamscoregoal")]
    public required int TeamScoreGoal { get; set; }

    [FromForm(Name = "playerscoregoal")]
    public required int PlayerScoreGoal { get; set; }

    [FromForm(Name = "numrounds")]
    public required int NumberOfRounds { get; set; }

    [FromForm(Name = "release_stage")]
    public required string ReleaseStage { get; set; }

    [FromForm(Name = "banned_heroes")]
    public string? BannedHeroes { get; set; }

    [FromForm(Name = "event_id")]
    public int? ScheduledEventID { get; set; }

    [FromForm(Name = "matchup_id")]
    public int? ScheduledMatchID { get; set; }

    [FromForm(Name = "mvp")]
    public int? MVPAccountID { get; set; }

    [FromForm(Name = "awd_mann")]
    public required int AwardMostAnnihilations { get; set; }

    [FromForm(Name = "awd_mqk")]
    public required int AwardMostQuadKills { get; set; }

    [FromForm(Name = "awd_lgks")]
    public required int AwardLargestKillStreak { get; set; }

    [FromForm(Name = "awd_msd")]
    public required int AwardMostSmackdowns { get; set; }

    [FromForm(Name = "awd_mkill")]
    public required int AwardMostKills { get; set; }

    [FromForm(Name = "awd_masst")]
    public required int AwardMostAssists { get; set; }

    [FromForm(Name = "awd_ledth")]
    public required int AwardLeastDeaths { get; set; }

    [FromForm(Name = "awd_mbdmg")]
    public required int AwardMostBuildingDamage { get; set; }

    [FromForm(Name = "awd_mwk")]
    public required int AwardMostWardsKilled { get; set; }

    [FromForm(Name = "awd_mhdd")]
    public required int AwardMostHeroDamageDealt { get; set; }

    [FromForm(Name = "awd_hcs")]
    public required int AwardHighestCreepScore { get; set; }

    [FromForm(Name = "submission_debug")]
    public required string SubmissionDebug { get; set; }
}

// Properties Common To All Game Modes
public partial class IndividualPlayerStats
{
    [FromForm(Name = "nickname")]
    public required string AccountName { get; set; }

    [FromForm(Name = "clan_tag")]
    public string? ClanTag { get; set; }

    [FromForm(Name = "clan_id")]
    public required int ClanID { get; set; }

    [FromForm(Name = "team")]
    public required int Team { get; set; }

    [FromForm(Name = "position")]
    public required int LobbyPosition { get; set; }

    [FromForm(Name = "group_num")]
    public required int GroupNumber { get; set; }

    [FromForm(Name = "benefit")]
    public required int Benefit { get; set; }

    [FromForm(Name = "hero_id")]
    public required uint HeroProductID { get; set; }

    [FromForm(Name = "alt_avatar_name")]
    public string? AlternativeAvatarName { get; set; }

    [FromForm(Name = "alt_avatar_pid")]
    public uint? AlternativeAvatarProductID { get; set; }

    [FromForm(Name = "ward_name")]
    public string? WardProductName { get; set; }

    [FromForm(Name = "ward_pid")]
    public uint? WardProductID { get; set; }

    [FromForm(Name = "taunt_name")]
    public string? TauntProductName { get; set; }

    [FromForm(Name = "taunt_pid")]
    public uint? TauntProductID { get; set; }

    [FromForm(Name = "announcer_name")]
    public string? AnnouncerProductName { get; set; }

    [FromForm(Name = "announcer_pid")]
    public uint? AnnouncerProductID { get; set; }

    [FromForm(Name = "courier_name")]
    public string? CourierProductName { get; set; }

    [FromForm(Name = "courier_pid")]
    public uint? CourierProductID { get; set; }

    [FromForm(Name = "account_icon_name")]
    public string? AccountIconProductName { get; set; }

    [FromForm(Name = "account_icon_pid")]
    public uint? AccountIconProductID { get; set; }

    [FromForm(Name = "chat_color_name")]
    public string? ChatColourProductName { get; set; }

    [FromForm(Name = "chat_color_pid")]
    public uint? ChatColourProductID { get; set; }

    [FromForm(Name = "wins")]
    public required int Win { get; set; }

    [FromForm(Name = "losses")]
    public required int Loss { get; set; }

    [FromForm(Name = "discos")]
    public required int Disconnected { get; set; }

    [FromForm(Name = "concedes")]
    public required int Conceded { get; set; }

    [FromForm(Name = "kicked")]
    public required int Kicked { get; set; }

    [FromForm(Name = "social_bonus")]
    public required int SocialBonus { get; set; }

    [FromForm(Name = "used_token")]
    public required int UsedToken { get; set; }

    [FromForm(Name = "concedevotes")]
    public required int ConcedeVotes { get; set; }

    [FromForm(Name = "herokills")]
    public required int HeroKills { get; set; }

    [FromForm(Name = "herodmg")]
    public required int HeroDamage { get; set; }

    [FromForm(Name = "herokillsgold")]
    public required int GoldFromHeroKills { get; set; }

    [FromForm(Name = "heroassists")]
    public required int HeroAssists { get; set; }

    [FromForm(Name = "heroexp")]
    public required int HeroExperience { get; set; }

    [FromForm(Name = "deaths")]
    public required int HeroDeaths { get; set; }

    [FromForm(Name = "buybacks")]
    public required int Buybacks { get; set; }

    [FromForm(Name = "goldlost2death")]
    public required int GoldLostToDeath { get; set; }

    [FromForm(Name = "secs_dead")]
    public required int SecondsDead { get; set; }

    [FromForm(Name = "teamcreepkills")]
    public required int TeamCreepKills { get; set; }

    [FromForm(Name = "teamcreepdmg")]
    public required int TeamCreepDamage { get; set; }

    [FromForm(Name = "teamcreepgold")]
    public required int TeamCreepGold { get; set; }

    [FromForm(Name = "teamcreepexp")]
    public required int TeamCreepExperience { get; set; }

    [FromForm(Name = "neutralcreepkills")]
    public required int NeutralCreepKills { get; set; }

    [FromForm(Name = "neutralcreepdmg")]
    public required int NeutralCreepDamage { get; set; }

    [FromForm(Name = "neutralcreepgold")]
    public required int NeutralCreepGold { get; set; }

    [FromForm(Name = "neutralcreepexp")]
    public required int NeutralCreepExperience { get; set; }

    [FromForm(Name = "bdmg")]
    public required int BuildingDamage { get; set; }

    [FromForm(Name = "razed")]
    public required int BuildingsRazed { get; set; }

    [FromForm(Name = "bdmgexp")]
    public required int ExperienceFromBuildings { get; set; }

    [FromForm(Name = "bgold")]
    public required int GoldFromBuildings { get; set; }

    [FromForm(Name = "denies")]
    public required int Denies { get; set; }

    [FromForm(Name = "exp_denied")]
    public required int ExperienceDenied { get; set; }

    [FromForm(Name = "gold")]
    public required int Gold { get; set; }

    [FromForm(Name = "gold_spent")]
    public required int GoldSpent { get; set; }

    [FromForm(Name = "exp")]
    public required int Experience { get; set; }

    [FromForm(Name = "actions")]
    public required int Actions { get; set; }

    [FromForm(Name = "secs")]
    public required int SecondsPlayed { get; set; }

    [FromForm(Name = "level")]
    public required int HeroLevel { get; set; }

    [FromForm(Name = "consumables")]
    public required int ConsumablesPurchased { get; set; }

    [FromForm(Name = "wards")]
    public required int WardsPlaced { get; set; }

    [FromForm(Name = "bloodlust")]
    public required int FirstBlood { get; set; }

    [FromForm(Name = "doublekill")]
    public required int DoubleKill { get; set; }

    [FromForm(Name = "triplekill")]
    public required int TripleKill { get; set; }

    [FromForm(Name = "quadkill")]
    public required int QuadKill { get; set; }

    [FromForm(Name = "annihilation")]
    public required int Annihilation { get; set; }

    [FromForm(Name = "ks3")]
    public required int KillStreak03 { get; set; }

    [FromForm(Name = "ks4")]
    public required int KillStreak04 { get; set; }

    [FromForm(Name = "ks5")]
    public required int KillStreak05 { get; set; }

    [FromForm(Name = "ks6")]
    public required int KillStreak06 { get; set; }

    [FromForm(Name = "ks7")]
    public required int KillStreak07 { get; set; }

    [FromForm(Name = "ks8")]
    public required int KillStreak08 { get; set; }

    [FromForm(Name = "ks9")]
    public required int KillStreak09 { get; set; }

    [FromForm(Name = "ks10")]
    public required int KillStreak10 { get; set; }

    [FromForm(Name = "ks15")]
    public required int KillStreak15 { get; set; }

    [FromForm(Name = "smackdown")]
    public required int Smackdown { get; set; }

    [FromForm(Name = "humiliation")]
    public required int Humiliation { get; set; }

    [FromForm(Name = "nemesis")]
    public required int Nemesis { get; set; }

    [FromForm(Name = "retribution")]
    public required int Retribution { get; set; }

    [FromForm(Name = "score")]
    public required int Score { get; set; }

    [FromForm(Name = "gameplaystat0")]
    public required double GameplayStat0 { get; set; }

    [FromForm(Name = "gameplaystat1")]
    public required double GameplayStat1 { get; set; }

    [FromForm(Name = "gameplaystat2")]
    public required double GameplayStat2 { get; set; }

    [FromForm(Name = "gameplaystat3")]
    public required double GameplayStat3 { get; set; }

    [FromForm(Name = "gameplaystat4")]
    public required double GameplayStat4 { get; set; }

    [FromForm(Name = "gameplaystat5")]
    public required double GameplayStat5 { get; set; }

    [FromForm(Name = "gameplaystat6")]
    public required double GameplayStat6 { get; set; }

    [FromForm(Name = "gameplaystat7")]
    public required double GameplayStat7 { get; set; }

    [FromForm(Name = "gameplaystat8")]
    public required double GameplayStat8 { get; set; }

    [FromForm(Name = "gameplaystat9")]
    public required double GameplayStat9 { get; set; }

    [FromForm(Name = "time_earning_exp")]
    public required int TimeEarningExperience { get; set; }
}

// Properties Specific To Public Matches
public partial class IndividualPlayerStats
{
    [FromForm(Name = "pub_skill")]
    public double PublicSkillRatingChange { get; set; }

    [FromForm(Name = "pub_count")]
    public int PublicMatch { get; set; }
}

// Properties Specific To Ranked (Arranged Matchmaking) Matches
public partial class IndividualPlayerStats
{
    [FromForm(Name = "amm_team_rating")]
    public double RankedSkillRatingChange { get; set; }

    [FromForm(Name = "amm_team_count")]
    public int RankedMatch { get; set; }

    [FromForm(Name = "achievement_data")]
    public string? AchievementData { get; set; }
}

public static class StatsForSubmissionRequestFormExtensions
{
    public static MatchStatistics ToMatchStatistics(this StatsForSubmissionRequestForm form, int? matchServerID = null, string? hostAccountName = null)
    {
        MatchStatistics statistics = new()
        {
            // A Stats Re-Submission Request Form Contains The Match Server ID While A Stats Submission Request Form Does Not
            ServerID = form.ServerID ?? (matchServerID ?? throw new NullReferenceException("Server ID Is NULL")),

            // A Stats Re-Submission Request Form Contains The Host Account Name While A Stats Submission Request Form Does Not
            HostAccountName = form.HostAccountName ?? (hostAccountName ?? throw new NullReferenceException("Host Account Name Is NULL")),

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
            ScheduledMatchID = form.MatchStats.TryGetValue("matchup_id", out string? matchupId) ? int.Parse(matchupId) : null,
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
            { SourceID = frag.SourceID, TargetID = frag.TargetID, GameTimeSeconds = frag.GameTimeSeconds, SupporterIDs = frag.SupporterIDs }).ToList()
        };

        return statistics;
    }

    public static PlayerEntity ToPlayerStatistics(this StatsForSubmissionRequestForm form, int playerIndex, int accountID, string accountName, int? clanID, string? clanTag)
    {
        string hero = form.PlayerStats[playerIndex].Keys.Single();

        Dictionary<string, string> player = form.PlayerStats[playerIndex][hero];

        PlayerEntity statistics = new()
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
            HeroProductID = uint.Parse(player["hero_id"]) == uint.MaxValue ? uint.MinValue : uint.Parse(player["hero_id"]),
            AlternativeAvatarName = player.TryGetValue("alt_avatar_name", out string? altAvatarName) ? altAvatarName : null,
            AlternativeAvatarProductID = player.TryGetValue("alt_avatar_pid", out string? altAvatarPid) && uint.TryParse(altAvatarPid, out uint pid) && pid != uint.MaxValue ? pid : null,
            WardProductName = player.TryGetValue("ward_name", out string? wardName) ? wardName : null,
            WardProductID = player.TryGetValue("ward_pid", out string? wardPid) && uint.TryParse(wardPid, out uint wpid) && wpid != uint.MaxValue ? wpid : null,
            TauntProductName = player.TryGetValue("taunt_name", out string? tauntName) ? tauntName : null,
            TauntProductID = player.TryGetValue("taunt_pid", out string? tauntPid) && uint.TryParse(tauntPid, out uint tpid) && tpid != uint.MaxValue ? tpid : null,
            AnnouncerProductName = player.TryGetValue("announcer_name", out string? announcerName) ? announcerName : null,
            AnnouncerProductID = player.TryGetValue("announcer_pid", out string? announcerPid) && uint.TryParse(announcerPid, out uint apid) && apid != uint.MaxValue ? apid : null,
            CourierProductName = player.TryGetValue("courier_name", out string? courierName) ? courierName : null,
            CourierProductID = player.TryGetValue("courier_pid", out string? courierPid) && uint.TryParse(courierPid, out uint cpid) && cpid != uint.MaxValue ? cpid : null,
            AccountIconProductName = player.TryGetValue("account_icon_name", out string? iconName) ? iconName : null,
            AccountIconProductID = player.TryGetValue("account_icon_pid", out string? iconPid) && uint.TryParse(iconPid, out uint ipid) && ipid != uint.MaxValue ? ipid : null,
            ChatColourProductName = player.TryGetValue("chat_color_name", out string? chatName) ? chatName : null,
            ChatColourProductID = player.TryGetValue("chat_color_pid", out string? chatPid) && uint.TryParse(chatPid, out uint chpid) && chpid != uint.MaxValue ? chpid : null,
            Inventory = form.PlayerInventory is not null && form.PlayerInventory.TryGetValue(playerIndex, out Dictionary<int, string>? inventory)
                ? [.. inventory.Values]
                : [],
            Win = int.Parse(player["wins"]),
            Loss = int.Parse(player["losses"]),
            Disconnected = int.Parse(player["discos"]),
            Conceded = int.Parse(player["concedes"]),
            Kicked = int.Parse(player["kicked"]),
            PublicMatch = player.TryGetValue("pub_count", out string? pubCount) ? int.Parse(pubCount) : 0,
            PublicSkillRatingChange = player.TryGetValue("pub_skill", out string? pubSkill) ? double.Parse(pubSkill) : 0,
            RankedMatch = player.TryGetValue("amm_team_count", out string? rankCount) ? int.Parse(rankCount) : 0,
            RankedSkillRatingChange = player.TryGetValue("amm_team_rating", out string? rankSkill) ? double.Parse(rankSkill) : 0,
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

            ItemHistory = form.ItemHistory?.Where(item => item.AccountID == accountID).Select(item => new MERRICK.DatabaseContext.Entities.Statistics.ItemEvent
            { ItemName = item.ItemName, GameTimeSeconds = item.GameTimeSeconds, EventType = item.EventType }).ToList(),

            AbilityHistory = form.AbilityHistory is not null && form.AbilityHistory.TryGetValue(accountID, out List<AbilityEvent>? abilities)
                ? [.. abilities.Select(ability => new MERRICK.DatabaseContext.Entities.Statistics.AbilityEvent { HeroName = ability.HeroName, AbilityName = ability.AbilityName, GameTimeSeconds = ability.GameTimeSeconds, SlotIndex = ability.SlotIndex })]
                : null
        };

        return statistics;
    }
}

public class ItemEvent
{
    [FromForm(Name = "account_id")]
    public required int AccountID { get; set; }

    [FromForm(Name = "cli_name")]
    public required string ItemName { get; set; }

    [FromForm(Name = "secs")]
    public required int GameTimeSeconds { get; set; }

    [FromForm(Name = "action")]
    public required byte EventType { get; set; }
}

public class AbilityEvent
{
    [FromForm(Name = "hero_cli_name")]
    public required string HeroName { get; set; }

    [FromForm(Name = "ability_cli_name")]
    public required string AbilityName { get; set; }

    [FromForm(Name = "secs")]
    public required int GameTimeSeconds { get; set; }

    [FromForm(Name = "slot")]
    public required byte SlotIndex { get; set; }
}

public class FragEvent
{
    [FromForm(Name = "killer_id")]
    public required int SourceID { get; set; }

    [FromForm(Name = "target_id")]
    public required int TargetID { get; set; }

    [FromForm(Name = "secs")]
    public required int GameTimeSeconds { get; set; }

    [FromForm(Name = "assisters")]
    public required List<int>? SupporterIDs { get; set; }
}
