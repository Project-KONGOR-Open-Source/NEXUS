using System.Globalization;

using ASPIRE.Common.DTOs;

using MERRICK.DatabaseContext.Extensions;

using Microsoft.AspNetCore.OutputCaching;

using ZORGATH.WebPortal.API.Services;

namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[EnableRateLimiting(RateLimiterPolicies.Strict)]
public partial class UserController(
    MerrickContext databaseContext,
    ILogger<UserController> logger,
    IUserService userService,
    IAuthenticationService authenticationService) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private ILogger Logger { get; } = logger;
    private IUserService UserService { get; } = userService;
    private IAuthenticationService AuthenticationService { get; } = authenticationService;

    [LoggerMessage(Level = LogLevel.Error, Message = "[BUG] Unknown User Role \"{RoleName}\"")]
    private partial void LogUnknownUserRole(string roleName);

    [HttpPost("Register", Name = "Register User And Main Account")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GetBasicUserDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterUserAndMainAccount([FromBody] RegisterUserAndMainAccountDTO payload)
    {
        ServiceResult<User> result = await UserService.RegisterUserAsync(payload);

        if (result.IsSuccess)
        {
            User user = result.Data!;
            Account account = user.Accounts.First();

            return CreatedAtAction(nameof(GetUser), new { id = user.ID },
                new GetBasicUserDTO(user.ID, user.EmailAddress, [new GetBasicAccountDTO(account.ID, account.Name)]));
        }

        return result.ErrorType switch
        {
            ServiceErrorType.BadRequest => result.ValidationErrors != null
                ? BadRequest(result.ValidationErrors)
                : BadRequest(result.ErrorMessage),
            ServiceErrorType.NotFound => NotFound(result.ErrorMessage),
            ServiceErrorType.Conflict => Conflict(result.ErrorMessage),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.ErrorMessage)
        };
    }

    [HttpPost("LogIn", Name = "Log In User")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GetAuthenticationTokenDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> LogInUser([FromBody] LogInUserDTO payload)
    {
        ServiceResult<GetAuthenticationTokenDTO> result = await AuthenticationService.AuthenticateUserAsync(payload);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(result.ErrorMessage),
            ServiceErrorType.Unauthorized => Unauthorized(result.ErrorMessage),
            ServiceErrorType.UnprocessableEntity => UnprocessableEntity(result.ErrorMessage),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.ErrorMessage)
        };
    }

    [HttpGet("{id}", Name = "Get User")]
    [Authorize(UserRoles.AllRoles)]
    [OutputCache(PolicyName = OutputCachePolicies.CacheForFiveMinutes, VaryByHeaderNames = new[] { "Authorization" })]
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

        LogUnknownUserRole(role);

        return BadRequest($@"Unknown User Role ""{role}""");
    }
}
