using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MERRICK.DatabaseContext.Persistence;

public class DebugMerrickDbContextFactory : IDesignTimeDbContextFactory<MerrickContext>
{
    public MerrickContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<MerrickContext> optionsBuilder = new DbContextOptionsBuilder<MerrickContext>();
        
        // Default to 127.0.0.1:49885 (Discovered via docker ps) with SA Login (Found via docker inspect).
        // The port is currently dynamic/ephemeral in the running container.
        string connectionString = Environment.GetEnvironmentVariable("MERRICK_CONNECTION_STRING") 
                                  ?? "Server=127.0.0.1,49885;Database=merrick;User Id=sa;Password=MerrickDevPassword2025;TrustServerCertificate=True;";

        optionsBuilder.UseSqlServer(connectionString);

        return new MerrickContext(optionsBuilder.Options);
    }
}
