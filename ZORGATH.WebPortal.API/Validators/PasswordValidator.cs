namespace ZORGATH.WebPortal.API.Validators;

using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using FluentValidation;

public class PasswordValidator : AbstractValidator<string>
{
    private IdentityOptions IdentityOptions { get; }

    public PasswordValidator(IOptions<IdentityOptions> options)
    {
        IdentityOptions = options.Value;
        PasswordOptions passwordOptions = IdentityOptions.Password;

        RuleFor(password => password).NotEmpty().WithMessage("Password Must Not Be Empty");

        RuleFor(password => password).MinimumLength(passwordOptions.RequiredLength)
            .WithMessage($"Password Must Be At Least {passwordOptions.RequiredLength} Characters Long");

        if (passwordOptions.RequireNonAlphanumeric)
        {
            RuleFor(password => password).Must(password => MeetsMinimumSpecialCharactersCountRequirement(password, 1))
                .WithMessage("Password Must Contain At Least One Special Character");
        }
        
        if (passwordOptions.RequiredUniqueChars > 0)
        {
             RuleFor(password => password).Must(password => password.Distinct().Count() >= passwordOptions.RequiredUniqueChars)
                .WithMessage($"Password Must Contain At Least {passwordOptions.RequiredUniqueChars} Unique Characters");
        }

        if (passwordOptions.RequireDigit)
        {
             RuleFor(password => password).Must(password => password.Any(char.IsDigit))
                .WithMessage("Password Must Contain At Least One Digit");
        }

        if (passwordOptions.RequireLowercase)
        {
             RuleFor(password => password).Must(password => password.Any(char.IsLower))
                .WithMessage("Password Must Contain At Least One Lowercase Letter");
        }
        
        if (passwordOptions.RequireUppercase)
        {
             RuleFor(password => password).Must(password => password.Any(char.IsUpper))
                .WithMessage("Password Must Contain At Least One Uppercase Letter");
        }
    }

    private static bool MeetsMinimumSpecialCharactersCountRequirement(string password, int minimum)
        => password.Count(character => char.IsLetterOrDigit(character) is false) >= minimum;
}
