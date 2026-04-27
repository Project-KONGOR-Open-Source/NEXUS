namespace ASPIRE.Tests.Utilities.Container;

/// <summary>
///     Manages pre-pulling of Docker images used by the integration test suite.
///     Pulling images up front (rather than on first container start) keeps per-test timings accurate and prevents concurrent tests from racing against the same pull.
/// </summary>
/// <remarks>
///     Talks to the Docker Engine HTTP API directly over the platform-native transport (named pipe on Windows, Unix domain socket on Linux and macOS, raw TCP/HTTP otherwise) so the host running the tests does not need the <c>docker</c> CLI on its <c>PATH</c> and so that the test suite does not depend on any specific managed Docker client library.
/// </remarks>
public static class DockerImageManager
{
    /// <summary>
    ///     Ensures each image is present locally, pulling only those that are missing.
    /// </summary>
    public static async Task EnsureImagesArePulled(params string[] images)
    {
        using HttpClient client = CreateDockerEngineClient();

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
                await PullImage(client, image);

                Console.WriteLine($"  [PULLED__] {image}");
            }

            catch (Exception exception)
            {
                Console.WriteLine($"  [WARNING_] Failed To Pull {image}: {exception.Message}");
            }
        }
    }

    private static async Task<bool> ImageIsPresent(HttpClient client, string image)
    {
        try
        {
            using HttpRequestMessage request = new(HttpMethod.Get, $"/images/{Uri.EscapeDataString(image)}/json");

            using HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            return response.IsSuccessStatusCode;
        }

        catch (Exception exception)
        {
            Console.WriteLine($"  [WARNING_] Failed To Inspect {image}: {exception.Message}");

            return false;
        }
    }

    private static async Task PullImage(HttpClient client, string image)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, $"/images/create?fromImage={Uri.EscapeDataString(image)}");

        using HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        if (response.IsSuccessStatusCode is false)
        {
            string body = await response.Content.ReadAsStringAsync();

            throw new InvalidOperationException($@"Docker Engine Returned ""{(int)response.StatusCode} {response.ReasonPhrase}"": {body}");
        }

        // The Docker Engine Streams A JSON Object Per Line Until The Pull Completes, So The Stream Needs To Be Drained To Ensure The Pull Has Actually Finished Before Returning

        await using Stream stream = await response.Content.ReadAsStreamAsync();

        using StreamReader reader = new(stream);

        while (await reader.ReadLineAsync() is not null) { }
    }

    private static HttpClient CreateDockerEngineClient()
    {
        Uri endpoint = new(DockerEndpointResolver.GetDockerEndpoint());

        SocketsHttpHandler handler = new()
        {
            ConnectCallback = (context, cancellationToken) => ConnectToDockerEngine(endpoint, cancellationToken)
        };

        return new HttpClient(handler, disposeHandler: true)
        {
            BaseAddress = new Uri("http://localhost"),
            Timeout = Timeout.InfiniteTimeSpan
        };
    }

    private static async ValueTask<Stream> ConnectToDockerEngine(Uri endpoint, CancellationToken cancellationToken)
    {
        switch (endpoint.Scheme)
        {
            case "npipe":
            {
                string pipeName = endpoint.AbsolutePath.TrimStart('/').Replace("pipe/", string.Empty, StringComparison.OrdinalIgnoreCase);

                NamedPipeClientStream pipe = new(endpoint.Host == "." ? "." : endpoint.Host, pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

                await pipe.ConnectAsync(cancellationToken);

                return pipe;
            }

            case "unix":
            {
                Socket socket = new(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

                await socket.ConnectAsync(new UnixDomainSocketEndPoint(endpoint.LocalPath), cancellationToken);

                return new NetworkStream(socket, ownsSocket: true);
            }

            case "tcp":
            case "http":
            case "https":
            {
                Socket socket = new(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };

                await socket.ConnectAsync(endpoint.Host, endpoint.Port, cancellationToken);

                return new NetworkStream(socket, ownsSocket: true);
            }

            default:
            {
                throw new NotSupportedException($@"Docker Endpoint Scheme ""{endpoint.Scheme}"" Is Not Supported");
            }
        }
    }
}
