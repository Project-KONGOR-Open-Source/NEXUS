namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

public class StatsForSubmissionRequestForm
{
    public MatchResults match_stats { get; set; } = new();

    public Dictionary<int, Dictionary<string, int>> team_stats { get; set; } = new();

    public Dictionary<int, Dictionary<string, PlayerMatchResults>> player_stats { get; set; } = new();

    public Dictionary<int, Dictionary<int, string>> inventory { get; set; } = new();
}

// TODO: Make These Classes Entities

// [Index(nameof(match_id), IsUnique = true)]
public class MatchResults
{
    public int match_id { get; set; }
    public long server_id { get; set; }
    public string? map { get; set; }
    public string? map_version { get; set; }
    public int time_played { get; set; }
    public int file_size { get; set; }
    public string? file_name { get; set; }
    public int c_state { get; set; }
    public string? version { get; set; }
    public int avgpsr { get; set; }
    public int avgpsr_team1 { get; set; }
    public int avgpsr_team2 { get; set; }
    public string? gamemode { get; set; }
    public int teamscoregoal { get; set; }
    public int playerscoregoal { get; set; }
    public int numrounds { get; set; }
    public string? release_stage { get; set; }
    public string? banned_heroes { get; set; }
    public int awd_mann { get; set; }
    public int awd_mqk { get; set; }
    public int awd_lgks { get; set; }
    public int awd_msd { get; set; }
    public int awd_mkill { get; set; }
    public int awd_masst { get; set; }
    public int awd_ledth { get; set; }
    public int awd_mbdmg { get; set; }
    public int awd_mwk { get; set; }
    public int awd_mhdd { get; set; }
    public int awd_hcs { get; set; }
    public int mvp { get; set; }
    public string? submission_debug { get; set; }

    // Additional field that are not set by the game server and populated manually.
    public string? name { get; set; } // server name
    public string? date { get; set; } // DEPRECATED -> TODO: convert this to a [NotMapped] string generated from "datetime"
    public DateTime datetime { get; set; }
    public string? time { get; set; } // time in seconds since the day started.
    public long timestamp { get; set; }  // unix timestamp in seconds since epoch
    public string? inventory { get; set; }
}

// [Index(nameof(match_id), IsUnique = false)]
public class PlayerMatchResults
{
    public PlayerMatchResults()
    {
    }

    public PlayerMatchResults(List<PlayerMatchResults> other)
    {
        wins = other.Sum(r => r.wins);
        losses = other.Sum(r => r.losses);
        discos = other.Sum(r => r.discos);
        concedes = other.Sum(r => r.concedes);
        kicked = other.Sum(r => r.kicked);
        social_bonus = other.Sum(r => r.social_bonus);
        used_token = other.Sum(r => r.used_token);
        pub_skill = other.Sum(r => r.pub_skill);
        pub_count = other.Sum(r => r.pub_count);
        amm_team_rating = other.Sum(r => r.amm_team_rating);
        amm_team_count = other.Sum(r => r.amm_team_count);
        concedevotes = other.Sum(r => r.concedevotes);
        herokills = other.Sum(r => r.herokills);
        herodmg = other.Sum(r => r.herodmg);
        herokillsgold = other.Sum(r => r.herokillsgold);
        heroassists = other.Sum(r => r.heroassists);
        heroexp = other.Sum(r => r.heroexp);
        deaths = other.Sum(r => r.deaths);
        buybacks = other.Sum(r => r.buybacks);
        goldlost2death = other.Sum(r => r.goldlost2death);
        secs_dead = other.Sum(r => r.secs_dead);
        teamcreepkills = other.Sum(r => r.teamcreepkills);
        teamcreepdmg = other.Sum(r => r.teamcreepdmg);
        teamcreepgold = other.Sum(r => r.teamcreepgold);
        teamcreepexp = other.Sum(r => r.teamcreepexp);
        neutralcreepkills = other.Sum(r => r.neutralcreepkills);
        neutralcreepdmg = other.Sum(r => r.neutralcreepdmg);
        neutralcreepgold = other.Sum(r => r.neutralcreepgold);
        neutralcreepexp = other.Sum(r => r.neutralcreepexp);
        bdmg = other.Sum(r => r.bdmg);
        razed = other.Sum(r => r.razed);
        bdmgexp = other.Sum(r => r.bdmgexp);
        bgold = other.Sum(r => r.bgold);
        denies = other.Sum(r => r.denies);
        exp_denied = other.Sum(r => r.exp_denied);
        gold = other.Sum(r => r.gold);
        gold_spent = other.Sum(r => r.gold_spent);
        exp = other.Sum(r => r.exp);
        actions = other.Sum(r => r.actions);
        secs = other.Sum(r => r.secs);
        level = other.Sum(r => r.level);
        consumables = other.Sum(r => r.consumables);
        wards = other.Sum(r => r.wards);
        bloodlust = other.Sum(r => r.bloodlust);
        doublekill = other.Sum(r => r.doublekill);
        triplekill = other.Sum(r => r.triplekill);
        quadkill = other.Sum(r => r.quadkill);
        annihilation = other.Sum(r => r.annihilation);
        ks3 = other.Sum(r => r.ks3);
        ks4 = other.Sum(r => r.ks4);
        ks5 = other.Sum(r => r.ks5);
        ks6 = other.Sum(r => r.ks6);
        ks7 = other.Sum(r => r.ks7);
        ks8 = other.Sum(r => r.ks8);
        ks9 = other.Sum(r => r.ks9);
        ks10 = other.Sum(r => r.ks10);
        ks15 = other.Sum(r => r.ks15);
        smackdown = other.Sum(r => r.smackdown);
        humiliation = other.Sum(r => r.humiliation);
        nemesis = other.Sum(r => r.nemesis);
        retribution = other.Sum(r => r.retribution);
        score = other.Sum(r => r.score);
        gameplaystat0 = other.Sum(r => r.gameplaystat0);
        gameplaystat1 = other.Sum(r => r.gameplaystat1);
        gameplaystat2 = other.Sum(r => r.gameplaystat2);
        gameplaystat3 = other.Sum(r => r.gameplaystat3);
        gameplaystat4 = other.Sum(r => r.gameplaystat4);
        gameplaystat5 = other.Sum(r => r.gameplaystat5);
        gameplaystat6 = other.Sum(r => r.gameplaystat6);
        gameplaystat7 = other.Sum(r => r.gameplaystat7);
        gameplaystat8 = other.Sum(r => r.gameplaystat8);
        gameplaystat9 = other.Sum(r => r.gameplaystat9);
        time_earning_exp = other.Sum(r => r.time_earning_exp);

        if (other.Count == 0)
        {
            // Should not happen.
            map = null;
        }

        else
        {
            map = other[0].map;
        }
    }

