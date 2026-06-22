namespace KONGOR.MasterServer.Controllers.CloudEnrollment;

[ApiController]
[Route("master/cloud/set-user-enrollment")]
[Consumes("multipart/form-data")]
public class CloudEnrollmentController(MerrickContext databaseContext, IDatabase distributedCache, ILogger<CloudEnrollmentController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;

    private IDatabase DistributedCache { get; } = distributedCache;

    private ILogger Logger { get; } = logger;

    [HttpPost(Name = "Set Cloud Enrollment")]
    public async Task<IActionResult> SetUserEnrollment()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        string? enroll = Request.Form["enroll"];

        if (enroll is null)
            return BadRequest(@"Missing Value For Form Parameter ""enroll""");

        string? automaticUpload = Request.Form["cloud_autoupload"];

        if (automaticUpload is null)
            return BadRequest(@"Missing Value For Form Parameter ""cloud_autoupload""");

        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);

        if (isValid.Equals(false) || accountName is null)
        {
            Logger.LogWarning(@"Configuration Backup Enrollment Request With Invalid Cookie ""{SessionCookie}"" From ""{IPAddress}""",
                cookie, Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN");

            return Unauthorized(@$"Unrecognised Cookie ""{cookie}""");
        }

        Account account = await MerrickContext.Accounts
            .Include(queriedAccount => queriedAccount.ConfigurationBackup)
            .SingleAsync(queriedAccount => queriedAccount.Name.Equals(accountName));

        ConfigurationBackup? configurationBackup = account.ConfigurationBackup;

        if (configurationBackup is null)
        {
            configurationBackup = new ConfigurationBackup { Account = account };

            account.ConfigurationBackup = configurationBackup;
        }

        configurationBackup.UseCloud = ParseFlag(enroll);
        configurationBackup.AutomaticUpload = ParseFlag(automaticUpload);

        await MerrickContext.SaveChangesAsync();

        ConfigurationBackupInformation configurationBackupInformation = new (account.ID, configurationBackup.UseCloud, configurationBackup.AutomaticUpload, configurationBackup.FileModificationTime);

        return Ok(PhpSerialization.Serialize(new SetUserCloudEnrollmentResponse { Data = configurationBackupInformation }));
    }

    /// <summary>
    ///     The client sends these flags as the integers "0" or "1".
    ///     Any non-zero integer is treated as enabled, matching the original server's truthiness check.
    /// </summary>
    private static bool ParseFlag(string? value) => int.TryParse(value, out int flag) && flag is not 0;
}
