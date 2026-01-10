namespace KONGOR.MasterServer.Models.RequestResponse.GameData;

public class GuideListResponse
{
    public GuideListResponse(IEnumerable<HeroGuide> guides, Account requestingAccount, int hostTime)
    {
        IEnumerable<string> guidesForResponse = from guide in guides

            // The Featured Guide Is Considered The Default Guide, So It Is Displayed Without An Author Name
            let author = guide.Featured ? string.Empty :
                guide.Author.Clan is null ? guide.Author.Name : $"[{guide.Author.Clan.Tag}]" + guide.Author.Name
            select new StringBuilder().Append($"{guide.ID}").Append('|')
                .Append(guide.TimestampCreated.ToString("dd MMMM yyyy HH:mm:ss")).Append('|')
                .Append(author).Append('|')
                .Append(guide.Name).Append('|')
                .Append(new List<string> { "not_def", "is_def" }.First())
                .Append('|') // TODO: Implement Support For Default Guide
                .Append(new List<string> { "not_fav", "is_fav" }.First())
                .Append('|') // TODO: Implement Support For Favourite Guide
                .Append($"{guide.Rating}").Append('|')
                .Append(guide.Author?.ID.Equals(requestingAccount.ID) ?? false ? "yours" : "not_yours").Append('|')
                .Append(guide.Featured ? 1.ToString() : 0.ToString())
            into guide
            select guide.ToString();

        GuideList = string.Join('`', guidesForResponse);

        HostTime = hostTime;
    }

    [PhpProperty("errors")] public string Errors => string.Empty;

    [PhpProperty("success")] public int Success => 1;

    [PhpProperty("guide_list")] public string GuideList { get; set; }

    [PhpProperty("hosttime")] public int HostTime { get; set; }

    [PhpProperty("vested_threshold")] public int VestedThreshold => 5;

    [PhpProperty(0)] public bool Zero => true;
}