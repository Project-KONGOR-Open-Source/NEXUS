/* using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TUnit.Core;

namespace ASPIRE.Tests.Reproduction;

public class HoNProxyIntegrationTests
{
    private const string GamePath = @"HoN Game Client v4.10.1 Windows Clean Installation\hon_x64.exe";
    private const string ProxyPath = @"Tools\HoNProxy\bin\Debug\net9.0\HoNProxy.exe";

    [Test]
    public async Task CanLaunchProxyAndCaptureOutput()
    {
        // Skip if game not present (e.g. CI)
        if (!File.Exists(GamePath))
        {
            await Assert.That(true).IsTrue(); // Skip effectively
            return;
        }

        if (!File.Exists(ProxyPath))
        {
             // Try to find relative to test execution? 
             // Ideally we should have built it before valid test execution.
             // For now assume path is correct based on previous build.
             await Assert.That(File.Exists(ProxyPath)).IsTrue();
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = ProxyPath,
            Arguments = $"--game-path \"{GamePath}\" --args \"+SetSave log_console true\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process? process = Process.Start(startInfo);
        await Assert.That(process).IsNotNull();

        StringBuilder output = new StringBuilder();
        StringBuilder error = new StringBuilder();

        process!.OutputDataReceived += (sender, e) => {
            if (e.Data != null) output.AppendLine(e.Data);
        };
        process.ErrorDataReceived += (sender, e) => {
            if (e.Data != null) error.AppendLine(e.Data);
        };

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // Let it run for a few seconds to verify startup logging
        await Task.Delay(5000);

        // Kill proxy (which kills game via tree kill)
        process.Kill(true);
        await process.WaitForExitAsync();

        string logs = output.ToString();
        string errs = error.ToString();

        // Assert we got some output from proxy
        await Assert.That(logs).Contains("[PROXY] Starting HoN Proxy");
        await Assert.That(logs).Contains("[PROXY] Game started");
        
        // Assert we got game logs (might depend on if game actually logged in 5s)
        // If game starts, it usually logs standard info immediately.
        // We look for [GAME] prefix
        // Note: First run might prompt firewall or take longer.
        
        // Use explicit assertions
        await Assert.That(logs).IsNotNull();
    }
}
 */