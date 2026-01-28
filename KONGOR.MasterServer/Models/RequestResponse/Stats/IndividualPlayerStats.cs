namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

public partial class IndividualPlayerStats
{
    [FromForm(Name = "nickname")] public required string AccountName { get; set; }

    [FromForm(Name = "clan_tag")] public string? ClanTag { get; set; }

    [FromForm(Name = "clan_id")] public required int ClanID { get; set; }

    [FromForm(Name = "team")] public required int Team { get; set; }

    [FromForm(Name = "position")] public required int LobbyPosition { get; set; }

    [FromForm(Name = "group_num")] public required int GroupNumber { get; set; }

    [FromForm(Name = "benefit")] public required int Benefit { get; set; }

    [FromForm(Name = "hero_id")] public required uint HeroProductID { get; set; }

    [FromForm(Name = "alt_avatar_name")] public string? AlternativeAvatarName { get; set; }

    [FromForm(Name = "alt_avatar_pid")] public uint? AlternativeAvatarProductID { get; set; }

    [FromForm(Name = "ward_name")] public string? WardProductName { get; set; }

    [FromForm(Name = "ward_pid")] public uint? WardProductID { get; set; }

    [FromForm(Name = "taunt_name")] public string? TauntProductName { get; set; }

    [FromForm(Name = "taunt_pid")] public uint? TauntProductID { get; set; }

    [FromForm(Name = "announcer_name")] public string? AnnouncerProductName { get; set; }

    [FromForm(Name = "announcer_pid")] public uint? AnnouncerProductID { get; set; }

    [FromForm(Name = "courier_name")] public string? CourierProductName { get; set; }

    [FromForm(Name = "courier_pid")] public uint? CourierProductID { get; set; }

    [FromForm(Name = "account_icon_name")] public string? AccountIconProductName { get; set; }

    [FromForm(Name = "account_icon_pid")] public uint? AccountIconProductID { get; set; }

    [FromForm(Name = "chat_color_name")] public string? ChatColourProductName { get; set; }

    [FromForm(Name = "chat_color_pid")] public uint? ChatColourProductID { get; set; }

    [FromForm(Name = "wins")] public required int Win { get; set; }

    [FromForm(Name = "losses")] public required int Loss { get; set; }

    [FromForm(Name = "discos")] public required int Disconnected { get; set; }

    [FromForm(Name = "concedes")] public required int Conceded { get; set; }

    [FromForm(Name = "kicked")] public required int Kicked { get; set; }

    [FromForm(Name = "social_bonus")] public required int SocialBonus { get; set; }

    [FromForm(Name = "used_token")] public required int UsedToken { get; set; }

    [FromForm(Name = "concedevotes")] public required int ConcedeVotes { get; set; }

    [FromForm(Name = "herokills")] public required int HeroKills { get; set; }

    [FromForm(Name = "herodmg")] public required int HeroDamage { get; set; }

    [FromForm(Name = "herokillsgold")] public required int GoldFromHeroKills { get; set; }

    [FromForm(Name = "heroassists")] public required int HeroAssists { get; set; }

    [FromForm(Name = "heroexp")] public required int HeroExperience { get; set; }

    [FromForm(Name = "deaths")] public required int HeroDeaths { get; set; }

    [FromForm(Name = "buybacks")] public required int Buybacks { get; set; }

    [FromForm(Name = "goldlost2death")] public required int GoldLostToDeath { get; set; }

    [FromForm(Name = "secs_dead")] public required int SecondsDead { get; set; }

    [FromForm(Name = "teamcreepkills")] public required int TeamCreepKills { get; set; }

    [FromForm(Name = "teamcreepdmg")] public required int TeamCreepDamage { get; set; }

    [FromForm(Name = "teamcreepgold")] public required int TeamCreepGold { get; set; }

    [FromForm(Name = "teamcreepexp")] public required int TeamCreepExperience { get; set; }

    [FromForm(Name = "neutralcreepkills")] public required int NeutralCreepKills { get; set; }

