namespace MERRICK.Database.Services;

public class DatabaseHealthCheck(DatabaseInitializer initializer) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Task? task = initializer.ExecuteTask;

        return task switch
        {
            { IsCompletedSuccessfully: true }   => Task.FromResult(HealthCheckResult.Healthy()),
            { IsFaulted: true }                 => Task.FromResult(HealthCheckResult.Unhealthy(task.Exception.InnerException?.Message, task.Exception)),
            { IsCanceled: true }                => Task.FromResult(HealthCheckResult.Unhealthy("Database Initialization Was Cancelled")),
            _                                   => Task.FromResult(HealthCheckResult.Degraded("Database Initialization Is Still In Progress"))
        };
    }
}
