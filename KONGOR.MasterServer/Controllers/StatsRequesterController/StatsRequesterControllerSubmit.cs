namespace KONGOR.MasterServer.Controllers.StatsRequesterController;

public partial class StatsRequesterController
{
    private async Task<IActionResult> HandleStatsSubmission(StatsForSubmissionRequestForm form)
    {
        // TODO: Add Cookie Validation

        // if (Cache.ValidateAccountSessionCookie(form.Cookie, out string? _).Equals(false))
        // {
        //     Logger.LogWarning($@"IP Address ""{Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN"}"" Has Made A Stats Controller Request With Forged Cookie ""{form.Cookie}""");
        //
        //     return Unauthorized($@"Unrecognized Cookie ""{form.Cookie}""");
        // }

        string? session = Request.Form["session"];

        if (session is null)
            return BadRequest(@"Missing Value For Form Parameter ""session""");

        // TODO: Validate Session

        return Ok();
    }

    private async Task<IActionResult> HandleStatsResubmission(StatsForSubmissionRequestForm form)
    {
        // TODO: Add Password Validation
        // TODO: Add Stats Resubmission Key Validation

        string? login = Request.Form["login"];

        if (login is null)
            return BadRequest(@"Missing Value For Form Parameter ""login""");

        // TODO: Validate Host Account Name

        string? password = Request.Form["pass"];

        if (password is null)
            return BadRequest(@"Missing Value For Form Parameter ""pass""");

        // TODO: Validate Host Account Password

        string? resubmissionKey = Request.Form["resubmission_key"];

        if (resubmissionKey is null)
            return BadRequest(@"Missing Value For Form Parameter ""resubmission_key""");

        // TODO: Do Something With Stats Resubmission Key

        string? serverID = Request.Form["server_id"];

        if (serverID is null)
            return BadRequest(@"Missing Value For Form Parameter ""server_id""");

        // TODO: Do Something With Server ID

        return Ok();
    }
}