    [FromForm(Name = "neutralcreepdmg")] public required int NeutralCreepDamage { get; set; }

    [FromForm(Name = "neutralcreepgold")] public required int NeutralCreepGold { get; set; }

    [FromForm(Name = "neutralcreepexp")] public required int NeutralCreepExperience { get; set; }

    [FromForm(Name = "bdmg")] public required int BuildingDamage { get; set; }

    [FromForm(Name = "razed")] public required int BuildingsRazed { get; set; }

    [FromForm(Name = "bdmgexp")] public required int ExperienceFromBuildings { get; set; }

    [FromForm(Name = "bgold")] public required int GoldFromBuildings { get; set; }

    [FromForm(Name = "denies")] public required int Denies { get; set; }

    [FromForm(Name = "exp_denied")] public required int ExperienceDenied { get; set; }

    [FromForm(Name = "gold")] public required int Gold { get; set; }

    [FromForm(Name = "gold_spent")] public required int GoldSpent { get; set; }

    [FromForm(Name = "exp")] public required int Experience { get; set; }

    [FromForm(Name = "actions")] public required int Actions { get; set; }

    [FromForm(Name = "secs")] public required int SecondsPlayed { get; set; }

    [FromForm(Name = "level")] public required int HeroLevel { get; set; }

    [FromForm(Name = "consumables")] public required int ConsumablesPurchased { get; set; }

    [FromForm(Name = "wards")] public required int WardsPlaced { get; set; }

    [FromForm(Name = "bloodlust")] public required int FirstBlood { get; set; }

    [FromForm(Name = "doublekill")] public required int DoubleKill { get; set; }

    [FromForm(Name = "triplekill")] public required int TripleKill { get; set; }

    [FromForm(Name = "quadkill")] public required int QuadKill { get; set; }

    [FromForm(Name = "annihilation")] public required int Annihilation { get; set; }

    [FromForm(Name = "ks3")] public required int KillStreak03 { get; set; }

    [FromForm(Name = "ks4")] public required int KillStreak04 { get; set; }

    [FromForm(Name = "ks5")] public required int KillStreak05 { get; set; }

    [FromForm(Name = "ks6")] public required int KillStreak06 { get; set; }

    [FromForm(Name = "ks7")] public required int KillStreak07 { get; set; }

    [FromForm(Name = "ks8")] public required int KillStreak08 { get; set; }

    [FromForm(Name = "ks9")] public required int KillStreak09 { get; set; }

    [FromForm(Name = "ks10")] public required int KillStreak10 { get; set; }

    [FromForm(Name = "ks15")] public required int KillStreak15 { get; set; }

    [FromForm(Name = "smackdown")] public required int Smackdown { get; set; }

    [FromForm(Name = "humiliation")] public required int Humiliation { get; set; }

    [FromForm(Name = "nemesis")] public required int Nemesis { get; set; }

    [FromForm(Name = "retribution")] public required int Retribution { get; set; }

    [FromForm(Name = "score")] public required int Score { get; set; }

    [FromForm(Name = "gameplaystat0")] public required double GameplayStat0 { get; set; }

    [FromForm(Name = "gameplaystat1")] public required double GameplayStat1 { get; set; }

    [FromForm(Name = "gameplaystat2")] public required double GameplayStat2 { get; set; }

    [FromForm(Name = "gameplaystat3")] public required double GameplayStat3 { get; set; }

    [FromForm(Name = "gameplaystat4")] public required double GameplayStat4 { get; set; }

    [FromForm(Name = "gameplaystat5")] public required double GameplayStat5 { get; set; }

    [FromForm(Name = "gameplaystat6")] public required double GameplayStat6 { get; set; }

    [FromForm(Name = "gameplaystat7")] public required double GameplayStat7 { get; set; }

    [FromForm(Name = "gameplaystat8")] public required double GameplayStat8 { get; set; }

    [FromForm(Name = "gameplaystat9")] public required double GameplayStat9 { get; set; }

    [FromForm(Name = "time_earning_exp")] public required int TimeEarningExperience { get; set; }
}
