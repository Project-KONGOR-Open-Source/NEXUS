namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class CloudStorageInformation
{
    /// <summary>
    ///     The ID of the account.
    /// </summary>
    [PHPProperty("account_id")]
    public required string AccountID { get; set; }

    /// <summary>
    ///     Whether to automatically download the backup of the game client configuration files from the cloud or not on login.
    ///     <br />
    ///     0 = False; 1 = True
    /// </summary>
    [PHPProperty("use_cloud")]
    public required string UseCloud { get; set; }

    /// <summary>
    ///     Whether to automatically upload the backup of the game client configuration files to the cloud or not after making
    ///     changes to the settings.
    ///     <br />
    ///     0 = False; 1 = True
    /// </summary>
    [PHPProperty("cloud_autoupload")]
    public required string AutomaticCloudUpload { get; set; }

    /// <summary>
    ///     The timestamp in "yyyy-MM-dd HH:mm:ss" format of when "cloud.zip" was last modified.
    ///     This value is extracted from "cloud.zip", which is the local copy of the backup of the game client configuration
    ///     files.
    /// </summary>
    [PHPProperty("file_modify_time")]
    public required string BackupLastUpdatedTime { get; set; }
}
