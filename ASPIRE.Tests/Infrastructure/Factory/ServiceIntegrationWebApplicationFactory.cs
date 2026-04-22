namespace ASPIRE.Tests.Infrastructure.Factory;

/// <summary>
///     Abstract base <see cref="WebApplicationFactory{TEntryPoint}"/> for service integration tests.
///     Provides container lifecycle management, per-test SQL Server database isolation, Redis key-prefix scoping, and WireMock path scoping that are common to every service.
/// </summary>
/// <remarks>
///     Uses the Curiously Recurring Template Pattern so that fluent builder methods (<see cref="WithSQLServerContainer"/>, <see cref="WithRedisContainer"/>, <see cref="WithWireMockContainer"/>, <see cref="InitialiseAsync"/>) return the concrete factory type, enabling method chaining in tests.
///     <code>
///         await factory.WithSQLServerContainer().WithRedisContainer().InitialiseAsync();
///     </code>
///     Every service uses <see cref="MerrickContext"/> as its sole <see cref="DbContext"/>, so the base library owns its registration and migration.
///     Derived factories override <see cref="ConfigureEnvironment"/> and <see cref="ConfigureAdditionalServices"/> to supply per-service settings and service replacements (e.g. an email service test double, or a loopback remote-IP startup filter).
/// </remarks>
/// <typeparam name="TSelf">The derived factory type, enabling fluent method chaining.</typeparam>
/// <typeparam name="TAssemblyMarker">The assembly-marker interface of the service under test.</typeparam>
public abstract class ServiceIntegrationWebApplicationFactory<TSelf, TAssemblyMarker>(ServiceContainerContext containerContext) : WebApplicationFactory<TAssemblyMarker>, IAsyncDisposable
    where TSelf : ServiceIntegrationWebApplicationFactory<TSelf, TAssemblyMarker>
    where TAssemblyMarker : class
{
    /// <summary>
    ///     The unique identifier associated with this factory instance.
    ///     Every per-test scoping decision derives from this value so that concurrent tests never collide.
    /// </summary>
    public Guid GUID { get; } = Guid.CreateVersion7();

    private string DatabaseName => $"test_{GUID:N}";

    private string RedisKeyPrefix => $"test:{GUID:N}:";

    private string WireMockPathPrefix => $"test/{GUID:N}";

    /// <summary>
    ///     Indicates whether <see cref="WithSQLServerContainer"/> was called on this factory.
    ///     Exposed to derived classes so their <see cref="ConfigureAdditionalServices"/> overrides can make container-aware registration decisions.
    /// </summary>
    protected bool UseSQLServerContainer { get; private set; } = false;

    /// <summary>
    ///     Indicates whether <see cref="WithRedisContainer"/> was called on this factory.
    ///     Exposed to derived classes so their <see cref="ConfigureAdditionalServices"/> overrides can make container-aware registration decisions.
    /// </summary>
    protected bool UseRedisContainer { get; private set; } = false;

    /// <summary>
    ///     Indicates whether <see cref="WithWireMockContainer"/> was called on this factory.
    ///     Exposed to derived classes so their <see cref="ConfigureAdditionalServices"/> overrides can make container-aware registration decisions.
    /// </summary>
    protected bool UseWireMockContainer { get; private set; } = false;

    private AsynchronousLock Lock { get; } = new();

    private bool IsInitialised { get; set; } = false;

    /// <summary>
    ///     The scoped WireMock client for configuring mock HTTP responses.
    ///     All mapping operations automatically prepend a unique path prefix derived from <see cref="GUID"/> so that concurrent tests never collide.
    ///     Only valid after calling <see cref="InitialiseAsync"/> with <see cref="WithWireMockContainer"/> enabled beforehand.
    /// </summary>
    public ScopedWireMockClient WireMockClient => new(containerContext.WireMock.AdministrativeClient, WireMockPathPrefix);

    /// <summary>
    ///     The scoped WireMock URL for this factory instance.
    ///     Includes a unique path prefix derived from <see cref="GUID"/> so that mappings from different tests never collide.
    /// </summary>
    public string WireMockURL => containerContext.WireMock.GetScopedURL(GUID);

    /// <summary>
    ///     The scoped WireMock URI for this factory instance.
    /// </summary>
    public Uri WireMockURI => new(WireMockURL);

    /// <summary>
    ///     Builds an absolute URL beneath the scoped WireMock URI for this factory instance.
    /// </summary>
    /// <param name="relativePath">The relative path to append to the scoped WireMock URL.</param>
    public string BuildWireMockEndpointURL(string relativePath)
        => $"{WireMockURL.TrimEnd('/')}/{relativePath.TrimStart('/')}";

    /// <summary>
    ///     Enables the SQL Server container for this factory.
    ///     Must be called before <see cref="InitialiseAsync"/>.
    /// </summary>
    public TSelf WithSQLServerContainer()
    {
        ThrowIfInitialised();

        UseSQLServerContainer = true;

        return (TSelf)this;
    }

    /// <summary>
    ///     Enables the Redis container for this factory.
    ///     Must be called before <see cref="InitialiseAsync"/>.
    /// </summary>
    public TSelf WithRedisContainer()
    {
        ThrowIfInitialised();

        UseRedisContainer = true;

        return (TSelf)this;
    }

    /// <summary>
    ///     Enables the WireMock container for this factory.
    ///     Must be called before <see cref="InitialiseAsync"/>.
    /// </summary>
    public TSelf WithWireMockContainer()
    {
        ThrowIfInitialised();

        UseWireMockContainer = true;

        return (TSelf)this;
    }

    /// <summary>
    ///     Starts the requested containers and, if a SQL Server container is enabled, creates the per-test database and runs all Entity Framework Core migrations.
    ///     Idempotent and thread-safe: concurrent callers will await the same initialisation work.
    /// </summary>
    public async Task<TSelf> InitialiseAsync()
    {
        using (await Lock.EnterScope())
        {
            if (IsInitialised is false)
            {
                if (UseWireMockContainer)
                {
                    await containerContext.WireMock.StartAsync();
                }

                if (UseRedisContainer)
                {
                    await containerContext.Redis.StartAsync();
                }

                if (UseSQLServerContainer)
                {
                    await containerContext.SQLServer.StartAsync();

                    await EnsureDatabaseCreated();
                }

                IsInitialised = true;
            }

            return (TSelf)this;
        }
    }

    /// <summary>
    ///     Applies per-service web-host settings (typically environment variables exposed via <see cref="IWebHostBuilder.UseSetting"/>).
    ///     Called at the beginning of <see cref="ConfigureWebHost"/>, before any service replacements run.
    /// </summary>
    /// <param name="builder">The web host builder to configure.</param>
    protected virtual void ConfigureEnvironment(IWebHostBuilder builder) { }

    /// <summary>
    ///     Applies per-service dependency-injection replacements (for example, swapping an email sender for a test double or registering a startup filter).
    ///     Called after the base factory has replaced the <see cref="MerrickContext"/> and cache store registrations.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    protected virtual void ConfigureAdditionalServices(IServiceCollection services) { }

    /// <summary>
    ///     Registers additional <see cref="DbContext"/> types that specific services require beyond the shared <see cref="MerrickContext"/>.
    ///     Left empty by default since every current service uses only the shared context.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The per-test SQL Server connection string.</param>
    protected virtual void RegisterAdditionalDatabaseContexts(IServiceCollection services, string connectionString) { }

    /// <summary>
    ///     Migrates additional <see cref="DbContext"/> types registered by <see cref="RegisterAdditionalDatabaseContexts"/>.
    ///     Left empty by default; the shared <see cref="MerrickContext"/> is migrated by the base factory itself.
    /// </summary>
    /// <param name="serviceProvider">The scoped service provider.</param>
    protected virtual Task MigrateAdditionalDatabaseContexts(IServiceProvider serviceProvider) => Task.CompletedTask;

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        ConfigureEnvironment(builder);

        builder.ConfigureServices(services =>
        {
            if (UseSQLServerContainer)
            {
                string connectionString = containerContext.SQLServer.GetConnectionString(DatabaseName);

                RemoveDbContextRegistrations<MerrickContext>(services);

                services.AddDbContext<MerrickContext>(options =>
                {
                    options.UseSqlServer(connectionString, sqlServerOptions => sqlServerOptions.MigrationsHistoryTable("MigrationsHistory", MerrickContext.MetadataSchema));
                    options.EnableThreadSafetyChecks();
                    options.AddMerrickInterceptors();
                });

                RegisterAdditionalDatabaseContexts(services, connectionString);
            }

            if (UseRedisContainer)
            {
                services.RemoveAll<IConnectionMultiplexer>();
                services.RemoveAll<IDatabase>();

                IConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(containerContext.Redis.ConnectionString);

                services.AddSingleton<IConnectionMultiplexer>(multiplexer);
                services.AddSingleton<IDatabase>(_ => multiplexer.GetDatabase().WithKeyPrefix(RedisKeyPrefix));
            }

            ConfigureAdditionalServices(services);
        });
    }

    /// <summary>
    ///     Removes every registration associated with the supplied <see cref="DbContext"/> type so that the test replacement has a clean slate.
    ///     Aspire's <c>AddSqlServerDbContext</c> registers not just the typed options and the context itself but also an <c>IDbContextOptionsConfiguration&lt;TContext&gt;</c> entry holding the production configuration callback (connection string, interceptors, logging) and, under pooling, <c>IDbContextPool&lt;TContext&gt;</c> plus <c>IScopedDbContextLease&lt;TContext&gt;</c>.
    ///     Leaving those in place would double every interceptor. For example, <c>AccountStatisticsInterceptor</c> would add its rows twice per <see cref="DbContext.SaveChangesAsync(CancellationToken)"/> and cause unique-index violations, so anything whose service type mentions <typeparamref name="TContext"/> is purged.
    /// </summary>
    protected static void RemoveDbContextRegistrations<TContext>(IServiceCollection services) where TContext : DbContext
    {
        ServiceDescriptor[] toRemove = [.. services
            .Where(descriptor => descriptor.ServiceType == typeof(TContext)
                || descriptor.ServiceType == typeof(DbContextOptions)
                || (descriptor.ServiceType.IsGenericType && descriptor.ServiceType.GetGenericArguments().Any(argument => argument == typeof(TContext))))];

        foreach (ServiceDescriptor descriptor in toRemove)
        {
            services.Remove(descriptor);
        }
    }

    private async Task EnsureDatabaseCreated()
    {
        try
        {
            using IServiceScope scope = Services.CreateScope();

            MerrickContext merrickContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

            await merrickContext.Database.MigrateAsync();

            await MigrateAdditionalDatabaseContexts(scope.ServiceProvider);
        }

        catch
        {
            // If Migration Fails Partway Through, Drop The Half-Created Database Before Propagating So It Cannot Linger Inside The Shared Container
            await DropDatabase();

            throw;
        }
    }

    /// <summary>
    ///     Disposes the factory and drops the per-test database if one was created.
    ///     The containers themselves are singletons owned by the dependency resolver and are torn down at the end of the test run.
    /// </summary>
    public override async ValueTask DisposeAsync()
    {
        if (UseSQLServerContainer && IsInitialised)
        {
            await DropDatabase();
        }

        await base.DisposeAsync();

        GC.SuppressFinalize(this);
    }

    private async Task DropDatabase()
    {
        try
        {
            string masterConnectionString = containerContext.SQLServer.GetConnectionString("master");

            await using SqlConnection connection = new(masterConnectionString);

            await connection.OpenAsync();

            string sql =
            $"""
                IF DB_ID('{DatabaseName}') IS NOT NULL
                BEGIN
                    ALTER DATABASE [{DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE [{DatabaseName}];
                END
            """;

            await using SqlCommand command = new(sql, connection);

            await command.ExecuteNonQueryAsync();
        }

        catch (Exception exception)
        {
            // Cleanup Is Best-Effort Because The Container Itself Will Be Destroyed At The End Of The Test Run, But The Failure Is Surfaced To "stderr" So Flaky Cleanup Issues Remain Visible
            Console.Error.WriteLine($@"[CLEANUP] Failed To Drop Database ""{DatabaseName}"": {exception.Message}");
        }
    }

    private void ThrowIfInitialised([CallerMemberName] string memberName = "")
    {
        if (IsInitialised)
        {
            throw new InvalidOperationException($@"Cannot Call ""{memberName}"" After Initialisation");
        }
    }
}
