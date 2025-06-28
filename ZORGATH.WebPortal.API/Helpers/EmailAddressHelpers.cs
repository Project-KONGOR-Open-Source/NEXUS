namespace ZORGATH.WebPortal.API.Helpers;

public static class EmailAddressHelpers
{
    public static IActionResult SanitizeEmailAddress(string email)
    {
        if (ZORGATH.RunsInDevelopmentMode is false)
        {
            if (email.Split('@').First().Contains('+'))
                return new BadRequestObjectResult(@"Alias Creating Character ""+"" Is Not Allowed");

            string[] allowedEmailProviders = Enumerable.Empty<string>()
                .Concat(new[] { "outlook", "hotmail", "live", "msn" }) // Microsoft Outlook
                .Concat(new[] { "protonmail", "proton" }) // Proton Mail
                .Concat(new[] { "gmail", "googlemail" }) // Google Mail
                .Concat(new[] { "yahoo", "rocketmail", "ymail" }) // Yahoo Mail
                .Concat(new[] { "aol", "yandex", "gmx", "mail" }) // AOL Mail, Yandex Mail, GMX Mail, mail.com
                .Concat(new[] { "icloud", "me", "mac" }) // iCloud Mail
                .ToArray();

            Regex pattern = new (@"^(?<local>[a-zA-Z0-9_\-.]+)@(?<domain>[a-zA-Z]+)\.(?<tld>[a-zA-Z]{1,3}|co.uk)$");

            Match match = pattern.Match(email);

            if (match.Success.Equals(false))
                return new BadRequestObjectResult($@"Email Address ""{email}"" Is Not Valid");

            string local = match.Groups["local"].Value;
            string domain = match.Groups["domain"].Value;
            string tld = match.Groups["tld"].Value;

            if (allowedEmailProviders.Contains(domain).Equals(false))
                return new BadRequestObjectResult($@"Email Address Provider ""{domain}"" Is Not Allowed");

            // These Email Providers Ignore Period Characters
            // Users Can Create Aliases With The Same Email Address By Simply Adding Some Period Characters To The Local Part
            if (domain is "protonmail" or "proton" or "gmail" or "googlemail")
                local = local.Replace(".", string.Empty);

            email = $"{local}@{domain}.{tld}";
        }

        return new ContentResult
        {
            Content = email
        };
    }
}
