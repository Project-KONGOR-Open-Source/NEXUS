namespace MERRICK.DatabaseContext.Extensions;

/// <summary>
///     Extension methods for <see cref="DbContextOptionsBuilder"/>.
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    ///     Adds all required interceptors for the Merrick database context.
    /// </summary>
    /// <param name="optionsBuilder">
    ///     The options builder to configure.
    /// </param>
    /// <returns>
    ///     The same options builder instance for method chaining.
    /// </returns>
    public static DbContextOptionsBuilder AddMerrickInterceptors(this DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new AccountStatisticsInterceptor());

        return optionsBuilder;
    }
}
