namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.MessageOfTheDay;

/// <summary>
///     Integration tests for the fonts the master server serves to the message of the day and the announcement pages.
///     The files are the web portal's own, so both show the same font.
/// </summary>
public sealed class FontTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    private static readonly string[] FontFileNames =
    [
        "ubuntu-light.ttf", "ubuntu-light-italic.ttf", "ubuntu-regular.ttf", "ubuntu-italic.ttf",
        "ubuntu-medium.ttf", "ubuntu-medium-italic.ttf", "ubuntu-bold.ttf", "ubuntu-bold-italic.ttf",
        "ubuntu-mono-regular.ttf", "ubuntu-mono-italic.ttf", "ubuntu-mono-bold.ttf", "ubuntu-mono-bold-italic.ttf"
    ];

    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();

    [Test]
    public async Task Each_Portal_Font_Is_Served_From_The_Master_Server()
    {
        using HttpClient client = webApplicationFactory.CreateClient();

        foreach (string fontFileName in FontFileNames)
        {
            using HttpResponseMessage response = await client.GetAsync($"/fonts/{fontFileName}");

            byte[] content = await response.Content.ReadAsByteArrayAsync();

            using (Assert.Multiple())
            {
                await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

                // Every TrueType File Starts With The Version Tag 0x00010000
                await Assert.That(content.Take(4).ToArray()).IsEquivalentTo(new byte[] { 0x00, 0x01, 0x00, 0x00 });
            }
        }
    }

    [Test]
    public async Task The_Served_Font_Is_Ubuntu_Sans()
    {
        using HttpClient client = webApplicationFactory.CreateClient();

        byte[] content = await client.GetByteArrayAsync("/fonts/ubuntu-regular.ttf");

        // The Name Table Stores The Family Name In UTF-16 Big-Endian
        byte[] familyName = Encoding.BigEndianUnicode.GetBytes("Ubuntu Sans");

        await Assert.That(content.AsSpan().IndexOf(familyName)).IsGreaterThanOrEqualTo(0);
    }

    [Test]
    public async Task The_Served_Code_Font_Is_Ubuntu_Sans_Mono()
    {
        using HttpClient client = webApplicationFactory.CreateClient();

        byte[] content = await client.GetByteArrayAsync("/fonts/ubuntu-mono-regular.ttf");

        byte[] familyName = Encoding.BigEndianUnicode.GetBytes("Ubuntu Sans Mono");

        await Assert.That(content.AsSpan().IndexOf(familyName)).IsGreaterThanOrEqualTo(0);
    }
}
