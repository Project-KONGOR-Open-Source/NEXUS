using System.Globalization;
using KONGOR.MasterServer.Models.RequestResponse.Cloud;
using KONGOR.MasterServer.Services.Requester;

namespace KONGOR.MasterServer.Controllers;

[ApiController]
[Route("master/storage")]
public class StorageController(
    MerrickContext dbContext,
    IDatabase distributedCache,
    IWebHostEnvironment environment,
    IConfiguration configuration) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = dbContext;
    private IDatabase DistributedCache { get; } = distributedCache;
    private IWebHostEnvironment Environment { get; } = environment;
    private IConfiguration Configuration { get; } = configuration;

    [HttpPost("store")]
    public async Task<IActionResult> Store(
        [FromForm(Name = "cookie")] string? cookie,
        [FromForm(Name = "bucket")] string bucket,
        [FromForm(Name = "file_modify_time")] string? fileModifyTime,
        IFormFile? file)
    {
        // Parameter binding might fail if names don't match form-data exactly or if they are missing.
        // The log shows "cloud.zip" as the name for the file part.
        // We can also try manual binding if needed, but let's try standard model binding first.
        // NOTE: The log shows 'cloud.zip' as the form field name for the file, NOT 'file'.
        // So we need to access Request.Form.Files or bind with the correct name.
        
        // Manual extraction for reliability given the specific client behavior
        if (string.IsNullOrEmpty(cookie))
        {
             cookie = ClientRequestHelper.GetCookie(Request);
        }

        if (string.IsNullOrEmpty(cookie))
        {
            return Unauthorized("Missing Cookie");
        }

        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie, MerrickContext);

        if (!isValid || string.IsNullOrEmpty(accountName))
        {
            return Unauthorized("Invalid Cookie");
        }
        
        if (Request.Form.Files.Count == 0)
        {
             return BadRequest("No file uploaded");
        }
        
        IFormFile uploadedFile = Request.Form.Files[0]; // Usually named "cloud.zip"

        Account? account = await MerrickContext.Accounts
            .FirstOrDefaultAsync(a => a.Name == accountName);

        if (account == null)
        {
            return Unauthorized("Account Not Found");
        }

        try 
        {
            // Resolve Storage Path
            string relativePath = Configuration["Storage:CloudPath"] ?? "App_Data/Cloud";
            string storageRoot = Path.Combine(Environment.ContentRootPath, relativePath);
            string accountStoragePath = Path.Combine(storageRoot, account.ID.ToString());
            
            if (!Directory.Exists(accountStoragePath))
            {
                Directory.CreateDirectory(accountStoragePath);
            }
            
            string filePath = Path.Combine(accountStoragePath, "cloud.zip");

            await using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                await uploadedFile.CopyToAsync(stream);
            }
            
            // Update Account Entity
            // Update Account Entity
            account.UseCloud = true;
            
            if (!string.IsNullOrEmpty(fileModifyTime) && DateTimeOffset.TryParseExact(fileModifyTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset parsedTime))
            {
                 account.BackupLastUpdatedTime = parsedTime;
            }
            else
            {
                 account.BackupLastUpdatedTime = DateTime.UtcNow;
            }

            await MerrickContext.SaveChangesAsync();

            StorageResponse response = new() { Success = true };
            return Ok(PhpSerialization.Serialize(new StorageResponse { Success = true })); // Return object, not just true
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

    [HttpPost("retrieve")]
    public async Task<IActionResult> Retrieve(
        [FromForm(Name = "cookie")] string? cookie,
        [FromForm(Name = "bucket")] string bucket)
    {
        if (string.IsNullOrEmpty(cookie))
        {
             cookie = ClientRequestHelper.GetCookie(Request);
        }
        
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

        // Resolve Storage Path
        string relativePath = Configuration["Storage:CloudPath"] ?? "App_Data/Cloud";
        string storageRoot = Path.Combine(Environment.ContentRootPath, relativePath);
        string accountStoragePath = Path.Combine(storageRoot, account.ID.ToString());
        string filePath = Path.Combine(accountStoragePath, "cloud.zip");

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return File(fileStream, "application/octet-stream", "cloud.zip");
    }
}
