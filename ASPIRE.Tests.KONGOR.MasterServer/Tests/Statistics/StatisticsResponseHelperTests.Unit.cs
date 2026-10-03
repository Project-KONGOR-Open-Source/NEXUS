namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Statistics;

/// <summary>
///     Pure-logic tests for <see cref="StatisticsResponseHelper"/>, covering the per-match and per-minute average calculations and the favourite-hero resolution used across the show_stats responses.
/// </summary>
public sealed class StatisticsResponseHelperTests_Unit
{
    private static User BuildUser() => new ()
    {
        EmailAddress = "statistics.tests@kongor.com",
        Role = new Role { Name = UserRoles.User },
        SRPPasswordSalt = "salt",
        SRPPasswordHash = "hash",
        OwnedStoreItems = []
    };

    private static AccountStatistics BuildStatistics(int matchesPlayed, params HeroStats[] heroes) => new ()
    {
        Account = new Account { Name = "StatisticsTester", User = BuildUser(), IsMain = true },
        Type = AccountStatisticsType.Matchmaking,
        PlacementMatchesData = null,
        MatchesPlayed = matchesPlayed,
        HeroStatistics = new HeroStatisticsSummary { Heroes = [.. heroes] }
    };

    private static HeroStats BuildHero(string identifier, int gamesPlayed) => new () { HeroIdentifier = identifier, GamesPlayed = gamesPlayed };

    [Test]
    [Arguments(100, 10, 10.0)]
    [Arguments(15, 4, 3.75)]
    [Arguments(50, 0, 0.0)]
    public async Task CalculatePerMatchAverage_Divides_The_Total_By_The_Match_Count_And_Guards_Against_Zero(int total, int matchesPlayed, double expected)
        => await Assert.That(StatisticsResponseHelper.CalculatePerMatchAverage(total, matchesPlayed)).IsEqualTo(expected);

    [Test]
    [Arguments(600, 120, 300.0)]
    [Arguments(450, 90, 300.0)]
    [Arguments(100, 0, 0.0)]
    public async Task CalculatePerMinuteAverage_Divides_The_Total_By_The_Elapsed_Minutes_And_Guards_Against_Zero(int total, int seconds, double expected)
        => await Assert.That(StatisticsResponseHelper.CalculatePerMinuteAverage(total, seconds)).IsEqualTo(expected);

    [Test]
    public async Task GetFavouriteHeroes_Orders_By_Matches_Played_Descending_And_Caps_At_Five()
    {
        AccountStatistics statistics = BuildStatistics(20,
            BuildHero("Hero_Accursed", 2),
            BuildHero("Hero_Pyromancer", 8),
            BuildHero("Hero_Chi", 4),
            BuildHero("Hero_Aluna", 1),
            BuildHero("Hero_Andromeda", 3),
            BuildHero("Hero_Apex", 5));

        IReadOnlyList<FavouriteHero> favourites = StatisticsResponseHelper.GetFavouriteHeroes(statistics);

        using (Assert.Multiple())
        {
            // Six Heroes Were Played, But Only The Top Five By Matches Played Are Returned
            await Assert.That(favourites.Count).IsEqualTo(5);

            // The Most-Played Hero Comes First, And Its Identifier Is Surfaced Verbatim
            await Assert.That(favourites[0].Identifier).IsEqualTo("Hero_Pyromancer");

            // 8 Of 20 Matches Played With The Most-Played Hero Is 40 Percent
            await Assert.That(favourites[0].PlayRatePercentage).IsEqualTo(40.0);

            // The Single-Match Hero Falls Outside The Top Five And Is Dropped
            await Assert.That(favourites.Any(favourite => favourite.Identifier is "Hero_Aluna")).IsFalse();
        }
    }

    [Test]
    [Arguments("Hero_Pyromancer", "pyromancer")]
    [Arguments("Hero_Chi", "chi")]
    [Arguments("Hero_MasterOfArms", "masterofarms")]
    public async Task GetFavouriteHeroes_Resolves_The_Icon_Texture_Name_From_The_Identifier(string identifier, string expectedTextureName)
    {
        AccountStatistics statistics = BuildStatistics(10, BuildHero(identifier, 10));

        await Assert.That(StatisticsResponseHelper.GetFavouriteHeroes(statistics).Single().TextureName).IsEqualTo(expectedTextureName);
    }

    [Test]
    public async Task GetFavouriteHeroes_Returns_An_Empty_List_When_No_Heroes_Have_Been_Played()
        => await Assert.That(StatisticsResponseHelper.GetFavouriteHeroes(BuildStatistics(0)).Count).IsEqualTo(0);

    [Test]
    public async Task GetFavouriteHeroes_Reports_A_Zero_Play_Rate_When_No_Matches_Have_Been_Played()
    {
        // A Hero With Recorded Games But No Matches Played Must Not Divide By Zero
        AccountStatistics statistics = BuildStatistics(0, BuildHero("Hero_Accursed", 3));

        await Assert.That(StatisticsResponseHelper.GetFavouriteHeroes(statistics).Single().PlayRatePercentage).IsEqualTo(0.0);
    }
}
