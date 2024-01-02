//namespace ZORGATH.WebPortal.API.Controllers;

//[ApiController]
//[Route("[controller]")]
//[Consumes("application/json")]
//public class UserController(MerrickContext databaseContext, UserManager<User> userManager, IConfiguration configuration, IEmailService emailService, ILogger<UserController> logger) : ControllerBase
//{
//    private MerrickContext MerrickContext { get; init; } = databaseContext;
//    private UserManager<User> UserManager { get; init; } = userManager;
//    private IConfiguration Configuration { get; init; } = configuration;
//    private IEmailService EmailService { get; init; } = emailService;
//    private ILogger Logger { get; init; } = logger;

//    // TODO: Map All Request/Response Data To Contracts
//    // TODO: [OutputCache] On Get Requests

//    [HttpPost("Register", Name = "Register User And Main Account")]
//    [AllowAnonymous]
//    public async Task<IActionResult> RegisterUserAndMainAccount([FromBody] RegisterUserAndMainAccountDTO payload)
//    {
//        if (payload.Password.Equals(payload.ConfirmPassword).Equals(false))
//            return BadRequest($@"Password ""{payload.ConfirmPassword}"" Does Not Match ""{payload.Password}"" (These Values Are Only Visible To You)");

//        Token? token = await MerrickContext.Tokens.SingleOrDefaultAsync(token => token.ID.ToString().Equals(payload.Token) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

//        if (token is null)
//        {
//            return NotFound($@"Email Registration Token ""{payload.Token}"" Was Not Found");
//        }

//        IActionResult result = EmailAddressHelpers.SanitizeEmailAddress(token.EmailAddress);

//        if (result is not ContentResult contentResult)
//        {
//            return result;
//        }

//        if (contentResult.Content is null)
//        {
//            Logger.LogError($@"[BUG] Sanitized Email Address ""{token.EmailAddress}"" Is NULL");

//            return UnprocessableEntity($@"Unable To Process Email Address ""{token.EmailAddress}""");
//        }

//        string sanitizedEmailAddress = contentResult.Content;

//        if (await MerrickContext.Users.Where(user => user.SanitizedEmailAddress.Equals(sanitizedEmailAddress)).AnyAsync())
//        {
//            return Conflict($@"User With Email ""{token.EmailAddress}"" Already Exists");
//        }

//        if (await MerrickContext.Accounts.Where(account => account.Name.Equals(payload.Name)).AnyAsync())
//        {
//            return Conflict($@"Account With Name ""{payload.Name}"" Already Exists");
//        }

//        string salt = SRPRegistrationHandlers.GeneratePasswordSRPSalt(); // TODO: Maybe Put This In The Constructor

//        User user = new()
//        {
//            Name = payload.Name,
//            EmailAddress = token.EmailAddress,
//            SanitizedEmailAddress = sanitizedEmailAddress,
//            SRPSalt = SRPRegistrationHandlers.GeneratePasswordSalt(),
//            SRPPasswordSalt = salt,
//            SRPPasswordHash = SRPRegistrationHandlers.HashAccountPassword(payload.Password, salt),

//            // TODO: Resolve This Duplication (Try Not Using Identity At All)

//            UserName = payload.Name
//        };

//        IdentityResult attempt = await UserManager.CreateAsync(user, payload.Password);

//        if (attempt.Errors.Any())
//        {
//            string errors = string.Join("; ", attempt.Errors.Select(error => error.Description));

//            Logger.LogError($@"[BUG] Unable To Create Identity For User Name ""{payload.Name}"": {errors}");

//            return UnprocessableEntity($@"Unable To Create Identity For User Name ""{payload.Name}""");
//        }

//        await UserManager.AddToRoleAsync(user, UserRoles.User);

//        DateTime now = DateTime.UtcNow;

//        Account account = new()
//        {
//            Name = payload.Name,
//            User = user
//        };

//        user.Accounts.Add(account);

//        MerrickContext.Tokens.Remove(token);

//        await MerrickContext.SaveChangesAsync();

//        await EmailService.SendEmailAddressRegistrationConfirmation(user.EmailAddress, account.Name);

//        return CreatedAtAction(nameof(GetUser), new { id = user.ID },
//            new { Id = user.ID, Name = user.Name, EmailAddress = user.EmailAddress, Accounts = user.Accounts.Select(record => record.ID).ToList() });
//    }

//    [HttpGet("{id}", Name = "Get User")]
//    [Authorize(Roles = UserRoles.AllRoles)]
//    public async Task<IActionResult> GetUser(Guid id)
//    {
//        User? user = await UserManager.Users
//            .Include(record => record.Accounts)
//            .SingleOrDefaultAsync(record => record.ID.Equals(id));

//        if (user is null) return NotFound($@"User With ID ""{id}"" Was Not Found");

//        if (User.IsInRole(UserRoles.Administrator))
//        {
//            return Ok(new
//            {
//                Name = user.Name,
//                EmailAddress = user.EmailAddress,
//                Accounts = user.Accounts.Select(account => account.ID).ToList()
//            });
//        }

//        else
//        {
//            return Ok(new
//            {
//                Name = user.Name,
//                Accounts = user.Accounts.Select(account => account.ID).ToList()
//            });
//        }
//    }
//}
