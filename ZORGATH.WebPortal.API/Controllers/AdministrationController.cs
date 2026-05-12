namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[Authorize(Policy = UserRoles.Administrator)]
[EnableRateLimiting(RateLimiterPolicies.Strict)]
public class AdministrationController(MerrickContext databaseContext, ILogger<AdministrationController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private ILogger Logger { get; } = logger;

    private static readonly TimeSpan HostAccountAuthorisationTokenValidity = TimeSpan.FromHours(24);

    [HttpPost("Authorise/Host", Name = "Issue Host Account Authorisation Token")]
    [ProducesResponseType(typeof(HostAccountAuthorisationTokenDTO), StatusCodes.Status201Created)]
    public async Task<IActionResult> IssueHostAccountAuthorisationToken()
    {
        Token token = new ()
        {
            Purpose = TokenPurpose.HostAccountAuthorisation,
            EmailAddress = string.Empty,
            Value = Guid.CreateVersion7(),
            Data = AccountType.ServerHost.ToString(),
            Validity = HostAccountAuthorisationTokenValidity
        };

        await MerrickContext.Tokens.AddAsync(token);
        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(@"Issued Host Account Authorisation Token ""{Value}""", token.Value);

        HostAccountAuthorisationTokenDTO response = new (token.Value, token.TimestampCreated + token.Validity);

        return CreatedAtAction(nameof(IssueHostAccountAuthorisationToken), response);
    }
}
