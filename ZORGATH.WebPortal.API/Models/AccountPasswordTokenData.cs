namespace ZORGATH.WebPortal.API.Models;

/// <summary>
///     Represents pre-computed account password hashes stored in a token's <see cref="Token.Data"/> field as serialised JSON.
///     Used by the account password reset and update flows to defer applying the new password until the user confirms via email.
/// </summary>
public record AccountPasswordTokenData(string SanitizedEmailAddress, string SRPPasswordSalt, string SRPPasswordHash, string PBKDF2PasswordHash);
