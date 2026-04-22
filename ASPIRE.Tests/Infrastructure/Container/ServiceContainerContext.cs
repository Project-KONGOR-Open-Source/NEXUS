namespace ASPIRE.Tests.Infrastructure.Container;

/// <summary>
///     Groups the singleton container instances shared across the test assembly.
///     Passed through the factory chain so that adding a new container type requires only updating this record and the resolver registration, rather than every concrete factory signature.
/// </summary>
/// <param name="SQLServer">The SQL Server container.</param>
/// <param name="Redis">The Redis container.</param>
/// <param name="WireMock">The WireMock container.</param>
public sealed record ServiceContainerContext(SQLServerContainer SQLServer, RedisContainer Redis, WireMockContainer WireMock);
