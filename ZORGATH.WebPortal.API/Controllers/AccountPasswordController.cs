namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("Password")]
[Consumes("application/json")]
[EnableRateLimiting(RateLimiterPolicies.Strict)]
public class AccountPasswordController(MerrickContext databaseContext, ILogger<AccountPasswordController> logger, IEmailService emailService, IWebHostEnvironment hostEnvironment) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private ILogger Logger { get; } = logger;
    private IEmailService EmailService { get; } = emailService;
    private IWebHostEnvironment HostEnvironment { get; } = hostEnvironment;

    [HttpPost("Reset/Request", Name = "Request Account Password Reset")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(string), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> RequestAccountPasswordReset(RequestAccountPasswordResetDTO payload)
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
        const string successMessage = "Account Password Reset Token Was Successfully Issued";

        List<string> accountNames = await MerrickContext.Users
            .Where(user => user.EmailAddress.Equals(sanitizedEmailAddress))
            .SelectMany(user => user.Accounts.Select(account => account.Name))
            .ToListAsync();

        // Do Not Send An Email If There Are No Accounts Linked To The Email Address
        if (accountNames.Count is 0)
            return Ok(successMessage);

        Token? existingToken = await MerrickContext.Tokens.SingleOrDefaultAsync(token =>
            token.Purpose.Equals(TokenPurpose.AccountPasswordReset)
            && token.EmailAddress.Equals(payload.EmailAddress)
            && token.TimestampConsumed == null);

        if (existingToken is not null)
            return Ok(successMessage);

        // Generate A Random Password And Pre-Compute The Hashes
        string generatedPassword = AccountPasswordGenerationHelpers.GenerateRandomPassword();

        string salt = SRPRegistrationHandlers.GenerateSRPPasswordSalt();
        string srpHash = SRPRegistrationHandlers.ComputeSRPPasswordHash(generatedPassword, salt);
        string pbkdf2Hash = new PasswordHasher<User>().HashPassword(null!, generatedPassword);

        AccountPasswordTokenData tokenData = new(sanitizedEmailAddress, salt, srpHash, pbkdf2Hash);

        Token token = new()
        {
            Purpose = TokenPurpose.AccountPasswordReset,
            EmailAddress = payload.EmailAddress,
            Value = Guid.CreateVersion7(),
            Data = JsonSerializer.Serialize(tokenData)
        };

        await MerrickContext.Tokens.AddAsync(token);
        await MerrickContext.SaveChangesAsync();

        bool sent = await EmailService.SendAccountPasswordResetLink(payload.EmailAddress, token.Value.ToString(), generatedPassword, accountNames);

        if (sent.Equals(false))
        {
            MerrickContext.Tokens.Remove(token);
            await MerrickContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Failed To Send Account Password Reset Email");
        }

        return Ok(successMessage);
    }

    [HttpPost("Reset/Confirm", Name = "Confirm Account Password Reset")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmAccountPasswordReset(ConfirmAccountPasswordResetDTO payload)
    {
        Token? token = await MerrickContext.Tokens.SingleOrDefaultAsync(token =>
            token.Value.ToString().Equals(payload.Token)
            && token.Purpose.Equals(TokenPurpose.AccountPasswordReset)
            && token.TimestampConsumed == null);

        if (token is null)
            return NotFound($@"Account Password Reset Token ""{payload.Token}"" Was Not Found");

        AccountPasswordTokenData? tokenData = JsonSerializer.Deserialize<AccountPasswordTokenData>(token.Data);

        if (tokenData is null)
        {
            Logger.LogError(@"[BUG] Failed To Deserialise Account Password Token Data For Token ""{Token}""", payload.Token);

            return UnprocessableEntity("Failed To Process Account Password Reset Token");
        }

        User? user = await MerrickContext.Users.Include(user => user.Accounts).SingleOrDefaultAsync(user => user.EmailAddress.Equals(tokenData.SanitizedEmailAddress));

        if (user is null)
            return NotFound($@"User With Email Address ""{token.EmailAddress}"" Was Not Found");

        user.SRPPasswordSalt = tokenData.SRPPasswordSalt;
        user.SRPPasswordHash = tokenData.SRPPasswordHash;
        user.PBKDF2PasswordHash = tokenData.PBKDF2PasswordHash;

        token.TimestampConsumed = DateTimeOffset.UtcNow;

        await MerrickContext.SaveChangesAsync();

        List<string> accountNames = user.Accounts.Select(account => account.Name).ToList();

        bool sent = await EmailService.SendAccountPasswordResetConfirmation(user.EmailAddress, accountNames);

        if (sent.Equals(false))
            Logger.LogWarning("Account Password Was Reset Successfully But A Confirmation Email Could Not Be Sent To {EmailAddress}", user.EmailAddress);

        return Ok("Account Password Was Reset Successfully");
    }

    [HttpPost("Update/Request", Name = "Request Account Password Update")]
    [Authorize(Policy = UserRoles.AllRoles)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> RequestAccountPasswordUpdate(RequestAccountPasswordUpdateDTO payload)
    {
        if (payload.Password.Equals(payload.ConfirmPassword).Equals(false))
            return BadRequest($@"Password ""{payload.ConfirmPassword}"" Does Not Match ""{payload.Password}"" (These Values Are Only Visible To You)");

        // Validate Password Strength In Non-Development Environments
        if (HostEnvironment.IsDevelopment() is false)
        {
            ValidationResult passwordValidationResult = await new PasswordValidator().ValidateAsync(payload.Password);

            if (passwordValidationResult.IsValid is false)
                return BadRequest(passwordValidationResult.Errors.Select(error => error.ErrorMessage));
        }

        string userEmailAddress = User.Claims.GetUserEmailAddress();

        User? user = await MerrickContext.Users.Include(user => user.Accounts).SingleOrDefaultAsync(user => user.EmailAddress.Equals(userEmailAddress));

        if (user is null)
            return NotFound($@"User With Email Address ""{userEmailAddress}"" Was Not Found");

        PasswordVerificationResult passwordVerificationResult = new PasswordHasher<User>().VerifyHashedPassword(user, user.PBKDF2PasswordHash, payload.CurrentPassword);

        if (passwordVerificationResult is not PasswordVerificationResult.Success)
            return Unauthorized("The Submitted Current Password Is Incorrect");

        // Pre-Compute The New Password Hashes
        string salt = SRPRegistrationHandlers.GenerateSRPPasswordSalt();
        string srpHash = SRPRegistrationHandlers.ComputeSRPPasswordHash(payload.Password, salt);
        string pbkdf2Hash = new PasswordHasher<User>().HashPassword(user, payload.Password);

        AccountPasswordTokenData tokenData = new(userEmailAddress, salt, srpHash, pbkdf2Hash);

        Token? existingToken = await MerrickContext.Tokens.SingleOrDefaultAsync(token => token.Purpose.Equals(TokenPurpose.AccountPasswordUpdate) && token.EmailAddress.Equals(userEmailAddress) && token.TimestampConsumed == null);

        if (existingToken is not null) // Invalidate Any Pending Token So Old Confirmation Links Cannot Be Used To Confirm A Stale Or Different Target Account Password
        {
            MerrickContext.Tokens.Remove(existingToken);

            await MerrickContext.SaveChangesAsync();
        }

        Token token = new ()
        {
            Purpose = TokenPurpose.AccountPasswordUpdate,
            EmailAddress = userEmailAddress,
            Value = Guid.CreateVersion7(),
            Data = JsonSerializer.Serialize(tokenData)
        };

        await MerrickContext.Tokens.AddAsync(token);
        await MerrickContext.SaveChangesAsync();

        List<string> accountNames = user.Accounts.Select(account => account.Name).ToList();

        bool sent = await EmailService.SendAccountPasswordUpdateLink(userEmailAddress, token.Value.ToString(), accountNames);

        if (sent.Equals(false))
        {
            MerrickContext.Tokens.Remove(token);
            await MerrickContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Failed To Send Account Password Update Confirmation Email");
        }

        return Ok("Account Password Update Token Was Successfully Issued");
    }

    [HttpPost("Update/Confirm", Name = "Confirm Account Password Update")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmAccountPasswordUpdate(ConfirmAccountPasswordUpdateDTO payload)
    {
        Token? token = await MerrickContext.Tokens.SingleOrDefaultAsync(token =>
            token.Value.ToString().Equals(payload.Token)
            && token.Purpose.Equals(TokenPurpose.AccountPasswordUpdate)
            && token.TimestampConsumed == null);

        if (token is null)
            return NotFound($@"Account Password Update Token ""{payload.Token}"" Was Not Found");

        AccountPasswordTokenData? tokenData = JsonSerializer.Deserialize<AccountPasswordTokenData>(token.Data);

        if (tokenData is null)
        {
            Logger.LogError(@"[BUG] Failed To Deserialise Account Password Token Data For Token ""{Token}""", payload.Token);

            return UnprocessableEntity("Failed To Process Account Password Update Token");
        }

        User? user = await MerrickContext.Users.Include(user => user.Accounts).SingleOrDefaultAsync(user => user.EmailAddress.Equals(tokenData.SanitizedEmailAddress));

        if (user is null)
            return NotFound($@"User With Email Address ""{token.EmailAddress}"" Was Not Found");

        user.SRPPasswordSalt = tokenData.SRPPasswordSalt;
        user.SRPPasswordHash = tokenData.SRPPasswordHash;
        user.PBKDF2PasswordHash = tokenData.PBKDF2PasswordHash;

        token.TimestampConsumed = DateTimeOffset.UtcNow;

        await MerrickContext.SaveChangesAsync();

        List<string> accountNames = user.Accounts.Select(account => account.Name).ToList();

        bool sent = await EmailService.SendAccountPasswordUpdateConfirmation(user.EmailAddress, accountNames);

        if (sent.Equals(false))
            Logger.LogWarning("Account Password Was Updated Successfully But A Confirmation Email Could Not Be Sent To {EmailAddress}", user.EmailAddress);

        return Ok("Account Password Was Updated Successfully");
    }
}
