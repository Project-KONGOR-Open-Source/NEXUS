namespace KONGOR.MasterServer.Models.RequestResponse.GameData;

public class GuideResponseError(int hostTime)
{
    [PhpProperty("errors")] public string Errors => "no_guides_found";

    [PhpProperty("success")] public int Success => 0;

    [PhpProperty("hosttime")] public int HostTime { get; set; } = hostTime;

    [PhpProperty("vested_threshold")] public int VestedThreshold => 5;

    [PhpProperty(0)] public bool Zero => true;
}