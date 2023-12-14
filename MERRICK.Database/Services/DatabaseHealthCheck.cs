namespace MERRICK.Database.Services;

internal class DatabaseHealthCheck(DatabaseInitialiser initialiser) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Task? task = initialiser.ExecuteTask;

        return task switch
        {
            { IsCompletedSuccessfully: true }   => Task.FromResult(HealthCheckResult.Healthy()),
            { IsFaulted: true }                 => Task.FromResult(HealthCheckResult.Unhealthy(task.Exception.InnerException?.Message, task.Exception)),
            { IsCanceled: true }                => Task.FromResult(HealthCheckResult.Unhealthy("Database Initialisation Was Cancelled")),
            _                                   => Task.FromResult(HealthCheckResult.Degraded("Database Initialization Is Still In Progress"))
        };
    }
}
