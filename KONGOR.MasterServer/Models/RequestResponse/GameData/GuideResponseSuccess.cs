namespace KONGOR.MasterServer.Models.RequestResponse.GameData;

public class GuideResponseSuccess(HeroGuide guide, int hostTime)
{
    [PHPProperty("errors")]
    public string Errors => string.Empty;

    [PHPProperty("success")]
    public int Success => 1;

    [PHPProperty("datetime")]
    public DateTimeOffset TimestampCreated { get; set; } = guide.TimestampCreated;

    [PHPProperty("author_name")]
    public string AuthorName { get; set; } = guide.Author.Name;

    [PHPProperty("hero_cli_name")]
    public string HeroIdentifier { get; set; } = guide.HeroIdentifier;

    [PHPProperty("guide_name")]
    public string Name { get; set; } = guide.Name;

    [PHPProperty("hero_name")]
    public string HeroName { get; set; } = guide.HeroName;

    [PHPProperty("default")]
    public int Default => 0;

    [PHPProperty("favorite")]
    public int Favorite => 0;

    [PHPProperty("rating")]
    public float Rating { get; set; } = guide.Rating;

    [PHPProperty("thumb")]
    public string Thumb => guide.UpVotes is 0 ? "noVote" : guide.UpVotes.ToString();

    [PHPProperty("premium")]
    public bool Featured { get; set; } = guide.Featured;

    [PHPProperty("i_start")]
    public string StartingItems { get; set; } = guide.StartingItems;

    [PHPProperty("i_laning")]
    public string EarlyGameItems { get; set; } = guide.EarlyGameItems;

    [PHPProperty("i_core")]
    public string CoreItems { get; set; } = guide.CoreItems;

    [PHPProperty("i_luxury")]
    public string LuxuryItems { get; set; } = guide.LuxuryItems;

    [PHPProperty("abilQ")]
    public string AbilityQueue { get; set; } = guide.AbilityQueue;

    [PHPProperty("txt_intro")]
    public string Intro { get; set; } = guide.Intro;

    [PHPProperty("txt_guide")]
    public string Content { get; set; } = guide.Content;

    [PHPProperty("hosttime")]
    public int HostTime { get; set; } = hostTime;

    [PHPProperty("vested_threshold")]
    public int VestedThreshold => 5;

    [PHPProperty(0)]
    public bool Zero => true;
}
