using System.Security.Cryptography;
using System.Text;

using SecureRemotePassword;

namespace DAWNBRINGER.WebPortal.UI.Utilities;

public static class SRP
{
    /// <summary>
    ///     Generates a 64-character long SHA256 hash of the account's password.
    ///     The uppercase hashes (C# default) in this method need to be lowercased.
    /// </summary>
    public static string ComputeSRPPasswordHash(string password, string salt, bool passwordIsHashed = false)
    {
        string passwordHash = passwordIsHashed
            ? password
            : Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(password))).ToLower();

        string magickedPasswordHash = passwordHash + salt + MagicStringOne;

        string magickedPasswordHashHash =
            Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(magickedPasswordHash))).ToLower();

        string magickedMagickedPasswordHashHash = magickedPasswordHashHash + MagicStringTwo;

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(magickedMagickedPasswordHashHash))).ToLower();
    }

    /// <summary>
    ///     Generates a 64-character long SRP password salt.
    /// </summary>
    public static string GenerateSRPPasswordSalt()
    {
        return SrpInteger.RandomInteger(64 / 2).ToHex();
    }

    # region Secure Remote Password Magic Strings

    // Thank you, Anton Romanov (aka Theli), for making these values public.
    private const string MagicStringOne = "[!~esTo0}";
    private const string MagicStringTwo = "taquzaph_?98phab&junaj=z=kuChusu";

    # endregion
}