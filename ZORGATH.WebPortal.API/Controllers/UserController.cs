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

        IEnumerable<Claim> openIDClaims = new List<Claim>
        {
            # region JWT Claims Documentation
            // OpenID (This Implementation): https://openid.net/specs/openid-connect-core-1_0.html#IDToken
            // auth0: https://auth0.com/docs/secure/tokens/json-web-tokens/json-web-token-claims
            // Internet Assigned Numbers Authority: https://www.iana.org/assignments/jwt/jwt.xhtml
            // RFC7519: https://www.rfc-editor.org/rfc/rfc7519.html#section-4
            # endregion

            new(JwtRegisteredClaimNames.Iss, "TODO: Get The Hosting URL From Configuration Or Request Data"),
            new(JwtRegisteredClaimNames.Sub, account.Name),
            new(JwtRegisteredClaimNames.Aud, $"{user.ID}:{account.ID}"),
            new(JwtRegisteredClaimNames.Exp, Convert.ToInt64((DateTime.UtcNow.AddHours(24) - DateTime.MinValue).TotalSeconds).ToString()),
            new(JwtRegisteredClaimNames.Iat, Convert.ToInt64((DateTime.UtcNow - DateTime.MinValue).TotalSeconds).ToString()),
            new(JwtRegisteredClaimNames.AuthTime, Convert.ToInt64((DateTime.UtcNow - DateTime.MinValue).TotalSeconds).ToString()),
            new(JwtRegisteredClaimNames.Nonce, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, user.EmailAddress)
        };

        IEnumerable<Claim> customClaims = new List<Claim>
        {
            new("UserID", user.ID.ToString()),
            new("AccountID", account.ID.ToString()),
            new("ClanName", account.Clan?.Name ?? string.Empty),
            new("ClanTag", account.Clan?.Tag ?? string.Empty),
            new("MainAccount", account.IsMain.ToString())
        };

        if (new[] {UserRoles.Administrator, UserRoles.User}.Contains(user.Role.Name).Equals(false))
        {
            Logger.LogError($@"[BUG] Unknown User Role ""{user.Role.Name}""");

            return UnprocessableEntity($@"Unknown User Role ""{user.Role.Name}""");
        }

        IEnumerable<Claim> userRoleClaims = user.Role.Name is UserRoles.Administrator ? UserRoleClaims.Administrator : UserRoleClaims.User;

        IEnumerable<Claim> claim = Enumerable.Empty<Claim>().Union(openIDClaims).Union(customClaims).Union(userRoleClaims);

        // TODO: Implement Secrets Vault

        SymmetricSecurityKey signingKey = new(Encoding.UTF8.GetBytes(/*Configuration["JWT:SigningKey"]*/"MY-SUPER-SECRET-KEY"));
        SigningCredentials signingCredentials = new(signingKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new
        (
            Configuration["JWT:Issuer"],
            Configuration["JWT:Audience"],
            claims,
            expires: DateTime.UtcNow.AddHours(Convert.ToInt32(Configuration["JWT:DurationInHours"])),
            signingCredentials: signingCredentials
        );

        return Ok(new Dictionary<string, object> { { "identifier", identity.Id }, { "token", new JwtSecurityTokenHandler().WriteToken(token) }, { "verified", identity.EmailConfirmed } });
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
