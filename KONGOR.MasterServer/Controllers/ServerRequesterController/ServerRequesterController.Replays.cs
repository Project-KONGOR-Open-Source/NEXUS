using KONGOR.MasterServer.Logging;

namespace KONGOR.MasterServer.Controllers.ServerRequesterController;

public partial class ServerRequesterController
{
    private Task<IActionResult> HandleGetSpectatorHeader()
    {
        string baseURL = Configuration.CDN.Host;
        string targetURL = $"{baseURL}/replays/upload";

        Dictionary<string, object> response = new()
        {
            // The Target URL For Uploading Replay Files
            ["target_url"] = targetURL,

            // The Headers Required For Uploading Replay Files
            // Regardless Of Whether Amazon Simple Storage Service (S3) Is Used Or Not, These Headers Are Recursively Applied To The Upload Request
            // The "s3_header" Key Is Hard-Coded In The Game Client, So It Must Be Included Even If S3 Is Not Used
            ["s3_header"] = new Dictionary<string, string> { ["Content-Type"] = "application/zip" }
        };

        return Task.FromResult<IActionResult>(Ok(PhpSerialization.Serialize(response)));
    }

    private async Task<IActionResult> HandleSetReplaySize()
    {
        string? session = Request.Form["session"];

        if (session is null)
        {
            Logger.LogMissingSessionInSetReplaySize();
            return BadRequest(PhpSerialization.Serialize(new { error = "Missing Session" }));
        }

        MatchServer? matchServer = await DistributedCache.GetMatchServerBySessionCookie(session);

        if (matchServer is null)
        {
            // Log Warning But Return OK To Prevent Crashes/Retries
            Logger.LogMatchServerNotFoundForSession(session);
            return Ok(PhpSerialization.Serialize(new { result = "OK" }));
        }

        string? matchIdString = Request.Form["match_id"];
        if (matchIdString is null)
        {
            Logger.LogMissingMatchId();
            return BadRequest(PhpSerialization.Serialize(new { error = "Missing Match ID" }));
        }

        string? fileSizeString = Request.Form["file_size"];
        if (fileSizeString is null)
        {
            Logger.LogMissingFileSize();
            return BadRequest(PhpSerialization.Serialize(new { error = "Missing File Size" }));
        }

        if (int.TryParse(matchIdString, out int matchId) && int.TryParse(fileSizeString, out int fileSize))
        {
            // TODO: Update Match Statistics With Replay Size
            Logger.LogReceivedReplaySize(matchId, fileSize);
        }
        else
        {
            Logger.LogInvalidMatchIdOrFileSize(matchIdString, fileSizeString);
        }

        // Always return OK to satisfy the server
        return Ok(PhpSerialization.Serialize(new { set_replay_size = "OK" }));
    }
}