using KONGOR.MasterServer.Extensions.Cache;
using KONGOR.MasterServer.Models.RequestResponse.SRP;
using KONGOR.MasterServer.Services.Requester;
using MERRICK.DatabaseContext.Entities.Core;
using MERRICK.DatabaseContext.Persistence;
using Microsoft.EntityFrameworkCore;
using PhpSerializerNET;
using StackExchange.Redis;

namespace KONGOR.MasterServer.Controllers.StorageStatusController;

[ApiController]
[Route("master/storage/status")]
public class StorageStatusController(MerrickContext dbContext, IDatabase distributedCache) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = dbContext;
    private IDatabase DistributedCache { get; } = distributedCache;

    [HttpPost(Name = "Storage Status")]
    public async Task<IActionResult> StorageStatus()
    {
        string? cookie = ClientRequestHelper.GetCookie(Request);

        if (string.IsNullOrEmpty(cookie))
        {
            return Unauthorized("Missing Cookie");
        }

        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie, MerrickContext);

        if (!isValid || string.IsNullOrEmpty(accountName))
        {
            return Unauthorized("Invalid Cookie");
        }

        Account? account = await MerrickContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Name == accountName);

        if (account == null)
        {
            return Unauthorized("Account Not Found");
        }

        StorageStatusResponse response = new()
        {
            CloudStorageInformation = new CloudStorageInformation
            {
                AccountID = account.ID.ToString(),
                UseCloud = account.UseCloud ? "1" : "0",
                AutomaticCloudUpload = account.AutomaticCloudUpload ? "1" : "0",
                BackupLastUpdatedTime = account.BackupLastUpdatedTime?.ToString("yyyy-MM-dd HH:mm:ss") ??
                                        DateTimeOffset.MinValue.ToString("yyyy-MM-dd HH:mm:ss")
            }
        };

        return Ok(PhpSerialization.Serialize(response));
    }
}
