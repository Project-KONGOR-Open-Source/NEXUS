namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

// TODO: The Following May Be Needed: AverageMMR, AverageMMRTeamOne, AverageMMRTeamTwo, etc.

// Properties Common To Both "submit_stats" And "resubmit_stats" Requests
public partial class StatsForSubmissionRequestForm
{
    [FromForm(Name = "f")]
    public required string Function { get; set; }

    [FromForm(Name = "match_stats")]
    public required MatchStats MatchStats { get; set; }

    [FromForm(Name = "team_stats")]
    public required Dictionary<int, Dictionary<string, int>> TeamStats { get; set; }

    [FromForm(Name = "player_stats")]
    public required Dictionary<int, Dictionary<string, IndividualPlayerStats>> PlayerStats { get; set; }

    [FromForm(Name = "inventory")]
    public required Dictionary<int, Dictionary<int, string>> PlayerInventory { get; set; }
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

    // TODO: MMR And Casual MMR May Need To Also Be Added Here

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
    public required uint ProductID { get; set; }

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
    public required int ConsumablesUsed { get; set; }

    [FromForm(Name = "wards")]
    public required int WardsPlaced { get; set; }

    [FromForm(Name = "bloodlust")]
    public required int Bloodlust { get; set; }

    [FromForm(Name = "doublekill")]
    public required int DoubleKill { get; set; }

    [FromForm(Name = "triplekill")]
    public required int TripleKill { get; set; }

    [FromForm(Name = "quadkill")]
    public required int QuadKill { get; set; }

    [FromForm(Name = "annihilation")]
    public required int Annihilation { get; set; }

    [FromForm(Name = "ks3")]
    public required int KillStreak3 { get; set; }

    [FromForm(Name = "ks4")]
    public required int KillStreak4 { get; set; }

    [FromForm(Name = "ks5")]
    public required int KillStreak5 { get; set; }

    [FromForm(Name = "ks6")]
    public required int KillStreak6 { get; set; }

    [FromForm(Name = "ks7")]
    public required int KillStreak7 { get; set; }

    [FromForm(Name = "ks8")]
    public required int KillStreak8 { get; set; }

    [FromForm(Name = "ks9")]
    public required int KillStreak9 { get; set; }

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

// Properties Specific To Ranked (Arranged Matchmaking) Solo Matches
public partial class IndividualPlayerStats
{
    [FromForm(Name = "amm_solo_rating")]
    public double SoloRankedSkillRatingChange { get; set; }

    [FromForm(Name = "amm_solo_count")]
    public int SoloRankedMatch { get; set; }
}

// Properties Specific To Ranked (Arranged Matchmaking) Team Matches
public partial class IndividualPlayerStats
{
    [FromForm(Name = "amm_team_rating")]
    public double TeamRankedSkillRatingChange { get; set; }

    [FromForm(Name = "amm_team_count")]
    public int TeamRankedMatch { get; set; }
}

public static class StatsForSubmissionRequestFormExtensions
{
    public static MatchStatistics ToMatchStatistics(this StatsForSubmissionRequestForm form, int? matchServerID = null, string? hostAccountName = null)
    {
        MatchStatistics statistics = new ()
        {
            // A Stats Re-Submission Request Form Contains The Match Server ID While A Stats Submission Request Form Does Not
            ServerID = form.ServerID ?? (matchServerID ?? throw new NullReferenceException("Server ID Is NULL")),

            // A Stats Re-Submission Request Form Contains The Host Account Name While A Stats Submission Request Form Does Not
            HostAccountName = form.HostAccountName ?? (hostAccountName ?? throw new NullReferenceException("Host Account Name Is NULL")),

            MatchID = form.MatchStats.MatchID,
            Map = form.MatchStats.Map,
            MapVersion = form.MatchStats.MapVersion,
            TimePlayed = form.MatchStats.TimePlayed,
            FileSize = form.MatchStats.FileSize,
            FileName = form.MatchStats.FileName,
            ConnectionState = form.MatchStats.ConnectionState,
            Version = form.MatchStats.Version,
            AveragePSR = form.MatchStats.AveragePSR,
            AveragePSRTeamOne = form.MatchStats.AveragePSRTeamOne,
            AveragePSRTeamTwo = form.MatchStats.AveragePSRTeamTwo,
            GameMode = form.MatchStats.GameMode,
            ScoreTeam1 = form.TeamStats.First().Value.Single().Value,
            ScoreTeam2 = form.TeamStats.Last().Value.Single().Value,
            TeamScoreGoal = form.MatchStats.TeamScoreGoal,
            PlayerScoreGoal = form.MatchStats.PlayerScoreGoal,
            NumberOfRounds = form.MatchStats.NumberOfRounds,
            ReleaseStage = form.MatchStats.ReleaseStage,
            BannedHeroes = form.MatchStats.BannedHeroes,
            AwardMostAnnihilations = form.MatchStats.AwardMostAnnihilations,
            AwardMostQuadKills = form.MatchStats.AwardMostQuadKills,
            AwardLargestKillStreak = form.MatchStats.AwardLargestKillStreak,
            AwardMostSmackdowns = form.MatchStats.AwardMostSmackdowns,
            AwardMostKills = form.MatchStats.AwardMostKills,
            AwardMostAssists = form.MatchStats.AwardMostAssists,
            AwardLeastDeaths = form.MatchStats.AwardLeastDeaths,
            AwardMostBuildingDamage = form.MatchStats.AwardMostBuildingDamage,
            AwardMostWardsKilled = form.MatchStats.AwardMostWardsKilled,
            AwardMostHeroDamageDealt = form.MatchStats.AwardMostHeroDamageDealt,
            AwardHighestCreepScore = form.MatchStats.AwardHighestCreepScore
        };

        return statistics;
    }

