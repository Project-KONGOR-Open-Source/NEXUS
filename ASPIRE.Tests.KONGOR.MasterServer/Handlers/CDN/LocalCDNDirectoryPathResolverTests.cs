namespace ASPIRE.Tests.KONGOR.MasterServer.Handlers.CDN;

/// <summary>
///     Pure-logic tests for <see cref="LocalCDNDirectoryPathResolver"/>, verifying path resolution for CDN local directory configuration.
/// </summary>
public sealed class LocalCDNDirectoryPathResolverTests
{
    [Test]
    public async Task Resolving_User_Profile_Token_Expands_To_Home_Directory()
    {
        string? resolved = LocalCDNDirectoryPathResolver.Resolve("{UserProfile}/CDN", AppContext.BaseDirectory);
        string expectedHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        await Assert.That(resolved).IsNotNull();
        await Assert.That(resolved).StartsWith(expectedHome);
    }

    [Test]
    public async Task Resolving_Directory_User_Token_Expands_To_Home_Directory()
    {
        string? resolved = LocalCDNDirectoryPathResolver.Resolve("Directory.User/CDN", AppContext.BaseDirectory);
        string expectedHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        await Assert.That(resolved).IsNotNull();
        await Assert.That(resolved).StartsWith(expectedHome);
    }

    [Test]
    public async Task Resolving_Relative_Path_Combines_With_Content_Root()
    {
        string baseDirectory = AppContext.BaseDirectory;
        string? resolved = LocalCDNDirectoryPathResolver.Resolve("subfolder/cdn", baseDirectory);
        string expected = Path.GetFullPath(Path.Combine(baseDirectory, "subfolder/cdn"));

        await Assert.That(resolved).IsEqualTo(expected);
    }

    [Test]
    public async Task Resolving_Fully_Qualified_Path_Returns_Same_Normalised_Path()
    {
        string fullPath = Path.GetFullPath(AppContext.BaseDirectory);
        string? resolved = LocalCDNDirectoryPathResolver.Resolve(fullPath, AppContext.BaseDirectory);

        await Assert.That(resolved).IsEqualTo(fullPath);
    }

    [Test]
    public async Task Resolving_Null_Or_Empty_Path_Returns_Null()
    {
        await Assert.That(LocalCDNDirectoryPathResolver.Resolve(null, AppContext.BaseDirectory)).IsNull();
        await Assert.That(LocalCDNDirectoryPathResolver.Resolve("   ", AppContext.BaseDirectory)).IsNull();
    }
}
