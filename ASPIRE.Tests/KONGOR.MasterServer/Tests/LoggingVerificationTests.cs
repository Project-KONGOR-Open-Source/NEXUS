using ASPIRE.Tests.KONGOR.MasterServer.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using TUnit.Assertions;
using TUnit.Core;

namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

public class LoggingVerificationTests
{
    [Test]
    public async Task LogsAreWrittenToFile()
    {
        // Arrange
        // We need to use a unique subdirectory for logs for this test to avoid conflicts, or just check the default
        // Since the app code hardcodes "logs/master_server.log", we will check that location relative to working dir.
        string logDir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
        string logFile = Path.Combine(logDir, "master_server.log");

        // Clean up previous runs if any
        if (File.Exists(logFile)) File.Delete(logFile);
        if (Directory.Exists(logDir)) Directory.Delete(logDir, true);

        // Act
        // Scope the factory to ensure it is disposed (and file locks released) before we try to read the file
        {
            await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance("UsingLoggingTest");
            using HttpClient client = factory.CreateClient();

            // Make a request to trigger some logs (even a 404 is fine as it logs the request)
            HttpResponseMessage response = await client.GetAsync("/health"); 
        }

        // Assert
        // Allow a small delay for the async file sink to flush and handle release
        await Task.Delay(500); 

        // Check if directory exists
        await Assert.That(Directory.Exists(logDir)).IsTrue();
        
        // Use directory enumeration to find the logs
        string[] files = Directory.GetFiles(logDir, "master_server*.log");
        
        await Assert.That(files).IsNotEmpty();
        
        // Read file using FileShare.ReadWrite to avoid locking issues if the process is still holding it for some reason
        string content;
        using (FileStream fs = File.Open(files.First(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (StreamReader sr = new StreamReader(fs))
        {
            content = await sr.ReadToEndAsync();
        }

        await Assert.That(content).IsNotEmpty();
        
        // Cleanup logs after test to keep local environment clean (User Request)
        try
        {
            if (Directory.Exists(logDir)) Directory.Delete(logDir, true);
        }
        catch 
        {
            // Best effort cleanup
        }
    }
}

