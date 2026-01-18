namespace KONGOR.MasterServer.Models.RequestResponse.GameData;

public class GuideResponseError(int hostTime)
{
    [PHPProperty("errors")]
    public string Errors => "no_guides_found";

    [PHPProperty("success")]
    public int Success => 0;

    [PHPProperty("hosttime")]
    public int HostTime { get; set; } = hostTime;

    [PHPProperty("vested_threshold")]
    public int VestedThreshold => 5;

    [PHPProperty(0)]
    public bool Zero => true;
}
