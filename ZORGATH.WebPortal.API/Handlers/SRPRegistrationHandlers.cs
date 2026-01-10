namespace ZORGATH.WebPortal.API.Handlers;

public static class SRPRegistrationHandlers
{
    /// <summary>
    ///     Generates a 64-character long SHA256 hash of the account's password.
    ///     The uppercase hashes (C# default) in this method need to be lowercased, to match the lowercase hashes (C++ default)
    ///     that the game client generates.
    ///     <br />
    ///     The expectation is that the password is not hashed for SRP registration, but is hashed for SRP authentication.
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
    ///     The value needs to be divided by 2, because there are 2 hexadecimal digits per byte.
    /// </summary>
    public static string GenerateSRPPasswordSalt()
    {
        return SrpInteger.RandomInteger(64 / 2).ToHex();
    }

    # region Secure Remote Password Magic Strings

    // Thank you, Anton Romanov (aka Theli), for making these values public: https://github.com/theli-ua/pyHoNBot/blob/cabde31b8601c1ca55dc10fcf663ec663ec0eb71/hon/masterserver.py#L37.
    // The first magic string is also present in the k2_x64 DLL of the Windows client, between offsets 0xF2F4D0 and 0xF2F4D0.
    // It is not clear how the second magic string was obtained.
    // Project KONGOR would have not been possible without having these values in the public domain.

    private const string MagicStringOne = "[!~esTo0}";
    private const string MagicStringTwo = "taquzaph_?98phab&junaj=z=kuChusu";

    # endregion
}