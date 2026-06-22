namespace KONGOR.MasterServer.Controllers.StorageRetrieveController;

[ApiController]
[Route("master/storage/retrieve")]
[Consumes("multipart/form-data")]
public class StorageRetrieveController(MerrickContext databaseContext, IDatabase distributedCache, ILogger<StorageRetrieveController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;

    private IDatabase DistributedCache { get; } = distributedCache;

    private ILogger Logger { get; } = logger;

    [HttpPost(Name = "Storage Retrieve")]
    public async Task<IActionResult> StorageRetrieve()
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

            return Unauthorized(PhpSerialization.Serialize(new StorageResponse { Success = false, Messages = missingArchiveMessage }));
        }

        Account account = await MerrickContext.Accounts
            .Include(queriedAccount => queriedAccount.ConfigurationBackup)
            .SingleAsync(queriedAccount => queriedAccount.Name.Equals(accountName));

        byte[]? configurationArchive = account.ConfigurationBackup?.ConfigurationArchive;

        if (configurationArchive is null || configurationArchive.Length is 0)
            return Ok(PhpSerialization.Serialize(new StorageResponse { Success = false, Messages = missingArchiveMessage }));

        return File(configurationArchive, "application/octet-stream");
    }
}
