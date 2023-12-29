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

        IActionResult result = SanitiseEmailAddress(payload.EmailAddress);

        if (result is not ContentResult contentResult) return result;

        string sanitisedEmailAddress = contentResult.Content ?? throw new NullReferenceException("Sanitised Email Address Is NULL");

        Token? token = await MerrickContext.Tokens.SingleOrDefaultAsync(token => token.EmailAddress.Equals(sanitisedEmailAddress) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        if (token is null)
        {
            token = new Token()
            {
                Purpose = TokenPurpose.EmailAddressVerification,
                EmailAddress = sanitisedEmailAddress,
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

    private static IActionResult SanitiseEmailAddress(string email)
    {
        if (ZORGATH.RunsInDevelopmentMode is false)
        {
            if (email.Split('@').First().Contains('+'))
                return new BadRequestObjectResult(@"Alias Creating Character ""+"" Is Not Allowed");

            string[] allowedEmailProviders = Enumerable.Empty<string>()
                .Concat(new[] { "outlook", "hotmail", "live", "msn" }) // Microsoft Outlook
                .Concat(new[] { "protonmail", "proton" }) // Proton Mail
                .Concat(new[] { "gmail", "googlemail" }) // Google Mail
                .Concat(new[] { "yahoo", "rocketmail", "ymail" }) // Yahoo Mail
                .Concat(new[] { "aol", "yandex", "gmx", "mail" }) // AOL Mail, Yandex Mail, GMX Mail, mail.com
                .Concat(new[] { "icloud", "me", "mac" }) // iCloud Mail
                .ToArray();

            Regex pattern = new(@"^(?<local>[a-zA-Z0-9_\-.]+)@(?<domain>[a-zA-Z]+)\.(?<tld>[a-zA-Z]{1,3}|co.uk)$");

            Match match = pattern.Match(email);

            if (match.Success.Equals(false))
                return new BadRequestObjectResult($@"Email Address ""{email}"" Is Not Valid");

            string local = match.Groups["local"].Value;
            string domain = match.Groups["domain"].Value;
            string tld = match.Groups["tld"].Value;

            if (allowedEmailProviders.Contains(domain).Equals(false))
                return new BadRequestObjectResult($@"Email Address Provider ""{domain}"" Is Not Allowed");

            // These Email Providers Ignore Period Characters
            // Users Can Create Aliases With The Same Email Address By Simply Adding Some Period Characters To The Local Part
            if (domain is "protonmail" or "proton" or "gmail" or "googlemail")
                local = local.Replace(".", string.Empty);

            email = $"{local}@{domain}.{tld}";
        }

        return new ContentResult
        {
            Content = email
        };
    }
}
