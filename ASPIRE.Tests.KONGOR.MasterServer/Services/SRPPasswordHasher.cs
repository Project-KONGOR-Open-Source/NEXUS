namespace ASPIRE.Tests.KONGOR.MasterServer.Services;

/// <summary>
///     Test-local implementation of the SRP password hashing and salt generation used during account registration.
///     Duplicated (rather than referenced from <c>ZORGATH.WebPortal.API</c>) so the KONGOR test project has no cross-service project reference.
///     Must produce byte-for-byte identical output to the production implementation in the web portal because tests fabricate accounts that are later authenticated against the KONGOR master server.
/// </summary>
/// <remarks>
///     The two magic strings below are part of the SRP wire protocol expected by the Heroes Of Newerth client.
///     The first is present in the <c>k2_x64</c> DLL shipped with the Windows client; the second was obtained from the public <c>pyHoNBot</c> project (https://github.com/theli-ua/pyHoNBot) — Project KONGOR would not have been possible without them being in the public domain.
/// </remarks>
internal static class SRPPasswordHasher
{
    private const string MagicStringOne = "[!~esTo0}";

    private const string MagicStringTwo = "taquzaph_?98phab&junaj=z=kuChusu";

    /// <summary>
    ///     Computes a 64-character lower-case SHA-256 hash of the supplied password for SRP registration.
    ///     Uses the same magic-string dance as the production implementation: MD5(password) || salt || MagicOne → MD5 → || MagicTwo → SHA-256.
    /// </summary>
    public static string ComputeSRPPasswordHash(string password, string salt, bool passwordIsHashed = false)
    {
        string passwordHash = passwordIsHashed ? password : Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(password))).ToLower();

        string magickedPasswordHash = passwordHash + salt + MagicStringOne;

        string magickedPasswordHashHash = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(magickedPasswordHash))).ToLower();

        string magickedMagickedPasswordHashHash = magickedPasswordHashHash + MagicStringTwo;

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(magickedMagickedPasswordHashHash))).ToLower();
    }

    /// <summary>
    ///     Generates a 32-byte (64 hexadecimal character) random salt for SRP registration.
    /// </summary>
    public static string GenerateSRPPasswordSalt()
        => SrpInteger.RandomInteger(64 / 2).ToHex();
}
