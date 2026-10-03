namespace KONGOR.MasterServer.Models.RequestResponse.ConfigurationBackup;

/// <summary>
///     The account's configuration backup settings, as reported to the game client.
///     Every field is serialised as a string, matching the format the client expects.
/// </summary>
public class ConfigurationBackupInformation(int accountID, bool useCloud, bool automaticUpload, string? fileModificationTime)
{
    [PHPProperty("account_id")]
    public string AccountID { get; } = accountID.ToString();

    [PHPProperty("use_cloud")]
    public string UseCloud { get; } = useCloud ? "1" : "0";

    [PHPProperty("cloud_autoupload")]
    public string CloudAutomaticUpload { get; } = automaticUpload ? "1" : "0";

    [PHPProperty("file_modify_time")]
    public string? FileModificationTime { get; } = fileModificationTime;
}
