namespace ZORGATH.WebPortal.API.Helpers;

/// <summary>
///     Generates random account passwords that meet the <see cref="PasswordValidator"/> requirements.
///     Avoids ambiguous characters (0/O/1/l/I) to improve readability and reduce user frustration when copying generated passwords from emails.
/// </summary>
public static class AccountPasswordGenerationHelpers
{
    private const string AlphabeticCharacters = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string NumericCharacters = "23456789";
    private const string SpecialCharacters = "!@#$%&*?";

    /// <summary>
    ///     Generates a cryptographically random account password containing alphabetic, numeric, and special characters.
    ///     The generated password meets the minimum requirements enforced by <see cref="PasswordValidator"/>.
    /// </summary>
    public static string GenerateRandomPassword(int alphabeticCount = 6, int numericCount = 3, int specialCount = 3)
    {
        Span<char> password = stackalloc char[alphabeticCount + numericCount + specialCount];

        int index = 0;

        for (int i = 0; i < alphabeticCount; i++)
            password[index++] = AlphabeticCharacters[RandomNumberGenerator.GetInt32(AlphabeticCharacters.Length)];

        for (int i = 0; i < numericCount; i++)
            password[index++] = NumericCharacters[RandomNumberGenerator.GetInt32(NumericCharacters.Length)];

        for (int i = 0; i < specialCount; i++)
            password[index++] = SpecialCharacters[RandomNumberGenerator.GetInt32(SpecialCharacters.Length)];

        // Fisher-Yates Shuffle
        for (int i = password.Length - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (password[i], password[j]) = (password[j], password[i]);
        }

        return new string(password);
    }
}
