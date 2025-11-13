namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[EnableRateLimiting(RateLimiterPolicies.Strict)]
public class EmailAddressController(MerrickContext databaseContext, ILogger<EmailAddressController> logger, IEmailService emailService) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private ILogger Logger { get; } = logger;
    private IEmailService EmailService { get; } = emailService;

    [HttpPost("Register", Name = "Register Email Address")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(string), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> RegisterEmailAddress(RegisterEmailAddressDTO payload)
    {
        if (payload.EmailAddress.Equals(payload.ConfirmEmailAddress).Equals(false))
            return BadRequest($@"Email Address ""{payload.ConfirmEmailAddress}"" Does Not Match ""{payload.EmailAddress}""");

        Token? token = await MerrickContext.Tokens.SingleOrDefaultAsync(token => token.EmailAddress.Equals(payload.EmailAddress) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        if (token is null)
        {
            IActionResult result = EmailAddressHelpers.SanitizeEmailAddress(payload.EmailAddress);

            if (result is not ContentResult contentResult)
            {
                return result;
            }

            if (contentResult.Content is null)
            {
                Logger.LogError(@"[BUG] Sanitized Email Address ""{Payload.EmailAddress}"" Is NULL", payload.EmailAddress);

                return UnprocessableEntity($@"Unable To Process Email Address ""{payload.EmailAddress}""");
            }

            string sanitizedEmailAddress = contentResult.Content;

            token = new Token()
            {
                Purpose = TokenPurpose.EmailAddressVerification,
                EmailAddress = payload.EmailAddress,
                Value = Guid.CreateVersion7(),
                Data = sanitizedEmailAddress
            };

            await MerrickContext.Tokens.AddAsync(token);
            await MerrickContext.SaveChangesAsync();

            bool sent = await EmailService.SendEmailAddressRegistrationLink(payload.EmailAddress, token.Value.ToString());

            if (sent.Equals(false))
            {
                MerrickContext.Tokens.Remove(token);
                await MerrickContext.SaveChangesAsync();

                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Failed To Send Email Address Verification Email");
            }
        }

        else
        {
            return token.TimestampConsumed is null
                ? BadRequest($@"A Registration Request For Email Address ""{payload.EmailAddress}"" Has Already Been Made (Check Your Email Inbox For A Registration Link)")
                : BadRequest($@"Email Address ""{payload.EmailAddress}"" Is Already Registered");
        }

        return Ok($@"Email Address Registration Token Was Successfully Created, And An Email Was Sent To Address ""{payload.EmailAddress}""");
    }
}
