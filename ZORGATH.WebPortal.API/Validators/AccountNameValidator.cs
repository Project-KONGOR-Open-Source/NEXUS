namespace ZORGATH.WebPortal.API.Validators;

public class AccountNameValidator : AbstractValidator<string>
{
    private const int MinimumAccountNameLength = 4;
    private const int MaximumAccountNameLengthProduction = 12;
    private const int MaximumAccountNameLengthDevelopment = 16;
    private const string AllowedCharactersPattern = @"^[a-zA-Z0-9_\-]+$";

    public AccountNameValidator(bool isDevelopment = false)
    {
        int maximumAccountNameLength = isDevelopment ? MaximumAccountNameLengthDevelopment : MaximumAccountNameLengthProduction;

        RuleFor(accountName => accountName).NotEmpty().WithMessage("Account Name Must Not Be Empty");

        RuleFor(accountName => accountName).MinimumLength(MinimumAccountNameLength)
            .WithMessage($"Account Name Must Be At Least {MinimumAccountNameLength} Characters Long");

        RuleFor(accountName => accountName).MaximumLength(maximumAccountNameLength)
            .WithMessage($"Account Name Must Be At Most {maximumAccountNameLength} Characters Long");

        RuleFor(accountName => accountName).Matches(AllowedCharactersPattern)
            .WithMessage("Account Name May Only Contain Letters, Numbers, Underscores, And Hyphens");
    }
}
