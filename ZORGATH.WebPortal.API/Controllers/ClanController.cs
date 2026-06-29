namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[EnableRateLimiting(RateLimiterPolicies.Strict)]
public class ClanController(MerrickContext databaseContext, ILogger<ClanController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;

    private ILogger Logger { get; } = logger;

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
        // These Limits Mirror The Maximum Lengths Of The Corresponding Columns On The "Clan" Entity
        const int clanTitleMaximumLength = 250;
        const int clanLogoMaximumLength = 50;

        if (payload.Title.Length > clanTitleMaximumLength)
            return BadRequest($@"The Clan Title Must Not Exceed {clanTitleMaximumLength} Characters");

        if (payload.Logo.Length > clanLogoMaximumLength)
            return BadRequest($@"The Clan Logo Identifier Must Not Exceed {clanLogoMaximumLength} Characters");

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
