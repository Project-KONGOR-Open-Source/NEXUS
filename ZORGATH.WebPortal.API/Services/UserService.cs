using ASPIRE.Common.DTOs;

namespace ZORGATH.WebPortal.API.Services;

public partial class UserService(
    MerrickContext databaseContext,
    IEmailService emailService,
    PasswordValidator passwordValidator,
    ILogger<UserService> logger) : IUserService
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private IEmailService EmailService { get; } = emailService;
    private PasswordValidator PasswordValidator { get; } = passwordValidator;
    private ILogger<UserService> Logger { get; } = logger;

    [LoggerMessage(Level = LogLevel.Warning, Message = "[Register] Password confirmation mismatch.")]
    private partial void LogPasswordMismatch();

    [LoggerMessage(Level = LogLevel.Warning, Message = "[Register] Password validation failed: {Errors}")]
    private partial void LogPasswordValidationFailed(string errors);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[Register] Token not found: {Token}")]
    private partial void LogTokenNotFound(string token);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[Register] Token already consumed: {Token}")]
    private partial void LogTokenConsumed(string token);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[Register] User email collision: {Email}")]
    private partial void LogEmailCollision(string email);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[Register] Account name collision: {Name}")]
    private partial void LogAccountCollision(string name);

    [LoggerMessage(Level = LogLevel.Error, Message = "[Register] User Role '{Role}' not found in database.")]
    private partial void LogRoleNotFound(string role);

    [LoggerMessage(Level = LogLevel.Information, Message = "[Register] Success. UserID: {UserID}, AccountID: {AccountID}")]
    private partial void LogRegistrationSuccess(int userId, int accountId);

    public async Task<ServiceResult<User>> RegisterUserAsync(RegisterUserAndMainAccountDTO payload)
    {
        if (payload.Password.Equals(payload.ConfirmPassword).Equals(false))
        {
            LogPasswordMismatch();
            return ServiceResult<User>.Failure(
                $@"Password ""{payload.ConfirmPassword}"" Does Not Match ""{payload.Password}"" (These Values Are Only Visible To You)",
                ServiceErrorType.BadRequest);
        }

        ValidationResult result = await PasswordValidator.ValidateAsync(payload.Password);

        if (result.IsValid is false)
        {
            string errors = string.Join(", ", result.Errors.Select(e => e.ErrorMessage));
            LogPasswordValidationFailed(errors);
            return ServiceResult<User>.ValidationFailure(result.Errors.Select(error => error.ErrorMessage));
        }

        Token? token = await MerrickContext.Tokens.SingleOrDefaultAsync(token =>
            token.Value.ToString().Equals(payload.Token) &&
            token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        if (token is null)
        {
            LogTokenNotFound(payload.Token);
            return ServiceResult<User>.Failure($@"Email Registration Token ""{payload.Token}"" Was Not Found", ServiceErrorType.NotFound);
        }

        if (token.TimestampConsumed is not null)
        {
            LogTokenConsumed(payload.Token);
            return ServiceResult<User>.Failure($@"Email Registration Token ""{payload.Token}"" Has Already Been Consumed", ServiceErrorType.Conflict);
        }

        string sanitizedEmailAddress = token.Data;

        if (await MerrickContext.Users.AnyAsync(user => user.EmailAddress.Equals(sanitizedEmailAddress)))
        {
            LogEmailCollision(sanitizedEmailAddress);
            return ServiceResult<User>.Failure($@"User With Email ""{token.EmailAddress}"" Already Exists", ServiceErrorType.Conflict);
        }

        if (await MerrickContext.Accounts.AnyAsync(account => account.Name.Equals(payload.Name)))
        {
            LogAccountCollision(payload.Name);
            return ServiceResult<User>.Failure($@"Account With Name ""{payload.Name}"" Already Exists", ServiceErrorType.Conflict);
        }

        Role? role = await MerrickContext.Roles.SingleOrDefaultAsync(role => role.Name.Equals(UserRoles.User));

        if (role is null)
        {
            LogRoleNotFound(UserRoles.User);
            return ServiceResult<User>.Failure($@"User Role ""{UserRoles.User}"" Was Not Found", ServiceErrorType.NotFound);
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

        LogRegistrationSuccess(user.ID, account.ID);

        await EmailService.SendEmailAddressRegistrationConfirmation(user.EmailAddress, account.Name);

        return ServiceResult<User>.Success(user);
    }
}
