namespace KONGOR.MasterServer.Controllers.ServerRequesterController;

public partial class ServerRequesterController
{
    private async Task<IActionResult> HandleGetSpectatorHeader()
    {
        string baseURL = $"{Request.Scheme}://{Request.Host}";
        string targetURL = $"{baseURL}/replays/upload";

        Dictionary<string, object> response = new ()
        {
            // The Target URL For Uploading Replay Files
            ["target_url"] = targetURL,

            // The Headers Required For Uploading Replay Files
            // Regardless Of Whether Amazon Simple Storage Service (S3) Is Used Or Not, These Headers Are Recursively Applied To The Upload Request
            // The "s3_header" Key Is Hard-Coded In The Game Client, So It Must Be Included Even If S3 Is Not Used
            ["s3_header"] = new Dictionary<string, string> { ["Content-Type"] = "application/zip" }
        };

        return Ok(response);
    }
}
