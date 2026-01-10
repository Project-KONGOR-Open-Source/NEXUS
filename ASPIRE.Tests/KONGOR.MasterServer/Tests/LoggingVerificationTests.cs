namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

public class LoggingVerificationTests
{
    [Test]
    public async Task LogsAreWrittenToFile()
    {
        // Arrange
        // We will check the "logs" location relative to working dir.
        string logDir = Path.Combine(Directory.GetCurrentDirectory(), "logs");

        // Act
        // Scope the factory to ensure it is disposed before we try to read the file
        {
            await using WebApplicationFactory<KONGORAssemblyMarker> factory =
                KONGORServiceProvider.CreateOrchestratedInstance("UsingLoggingTest");
            using HttpClient client = factory.CreateClient();

            // Make a request to trigger some logs (even a 404 is fine as it logs the request)
            await client.GetAsync("/health");
        }

        // Assert
        // Allow a small delay for the async file sink to flush and handle release
        await Task.Delay(500);

        // Check if directory exists
        await Assert.That(Directory.Exists(logDir)).IsTrue();

        // Use directory enumeration to find the logs and get the latest one
        DirectoryInfo directoryInfo = new(logDir);
        FileInfo? latestLogFile = directoryInfo.GetFiles("master_server*.log")
            .OrderByDescending(f => f.LastWriteTime)
            .FirstOrDefault();

        await Assert.That(latestLogFile).IsNotNull();

        // Read file using FileShare.ReadWrite to avoid locking issues if the process is still holding it
        string content;
        using (FileStream fs = File.Open(latestLogFile!.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (StreamReader sr = new(fs))
        {
            content = await sr.ReadToEndAsync();
        }

        await Assert.That(content).IsNotEmpty();

        // No cleanup - allow logs to persist as per user request/environment stability
    }
}