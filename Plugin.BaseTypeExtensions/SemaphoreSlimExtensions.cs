namespace Plugin.BaseTypeExtensions;

/// <summary>
/// Provides extension methods for working with <see cref="SemaphoreSlim"/> objects, including optional timeout patterns.
/// </summary>
public static class SemaphoreSlimExtensions
{
    /// <summary>
    /// Waits asynchronously to enter the semaphore within an optional timeout period.
    /// </summary>
    /// <param name="semaphore">The semaphore to wait on.</param>
    /// <param name="timeout">The optional timeout duration. If null, zero, or negative, the method calls the original WaitAsync (no timeout).</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the semaphore.</param>
    /// <returns>
    /// If timeout is null, zero, or negative: returns the result of the original WaitAsync call.
    /// Otherwise: returns a task that completes when the semaphore is entered or when the timeout expires.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="semaphore"/> is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.</exception>
    /// <remarks>
    /// This method provides a convenient way to add optional timeouts to semaphore wait operations. When no timeout is needed,
    /// simply pass null and the original WaitAsync is called. This avoids the overhead of timeout monitoring
    /// when it's not required.
    /// </remarks>
    public static async Task<bool> WaitBetterAsync(this SemaphoreSlim semaphore, TimeSpan? timeout, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(semaphore);
        if (!timeout.HasValue || timeout.Value <= TimeSpan.Zero)
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        // Cap very large timeouts to avoid ArgumentOutOfRangeException from SemaphoreSlim.WaitAsync
        // The maximum is Int32.MaxValue milliseconds (about 24.8 days)
        var actualTimeout = timeout.Value;
        const long maxAllowedTimeoutMs = int.MaxValue;
        if (actualTimeout.TotalMilliseconds > maxAllowedTimeoutMs)
        {
            actualTimeout = TimeSpan.FromMilliseconds(maxAllowedTimeoutMs);
        }

        return await semaphore.WaitAsync(actualTimeout, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Waits asynchronously to enter the semaphore within an optional timeout period in milliseconds.
    /// </summary>
    /// <param name="semaphore">The semaphore to wait on.</param>
    /// <param name="millisecondsTimeout">The optional timeout in milliseconds. If null, zero, or negative, the method calls the original WaitAsync (no timeout).</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the semaphore.</param>
    /// <returns>
    /// If timeout is null, zero, or negative: returns the result of the original WaitAsync call.
    /// Otherwise: returns a task that completes when the semaphore is entered or when the timeout expires.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="semaphore"/> is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.</exception>
    /// <remarks>
    /// This method provides a convenient way to add optional timeouts to semaphore wait operations. When no timeout is needed,
    /// simply pass null and the original WaitAsync is called. This avoids the overhead of timeout monitoring
    /// when it's not required.
    /// </remarks>
    public static async Task<bool> WaitBetterAsync(this SemaphoreSlim semaphore, int? millisecondsTimeout, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(semaphore);
        if (!millisecondsTimeout.HasValue || millisecondsTimeout.Value <= 0)
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        // Cap very large timeouts to avoid ArgumentOutOfRangeException from SemaphoreSlim.WaitAsync
        // The maximum is Int32.MaxValue milliseconds (about 24.8 days)
        var actualTimeout = millisecondsTimeout.Value;
        const int maxAllowedTimeoutMs = int.MaxValue;
        if (actualTimeout > maxAllowedTimeoutMs)
        {
            actualTimeout = maxAllowedTimeoutMs;
        }

        return await semaphore.WaitAsync(actualTimeout, cancellationToken).ConfigureAwait(false);
    }
}
