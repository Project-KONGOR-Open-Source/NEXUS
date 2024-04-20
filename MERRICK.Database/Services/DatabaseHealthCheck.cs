namespace MERRICK.Database.Services;

public class DatabaseHealthCheck(DatabaseInitializer initializer) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Task? task = initializer.ExecuteTask;

        return task switch
        {
            { IsCompletedSuccessfully: true }   => Task.FromResult(HealthCheckResult.Healthy("[HEALTHY] Database Is Accepting Connections")),
            { IsFaulted: true }                 => Task.FromResult(HealthCheckResult.Unhealthy($"[UNHEALTHY] {task.Exception.InnerException?.Message}", task.Exception)),
            { IsCanceled: true }                => Task.FromResult(HealthCheckResult.Unhealthy("[UNHEALTHY] Database Initialization Was Cancelled")),
            _                                   => Task.FromResult(HealthCheckResult.Unhealthy("[UNHEALTHY] Database Initialization Is In Progress"))
        };
    }
}
