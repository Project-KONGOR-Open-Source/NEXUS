namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
public class UserController(MerrickContext databaseContext, UserManager<User> userManager, IConfiguration configuration, IEmailService emailService, ILogger logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; init; } = databaseContext;
    private UserManager<User> UserManager { get; init; } = userManager;
    private IConfiguration Configuration { get; init; } = configuration;
    private IEmailService EmailService { get; init; } = emailService;
    private ILogger Logger { get; init; } = logger;

    [HttpPost("Register", Name = "Register User And Main Account")]
    public async Task<IActionResult> RegisterUserAndMainAccount([FromBody] RegisterUserAndMainAccountDTO payload)
    {
        if (payload.Password.Equals(payload.ConfirmPassword).Equals(false))
            return BadRequest($@"Password ""{payload.ConfirmPassword}"" Does Not Match ""{payload.Password}"" (These Values Are Only Visible To You)");

        IDbContextTransaction transaction = await MerrickContext.Database.BeginTransactionAsync();

        try
        {
            Token? token = await MerrickContext.Tokens.SingleOrDefaultAsync(token => token.Id.ToString().Equals(payload.Token) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

            if (token is null)
            {
                return NotFound($@"Email Registration Token ""{payload.Token}"" Was Not Found");
            }

            IActionResult result = EmailAddressHelpers.SanitiseEmailAddress(token.EmailAddress);

            if (result is not ContentResult contentResult) return result;

            if (contentResult.Content is null)
            {
                Logger.LogError($@"[BUG] Sanitised Email Address ""{token.EmailAddress}"" Is NULL");

                return UnprocessableEntity($@"Unable To Process Email Address ""{token.EmailAddress}""");
            }

            string sanitisedEmailAddress = contentResult.Content;

            if (await MerrickContext.Users.Where(user => user.SanitisedEmailAddress.Equals(sanitisedEmailAddress)).AnyAsync())
            {
                return Conflict($@"User With Email ""{token.EmailAddress}"" Already Exists");
            }

            if (await MerrickContext.Accounts.Where(account => account.Name.Equals(payload.Name)).AnyAsync())
            {
                return Conflict($@"Account With Name ""{payload.Name}"" Already Exists");
            }

            string salt = SRPRegistrationHelpers.GeneratePasswordSRPSalt(); // TODO: Maybe Put This In The Constructor

            User user = new()
            {
                Name = payload.Name,
                EmailAddress = token.EmailAddress,
                SanitisedEmailAddress = sanitisedEmailAddress,
                SRPSalt = SRPRegistrationHelpers.GeneratePasswordSalt(),
                SRPPasswordSalt = salt,
                SRPPasswordHash = SRPRegistrationHelpers.HashAccountPassword(payload.Password, salt),
            };

            IdentityResult attempt = await UserManager.CreateAsync(user, payload.Password);

            if (attempt.Errors.Any())
            {
                string errors = string.Join("; ", attempt.Errors.Select(error => error.Description));

                Logger.LogError($@"[BUG] Unable To Create Identity For User Name ""{payload.Name}"": {errors}");

                return UnprocessableEntity($@"Unable To Create Identity For User Name ""{payload.Name}""");
            }

            await UserManager.AddToRoleAsync(user, UserRoles.User);

            DateTime now = DateTime.UtcNow;

            Account account = new()
            {
                Name = payload.Name,
                User = user
            };

            user.Accounts.Add(account);

            MerrickContext.Tokens.Remove(token);

            await MerrickContext.SaveChangesAsync();

            await transaction.CommitAsync();

            await EmailService.SendEmailAddressRegistrationConfirmation(user.EmailAddress, account.Name);

            return CreatedAtAction(nameof(GetUser), new { id = user.Id },
                new { Id = user.Id, Name = user.Name, EmailAddress = user.EmailAddress, Accounts = user.Accounts.Select(record => record.Id).ToList() });
        }

        catch (Exception exception)
        {
            Logger.LogError(exception, $@"Unable To Create User And Account For Name ""{payload.Name}""");

            await transaction.RollbackAsync();

            return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
        }
    }

    [HttpGet("{id}", Name = "Get User")]
    [Authorize(Roles = UserRoles.AllRoles)]
    public async Task<IActionResult> GetUser(Guid id)
    {
        User? user = await UserManager.Users
            .Include(record => record.Accounts)
            .SingleOrDefaultAsync(record => record.Id.Equals(id));

        if (user is null) return NotFound($@"User With Id ""{id}"" Was Not Found");

        if (User.IsInRole(UserRoles.Administrator))
        {
            return Ok(new
            {
                Name = user.Name,
                EmailAddress = user.EmailAddress,
                Accounts = user.Accounts.Select(account => account.Id).ToList()
            });
        }

        else
        {
            return Ok(new
            {
                Name = user.Name,
                Accounts = user.Accounts.Select(account => account.Id).ToList()
            });
        }
    }
}
