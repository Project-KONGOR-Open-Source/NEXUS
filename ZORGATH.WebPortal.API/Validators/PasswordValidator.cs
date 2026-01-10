namespace ZORGATH.WebPortal.API.Validators;

public class PasswordValidator : AbstractValidator<string>
{
    private const int MinimumPasswordLength = 8;
    private const int MinimumAlphabeticCharactersCount = 2;
    private const int MinimumNumericCharactersCount = 2;
    private const int MinimumSpecialCharactersCount = 2;

    public PasswordValidator()
    {
        RuleFor(password => password).NotEmpty().WithMessage("Password Must Not Be Empty");

        RuleFor(password => password).MinimumLength(MinimumPasswordLength)
            .WithMessage($"Password Must To Be At Least {MinimumPasswordLength} Characters Long");

        RuleFor(password => password).Must(password =>
                MeetsMinimumAlphabeticCharactersCountRequirement(password, MinimumAlphabeticCharactersCount))
            .WithMessage($"Password Must Contain At Least {MinimumAlphabeticCharactersCount} Alphabetic Characters");

        RuleFor(password => password).Must(password =>
                MeetsMinimumNumericCharactersCountRequirement(password, MinimumNumericCharactersCount))
            .WithMessage($"Password Must Contain At Least {MinimumNumericCharactersCount} Numeric Characters");

        RuleFor(password => password).Must(password =>
                MeetsMinimumSpecialCharactersCountRequirement(password, MinimumSpecialCharactersCount))
            .WithMessage($"Password Must Contain At Least {MinimumSpecialCharactersCount} Special Characters");
    }

    private static bool MeetsMinimumAlphabeticCharactersCountRequirement(string password, int minimum)
    {
        return password.Count(char.IsLetter) >= minimum;
    }

    private static bool MeetsMinimumNumericCharactersCountRequirement(string password, int minimum)
    {
        return password.Count(char.IsNumber) >= minimum;
    }

    private static bool MeetsMinimumSpecialCharactersCountRequirement(string password, int minimum)
    {
        return password.Count(character => char.IsLetterOrDigit(character) is false) >= minimum;
    }
}