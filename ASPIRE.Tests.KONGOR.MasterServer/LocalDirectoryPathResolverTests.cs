namespace ASPIRE.Tests.KONGOR.MasterServer;

/// <summary>
///     Pure-logic tests for <see cref="LocalDirectoryPathResolver"/>, verifying path resolution for CDN local directory configuration.
/// </summary>
public sealed class LocalDirectoryPathResolverTests
{
    [Test]
    public async Task Resolving_User_Profile_Token_Expands_To_Home_Directory()
    {
        string? resolved = LocalDirectoryPathResolver.Resolve("{UserProfile}/CDN", AppContext.BaseDirectory);
        string expectedHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        await Assert.That(resolved).IsNotNull();
        await Assert.That(resolved).StartsWith(expectedHome);
    }

    [Test]
    public async Task Resolving_Directory_User_Token_Expands_To_Home_Directory()
    {
        string? resolved = LocalDirectoryPathResolver.Resolve("Directory.User/CDN", AppContext.BaseDirectory);
        string expectedHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        await Assert.That(resolved).IsNotNull();
        await Assert.That(resolved).StartsWith(expectedHome);
    }

    [Test]
    public async Task Resolving_Relative_Path_Combines_With_Content_Root()
    {
        string baseDirectory = AppContext.BaseDirectory;
        string? resolved = LocalDirectoryPathResolver.Resolve("subfolder/cdn", baseDirectory);
        string expected = Path.GetFullPath(Path.Combine(baseDirectory, "subfolder/cdn"));

        await Assert.That(resolved).IsEqualTo(expected);
    }

    [Test]
    public async Task Resolving_Fully_Qualified_Path_Returns_Same_Normalised_Path()
    {
        string fullPath = Path.GetFullPath(AppContext.BaseDirectory);
        string? resolved = LocalDirectoryPathResolver.Resolve(fullPath, AppContext.BaseDirectory);

        await Assert.That(resolved).IsEqualTo(fullPath);
    }

    [Test]
    public async Task Resolving_Null_Or_Empty_Path_Returns_Null()
    {
        await Assert.That(LocalDirectoryPathResolver.Resolve(null, AppContext.BaseDirectory)).IsNull();
        await Assert.That(LocalDirectoryPathResolver.Resolve("   ", AppContext.BaseDirectory)).IsNull();
    }
}
