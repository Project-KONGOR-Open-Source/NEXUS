namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
public class UserController(MerrickContext databaseContext, ILogger<UserController> logger, IEmailService emailService) : ControllerBase
{
    private MerrickContext MerrickContext { get; init; } = databaseContext;
    private ILogger Logger { get; init; } = logger;
    private IEmailService EmailService { get; init; } = emailService;

    [HttpPost("Register", Name = "Register User And Main Account")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GetBasicUserDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterUserAndMainAccount([FromBody] RegisterUserAndMainAccountDTO payload)
    {
        if (payload.Password.Equals(payload.ConfirmPassword).Equals(false))
            return BadRequest($@"Password ""{payload.ConfirmPassword}"" Does Not Match ""{payload.Password}"" (These Values Are Only Visible To You)");

        Token? token = await MerrickContext.Tokens.SingleOrDefaultAsync(token => token.ID.ToString().Equals(payload.Token) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        if (token is null)
        {
            return NotFound($@"Email Registration Token ""{payload.Token}"" Was Not Found");
        }

        string sanitizedEmailAddress = token.Data;

        if (await MerrickContext.Users.AnyAsync(user => user.EmailAddress.Equals(sanitizedEmailAddress)))
        {
            return Conflict($@"User With Email ""{token.EmailAddress}"" Already Exists");
        }

        if (await MerrickContext.Accounts.AnyAsync(account => account.Name.Equals(payload.Name)))
        {
            return Conflict($@"Account With Name ""{payload.Name}"" Already Exists");
        }

        string salt = SRPRegistrationHandlers.GeneratePasswordSRPSalt();

        Role? role = await MerrickContext.Roles.SingleOrDefaultAsync(role => role.Name == UserRoles.User);

        if (role is null)
        {
            return NotFound($@"User Role ""{UserRoles.User}"" Was Not Found");
        }

        User user = new()
        {
            EmailAddress = sanitizedEmailAddress,
            Role = role,
            SRPSalt = SRPRegistrationHandlers.GeneratePasswordSalt(),
            SRPPasswordSalt = salt,
            SRPPasswordHash = SRPRegistrationHandlers.HashAccountPassword(payload.Password, salt)
        };

        await MerrickContext.Users.AddAsync(user);

        Account account = new()
        {
            Name = payload.Name,
            User = user,
            IsMain = true
        };

        user.Accounts.Add(account);

        token.TimestampConsumed = DateTime.UtcNow;

        await MerrickContext.SaveChangesAsync();

        await EmailService.SendEmailAddressRegistrationConfirmation(user.EmailAddress, account.Name);

        return CreatedAtAction(nameof(GetUser), new { id = user.ID },
            new GetBasicUserDTO(user.ID, user.EmailAddress, [new GetBasicAccountDTO(account.ID, account.Name)]));
    }

    [HttpGet("{id}", Name = "Get User")]
    [Authorize(Roles = UserRoles.AllRoles)]
    [ProducesResponseType(typeof(GetBasicUserDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(Guid id)
    {
        User? user = await MerrickContext.Users
            .Include(record => record.Accounts)
            .ThenInclude(record => record.Clan)
            .SingleOrDefaultAsync(record => record.ID.Equals(id));

        if (user is null)
            return NotFound($@"User With ID ""{id}"" Was Not Found");

        // TODO: [OutputCache] On Get Requests

        if (User.IsInRole(UserRoles.Administrator))
        {
            return Ok(new GetBasicUserDTO(user.ID, user.EmailAddress,
                user.Accounts.Select(account => new GetBasicAccountDTO(account.ID, account.NameWithClanTag)).ToList()));
        }

        if (User.IsInRole(UserRoles.User))
        {
            return Ok(new GetBasicUserDTO(user.ID,
                user.EmailAddress.Select(character => char.IsLetterOrDigit(character) ? '*' : character).ToString() ?? new string('*', user.EmailAddress.Length),
                user.Accounts.Select(account => new GetBasicAccountDTO(account.ID, account.NameWithClanTag)).ToList()));
        }

        // TODO: Get Role

        Logger.LogError("[BUG] Unknown Requester Role");

        return BadRequest("Unknown Requester Role");
    }
}
