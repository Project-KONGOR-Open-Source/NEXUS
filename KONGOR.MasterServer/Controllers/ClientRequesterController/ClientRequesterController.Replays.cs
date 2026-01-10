namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private async Task<IActionResult> HandleUploadReplay()
    {
        // 2026-01-09: Implemented Basic Replay Upload Handler
        // The game client (or server acting as client) uploads the replay file here.
        // We must handle cases where the cookie is "NULL" or invalid by either skipping auth or validating via other means (e.g. valid match_id).
        // For now, we will be permissive to ensure the file gets saved.

        // Log Request Details
        Logger.LogInformation("[UploadReplay] Received Replay Upload Request.");

        try
        {
            IFormFile? file = Request.Form.Files["file"];
            string? matchID = Request.Form["match_id"];
            string? cookie = Request.Form["cookie"];

            Logger.LogInformation(
                $"[UploadReplay] MatchID: {matchID}, Cookie: {cookie}, File: {file?.FileName} ({file?.Length} bytes)");

            if (file is null || file.Length == 0)
            {
                Logger.LogError("[UploadReplay] No file uploaded.");
                return BadRequest("No file uploaded");
            }

            if (string.IsNullOrEmpty(matchID))
            {
                // Fallback: Try to parse match ID from filename (e.g. M123.honreplay)
                // However, usually the client provides the ID.
                Logger.LogWarning("[UploadReplay] Missing match_id param.");
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

            using (FileStream stream = new(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            Logger.LogInformation($"[UploadReplay] Saved replay to {filePath}");

            // Return Success per PHP Protocol
            // The legacy server returns a serialized array usually, or just HTTP 200.
            // We'll return a standard success response.
            Dictionary<string, object> response = new()
            {
                { "res", true } // Standard "result: true"
            };

            return Ok(PhpSerialization.Serialize(response));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[UploadReplay] Failed to handle upload.");
            return StatusCode(500, "Internal Server Error");
        }
    }
}