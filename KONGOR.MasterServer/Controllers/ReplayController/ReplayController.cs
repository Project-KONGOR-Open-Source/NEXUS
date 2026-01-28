using KONGOR.MasterServer.Logging;

namespace KONGOR.MasterServer.Controllers.ReplayController;

[ApiController]
public partial class ReplayController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ReplayController> _logger;

    public ReplayController(ILogger<ReplayController> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    [HttpPost("replays/upload")]
    [HttpPut("replays/upload")]
    [HttpHead("replays/upload")]
    [HttpPost("replays/{accountId}/{matchId}.honreplay")]
    [HttpPut("replays/{accountId}/{matchId}.honreplay")]
    [HttpHead("replays/{accountId}/{matchId}.honreplay")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UploadReplay(string? accountId = null, string? matchId = null)
    {
        if (Request.Method == "HEAD")
        {
            return Ok();
        }

        try
        {
            // Determine storage path (Create directories if needed)
            string replayDirectory = Path.Combine(_environment.ContentRootPath, "Resources", "Replays");
            if (accountId != null)
            {
                replayDirectory = Path.Combine(replayDirectory, accountId);
            }

            if (!Directory.Exists(replayDirectory))
            {
                Directory.CreateDirectory(replayDirectory);
            }

            string fileName = matchId != null ? $"{matchId}.honreplay" : $"upload_{DateTime.Now.Ticks}.honreplay";
            string filePath = Path.Combine(replayDirectory, fileName);

            // Handle Multipart Form (Standard Uploads from some clients)
            if (Request.HasFormContentType)
            {
                IFormCollection form = await Request.ReadFormAsync();
                IFormFile? file = form.Files.FirstOrDefault();
                if (file != null && file.Length > 0)
                {
                    if (matchId == null)
                    {
                        filePath = Path.Combine(replayDirectory, Path.GetFileName(file.FileName));
                    }

                    using FileStream stream = new(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);
                    _logger.LogSavedReplayForm(filePath);
                    return Ok();
                }
            }

            // Handle Raw Binary Body (Client specific behavior)
            if (Request.ContentLength > 0 || Request.Body.CanRead)
            {
                using FileStream stream = new(filePath, FileMode.Create);
                await Request.Body.CopyToAsync(stream);
                _logger.LogSavedReplayRaw(filePath);
                return Ok();
            }

            return BadRequest("No content received.");
        }
        catch (Exception ex)
        {
            _logger.LogUploadFailed(ex);
            return StatusCode(500, ex.Message);
        }
    }
}