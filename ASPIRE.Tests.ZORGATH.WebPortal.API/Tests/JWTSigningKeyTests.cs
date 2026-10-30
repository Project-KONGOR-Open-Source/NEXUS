using Microsoft.Extensions.Configuration;

namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Tests;

/// <summary>
///     Verifies that the web portal's JWT signing key lives in no settings file: the repository is public, and anyone holding the key could sign their own web portal tokens.
///     The application host generates the key instead, as a secret parameter.
/// </summary>
public sealed class JWTSigningKeyTests
{
    private static readonly string[] SettingsFileNames = [ "appsettings.json", "appsettings.Development.json", "appsettings.Production.json" ];

    /// <summary>
    ///     Finds the web portal API's project folder by walking up from the test's output folder to the repository root that holds it.
    /// </summary>
    private static string FindProjectDirectory()
    {
        for (DirectoryInfo? directory = new (AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            string candidate = Path.Combine(directory.FullName, "ZORGATH.WebPortal.API");

            if (File.Exists(Path.Combine(candidate, "ZORGATH.cs")))
                return candidate;
        }

        throw new DirectoryNotFoundException(@"No ""ZORGATH.WebPortal.API"" Folder Was Found Above The Test's Output Folder");
    }

    [Test]
    public async Task The_Settings_Files_Hold_No_JWT_Signing_Key()
    {
        string projectDirectory = FindProjectDirectory();

        using (Assert.Multiple())
        {
            foreach (string fileName in SettingsFileNames)
            {
                IConfiguration configuration = new ConfigurationBuilder().AddJsonFile(Path.Combine(projectDirectory, fileName), optional: false).Build();

                await Assert.That(configuration["Operational:JWT:SigningKey"]).IsNull().Because($@"""{fileName}"" Must Not Hold The Signing Key");
            }
        }
    }
}
