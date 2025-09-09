namespace KONGOR.MasterServer.Models.RequestResponse.GameData;

public class GuideResponseSuccess(HeroGuide guide, int hostTime)
{
    [PhpProperty("errors")]
    public string Errors => string.Empty;

    [PhpProperty("success")]
    public int Success => 1;

    [PhpProperty("datetime")]
    public DateTimeOffset TimestampCreated { get; set; } = guide.TimestampCreated;

    [PhpProperty("author_name")]
    public string AuthorName { get; set; } = guide.Author.Name;

    [PhpProperty("hero_cli_name")]
    public string HeroIdentifier { get; set; } = guide.HeroIdentifier;

    [PhpProperty("guide_name")]
    public string Name { get; set; } = guide.Name;

    [PhpProperty("hero_name")]
    public string HeroName { get; set; } = guide.HeroName;

    [PhpProperty("default")]
    public int Default => 0;

    [PhpProperty("favorite")]
    public int Favorite => 0;

    [PhpProperty("rating")]
    public float Rating { get; set; } = guide.Rating;

    [PhpProperty("thumb")]
    public string Thumb => guide.UpVotes is 0 ? "noVote" : guide.UpVotes.ToString();

    [PhpProperty("premium")]
    public bool Featured { get; set; } = guide.Featured;

    [PhpProperty("i_start")]
    public string StartingItems { get; set; } = guide.StartingItems;

    [PhpProperty("i_laning")]
    public string EarlyGameItems { get; set; } = guide.EarlyGameItems;

    [PhpProperty("i_core")]
    public string CoreItems { get; set; } = guide.CoreItems;

    [PhpProperty("i_luxury")]
    public string LuxuryItems { get; set; } = guide.LuxuryItems;

    [PhpProperty("abilQ")]
    public string AbilityQueue { get; set; } = guide.AbilityQueue;

    [PhpProperty("txt_intro")]
    public string Intro { get; set; } = guide.Intro;

    [PhpProperty("txt_guide")]
    public string Content { get; set; } = guide.Content;

    [PhpProperty("hosttime")]
    public int HostTime { get; set; } = hostTime;

    [PhpProperty("vested_threshold")]
    public int VestedThreshold => 5;

    [PhpProperty(0)]
    public bool Zero => true;
}
