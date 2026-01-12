using ASPIRE.Common.DTOs;
using MERRICK.DatabaseContext.Extensions;
using ASPIRE.Common.Constants;

namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[EnableRateLimiting(RateLimiterPolicies.Strict)]
public class UserController(
    MerrickContext databaseContext,
    ILogger<UserController> logger,
    IEmailService emailService,
    IOptions<OperationalConfiguration> configuration,
    IWebHostEnvironment hostEnvironment) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private ILogger Logger { get; } = logger;
    private IEmailService EmailService { get; } = emailService;
    private OperationalConfiguration Configuration { get; } = configuration.Value;
    private IWebHostEnvironment HostEnvironment { get; } = hostEnvironment;

    [HttpPost("Register", Name = "Register User And Main Account")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GetBasicUserDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterUserAndMainAccount([FromBody] RegisterUserAndMainAccountDTO payload)
    {
        if (payload.Password.Equals(payload.ConfirmPassword).Equals(false))
        {
            return BadRequest(
                $@"Password ""{payload.ConfirmPassword}"" Does Not Match ""{payload.Password}"" (These Values Are Only Visible To You)");
        }

        if (HostEnvironment.IsDevelopment() is false)
        {
            ValidationResult result = await new PasswordValidator().ValidateAsync(payload.Password);

            if (result.IsValid is false)
            {
                return BadRequest(result.Errors.Select(error => error.ErrorMessage));
            }
        }

        Token? token = await MerrickContext.Tokens.SingleOrDefaultAsync(token =>
            token.Value.ToString().Equals(payload.Token) &&
            token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        if (token is null)
        {
            return NotFound($@"Email Registration Token ""{payload.Token}"" Was Not Found");
        }

        if (token.TimestampConsumed is not null)
        {
            return Conflict($@"Email Registration Token ""{payload.Token}"" Has Already Been Consumed");
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

        Role? role = await MerrickContext.Roles.SingleOrDefaultAsync(role => role.Name.Equals(UserRoles.User));

        if (role is null)
        {
            return NotFound($@"User Role ""{UserRoles.User}"" Was Not Found");
        }

        string salt = SRPRegistrationHandlers.GenerateSRPPasswordSalt();

        User user = new()
        {
            EmailAddress = sanitizedEmailAddress,
            Role = role,
            SRPPasswordSalt = salt,
            SRPPasswordHash = SRPRegistrationHandlers.ComputeSRPPasswordHash(payload.Password, salt)
        };

        user.PBKDF2PasswordHash = new PasswordHasher<User>().HashPassword(user, payload.Password);

        await MerrickContext.Users.AddAsync(user);

        Account account = new() { Name = payload.Name, User = user, IsMain = true };

        user.Accounts.Add(account);

        token.TimestampConsumed = DateTimeOffset.UtcNow;

        await MerrickContext.SaveChangesAsync();

        await EmailService.SendEmailAddressRegistrationConfirmation(user.EmailAddress, account.Name);

        return CreatedAtAction(nameof(GetUser), new { id = user.ID },
            new GetBasicUserDTO(user.ID, user.EmailAddress, [new GetBasicAccountDTO(account.ID, account.Name)]));
    }

    [HttpPost("RegisterFromDiscord", Name = "Register User From Discord")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GetBasicUserDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterUserFromDiscord([FromBody] RegisterDiscordUserDTO payload)
    {
        if (await MerrickContext.Users.AnyAsync(user => user.EmailAddress.Equals(payload.Email)))
        {
            return Conflict($@"User With Email ""{payload.Email}"" Already Exists");
        }

        if (await MerrickContext.Accounts.AnyAsync(account => account.Name.Equals(payload.Name)))
        {
            return Conflict($@"Account With Name ""{payload.Name}"" Already Exists");
        }

        Role? role = await MerrickContext.Roles.SingleOrDefaultAsync(role => role.Name.Equals(UserRoles.User));

        if (role is null)
        {
            return NotFound($@"User Role ""{UserRoles.User}"" Was Not Found");
        }

        string salt = SRPRegistrationHandlers.GenerateSRPPasswordSalt();

        User user = new()
        {
            EmailAddress = payload.Email,
            Role = role,
            SRPPasswordSalt = salt,
            SRPPasswordHash = SRPRegistrationHandlers.ComputeSRPPasswordHash(payload.Password, salt)
        };

        // Initialize required PBKDF2 hash, though we use SRP primarily
        user.PBKDF2PasswordHash = new PasswordHasher<User>().HashPassword(user, payload.Password);

        await MerrickContext.Users.AddAsync(user);

        Account account = new() { Name = payload.Name, User = user, IsMain = true };

        user.Accounts.Add(account);

        await MerrickContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.ID },
            new GetBasicUserDTO(user.ID, user.EmailAddress, [new GetBasicAccountDTO(account.ID, account.Name)]));
    }

    [HttpPost("LogIn", Name = "Log In User")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GetAuthenticationTokenDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> LogInUser([FromBody] LogInUserDTO payload)
    {
        Account? account = await MerrickContext.Accounts
            .Include(account => account.User).ThenInclude(user => user.Role)
            .Include(account => account.Clan)
            .SingleOrDefaultAsync(account => account.Name.Equals(payload.Name));

        if (account is null)
        {
            return NotFound($@"Account ""{payload.Name}"" Was Not Found");
        }

        User user = account.User;

        PasswordVerificationResult result =
            new PasswordHasher<User>().VerifyHashedPassword(user, user.PBKDF2PasswordHash, payload.Password);

        if (result is not PasswordVerificationResult.Success)
        {
            return Unauthorized("Invalid User Name And/Or Password");
        }

        if (new[] { UserRoles.Administrator, UserRoles.User }.Contains(user.Role.Name).Equals(false))
        {
            Logger.LogError(@"[BUG] Unknown User Role ""{User.Role.Name}""", user.Role.Name);

            return UnprocessableEntity($@"Unknown User Role ""{user.Role.Name}""");
        }

        IEnumerable<Claim> userRoleClaims = user.Role.Name is UserRoles.Administrator
            ? UserRoleClaims.Administrator
            : UserRoleClaims.User;

        IEnumerable<Claim> openIDClaims = new List<Claim>
        {
            # region JWT Claims Documentation

            // OpenID (This Implementation): https://openid.net/specs/openid-connect-core-1_0.html#IDToken
            // auth0: https://auth0.com/docs/secure/tokens/json-web-tokens/json-web-token-claims
            // Internet Assigned Numbers Authority: https://www.iana.org/assignments/jwt/jwt.xhtml
            // RFC7519: https://www.rfc-editor.org/rfc/rfc7519.html#section-4

            # endregion

            new(JwtRegisteredClaimNames.Sub, account.Name, ClaimValueTypes.String),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.AuthTime, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Nonce, Guid.CreateVersion7().ToString(), ClaimValueTypes.String),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString(), ClaimValueTypes.String),
            new(JwtRegisteredClaimNames.Email, user.EmailAddress, ClaimValueTypes.Email)
        };

        IEnumerable<Claim> customClaims = new List<Claim>
        {
            new(Claims.UserID, user.ID.ToString(), ClaimValueTypes.String),
            new(Claims.AccountID, account.ID.ToString(), ClaimValueTypes.String),
            new(Claims.AccountIsMain, account.IsMain.ToString(), ClaimValueTypes.Boolean),
            new(Claims.ClanName, account.Clan?.Name ?? string.Empty, ClaimValueTypes.String),
            new(Claims.ClanTag, account.Clan?.Tag ?? string.Empty, ClaimValueTypes.String)
        };

        IEnumerable<Claim> allTokenClaims = Enumerable.Empty<Claim>().Union(userRoleClaims).Union(openIDClaims)
            .Union(customClaims).OrderBy(claim => claim.Type);

        JwtSecurityToken token = new
        (
            Configuration.JWT.Issuer,
            Configuration.JWT.Audience,
            allTokenClaims,
            expires: DateTimeOffset.UtcNow.AddHours(Configuration.JWT.DurationInHours).DateTime,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.JWT.SigningKey)),
                SecurityAlgorithms.HmacSha256) // TODO: Put The Signing Key In A Secrets Vault
        );

        string jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new GetAuthenticationTokenDTO(user.ID, "JWT", jwt));
    }

    [HttpGet("{id}", Name = "Get User")]
    [Authorize(UserRoles.AllRoles)]
    [ProducesResponseType(typeof(GetBasicUserDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(int id)
    {
        User? user = await MerrickContext.Users
            .Include(record => record.Role)
            .Include(record => record.Accounts).ThenInclude(record => record.Clan)
            .SingleOrDefaultAsync(record => record.ID.Equals(id));

        if (user is null)
        {
            return NotFound($@"User With ID ""{id}"" Was Not Found");
        }

        // TODO: [OutputCache] On Get Requests

        string role = User.Claims.GetUserRole();

        if (role.Equals(UserRoles.Administrator))
        {
            return Ok(new GetBasicUserDTO(user.ID, user.EmailAddress,
                user.Accounts.Select(account => new GetBasicAccountDTO(account.ID, account.GetNameWithClanTag())).ToList()));
        }

        if (role.Equals(UserRoles.User))
        {
            return Ok(new GetBasicUserDTO(user.ID,
                new string(user.EmailAddress.Select(character => char.IsLetterOrDigit(character) ? '*' : character)
                    .ToArray()),
                user.Accounts.Select(account => new GetBasicAccountDTO(account.ID, account.GetNameWithClanTag())).ToList()));
        }

        Logger.LogError(@"[BUG] Unknown User Role ""{User.Role}""", role);

        return BadRequest($@"Unknown User Role ""{role}""");
    }
}