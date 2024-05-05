namespace MERRICK.Database.Data;

public class DeserializationDTOs
{
    public record GuideGetDTO(int GuideID, string Name, string HeroName, string HeroIdentifier, string Intro, string Content,
        IList<string> StartingItems, IList<string> EarlyGameItems, IList<string> CoreItems, IList<string> LuxuryItems,
        IList<string> AbilityQueue, int AuthorID, float Rating, int UpVotes, int DownVotes, bool Public, bool Featured);
}
