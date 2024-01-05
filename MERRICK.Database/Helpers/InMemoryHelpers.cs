namespace MERRICK.Database.Helpers;

public static class InMemoryHelpers
{
    public static MerrickContext GetInMemoryMerrickContext(string? identifier = null)
    {
        DbContextOptionsBuilder<MerrickContext> builder = new();

        builder.UseInMemoryDatabase(identifier ?? Guid.NewGuid().ToString());

        DbContextOptions<MerrickContext> options = builder.Options;

        MerrickContext context = new(options);

        context.Database.EnsureCreated();

        return context;
    }
}
