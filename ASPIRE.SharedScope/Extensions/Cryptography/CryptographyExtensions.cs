namespace ASPIRE.ServiceDefaults.Extensions.Cryptography;

public static class CryptographyExtensions
{
    /// <summary>
    ///     Generates a hash code with a numeric value between 0 and 4,294,967,295 (unsigned integer).
    ///     Unlike .NET's GetHashCode(), this method will always generate the same numeric hash code for the same input value.
    /// </summary>
    public static uint GetDeterministicHashCode(this object hashable)
    {
        byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(hashable.ToString() ?? string.Empty));

        uint first = BitConverter.ToUInt32(hash, 0);
        uint second = BitConverter.ToUInt32(hash, 4);
        uint third = BitConverter.ToUInt32(hash, 8);
        uint fourth = BitConverter.ToUInt32(hash, 12);

        return first ^ second ^ third ^ fourth;
    }
}
