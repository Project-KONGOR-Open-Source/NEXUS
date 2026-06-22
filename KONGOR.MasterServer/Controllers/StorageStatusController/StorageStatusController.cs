namespace KONGOR.MasterServer.Controllers.StorageStatusController;

[ApiController]
[Route("master/storage/status")]
[Consumes("multipart/form-data")]
public class StorageStatusController(MerrickContext databaseContext, IDatabase distributedCache, ILogger<StorageStatusController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;

    private IDatabase DistributedCache { get; } = distributedCache;

    private ILogger Logger { get; } = logger;

    [HttpPost(Name = "Storage Status")]
    public async Task<IActionResult> StorageStatus()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);

        if (isValid.Equals(false) || accountName is null)
        {
            Logger.LogWarning(@"Configuration Backup Status Request With Invalid Cookie ""{SessionCookie}"" From ""{IPAddress}""",
                cookie, Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN");

            return Unauthorized(@$"Unrecognised Cookie ""{cookie}""");
        }

        Account account = await MerrickContext.Accounts
            .Include(queriedAccount => queriedAccount.ConfigurationBackup)
            .SingleAsync(queriedAccount => queriedAccount.Name.Equals(accountName));

        ConfigurationBackup? configurationBackup = account.ConfigurationBackup;

        ConfigurationBackupInformation configurationBackupInformation = configurationBackup is null
            ? new ConfigurationBackupInformation(account.ID, useCloud: false, automaticUpload: false, fileModificationTime: null)
            : new ConfigurationBackupInformation(account.ID, configurationBackup.UseCloud, configurationBackup.AutomaticUpload, configurationBackup.FileModificationTime);

        return Ok(PhpSerialization.Serialize(new StorageStatusResponse { ConfigurationBackupInformation = configurationBackupInformation }));
    }
}
