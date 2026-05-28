namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[EnableRateLimiting(RateLimiterPolicies.Strict)]
public class AccountsController(MerrickContext databaseContext, ILogger<AccountsController> logger, IWebHostEnvironment hostEnvironment) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private ILogger Logger { get; } = logger;
    private IWebHostEnvironment HostEnvironment { get; } = hostEnvironment;

    /// <summary>
    ///     Account types that may be created by calling the endpoint associated with the <see cref="CreateAccountFromToken"/> action method.
    ///     Tokens carrying any other <see cref="AccountType"/> in their <see cref="Token.Data"/> are rejected.
    /// </summary>
    private static readonly HashSet<AccountType> RedeemableAccountTypes = [AccountType.ServerHost];

    [HttpPost("{token:guid}", Name = "Create Sub-Account From Authorisation Token")]
    [Authorize(Policy = UserRoles.AllRoles)]
    [ProducesResponseType(typeof(GetBasicAccountDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(string), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(string), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAccountFromToken(Guid token, [FromBody] CreateSubAccountDTO payload)
    {
        ValidationResult accountNameValidationResult = await new AccountNameValidator(HostEnvironment.IsDevelopment()).ValidateAsync(payload.Name);

        if (accountNameValidationResult.IsValid is false)
            return BadRequest(accountNameValidationResult.Errors.Select(error => error.ErrorMessage));

        Token? authorisationToken = await MerrickContext.Tokens.SingleOrDefaultAsync(record => record.Value.Equals(token));

        if (authorisationToken is null)
            return NotFound($@"Authorisation Token ""{token}"" Was Not Found");

        if (authorisationToken.TimestampConsumed is not null)
            return Conflict($@"Authorisation Token ""{token}"" Has Already Been Consumed");

        if (authorisationToken.TimestampCreated + authorisationToken.Validity < DateTimeOffset.UtcNow)
            return StatusCode(StatusCodes.Status410Gone, $@"Authorisation Token ""{token}"" Has Expired");

        if (Enum.TryParse(authorisationToken.Data, out AccountType requestedAccountType).Equals(false))
        {
            Logger.LogError(@"[BUG] Authorisation Token ""{Token}"" Has Unparseable Account Type Data ""{Data}""", token, authorisationToken.Data);

            return UnprocessableEntity($@"Authorisation Token ""{token}"" Has An Invalid Account Type");
        }

        if (RedeemableAccountTypes.Contains(requestedAccountType).Equals(false))
            return UnprocessableEntity($@"Account Type ""{requestedAccountType}"" Is Not Redeemable Via This Endpoint");

        string authenticatedUserEmailAddress = User.Claims.GetUserEmailAddress();

        User? user = await MerrickContext.Users
            .Include(record => record.Accounts)
            .SingleOrDefaultAsync(record => record.EmailAddress.Equals(authenticatedUserEmailAddress));

        if (user is null)
            return NotFound($@"User With Email Address ""{authenticatedUserEmailAddress}"" Was Not Found");

        if (await MerrickContext.Accounts.AnyAsync(account => account.Name.Equals(payload.Name)))
            return Conflict($@"Account With Name ""{payload.Name}"" Already Exists");

        Account account = new ()
        {
            Name = payload.Name,
            User = user,
            Type = requestedAccountType,
            IsMain = false
        };

        user.Accounts.Add(account);

        authorisationToken.EmailAddress = authenticatedUserEmailAddress;
        authorisationToken.TimestampConsumed = DateTimeOffset.UtcNow;

        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(@"Created Sub-Account ""{AccountName}"" Of Type ""{AccountType}"" For User ""{EmailAddress}""", account.Name, requestedAccountType, authenticatedUserEmailAddress);

        GetBasicAccountDTO response = new (account.ID, account.Name);

        return CreatedAtAction(nameof(CreateAccountFromToken), new { token }, response);
    }
}
