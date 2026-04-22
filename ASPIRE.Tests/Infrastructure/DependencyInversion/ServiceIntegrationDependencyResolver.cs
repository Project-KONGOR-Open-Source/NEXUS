namespace ASPIRE.Tests.Infrastructure.DependencyInversion;

/// <summary>
///     Base TUnit <see cref="IClassConstructor"/> that resolves test class instances against a shared <see cref="IServiceProvider"/>.
///     Container instances (<see cref="SQLServerContainer"/>, <see cref="RedisContainer"/>, <see cref="WireMockContainer"/>) are registered as singletons so they are started once per test assembly and reused across tests.
///     The per-service factory is registered as transient so every test receives a fresh factory with its own per-test state (database name, Redis key prefix, WireMock path prefix).
/// </summary>
/// <remarks>
///     Each concrete service provides a sealed derivative (for example <c>KONGORIntegrationDependencyResolver</c>) that fills in the generic type parameters and implements <see cref="BuildFactory"/>.
///     The <c>TDerived</c> type parameter lets the shared service provider invoke <see cref="BuildFactory"/> without reflection: <c>new TDerived()</c> inside the factory registration yields a throwaway instance whose only job is to produce the factory.
/// </remarks>
/// <typeparam name="TDerived">The concrete derived resolver type.</typeparam>
/// <typeparam name="TFactory">The per-service factory type.</typeparam>
/// <typeparam name="TAssemblyMarker">The assembly-marker interface of the service under test.</typeparam>
public abstract class ServiceIntegrationDependencyResolver<TDerived, TFactory, TAssemblyMarker> : IClassConstructor, ITestEndEventReceiver
    where TDerived : ServiceIntegrationDependencyResolver<TDerived, TFactory, TAssemblyMarker>, new()
    where TFactory : ServiceIntegrationWebApplicationFactory<TFactory, TAssemblyMarker>
    where TAssemblyMarker : class
{
    private static readonly IServiceProvider SharedServices = BuildServiceProvider();

    // TUnit reuses a single <see cref="IClassConstructor"/> instance across every <c>[Arguments]</c> variant of a method, so scopes cannot be stored in an instance field without concurrent tests stomping on each other's state.
    // Keying by <see cref="TestContext.Id"/> gives every test its own slot and is independent of how TUnit schedules <see cref="Create"/> and <see cref="OnTestEnd"/> across threads.
    private static readonly ConcurrentDictionary<string, IServiceScope> ScopesByTestID = new();

    /// <summary>
    ///     Creates the concrete service-specific factory given the shared container context.
    /// </summary>
    protected abstract TFactory BuildFactory(ServiceContainerContext containerContext);

    /// <inheritdoc />
    public Task<object> Create([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type, ClassConstructorMetadata classConstructorMetadata)
    {
        IServiceScope scope = SharedServices.CreateAsyncScope();

        string testID = TestContext.Current?.Id ?? throw new InvalidOperationException("TestContext.Current Is NULL Inside Create; The Resolver Cannot Key The Scope Without A Test Identifier");

        if (ScopesByTestID.TryAdd(testID, scope) is false)
        {
            throw new InvalidOperationException($@"A Scope Has Already Been Registered For Test ID ""{testID}""; Create Should Only Be Invoked Once Per Test");
        }

        object instance = ActivatorUtilities.GetServiceOrCreateInstance(scope.ServiceProvider, type);

        return Task.FromResult(instance);
    }

    /// <inheritdoc />
    public ValueTask OnTestEnd(TestContext context)
    {
        if (ScopesByTestID.TryRemove(context.Id, out IServiceScope? scope) is false)
        {
            return ValueTask.CompletedTask;
        }

        if (scope is IAsyncDisposable asyncDisposable)
        {
            return asyncDisposable.DisposeAsync();
        }

        scope.Dispose();

        return ValueTask.CompletedTask;
    }

    private static IServiceProvider BuildServiceProvider()
    {
        ServiceCollection services = new();

        services.AddSingleton<SQLServerContainer>();
        services.AddSingleton<RedisContainer>();
        services.AddSingleton<WireMockContainer>();

        services.AddSingleton<ServiceContainerContext>(serviceProvider => new ServiceContainerContext
        (
            SQLServer: serviceProvider.GetRequiredService<SQLServerContainer>(),
            Redis:     serviceProvider.GetRequiredService<RedisContainer>(),
            WireMock:  serviceProvider.GetRequiredService<WireMockContainer>()
        ));

        services.AddTransient<TFactory>(serviceProvider =>
        {
            TDerived derived = new();

            return derived.BuildFactory(serviceProvider.GetRequiredService<ServiceContainerContext>());
        });

        return services.BuildServiceProvider();
    }
}
