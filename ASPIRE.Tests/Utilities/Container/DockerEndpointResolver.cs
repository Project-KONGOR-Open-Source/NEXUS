namespace ASPIRE.Tests.Utilities.Container;

/// <summary>
///     Resolves the Docker daemon endpoint for the current platform.
/// </summary>
public static class DockerEndpointResolver
{
    /// <summary>
    ///     Gets the Docker endpoint to use.
    ///     Prefers the <c>DOCKER_HOST</c> environment variable, then falls back to platform defaults.
    /// </summary>
    public static string GetDockerEndpoint()
    {
        string? dockerHost = Environment.GetEnvironmentVariable("DOCKER_HOST");

        if (string.IsNullOrEmpty(dockerHost) is false)
        {
            return dockerHost;
        }

        if (OperatingSystem.IsWindows())
        {
            return @"npipe://./pipe/docker_engine";
        }

        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            return @"unix:///var/run/docker.sock";
        }

        throw new PlatformNotSupportedException($@"Platform ""{RuntimeInformation.OSDescription}"" Is Not Supported");
    }
}
