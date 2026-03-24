namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[EnableRateLimiting(RateLimiterPolicies.Strict)]
public class EmailAddressController(MerrickContext databaseContext, ILogger<EmailAddressController> logger, IEmailService emailService, IWebHostEnvironment hostEnvironment) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private ILogger Logger { get; } = logger;
    private IEmailService EmailService { get; } = emailService;
    private IWebHostEnvironment HostEnvironment { get; } = hostEnvironment;

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
            IActionResult result = EmailAddressHelpers.SanitizeEmailAddress(payload.EmailAddress, HostEnvironment);

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

    [HttpPost("Recover", Name = "Request Account Password Recovery")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(string), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> RequestAccountPasswordRecovery(RequestAccountPasswordRecoveryDTO payload)
    {
        IActionResult result = EmailAddressHelpers.SanitizeEmailAddress(payload.EmailAddress, HostEnvironment);

        if (result is not ContentResult contentResult)
            return result;

        if (contentResult.Content is null)
        {
            Logger.LogError(@"[BUG] Sanitized Email Address ""{Payload.EmailAddress}"" Is NULL", payload.EmailAddress);

            return UnprocessableEntity($@"Unable To Process Email Address ""{payload.EmailAddress}""");
        }

        string sanitizedEmailAddress = contentResult.Content;

        // Always Return Success To Prevent Email Enumeration Attacks
        const string successMessage = "Account Password Recovery Token Was Successfully Issued";

        List<string> accountNames = await MerrickContext.Users
            .Where(user => user.EmailAddress.Equals(sanitizedEmailAddress))
            .SelectMany(user => user.Accounts.Select(account => account.Name))
            .ToListAsync();

        // Do Not Send An Email If There Are No Accounts Linked To The Email Address
        if (accountNames.Count is 0)
            return Ok(successMessage);

        Token? token = await MerrickContext.Tokens.SingleOrDefaultAsync(token => token.Data.Equals(sanitizedEmailAddress) && token.Purpose.Equals(TokenPurpose.AccountPasswordRecovery) && token.TimestampConsumed == null);

        if (token is null)
        {
            token = new Token()
            {
                Purpose = TokenPurpose.AccountPasswordRecovery,
                EmailAddress = payload.EmailAddress,
                Value = Guid.CreateVersion7(),
                Data = sanitizedEmailAddress
            };

            await MerrickContext.Tokens.AddAsync(token);
            await MerrickContext.SaveChangesAsync();
        }

        bool sent = await EmailService.SendAccountPasswordRecoveryLink(payload.EmailAddress, token.Value.ToString(), accountNames);

        if (sent.Equals(false))
        {
            MerrickContext.Tokens.Remove(token);

            await MerrickContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Failed To Send Account Password Recovery Email");
        }

        return Ok(successMessage);
    }

    [HttpPost("Reset", Name = "Reset Account Password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ResetAccountPassword(ResetAccountPasswordDTO payload)
    {
        if (payload.Password.Equals(payload.ConfirmPassword).Equals(false))
            return BadRequest($@"Account Password ""{payload.ConfirmPassword}"" Does Not Match ""{payload.Password}"" (These Values Are Only Visible To You)");

        if (HostEnvironment.IsDevelopment() is false)
        {
            ValidationResult validationResult = await new PasswordValidator().ValidateAsync(payload.Password);

            if (validationResult.IsValid is false)
                return BadRequest(validationResult.Errors.Select(error => error.ErrorMessage));
        }

        Token? token = await MerrickContext.Tokens.SingleOrDefaultAsync(token => token.Value.ToString().Equals(payload.Token) && token.Purpose.Equals(TokenPurpose.AccountPasswordRecovery) && token.TimestampConsumed == null);

        if (token is null)
            return NotFound($@"Account Password Recovery Token ""{payload.Token}"" Was Not Found");

        string sanitizedEmailAddress = token.Data;

        User? user = await MerrickContext.Users.Include(user => user.Accounts).SingleOrDefaultAsync(user => user.EmailAddress.Equals(sanitizedEmailAddress));

        if (user is null)
            return NotFound($@"User With Email Address ""{token.EmailAddress}"" Was Not Found");

        string salt = SRPRegistrationHandlers.GenerateSRPPasswordSalt();

        user.SRPPasswordSalt = salt;
        user.SRPPasswordHash = SRPRegistrationHandlers.ComputeSRPPasswordHash(payload.Password, salt);
        user.PBKDF2PasswordHash = new PasswordHasher<User>().HashPassword(user, payload.Password);

        token.TimestampConsumed = DateTimeOffset.UtcNow;

        await MerrickContext.SaveChangesAsync();

        List<string> accountNames = user.Accounts.Select(account => account.Name).ToList();

        bool sent = await EmailService.SendAccountPasswordRecoveryConfirmation(user.EmailAddress, accountNames);

        if (sent.Equals(false))
            Logger.LogWarning("Account Password Was Reset Successfully But A Confirmation Email Could Not Be Sent To {EmailAddress}", user.EmailAddress);

        return Ok($@"Account Password For Email Address ""{token.EmailAddress}"" Was Reset Successfully");
    }

    [HttpPost("Request", Name = "Request Email Address Update")]
    [Authorize(Policy = UserRoles.AllRoles)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(string), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(string), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> RequestEmailAddressUpdate(RequestEmailAddressUpdateDTO payload)
    {
        string userEmailAddress = User.Claims.GetUserEmailAddress();

        User? user = await MerrickContext.Users.SingleOrDefaultAsync(user => user.EmailAddress.Equals(userEmailAddress));

        if (user is null)
            return NotFound($@"User With Email Address ""{userEmailAddress}"" Was Not Found");

        PasswordVerificationResult passwordVerificationResult = new PasswordHasher<User>().VerifyHashedPassword(user, user.PBKDF2PasswordHash, payload.Password);

        if (passwordVerificationResult is not PasswordVerificationResult.Success)
            return Unauthorized("The Submitted Password Is Incorrect");

        IActionResult result = EmailAddressHelpers.SanitizeEmailAddress(payload.EmailAddress, HostEnvironment);

        if (result is not ContentResult contentResult)
            return result;

        if (contentResult.Content is null)
        {
            Logger.LogError(@"[BUG] Sanitized Email Address ""{Payload.EmailAddress}"" Is NULL", payload.EmailAddress);

            return UnprocessableEntity($@"Unable To Process Email Address ""{payload.EmailAddress}""");
        }

        string sanitizedEmailAddress = contentResult.Content;

        if (user.EmailAddress.Equals(sanitizedEmailAddress))
            return BadRequest("New Email Address Cannot Be The Same As The Current Email Address");

        if (await MerrickContext.Users.AnyAsync(existingUser => existingUser.EmailAddress.Equals(sanitizedEmailAddress)))
            return Conflict($@"Email Address ""{payload.EmailAddress}"" Is Already In Use");

        Token? token = await MerrickContext.Tokens.SingleOrDefaultAsync(token => token.EmailAddress.Equals(user.EmailAddress) && token.Purpose.Equals(TokenPurpose.EmailAddressUpdate) && token.TimestampConsumed == null);

        if (token is null)
        {
            token = new Token()
            {
                Purpose = TokenPurpose.EmailAddressUpdate,
                EmailAddress = user.EmailAddress,
                Value = Guid.CreateVersion7(),
                Data = sanitizedEmailAddress
            };

            await MerrickContext.Tokens.AddAsync(token);
            await MerrickContext.SaveChangesAsync();
        }

        bool sent = await EmailService.SendEmailAddressUpdateLink(sanitizedEmailAddress, token.Value.ToString());

        if (sent.Equals(false))
        {
            MerrickContext.Tokens.Remove(token);

            await MerrickContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Failed To Send Email Address Update Email");
        }

        return Ok("Email Address Update Token Was Successfully Issued");
    }

    [HttpPost("Update", Name = "Confirm Email Address Update")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ConfirmEmailAddressUpdate(ConfirmEmailAddressUpdateDTO payload)
    {
        Token? token = await MerrickContext.Tokens.SingleOrDefaultAsync(token => token.Value.ToString().Equals(payload.Token) && token.Purpose.Equals(TokenPurpose.EmailAddressUpdate) && token.TimestampConsumed == null);

        if (token is null)
            return NotFound($@"Email Address Update Token ""{payload.Token}"" Was Not Found");

        User? user = await MerrickContext.Users.SingleOrDefaultAsync(user => user.EmailAddress.Equals(token.EmailAddress));

        if (user is null)
            return NotFound($@"User With Email Address ""{token.EmailAddress}"" Was Not Found");

        string oldEmailAddress = user.EmailAddress;
        string newEmailAddress = token.Data;

        user.EmailAddress = newEmailAddress;

        token.TimestampConsumed = DateTimeOffset.UtcNow;

        await MerrickContext.SaveChangesAsync();

        bool sent = await EmailService.SendEmailAddressUpdateConfirmation(oldEmailAddress, newEmailAddress);

        if (sent.Equals(false))
            Logger.LogWarning("Email Address Was Updated Successfully But A Confirmation Email Could Not Be Sent To {EmailAddress}", newEmailAddress);

        return Ok("Email Address Was Updated Successfully");
    }
}
