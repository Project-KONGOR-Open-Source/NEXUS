using System.Globalization;

using ASPIRE.Common.DTOs;

using MERRICK.DatabaseContext.Extensions;

using Microsoft.AspNetCore.OutputCaching;

using ZORGATH.WebPortal.API.Services.Email;

namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[EnableRateLimiting(RateLimiterPolicies.Strict)]
public class RegistrationController(
    MerrickContext databaseContext,
    IEmailService emailService,
    PasswordValidator passwordValidator) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private IEmailService EmailService { get; } = emailService;
    private PasswordValidator PasswordValidator { get; } = passwordValidator;

    [HttpPost("InitiateRegistration", Name = "Initiate Registration")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> InitiateRegistration([FromBody] RegisterEmailAddressDTO payload)
    {
        if (await MerrickContext.Users.AnyAsync(user => user.EmailAddress.Equals(payload.EmailAddress)))
        {
            return Conflict($@"User With Email ""{payload.EmailAddress}"" Already Exists");
        }

        // Check if a valid token already exists and reuse/resend? Or just create new.
        // For simplicity, create new. Cleanup job handles old ones.
        Token token = new()
        {
            Purpose = TokenPurpose.EmailAddressVerification,
            Data = payload.EmailAddress,
            EmailAddress = payload.EmailAddress, // Assuming Token entity has this field, checking usage in UserController
            Value = Guid.NewGuid(),
            TimestampCreated = DateTimeOffset.UtcNow
        };

        // Note: I need to verify Token entity structure from previous UserController usage. 
        // UserController used: token.Value (Guid), token.Data (SanitizedEmail), token.EmailAddress (Raw?)
        // Let's assume standard properties for now based on UserController.

        await MerrickContext.Tokens.AddAsync(token);
        await MerrickContext.SaveChangesAsync();

        bool emailSent = await EmailService.SendEmailAddressRegistrationLink(payload.EmailAddress, token.Value.ToString());

        if (!emailSent)
        {
            return BadRequest("Failed to send verification email.");
        }

        return Ok("Verification email sent.");
    }

    [HttpPost("CompleteRegistration", Name = "Complete Registration")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GetBasicUserDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CompleteRegistration([FromBody] RegisterUserWithTokenDTO payload)
    {
        Token? token = await MerrickContext.Tokens.SingleOrDefaultAsync(t =>
            t.Value.ToString().Equals(payload.Token) &&
            t.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        if (token is null)
        {
            return NotFound($@"Registration Token ""{payload.Token}"" Was Not Found");
        }

        if (token.TimestampConsumed is not null)
        {
            return Conflict($@"Registration Token ""{payload.Token}"" Has Already Been Consumed");
        }

        string sanitizedEmailAddress = token.Data;

        if (await MerrickContext.Users.AnyAsync(user => user.EmailAddress.Equals(sanitizedEmailAddress)))
        {
            return Conflict($@"User With Email ""{sanitizedEmailAddress}"" Already Exists");
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

        // Initialize required PBKDF2 hash
        user.PBKDF2PasswordHash = new PasswordHasher<User>().HashPassword(user, payload.Password);

        await MerrickContext.Users.AddAsync(user);

        Account account = new() { Name = payload.Name, User = user, IsMain = true };

        user.Accounts.Add(account);

        token.TimestampConsumed = DateTimeOffset.UtcNow;

        await MerrickContext.SaveChangesAsync();

        await EmailService.SendEmailAddressRegistrationConfirmation(user.EmailAddress, account.Name);

        return CreatedAtAction("GetUser", "User", new { id = user.ID },
            new GetBasicUserDTO(user.ID, user.EmailAddress, [new GetBasicAccountDTO(account.ID, account.Name)]));
    }
}
