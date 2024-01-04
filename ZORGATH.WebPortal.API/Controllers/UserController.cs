namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
public class UserController(MerrickContext databaseContext, ILogger<UserController> logger, IConfiguration configuration, IEmailService emailService) : ControllerBase
{
    private MerrickContext MerrickContext { get; init; } = databaseContext;
    private ILogger Logger { get; init; } = logger;
    private IConfiguration Configuration { get; init; } = configuration;
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

        Role? role = await MerrickContext.Roles.SingleOrDefaultAsync(role => role.Name.Equals(UserRoles.User));

        if (role is null)
        {
            return NotFound($@"User Role ""{UserRoles.User}"" Was Not Found");
        }

        string salt = SRPRegistrationHandlers.GeneratePasswordSRPSalt();

        User user = new()
        {
            EmailAddress = sanitizedEmailAddress,
            Role = role,
            SRPSalt = SRPRegistrationHandlers.GeneratePasswordSalt(),
            SRPPasswordSalt = salt,
            SRPPasswordHash = SRPRegistrationHandlers.HashPassword(payload.Password, salt)
        };

        user.PBKDF2PasswordHash = new PasswordHasher<User>().HashPassword(user, payload.Password);

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

    [HttpPost("LogIn", Name = "Log In User")]
    [AllowAnonymous]
    // TODO: Add Responses
    public async Task<IActionResult> LogInUser([FromBody] LogInUserDTO payload)
    {
        Account? account = await MerrickContext.Accounts
            .Include(account => account.User).ThenInclude(user => user.Role)
            .Include(account => account.Clan)
            .SingleOrDefaultAsync(account => account.Name.Equals(payload.Name));

        if (account is null)
            return NotFound($@"Account ""{payload.Name}"" Was Not Found");

        User user = account.User;

        PasswordVerificationResult result = new PasswordHasher<User>().VerifyHashedPassword(user, user.PBKDF2PasswordHash, payload.Password);

        if (result is not PasswordVerificationResult.Success)
            return Unauthorized("Invalid User Name And/Or Password");

        if (new[] { UserRoles.Administrator, UserRoles.User }.Contains(user.Role.Name).Equals(false))
        {
            Logger.LogError($@"[BUG] Unknown User Role ""{user.Role.Name}""");

            return UnprocessableEntity($@"Unknown User Role ""{user.Role.Name}""");
        }

        IEnumerable<Claim> userRoleClaims = user.Role.Name is UserRoles.Administrator ? UserRoleClaims.Administrator : UserRoleClaims.User;

        IEnumerable<Claim> openIDClaims = new List<Claim>
        {
            # region JWT Claims Documentation
            // OpenID (This Implementation): https://openid.net/specs/openid-connect-core-1_0.html#IDToken
            // auth0: https://auth0.com/docs/secure/tokens/json-web-tokens/json-web-token-claims
            // Internet Assigned Numbers Authority: https://www.iana.org/assignments/jwt/jwt.xhtml
            // RFC7519: https://www.rfc-editor.org/rfc/rfc7519.html#section-4
            # endregion

            new(JwtRegisteredClaimNames.Sub, account.Name, ClaimValueTypes.String),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.AuthTime, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Nonce, Guid.NewGuid().ToString(), ClaimValueTypes.String),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString(), ClaimValueTypes.String),
            new(JwtRegisteredClaimNames.Email, user.EmailAddress, ClaimValueTypes.Email)
        };

        IEnumerable<Claim> customClaims = new List<Claim>
        {
            new("user_id", user.ID.ToString(), ClaimValueTypes.String),
            new("account_id", account.ID.ToString(), ClaimValueTypes.String),
            new("account_is_main", account.IsMain.ToString(), ClaimValueTypes.Boolean),
            new("clan_name", account.Clan?.Name ?? string.Empty, ClaimValueTypes.String),
            new("clan_tag", account.Clan?.Tag ?? string.Empty, ClaimValueTypes.String)
        };

        IEnumerable<Claim> allTokenClaims = Enumerable.Empty<Claim>().Union(userRoleClaims).Union(openIDClaims).Union(customClaims).OrderBy(claim => claim.Type);

        string? tokenSigningKey = Configuration["JWT:SigningKey"]; // TODO: Put The Signing Key In A Secrets Vault

        if (tokenSigningKey is null)
        {
            Logger.LogError("[BUG] JSON Web Token Signing Key Is NULL");

            return StatusCode(StatusCodes.Status500InternalServerError, "Unable To Generate Authentication Token");
        }

        JwtSecurityToken token = new
        (
            issuer: Configuration["JWT:Issuer"],
            audience: Configuration["JWT:Audience"],
            claims: allTokenClaims,
            expires: DateTime.UtcNow.AddHours(Convert.ToInt32(Configuration["JWT:DurationInHours"])),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSigningKey)), SecurityAlgorithms.HmacSha256)
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
    public async Task<IActionResult> GetUser(Guid id)
    {
        User? user = await MerrickContext.Users
            .Include(record => record.Role)
            .Include(record => record.Accounts).ThenInclude(record => record.Clan)
            .SingleOrDefaultAsync(record => record.ID.Equals(id));

        if (user is null)
            return NotFound($@"User With ID ""{id}"" Was Not Found");

        // TODO: [OutputCache] On Get Requests

        string role = User.Claims.GetUserRole();

        if (role.Equals(UserRoles.Administrator))
        {
            return Ok(new GetBasicUserDTO(user.ID, user.EmailAddress,
                user.Accounts.Select(account => new GetBasicAccountDTO(account.ID, account.NameWithClanTag)).ToList()));
        }

        if (role.Equals(UserRoles.User))
        {
            return Ok(new GetBasicUserDTO(user.ID,
                new string(user.EmailAddress.Select(character => char.IsLetterOrDigit(character) ? '*' : character).ToArray()),
                user.Accounts.Select(account => new GetBasicAccountDTO(account.ID, account.NameWithClanTag)).ToList()));
        }

        Logger.LogError($@"[BUG] Unknown User Role ""{role}""");

        return BadRequest($@"Unknown User Role ""{role}""");
    }
}
