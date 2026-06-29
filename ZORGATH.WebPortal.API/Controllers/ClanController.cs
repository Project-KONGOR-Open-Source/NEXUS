namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[EnableRateLimiting(RateLimiterPolicies.Strict)]
public class ClanController(MerrickContext databaseContext, ILogger<ClanController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private ILogger Logger { get; } = logger;

    // These Limits Mirror The Maximum Lengths Of The Corresponding Columns On The "Clan" And "Account" Entities.
    private const int ClanTitleMaximumLength = 250;
    private const int ClanLogoMaximumLength = 64;
    private const int ClanMessageMaximumLength = 255;

    /// <summary>
    ///     Updates the title and logo of the authenticated account's clan.
    ///     Only a clan leader or officer may change these values.
    /// </summary>
    [HttpPatch("Details", Name = "Set Clan Details")]
    [Authorize(Policy = UserRoles.AllRoles)]
    [ProducesResponseType(typeof(ClanDetailsDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetClanDetails([FromBody] SetClanDetailsDTO payload)
    {
        if (payload.Title.Length > ClanTitleMaximumLength)
            return BadRequest($@"The Clan Title Must Not Exceed {ClanTitleMaximumLength} Characters");

        if (payload.Logo.Length > ClanLogoMaximumLength)
            return BadRequest($@"The Clan Logo Identifier Must Not Exceed {ClanLogoMaximumLength} Characters");

        Account? account = await ResolveCurrentAccount();

        if (account is null)
            return NotFound("The Authenticated Account Could Not Be Found");

        if (account.Clan is null)
            return BadRequest("The Authenticated Account Is Not A Member Of A Clan");

        if (account.ClanTier is not (ClanTier.Leader or ClanTier.Officer))
            return Forbid();

        account.Clan.Title = payload.Title;
        account.Clan.Logo = payload.Logo;

        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(@"Account ""{AccountName}"" Updated The Title And Logo Of Clan ""{ClanName}""", account.Name, account.Clan.Name);

        return Ok(new ClanDetailsDTO(account.Clan.Name, account.Clan.Tag, account.Clan.Title, account.Clan.Logo));
    }

    /// <summary>
    ///     Updates the authenticated account's per-member clan message.
    /// </summary>
    [HttpPatch("Message", Name = "Set Clan Message")]
    [Authorize(Policy = UserRoles.AllRoles)]
    [ProducesResponseType(typeof(SetClanMessageDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetClanMessage([FromBody] SetClanMessageDTO payload)
    {
        if (payload.Message.Length > ClanMessageMaximumLength)
            return BadRequest($@"The Clan Message Must Not Exceed {ClanMessageMaximumLength} Characters");

        Account? account = await ResolveCurrentAccount();

        if (account is null)
            return NotFound("The Authenticated Account Could Not Be Found");

        if (account.Clan is null)
            return BadRequest("The Authenticated Account Is Not A Member Of A Clan");

        account.ClanMessage = payload.Message;

        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(@"Account ""{AccountName}"" Updated Its Clan Message", account.Name);

        return Ok(new SetClanMessageDTO(account.ClanMessage));
    }

    /// <summary>
    ///     Resolves the account currently in context for the authenticated user, preferring the account named in the "account_id" claim and falling back to the user's main account.
    /// </summary>
    private async Task<Account?> ResolveCurrentAccount()
    {
        string emailAddress = User.Claims.GetUserEmailAddress();

        User? user = await MerrickContext.Users
            .Include(record => record.Accounts).ThenInclude(account => account.Clan)
            .SingleOrDefaultAsync(record => record.EmailAddress.Equals(emailAddress));

        if (user is null)
            return null;

        string? currentAccountIDClaimValue = User.Claims.SingleOrDefault(claim => claim.Type.Equals(Claims.AccountID))?.Value;

        Account? currentAccount = int.TryParse(currentAccountIDClaimValue, out int currentAccountID)
            ? user.Accounts.SingleOrDefault(account => account.ID.Equals(currentAccountID))
            : null;

        return currentAccount ?? user.Accounts.Single(account => account.IsMain);
    }
}
