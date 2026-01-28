namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class HeroList
{
    /// <summary>
    ///     The list of free heroes in the rotation, from when HoN became free-to-play.
    ///     The assigned value is the latest list of heroes sent by the legacy HoN master server before the services shut down.
    ///     <br />
    ///     This property is obsolete.
    /// </summary>
    [PHPProperty("free")]
    public string FreeHeroes { get; set; } = "Hero_Genesis,Hero_Dorin_Tal,Hero_Adeve";
}
