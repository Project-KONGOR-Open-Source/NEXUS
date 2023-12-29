using Microsoft.EntityFrameworkCore.Storage;

namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
public class IdentityController(MerrickContext databaseContext, UserManager<User> userManager, IConfiguration configuration, IEmailService emailService, ILogger logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; init; } = databaseContext;
    private UserManager<User> UserManager { get; init; } = userManager;
    private IConfiguration Configuration { get; init; } = configuration;
    private IEmailService EmailService { get; init; } = emailService;
    private ILogger Logger { get; init; } = logger;

    [HttpPost("Register", Name = "Register User And Account")]
    public async Task<IActionResult> RegisterUserAndAccount([FromBody] RegisterUserAndAccountDTO payload)
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

            if (await MerrickContext.Users.Where(user => user.Email.Equals(token.EmailAddress) || user.Email.Equals(token.SanitisedEmailAddress)).AnyAsync())
            {
                return Conflict($@"User With Email ""{token.EmailAddress}"" Already Exists");
            }

            if (await MerrickContext.Accounts.Where(account => account.Name.Equals(payload.Name)).AnyAsync())
            {
                return Conflict($@"Account With Name ""{payload.Name}"" Already Exists");
            }

            string[] disallowedUsernames = await System.IO.File.ReadAllLinesAsync(FlatListData.ReservedAccountNames);

            if (disallowedUsernames.Contains(payload.Name, StringComparer.OrdinalIgnoreCase))
            {
                return Conflict($@"Account name ""{payload.Name}"" is reserved. If this account name belongs to you, please reach out to the Project KONGOR developers.");
            }

            AccountRegistrationData accountRegistrationData = SrpRegistrationHelpers.GenerateAccountRegistrationData(payload.Password);

            ElementUser identity = new(
                accountRegistrationData.Salt,
                accountRegistrationData.PasswordSalt,
                accountRegistrationData.HashedPassword,
                token.SanitisedEmailAddress, // Always Store The Sanitised Email Address To Prevent Registration With Aliases !
                JSONConfiguration.EconomyConfiguration.SignupRewards.GoldCoins,
                JSONConfiguration.EconomyConfiguration.SignupRewards.SilverCoins,
                JSONConfiguration.EconomyConfiguration.SignupRewards.PlinkoTickets,
                new List<string> { "ai.Default Icon", "cc.white", "t.Standard" }
            )
            {
                UserName = payload.Name, // TODO: Remove User Name From This Entity, Which Should Not Have A User Name
                NormalizedUserName = payload.Name.ToUpper()
            };

            IdentityResult result = await UserManager.CreateAsync(identity, payload.Password);

            if (result.Errors.Any())
            {
                return BadRequest(result.Errors);
            }

            // Assign Default Role
            await UserManager.AddToRoleAsync(identity, IdentityRoles.User);

            DateTime now = DateTime.UtcNow;

            Account account = new()
            {
                Name = payload.Name,
                User = identity,
                TimestampCreated = now,
                LastActivity = now,
                PlayerSeasonStatsPublic = new PlayerSeasonStatsPublic(),
                PlayerSeasonStatsRanked = new PlayerSeasonStatsRanked(),
                PlayerSeasonStatsRankedCasual = new PlayerSeasonStatsRankedCasual(),
                PlayerSeasonStatsMidWars = new PlayerSeasonStatsMidWars(),
                AutoConnectChatChannels = new List<string>(),
                SelectedUpgradeCodes = new List<string> { "ai.Default Icon", "cc.white", "t.Standard" },
                IpAddressCollection = new HashSet<string>(),
                MacAddressCollection = new HashSet<string>(),
                HardwareIdCollection = new HashSet<string>(),
                SystemInformationCollection = new HashSet<string>()
            };

            identity.Accounts.Add(account);

            // Create Database Records For Mastery And Mastery Rewards
            await MerrickContext.Masteries.AddAsync(new Mastery { User = identity });
            await MerrickContext.MasteryRewards.AddAsync(new MasteryRewards { User = identity });

            // Remove The Consumed Token
            MerrickContext.Tokens.Remove(token);

            // Save Everything
            await MerrickContext.SaveChangesAsync();

            IdentityForGetLimitedDTO identityForResponse = new
            (
                Id: identity.Id,
                UserName: identity.UserName,
                Email: identity.Email,
                Accounts: identity.Accounts.Select(account => account.AccountId).ToList() // TODO: await/async?
            );

            await transaction.CommitAsync();

            await EmailService.SendEmailAddressRegistrationSuccess(identity.Email, account.Name);

            return CreatedAtAction(nameof(GetIdentity), new { id = identity.Id }, identityForResponse);
        }

        catch (Exception exception)
        {
            Logger.Error(exception, "IdentitiesController.RegisterUserAndAccount");

            await transaction.RollbackAsync();

            return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
        }
    }
}
