namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
public class EmailController(MerrickContext databaseContext, UserManager<User> userManager, IEmailService emailService, ILogger<EmailController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; init; } = databaseContext;
    private UserManager<User> UserManager { get; init; } = userManager;
    private IEmailService EmailService { get; init; } = emailService;
    private ILogger Logger { get; init; } = logger;

    [HttpPost("Register", Name = "Register Email Address")]
    public async Task<IActionResult> RegisterEmailAddress(RegisterEmailAddressDTO payload)
    {
        if (payload.EmailAddress.Equals(payload.ConfirmEmailAddress).Equals(false))
            return BadRequest($@"Email Address ""{payload.ConfirmEmailAddress}"" Does Not Match ""{payload.EmailAddress}""");

        Token? token = await MerrickContext.Tokens.SingleOrDefaultAsync(token => token.EmailAddress.Equals(payload.EmailAddress) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        if (token is null)
        {
            token = new Token()
            {
                Purpose = TokenPurpose.EmailAddressVerification,
                EmailAddress = payload.EmailAddress,
                Data = Request.HttpContext.Connection.RemoteIpAddress is not null
                    ? Request.HttpContext.Connection.RemoteIpAddress.ToString()
                    : Request.HttpContext.Connection.LocalIpAddress is not null
                        ? Request.HttpContext.Connection.LocalIpAddress.ToString()
                        : string.Empty
            };

            await MerrickContext.Tokens.AddAsync(token);
            await MerrickContext.SaveChangesAsync();

            bool sent = await EmailService.SendEmailAddressRegistrationLink(payload.EmailAddress, token.Id.ToString());

            if (sent.Equals(false))
            {
                MerrickContext.Tokens.Remove(token);
                await MerrickContext.SaveChangesAsync();

                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Failed To Send Email Address Verification Email");
            }
        }

        else return BadRequest($@"A Registration Request For Email Address ""{payload.EmailAddress}"" Has Already Been Made (Check Your Email Inbox For A Registration Link)");

        return Ok($@"Email Address Registration Token Was Successfully Created, And An Email Was Sent To Address ""{payload.EmailAddress}""");
    }
}
