using KONGOR.MasterServer.Services.Requester;
// For PhpSerialization

namespace KONGOR.MasterServer.Handlers.ClientRequester;

public partial class ReplayHandler(ILogger<ReplayHandler> logger) : IClientRequestHandler
{
    private ILogger Logger { get; } = logger;

    [LoggerMessage(Level = LogLevel.Information, Message = "[UploadReplay] Received Replay Upload Request.")]
    private partial void LogReceiveUploadRequest();

    [LoggerMessage(Level = LogLevel.Information, Message = "[UploadReplay] MatchID: {MatchID}, Cookie: {Cookie}, File: {FileName} ({Length} bytes)")]
    private partial void LogUploadDetails(string matchID, string cookie, string fileName, long? length);

    [LoggerMessage(Level = LogLevel.Error, Message = "[UploadReplay] No file uploaded.")]
    private partial void LogNoFileUploaded();

    [LoggerMessage(Level = LogLevel.Warning, Message = "[UploadReplay] Missing match_id param.")]
    private partial void LogMissingMatchId();

    [LoggerMessage(Level = LogLevel.Information, Message = "[UploadReplay] Saved replay to {FilePath}")]
    private partial void LogReplaySaved(string filePath);

    [LoggerMessage(Level = LogLevel.Error, Message = "[UploadReplay] Failed to handle upload.")]
    private partial void LogUploadFailure(Exception ex);

    public async Task<IActionResult> HandleRequestAsync(HttpContext context)
    {
        HttpRequest Request = context.Request;

        LogReceiveUploadRequest();

        try
        {
            IFormFile? file = Request.Form.Files["file"];
            string? matchID = Request.Form["match_id"];
            string? cookie = ClientRequestHelper.GetCookie(Request);

            LogUploadDetails(matchID ?? "NULL", cookie ?? "NULL", file?.FileName ?? "NULL", file?.Length);

            if (file is null || file.Length == 0)
            {
                LogNoFileUploaded();
                return new BadRequestObjectResult("No file uploaded");
            }

            if (string.IsNullOrEmpty(matchID))
            {
                LogMissingMatchId();
            }

            // Define Storage Path
            string uploadDir = Path.Combine(Environment.CurrentDirectory, "replays");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            // Save File
            string fileName = !string.IsNullOrEmpty(matchID) ? $"M{matchID}.honreplay" : file.FileName;
            string filePath = Path.Combine(uploadDir, fileName);

            await using (FileStream stream = new(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            LogReplaySaved(filePath);

            Dictionary<string, object> response = new()
            {
                { "res", true }
            };

            return new OkObjectResult(PhpSerialization.Serialize(response));
        }
        catch (Exception ex)
        {
            LogUploadFailure(ex);
            return new ObjectResult("Internal Server Error") { StatusCode = 500 };
        }
    }
}