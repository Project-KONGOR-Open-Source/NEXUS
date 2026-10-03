namespace KONGOR.MasterServer.Controllers.UserConfigurationBackup;

[ApiController]
[Consumes("multipart/form-data")]
public class UserConfigurationBackupController(MerrickContext databaseContext, IDatabase distributedCache, ILogger<UserConfigurationBackupController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;

    private IDatabase DistributedCache { get; } = distributedCache;

    private ILogger Logger { get; } = logger;

    [HttpPost("master/cloud/set-user-enrollment", Name = "Enable Configuration Backup")]
    public async Task<IActionResult> EnableConfigurationBackup()
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

        return Ok(PhpSerialization.Serialize(new EnableConfigurationBackupResponse { Data = configurationBackupInformation }));
    }

    [HttpPost("master/storage/store", Name = "Store Configuration Backup")]
    public async Task<IActionResult> StoreConfigurationBackup()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);

        if (isValid.Equals(false) || accountName is null)
        {
            Logger.LogWarning(@"Configuration Backup Store Request With Invalid Cookie ""{SessionCookie}"" From ""{IPAddress}""",
                cookie, Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN");

            return Unauthorized(PhpSerialization.Serialize(new ConfigurationBackupResponse { Success = false, Messages = "Invalid Session Cookie" }));
        }

        IFormFile? configurationArchive = Request.Form.Files["cloud.zip"];

        if (configurationArchive is null)
            return Ok(PhpSerialization.Serialize(new ConfigurationBackupResponse { Success = false, Messages = "No File Payload Was Received" }));

        Account account = await MerrickContext.Accounts
            .Include(queriedAccount => queriedAccount.ConfigurationBackup)
            .SingleAsync(queriedAccount => queriedAccount.Name.Equals(accountName));

        ConfigurationBackup? configurationBackup = account.ConfigurationBackup;

        if (configurationBackup is null)
        {
            configurationBackup = new ConfigurationBackup { Account = account };

            account.ConfigurationBackup = configurationBackup;
        }

        configurationBackup.FileModificationTime = Request.Form["file_modify_time"];

        using (MemoryStream memoryStream = new ())
        {
            await configurationArchive.CopyToAsync(memoryStream);

            configurationBackup.ConfigurationArchive = memoryStream.ToArray();
        }

        await MerrickContext.SaveChangesAsync();

        return Ok(PhpSerialization.Serialize(new ConfigurationBackupResponse { Success = true }));
    }

    [HttpPost("master/storage/status", Name = "Get Configuration Backup Status")]
    public async Task<IActionResult> GetConfigurationBackupStatus()
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

        return Ok(PhpSerialization.Serialize(new ConfigurationBackupStatusResponse { ConfigurationBackupInformation = configurationBackupInformation }));
    }

    [HttpPost("master/storage/retrieve", Name = "Retrieve Configuration Backup")]
    public async Task<IActionResult> RetrieveConfigurationBackup()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);

        const string missingArchiveMessage = "Configuration Backup Not Found";

        if (isValid.Equals(false) || accountName is null)
        {
            Logger.LogWarning(@"Configuration Backup Retrieve Request With Invalid Cookie ""{SessionCookie}"" From ""{IPAddress}""",
                cookie, Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN");

            return Unauthorized(PhpSerialization.Serialize(new ConfigurationBackupResponse { Success = false, Messages = missingArchiveMessage }));
        }

        Account account = await MerrickContext.Accounts
            .Include(queriedAccount => queriedAccount.ConfigurationBackup)
            .SingleAsync(queriedAccount => queriedAccount.Name.Equals(accountName));

        byte[]? configurationArchive = account.ConfigurationBackup?.ConfigurationArchive;

        if (configurationArchive is null || configurationArchive.Length is 0)
            return Ok(PhpSerialization.Serialize(new ConfigurationBackupResponse { Success = false, Messages = missingArchiveMessage }));

        return File(configurationArchive, "application/octet-stream");
    }

    /// <summary>
    ///     The client sends these flags as the integers "0" or "1".
    ///     Any non-zero integer is treated as enabled, matching the original server's truthiness check.
    /// </summary>
    private static bool ParseFlag(string? value) => int.TryParse(value, out int flag) && flag is not 0;
}
