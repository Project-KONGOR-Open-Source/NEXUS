namespace ASPIRE.Tests.Infrastructure.Container;

/// <summary>
///     Manages the lifecycle of a SQL Server container for integration tests.
///     A single container instance is shared across all tests in the assembly; per-test isolation is achieved by creating a distinct database within the container for each factory instance.
/// </summary>
public sealed class SQLServerContainer : IAsyncDisposable
{
    /// <summary>
    ///     The Microsoft SQL Server container image used by the test suite.
    /// </summary>
    public const string Image = "mcr.microsoft.com/mssql/server:latest";

    /// <summary>
    ///     Display name used in error messages.
    /// </summary>
    public const string DisplayName = "SQL Server";

    /// <summary>
    ///     A random, per-container password set on the SA account. Regenerated on every test run.
    ///     SQL Server's complexity policy requires characters from three of four categories (upper, lower, digit, symbol); a bare <see cref="Guid"/> string is lower-case hex only, so we prepend an upper-case letter and a symbol.
    /// </summary>
    public string Password { get; } = $"PASSWORD_{Guid.CreateVersion7():N}";

    private AsynchronousLock Lock { get; } = new ();

    private MsSqlContainer? Self { get; set; }

    /// <summary>
    ///     The full connection string for the shared container. Targets the default database.
    /// </summary>
    public string ConnectionString
        => Self?.GetConnectionString() ?? throw new NullReferenceException($"{DisplayName} Container Connection String Is NULL");

    /// <summary>
    ///     Returns a connection string that targets the specified database within the shared container.
    ///     Used for per-test database isolation so each factory instance operates on its own database.
    /// </summary>
    /// <param name="databaseName">The name of the database to target.</param>
    public string GetConnectionString(string databaseName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);

        SqlConnectionStringBuilder builder = new (ConnectionString)
        {
            InitialCatalog = databaseName
        };

        return builder.ConnectionString;
    }

    /// <summary>
    ///     Starts the SQL Server container if it has not already been started.
    ///     Idempotent and thread-safe: concurrent callers will await the same startup work.
    ///     A container that exits during startup is replaced with a fresh one, up to a bounded number of attempts.
    /// </summary>
    public async Task StartAsync()
    {
        using (await Lock.EnterScope())
        {
            if (Self is not null)
            {
                return;
            }

            int maximumAttempts = 3;

            for (int attempt = 1; attempt <= maximumAttempts; attempt++)
            {
                MsSqlBuilder builder = new MsSqlBuilder(image: Image)
                    .WithPassword(Password)
                    .WithDockerEndpoint(DockerEndpointResolver.GetDockerEndpoint());

                Self = builder.Build();

                try
                {
                    await Self.StartAsync();

                    break;
                }

                // SQL Server 2025 Intermittently Crashes While Starting Up, So A Container That Exits Before Becoming Ready Is Replaced With A Fresh One: https://github.com/microsoft/mssql-docker/issues/974
                catch (ContainerNotRunningException) when (attempt < maximumAttempts)
                {
                    await Self.DisposeAsync();
                }
            }
        }
    }

    /// <summary>
    ///     Disposes the underlying SQL Server container.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (Self is not null)
        {
            await Self.DisposeAsync();
        }
    }
}
