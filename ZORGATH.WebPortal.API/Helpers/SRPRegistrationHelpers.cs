namespace ZORGATH.WebPortal.API.Helpers;

internal static class SRPRegistrationHelpers
{
    # region Secure Remote Password Magic Strings

    // Thank you, Anton Romanov (aka Theli), for making these values public: https://github.com/theli-ua/pyHoNBot/blob/master/hon/masterserver.py#L37.
    // The first magic string is also present in the k2_x64 DLL, between offsets 0xF2F4D0 and 0xF2F4D0.
    // It is not clear how the second magic string was reverse-engineered.
    // Project KONGOR would have not been possible without having these values in the public domain.

    private const string MagicStringOne = "[!~esTo0}";
    private const string MagicStringTwo = "taquzaph_?98phab&junaj=z=kuChusu";

    # endregion

    // TODO: Rename These Methods To Reflect Updated Column Names (Once They Get Updated)

    /// <summary>
    ///     Generates a 64-character long SHA256 hash of the account's password.
    /// </summary>
    internal static string HashAccountPassword(string password, string salt)
    {
        string passwordHash = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(password))).ToLower();

        string magickedPasswordHash = passwordHash + salt + MagicStringOne;

        string magickedPasswordHashMD5 = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(magickedPasswordHash))).ToLower();

        string doubleMagickedPasswordHashMD5 = magickedPasswordHashMD5 + MagicStringTwo;

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(doubleMagickedPasswordHashMD5))).ToLower();

        // TODO: Remember Why ToLower() On All The Hashes?
    }

    /// <summary>
    ///     Generates a 512-character long password salt.
    ///     The value 512 is for the purpose of consistency with the length of "B", the ephemeral key of the server.
    /// </summary>
    internal static string GeneratePasswordSalt()
        => SrpInteger.RandomInteger(512 / 2 /* Divide By 2 Because There Are 2 Hexadecimal Digits Per Byte */).ToHex();

    /// <summary>
    ///     Generates a 22-character long password SRP salt.
    ///     The value 22 is for the purpose of consistency with the original HoN salt length.
    /// </summary>
    internal static string GeneratePasswordSRPSalt()
        => SrpInteger.RandomInteger(22/ 2 /* Divide By 2 Because There Are 2 Hexadecimal Digits Per Byte */).ToHex();
}
