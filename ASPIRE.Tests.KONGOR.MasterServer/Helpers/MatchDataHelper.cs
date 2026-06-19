namespace ASPIRE.Tests.KONGOR.MasterServer.Helpers;

/// <summary>
///     Builders for match-related test fixtures (match information snapshots, match statistics, and participant statistics), shared across the test classes that need to seed matches.
/// </summary>
internal static class MatchDataHelper
{
    public static MatchInformation BuildMatchInformation(MatchType matchType, bool isCasual = false, string map = "caldavar")
    {
        return new MatchInformation
        {
            MatchID = 1,
            MatchName = "Test Match",
            ServerID = 1,
            ServerName = "Test Server",
            HostAccountName = "TestHost",
            Map = map,
            Version = "4.10.1.0",
            IsCasual = isCasual,
            MatchType = matchType,
            MatchMode = PublicMatchMode.GAME_MODE_NORMAL
        };
    }

    public static MatchParticipantStatistics BuildParticipant(int accountID, string accountName, int groupNumber, int win = 0, int loss = 0, int matchID = 1,
        int publicMatch = 1, double publicSkillRatingChange = 0, int rankedMatch = 0, double rankedSkillRatingChange = 0, int disconnected = 0)
    {
        return new MatchParticipantStatistics
        {
            MatchID                     = matchID,
            AccountID                   = accountID,
            AccountName                 = accountName,
            ClanID                      = null,
            ClanTag                     = null,
            Team                        = 1,
            LobbyPosition               = 0,
            GroupNumber                 = groupNumber,
            Benefit                     = 0,
            HeroProductID               = null,
            HeroIdentifier              = "Hero_Default",
            Inventory                   = [],
            Win                         = win,
            Loss                        = loss,
            Disconnected                = disconnected,
            Conceded                    = 0,
            Kicked                      = 0,
            PublicMatch                 = publicMatch,
            PublicSkillRatingChange     = publicSkillRatingChange,
            RankedMatch                 = rankedMatch,
            RankedSkillRatingChange     = rankedSkillRatingChange,
            SocialBonus                 = 0,
            UsedToken                   = 0,
            ConcedeVotes                = 0,
            HeroKills                   = 0,
            HeroDamage                  = 0,
            GoldFromHeroKills           = 0,
            HeroAssists                 = 0,
            HeroExperience              = 0,
            HeroDeaths                  = 0,
            Buybacks                    = 0,
            GoldLostToDeath             = 0,
            SecondsDead                 = 0,
            TeamCreepKills              = 0,
            TeamCreepDamage             = 0,
            TeamCreepGold               = 0,
            TeamCreepExperience         = 0,
            NeutralCreepKills           = 0,
            NeutralCreepDamage          = 0,
            NeutralCreepGold            = 0,
            NeutralCreepExperience      = 0,
            BuildingDamage              = 0,
            BuildingsRazed              = 0,
            ExperienceFromBuildings     = 0,
            GoldFromBuildings           = 0,
            Denies                      = 0,
            ExperienceDenied            = 0,
            Gold                        = 0,
            GoldSpent                   = 0,
            Experience                  = 0,
            Actions                     = 0,
            SecondsPlayed               = 0,
            HeroLevel                   = 0,
            ConsumablesPurchased        = 0,
            WardsPlaced                 = 0,
            FirstBlood                  = 0,
            DoubleKill                  = 0,
            TripleKill                  = 0,
            QuadKill                    = 0,
            Annihilation                = 0,
            KillStreak03                = 0,
            KillStreak04                = 0,
            KillStreak05                = 0,
            KillStreak06                = 0,
            KillStreak07                = 0,
            KillStreak08                = 0,
            KillStreak09                = 0,
            KillStreak10                = 0,
            KillStreak15                = 0,
            Smackdown                   = 0,
            Humiliation                 = 0,
            Nemesis                     = 0,
            Retribution                 = 0,
            Score                       = 0,
            GameplayStat0               = 0,
            GameplayStat1               = 0,
            GameplayStat2               = 0,
            GameplayStat3               = 0,
            GameplayStat4               = 0,
            GameplayStat5               = 0,
            GameplayStat6               = 0,
            GameplayStat7               = 0,
            GameplayStat8               = 0,
            GameplayStat9               = 0,
            TimeEarningExperience       = 0
        };
    }

    public static MatchStatistics BuildMatchStatistics(int matchID = 1, string map = "caldavar")
    {
        return new MatchStatistics
        {
            ServerID                 = 1,
            HostAccountName          = "TestHost",
            MatchID                  = matchID,
            Map                      = map,
            MapVersion               = "0.0.0",
            TimePlayed               = 0,
            FileSize                 = 0,
            FileName                 = "test.honreplay",
            ConnectionState          = 0,
            Version                  = "4.10.1.0",
            AveragePSR               = 1500,
            AveragePSRTeamOne        = 1500,
            AveragePSRTeamTwo        = 1500,
            GameMode                 = "normal",
            ScoreTeam1               = 0,
            ScoreTeam2               = 0,
            TeamScoreGoal            = 0,
            PlayerScoreGoal          = 0,
            NumberOfRounds           = 0,
            ReleaseStage             = "live",
            BannedHeroes             = null,
            AwardMostAnnihilations   = -1,
            AwardMostQuadKills       = -1,
            AwardLargestKillStreak   = -1,
            AwardMostSmackdowns      = -1,
            AwardMostKills           = -1,
            AwardMostAssists         = -1,
            AwardLeastDeaths         = -1,
            AwardMostBuildingDamage  = -1,
            AwardMostWardsKilled     = -1,
            AwardMostHeroDamageDealt = -1,
            AwardHighestCreepScore   = -1
        };
    }
}
