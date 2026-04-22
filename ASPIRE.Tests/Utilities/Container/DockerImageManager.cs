namespace ASPIRE.Tests.Utilities.Container;

/// <summary>
///     Manages pre-pulling of Docker images used by the integration test suite.
///     Pulling images up front (rather than on first container start) keeps per-test timings accurate and prevents concurrent tests from racing against the same pull.
/// </summary>
/// <remarks>
///     Drives the Docker daemon through <see cref="DockerClient"/> directly so the host running the tests does not need the <c>docker</c> CLI on its <c>PATH</c>.
/// </remarks>
public static class DockerImageManager
{
    /// <summary>
    ///     Ensures each image is present locally, pulling only those that are missing.
    /// </summary>
    public static async Task EnsureImagesArePulled(params string[] images)
    {
        DockerClientConfiguration configuration = new(new Uri(DockerEndpointResolver.GetDockerEndpoint()));

        using DockerClient client = configuration.CreateClient();

        foreach (string image in images)
        {
            if (await ImageIsPresent(client, image))
            {
                Console.WriteLine($"  [CACHED__] {image}");

                continue;
            }

            Console.WriteLine($"  [PULLING_] {image} ...");

            try
            {
                await client.Images.CreateImageAsync
                (
                    new ImagesCreateParameters { FromImage = image },
                    authConfig: null,
                    progress: new Progress<JSONMessage>()
                );

                Console.WriteLine($"  [PULLED__] {image}");
            }

            catch (Exception exception)
            {
                Console.WriteLine($"  [WARNING_] Failed To Pull {image}: {exception.Message}");
            }
        }
    }

    private static async Task<bool> ImageIsPresent(DockerClient client, string image)
    {
        try
        {
            await client.Images.InspectImageAsync(image);

            return true;
        }

        catch (DockerImageNotFoundException)
        {
            return false;
        }

        catch (Exception exception)
        {
            Console.WriteLine($"  [WARNING_] Failed To Inspect {image}: {exception.Message}");

            return false;
        }
    }
}
