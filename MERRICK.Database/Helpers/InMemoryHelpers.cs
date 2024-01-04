namespace MERRICK.Database.Helpers;

public static class InMemoryHelpers
{
    public static MerrickContext GetInMemoryMerrickContext()
    {
        DbContextOptionsBuilder<MerrickContext> builder = new();

        builder.UseInMemoryDatabase(nameof(MERRICK));

        DbContextOptions<MerrickContext> options = builder.Options;

        MerrickContext context = new(options);

        context.Database.EnsureCreated();

        return context;
    }
}
