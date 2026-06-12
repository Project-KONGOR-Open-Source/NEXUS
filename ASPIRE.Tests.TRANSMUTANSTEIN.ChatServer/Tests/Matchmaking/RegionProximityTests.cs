namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Locks down <see cref="RegionProximity"/> and the <see cref="GameRegions"/> aggregation it builds on: the circular UTC-offset distance model used to allocate the closest available match server when no requested region has an idle one.
/// </summary>
public sealed class RegionProximityTests
{
    [Test]
    public async Task A_Server_In_A_Requested_Region_Has_Zero_Distance()
    {
        await Assert.That(RegionProximity.GetDistance(["USE", "EU"], "EU")).IsEqualTo(0.0);
    }

    [Test]
    public async Task A_Server_In_The_Same_Aggregate_Has_Zero_Distance()
    {
        // "USE" And "USW" Both Belong To The "US" Aggregate
        await Assert.That(RegionProximity.GetDistance(["USE"], "USW")).IsEqualTo(0.0);
    }

    [Test]
    public async Task The_Newerth_Wildcard_Has_Zero_Distance_To_Every_Location()
    {
        await Assert.That(RegionProximity.GetDistance([GameRegions.Wildcard], "AU")).IsEqualTo(0.0);
    }

    [Test]
    public async Task An_Unknown_Server_Location_Normalises_To_The_Wildcard()
    {
        // A Server Advertising An Unknown Location Is Available To Every Match
        await Assert.That(RegionProximity.GetDistance(["USE"], "UNKNOWN")).IsEqualTo(0.0);
    }

    [Test]
    public async Task A_Neighbouring_Aggregate_Is_Closer_Than_A_Distant_One()
    {
        double distanceToLAT = RegionProximity.GetDistance(["US"], "LAT");
        double distanceToEU  = RegionProximity.GetDistance(["US"], "EU");

        await Assert.That(distanceToLAT).IsLessThan(distanceToEU);
    }

    [Test]
    public async Task Distance_Wraps_Around_The_Globe()
    {
        // US (-6.5) To SEA (+8.5) Is 15 Hours Apart Linearly But Only 9 Hours Across The Pacific
        await Assert.That(RegionProximity.GetDistance(["US"], "SEA")).IsEqualTo(9.0);
    }

    [Test]
    public async Task An_Unmapped_Match_Region_Has_The_Maximum_Distance()
    {
        // "DX" Aggregates To Itself And Has No UTC Offset, So A Server In A Different Known Aggregate Ranks Last (But Remains Selectable)
        await Assert.That(RegionProximity.GetDistance(["DX"], "EU")).IsEqualTo(RegionProximity.UnknownRegionDistance);
    }

    [Test]
    public async Task The_Distance_To_Multiple_Regions_Is_The_Closest_One()
    {
        double multiRegionDistance  = RegionProximity.GetDistance(["US", "LAT"], "EU");
        double singleRegionDistance = RegionProximity.GetDistance(["LAT"], "EU");

        await Assert.That(multiRegionDistance).IsEqualTo(singleRegionDistance);
    }

    [Test]
    [Arguments("USW")]
    [Arguments("USE")]
    [Arguments("EU")]
    [Arguments("AU")]
    [Arguments("LAT")]
    [Arguments("BR")]
    [Arguments("RU")]
    [Arguments("SG")]
    [Arguments("MY")]
    [Arguments("PH")]
    [Arguments("TH")]
    [Arguments("ID")]
    [Arguments("CN")]
    [Arguments("TR")]
    [Arguments("KR")]
    public async Task Every_Client_Region_Code_Maps_Into_An_Aggregate(string clientRegionCode)
    {
        string[] aggregates = ["US", "LAT", "EU", "SEA"];

        await Assert.That(aggregates.Contains(GameRegions.GetAggregate(clientRegionCode), StringComparer.OrdinalIgnoreCase)).IsTrue();
    }
}
