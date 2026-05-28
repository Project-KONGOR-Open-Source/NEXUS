namespace ASPIRE.Tests.Utilities.Concurrency;

/// <summary>
///     A thin asynchronous mutual-exclusion primitive built on <see cref="SemaphoreSlim"/>.
///     Returned <see cref="Scope"/> values are released when disposed, so the idiomatic use is <c>using (await Lock.EnterScope()) { ... }</c>.
/// </summary>
public sealed class AsynchronousLock : IDisposable
{
    private SemaphoreSlim Semaphore { get; } = new(initialCount: 1, maxCount: 1);

    /// <summary>
    ///     Acquires the lock asynchronously, returning a disposable <see cref="Scope"/> that releases the lock when disposed.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    public async Task<Scope> EnterScope(CancellationToken cancellationToken = default)
    {
        await Semaphore.WaitAsync(cancellationToken);

        return new Scope(Semaphore);
    }

    /// <summary>
    ///     Disposes the underlying <see cref="SemaphoreSlim"/>.
    /// </summary>
    public void Dispose()
    {
        Semaphore.Dispose();
    }

    /// <summary>
    ///     A disposable token representing lock ownership; releases the underlying <see cref="SemaphoreSlim"/> when disposed.
    /// </summary>
    public readonly struct Scope(SemaphoreSlim semaphore) : IDisposable
    {
        public void Dispose()
        {
            semaphore.Release();
        }
    }
}