    public static PlayerStatistics ToPlayerStatistics(this StatsForSubmissionRequestForm form, int playerIndex, int accountID, string accountName, int? clanID, string? clanTag)
    {
        string hero = form.PlayerStats[playerIndex].Keys.Single();

        IndividualPlayerStats player = form.PlayerStats[playerIndex][hero];

        PlayerStatistics statistics = new ()
        {
            MatchID = form.MatchStats.MatchID,
            AccountID = accountID,
            AccountName = accountName,
            ClanID = clanID,
            ClanTag = clanTag,
            Team = player.Team,
            LobbyPosition = player.LobbyPosition,
            GroupNumber = player.GroupNumber,
            Benefit = player.Benefit,
            ProductID = player.ProductID == uint.MaxValue ? uint.MinValue : player.ProductID,
            Inventory = form.PlayerInventory[playerIndex].Values.ToList(),
            Win = player.Win,
            Loss = player.Loss,
            Disconnected = player.Disconnected,
            Conceded = player.Conceded,
            Kicked = player.Kicked,
            PublicMatch = player.PublicMatch,
            PublicSkillRatingChange = player.PublicSkillRatingChange,
            SoloRankedMatch = player.SoloRankedMatch,
            SoloRankedSkillRatingChange = player.SoloRankedSkillRatingChange,
            TeamRankedMatch = player.TeamRankedMatch,
            TeamRankedSkillRatingChange = player.TeamRankedSkillRatingChange,
            SocialBonus = player.SocialBonus,
            UsedToken = player.UsedToken,
            ConcedeVotes = player.ConcedeVotes,
            HeroKills = player.HeroKills,
            HeroDamage = player.HeroDamage,
            GoldFromHeroKills = player.GoldFromHeroKills,
            HeroAssists = player.HeroAssists,
            HeroExperience = player.HeroExperience,
            HeroDeaths = player.HeroDeaths,
            Buybacks = player.Buybacks,
            GoldLostToDeath = player.GoldLostToDeath,
            SecondsDead = player.SecondsDead,
            TeamCreepKills = player.TeamCreepKills,
            TeamCreepDamage = player.TeamCreepDamage,
            TeamCreepGold = player.TeamCreepGold,
            TeamCreepExperience = player.TeamCreepExperience,
            NeutralCreepKills = player.NeutralCreepKills,
            NeutralCreepDamage = player.NeutralCreepDamage,
            NeutralCreepGold = player.NeutralCreepGold,
            NeutralCreepExperience = player.NeutralCreepExperience,
            BuildingDamage = player.BuildingDamage,
            BuildingsRazed = player.BuildingsRazed,
            ExperienceFromBuildings = player.ExperienceFromBuildings,
            GoldFromBuildings = player.GoldFromBuildings,
            Denies = player.Denies,
            ExperienceDenied = player.ExperienceDenied,
            Gold = player.Gold,
            GoldSpent = player.GoldSpent,
            Experience = player.Experience,
            Actions = player.Actions,
            SecondsPlayed = player.SecondsPlayed,
            HeroLevel = player.HeroLevel,
            ConsumablesUsed = player.ConsumablesUsed,
            WardsPlaced = player.WardsPlaced,
            Bloodlust = player.Bloodlust,
            DoubleKill = player.DoubleKill,
            TripleKill = player.TripleKill,
            QuadKill = player.QuadKill,
            Annihilation = player.Annihilation,
            KillStreak3 = player.KillStreak3,
            KillStreak4 = player.KillStreak4,
            KillStreak5 = player.KillStreak5,
            KillStreak6 = player.KillStreak6,
            KillStreak7 = player.KillStreak7,
            KillStreak8 = player.KillStreak8,
            KillStreak9 = player.KillStreak9,
            KillStreak10 = player.KillStreak10,
            KillStreak15 = player.KillStreak15,
            Smackdown = player.Smackdown,
            Humiliation = player.Humiliation,
            Nemesis = player.Nemesis,
            Retribution = player.Retribution,
            Score = player.Score,
            GameplayStat0 = player.GameplayStat0,
            GameplayStat1 = player.GameplayStat1,
            GameplayStat2 = player.GameplayStat2,
            GameplayStat3 = player.GameplayStat3,
            GameplayStat4 = player.GameplayStat4,
            GameplayStat5 = player.GameplayStat5,
            GameplayStat6 = player.GameplayStat6,
            GameplayStat7 = player.GameplayStat7,
            GameplayStat8 = player.GameplayStat8,
            GameplayStat9 = player.GameplayStat9,
            TimeEarningExperience = player.TimeEarningExperience
        };

        return statistics;
    }
}
