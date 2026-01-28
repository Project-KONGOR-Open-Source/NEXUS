using System.Globalization;

using ASPIRE.Common.DTOs;

namespace ZORGATH.WebPortal.API.Services;

public partial class AuthenticationService(
    MerrickContext databaseContext,
    IOptions<OperationalConfiguration> configuration,
    ILogger<AuthenticationService> logger) : IAuthenticationService
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private OperationalConfiguration Configuration { get; } = configuration.Value;
    private ILogger<AuthenticationService> Logger { get; } = logger;

    [LoggerMessage(Level = LogLevel.Warning, Message = "[Auth] Account not found: {AccountName}")]
    private partial void LogAccountNotFound(string accountName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[Auth] Invalid password for account: {AccountName}")]
    private partial void LogInvalidPassword(string accountName);

    [LoggerMessage(Level = LogLevel.Error, Message = "[Auth] Unknown/Invalid Role '{Role}' for account: {AccountName}")]
    private partial void LogUnknownRole(string role, string accountName);

    [LoggerMessage(Level = LogLevel.Information, Message = "[Auth] Login success: {AccountName} (ID: {Id})")]
    private partial void LogLoginSuccess(string accountName, int id);

    public async Task<ServiceResult<GetAuthenticationTokenDTO>> AuthenticateUserAsync(LogInUserDTO payload)
    {
        Account? account = await MerrickContext.Accounts
            .Include(account => account.User).ThenInclude(user => user.Role)
            .Include(account => account.Clan)
            .SingleOrDefaultAsync(account => account.Name.Equals(payload.Name));

        if (account is null)
        {
            LogAccountNotFound(payload.Name);
            return ServiceResult<GetAuthenticationTokenDTO>.Failure($@"Account ""{payload.Name}"" Was Not Found", ServiceErrorType.NotFound);
        }

        User user = account.User;

        PasswordVerificationResult result =
            new PasswordHasher<User>().VerifyHashedPassword(user, user.PBKDF2PasswordHash, payload.Password);

        if (result is not PasswordVerificationResult.Success)
        {
            LogInvalidPassword(payload.Name);
            return ServiceResult<GetAuthenticationTokenDTO>.Failure("Invalid User Name And/Or Password", ServiceErrorType.Unauthorized);
        }

        if (new[] { UserRoles.Administrator, UserRoles.User }.Contains(user.Role.Name).Equals(false))
        {
            LogUnknownRole(user.Role.Name, account.Name);
            return ServiceResult<GetAuthenticationTokenDTO>.Failure($@"Unknown User Role ""{user.Role.Name}""", ServiceErrorType.UnprocessableEntity);
        }

        IEnumerable<Claim> userRoleClaims = user.Role.Name is UserRoles.Administrator
            ? UserRoleClaims.Administrator
            : UserRoleClaims.User;

        IEnumerable<Claim> openIDClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, account.Name, ClaimValueTypes.String),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.AuthTime, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Nonce, Guid.CreateVersion7().ToString(), ClaimValueTypes.String),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString(), ClaimValueTypes.String),
            new(JwtRegisteredClaimNames.Email, user.EmailAddress, ClaimValueTypes.Email)
        };

        IEnumerable<Claim> customClaims = new List<Claim>
        {
            new(Claims.UserID, user.ID.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.String),
            new(Claims.AccountID, account.ID.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.String),
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
                SecurityAlgorithms.HmacSha256)
        );

        string jwt = new JwtSecurityTokenHandler().WriteToken(token);

        LogLoginSuccess(account.Name, user.ID);

        return ServiceResult<GetAuthenticationTokenDTO>.Success(new GetAuthenticationTokenDTO(user.ID, "JWT", jwt));
    }
}
