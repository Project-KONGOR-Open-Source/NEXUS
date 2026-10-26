namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Services;

/// <summary>
///     Covers <see cref="Distributions"/>: the version the chat handshake carries as four bytes comes back as the distributions' zero-padded text.
/// </summary>
public sealed class DistributionsTests
{
    [Test]
    public async Task Format_Version_Writes_Every_Part_After_The_Major_Version_As_Two_Digits()
        => await Assert.That(Distributions.FormatVersion(5, 26, 1, 5)).IsEqualTo("5.26.01.05");

    [Test]
    public async Task The_Distributions_Version_Survives_The_Four_Bytes_Of_The_Chat_Handshake()
    {
        byte[] parts = [.. Distributions.Version.Split('.').Select(byte.Parse)];

        await Assert.That(Distributions.FormatVersion(parts[0], parts[1], parts[2], parts[3])).IsEqualTo(Distributions.Version);
    }
}
