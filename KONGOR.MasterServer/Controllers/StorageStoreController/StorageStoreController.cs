namespace KONGOR.MasterServer.Controllers.StorageStoreController;

[ApiController]
[Route("master/storage/store")]
[Consumes("multipart/form-data")]
public class StorageStoreController(MerrickContext databaseContext, IDatabase distributedCache, ILogger<StorageStoreController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;

    private IDatabase DistributedCache { get; } = distributedCache;

    private ILogger Logger { get; } = logger;

    [HttpPost(Name = "Storage Store")]
    public async Task<IActionResult> StorageStore()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);

        if (isValid.Equals(false) || accountName is null)
        {
            Logger.LogWarning(@"Configuration Backup Store Request With Invalid Cookie ""{SessionCookie}"" From ""{IPAddress}""",
                cookie, Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN");

            return Unauthorized(PhpSerialization.Serialize(new StorageResponse { Success = false, Messages = "Invalid Session Cookie" }));
        }

        IFormFile? configurationArchive = Request.Form.Files["cloud.zip"];

        if (configurationArchive is null)
            return Ok(PhpSerialization.Serialize(new StorageResponse { Success = false, Messages = "No File Payload Was Received" }));

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

        return Ok(PhpSerialization.Serialize(new StorageResponse { Success = true }));
    }
}