    public string nickname { get; set; }
    public string? clan_tag { get; set; }
    public int clan_id { get; set; }
    public int team { get; set; }
    public int position { get; set; }
    public int group_num { get; set; }
    public int benefit { get; set; }
    public long hero_id { get; set; }
    public int wins { get; set; }
    public int losses { get; set; }
    public int discos { get; set; }
    public int concedes { get; set; }
    public int kicked { get; set; }
    public int social_bonus { get; set; }
    public int used_token { get; set; }
    public double pub_skill { get; set; }
    public int pub_count { get; set; }
    public double amm_team_rating { get; set; }
    public int amm_team_count { get; set; }
    public int concedevotes { get; set; }
    public int herokills { get; set; }
    public int herodmg { get; set; }
    public int herokillsgold { get; set; }
    public int heroassists { get; set; }
    public int heroexp { get; set; }
    public int deaths { get; set; }
    public int buybacks { get; set; }
    public int goldlost2death { get; set; }
    public int secs_dead { get; set; }
    public int teamcreepkills { get; set; }
    public int teamcreepdmg { get; set; }
    public int teamcreepgold { get; set; }
    public int teamcreepexp { get; set; }
    public int neutralcreepkills { get; set; }
    public int neutralcreepdmg { get; set; }
    public int neutralcreepgold { get; set; }
    public int neutralcreepexp { get; set; }
    public int bdmg { get; set; }
    public int razed { get; set; }
    public int bdmgexp { get; set; }
    public int bgold { get; set; }
    public int denies { get; set; }
    public int exp_denied { get; set; }
    public int gold { get; set; }
    public int gold_spent { get; set; }
    public int exp { get; set; }
    public int actions { get; set; }
    public int secs { get; set; }
    public int level { get; set; }
    public int consumables { get; set; }
    public int wards { get; set; }
    public int bloodlust { get; set; }
    public int doublekill { get; set; }
    public int triplekill { get; set; }
    public int quadkill { get; set; }
    public int annihilation { get; set; }
    public int ks3 { get; set; }
    public int ks4 { get; set; }
    public int ks5 { get; set; }
    public int ks6 { get; set; }
    public int ks7 { get; set; }
    public int ks8 { get; set; }
    public int ks9 { get; set; }
    public int ks10 { get; set; }
    public int ks15 { get; set; }
    public int smackdown { get; set; }
    public int humiliation { get; set; }
    public int nemesis { get; set; }
    public int retribution { get; set; }
    public int score { get; set; }
    public double gameplaystat0 { get; set; }
    public double gameplaystat1 { get; set; }
    public double gameplaystat2 { get; set; }
    public double gameplaystat3 { get; set; }
    public double gameplaystat4 { get; set; }
    public double gameplaystat5 { get; set; }
    public double gameplaystat6 { get; set; }
    public double gameplaystat7 { get; set; }
    public double gameplaystat8 { get; set; }
    public double gameplaystat9 { get; set; }
    public int time_earning_exp { get; set; }

    // Extra fields that we populate manually.
    public int match_id { get; set; }
    public int? account_id { get; set; }
    public string? map { get; set; }
    public string? cli_name { get; set; }
    public string? mdt { get; set; } // DEPRECATED -> TODO: convert this to a [NotMapped] string generated from "datetime"
    public DateTime datetime { get; set; }
}
