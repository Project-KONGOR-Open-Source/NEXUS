namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Handlers.CDN;

/// <summary>
///     Pure-logic tests for <see cref="LocalCDNDirectoryPathResolver"/>, verifying path resolution for CDN local directory configuration.
/// </summary>
public sealed class LocalCDNDirectoryPathResolverTests
{
    [Test]
    public async Task Resolving_ROOT_Token_Expands_To_Solution_Root_Directory()
    {
        string directoryResolved = LocalCDNDirectoryPathResolver.Resolve("{ROOT}");

        using (Assert.Multiple())
        {
            await Assert.That(File.Exists(Path.Combine(directoryResolved, "license"))).IsTrue();
            await Assert.That(File.Exists(Path.Combine(directoryResolved, "NEXUS.DistributedApplication.slnx"))).IsTrue();
        }
    }

    [Test]
    public async Task Resolving_ROOT_Token_With_Upward_Traversal_Expands_To_Sibling_Repository_Directory()
    {
        string directoryResolved = LocalCDNDirectoryPathResolver.Resolve("{ROOT}/../GEMINI/source");
        string directorySolutionRoot = LocalCDNDirectoryPathResolver.Resolve("{ROOT}");
        string directoryExpected = Path.GetFullPath(Path.Combine(directorySolutionRoot, "..", "GEMINI", "source"));

        await Assert.That(directoryResolved).IsEqualTo(directoryExpected);
    }

    [Test]
    public async Task Resolving_USER_Token_Expands_To_User_Profile_Directory()
    {
        string directoryResolved = LocalCDNDirectoryPathResolver.Resolve("{USER}/CDN");
        string directoryUserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string directoryExpected = Path.Combine(directoryUserProfile, "CDN");

        await Assert.That(directoryResolved).IsEqualTo(directoryExpected);
    }

    [Test]
    public async Task Resolving_TEMP_Token_Expands_To_Temporary_Directory()
    {
        string directoryResolved = LocalCDNDirectoryPathResolver.Resolve("{TEMP}/CDN");
        string directoryTemporaryFiles = Path.GetTempPath();
        string directoryExpected = Path.Combine(directoryTemporaryFiles, "CDN");

        await Assert.That(directoryResolved).IsEqualTo(directoryExpected);
    }

    [Test]
    public async Task Resolving_Relative_Path_Combines_With_Solution_Root_Directory()
    {
        string directoryResolved = LocalCDNDirectoryPathResolver.Resolve("cdn");
        string directorySolutionRoot = LocalCDNDirectoryPathResolver.Resolve("{ROOT}");
        string directoryExpected = Path.GetFullPath(Path.Combine(directorySolutionRoot, "cdn"));

        await Assert.That(directoryResolved).IsEqualTo(directoryExpected);
    }

    [Test]
    public async Task Resolving_Composite_Relative_Path_Combines_With_Solution_Root_Directory()
    {
        string directoryResolved = LocalCDNDirectoryPathResolver.Resolve("local/cdn");
        string directorySolutionRoot = LocalCDNDirectoryPathResolver.Resolve("{ROOT}");
        string directoryExpected = Path.GetFullPath(Path.Combine(directorySolutionRoot, "local", "cdn"));

        await Assert.That(directoryResolved).IsEqualTo(directoryExpected);
    }

    [Test]
    public async Task Resolving_Relative_Path_With_Upward_Traversal_Combines_With_Solution_Root_Directory()
    {
        string directoryResolved = LocalCDNDirectoryPathResolver.Resolve("../cdn");
        string directorySolutionRoot = LocalCDNDirectoryPathResolver.Resolve("{ROOT}");
        string directoryExpected = Path.GetFullPath(Path.Combine(directorySolutionRoot, "..", "cdn"));

        await Assert.That(directoryResolved).IsEqualTo(directoryExpected);
    }

    [Test]
    public async Task Resolving_Composite_Relative_Path_With_Upward_Traversal_Combines_With_Solution_Root_Directory()
    {
        string directoryResolved = LocalCDNDirectoryPathResolver.Resolve("../local/cdn");
        string directorySolutionRoot = LocalCDNDirectoryPathResolver.Resolve("{ROOT}");
        string directoryExpected = Path.GetFullPath(Path.Combine(directorySolutionRoot, "..", "local", "cdn"));

        await Assert.That(directoryResolved).IsEqualTo(directoryExpected);
    }

    [Test]
    public async Task Resolving_Fully_Qualified_Path_Returns_Same_Normalised_Path()
    {
        string directoryFullPath = Path.GetFullPath(AppContext.BaseDirectory);
        string directoryResolved = LocalCDNDirectoryPathResolver.Resolve(directoryFullPath);

        await Assert.That(directoryResolved).IsEqualTo(directoryFullPath);
    }
}
