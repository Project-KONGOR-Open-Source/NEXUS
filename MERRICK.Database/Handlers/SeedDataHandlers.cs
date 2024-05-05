namespace MERRICK.Database.Handlers;

public static class SeedDataHandlers
{
    public static async Task SeedHeroGuides(MerrickContext context, CancellationToken cancellationToken, ILogger logger)
    {
        if (await context.HeroGuides.AnyAsync(cancellationToken) || await context.Accounts.NoneAsync(cancellationToken)) return;

        Account author = await context.Accounts.FirstAsync(cancellationToken);

        DeserializationDTOs.GuideGetDTO[] guideDTOs = JsonSerializer.Deserialize<DeserializationDTOs.GuideGetDTO[]>
            (await File.ReadAllTextAsync(DataFiles.HeroGuides, cancellationToken), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

        // The Game Client Requires At Least One Featured And One Standard Guide Per Hero

        IEnumerable<HeroGuide> guides = guideDTOs.Select(guide => new HeroGuide()
        {
            Name = guide.Name,
            HeroName = guide.HeroName,
            HeroIdentifier = guide.HeroIdentifier,
            Intro = guide.Intro,
            Content = guide.Content,
            StartingItems = guide.StartingItems.ListToPipeSeparatedString(),
            EarlyGameItems = guide.EarlyGameItems.ListToPipeSeparatedString(),
            CoreItems = guide.CoreItems.ListToPipeSeparatedString(),
            LuxuryItems = guide.LuxuryItems.ListToPipeSeparatedString(),
            AbilityQueue = guide.AbilityQueue.ListToPipeSeparatedString(),
            Author = author,
            Rating = guide.Rating,
            UpVotes = guide.UpVotes,
            DownVotes = guide.DownVotes,
            Public = guide.Public,
            Featured = guide.Featured,
            TimestampCreated = DateTime.UtcNow
        });

        await context.HeroGuides.AddRangeAsync(guides, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
}
