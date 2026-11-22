
namespace Plugin.BaseTypeExtensions;

/// <summary>
/// Provides extension methods for working with <see cref="Task"/> and <see cref="Task{TResult}"/> objects, including optional timeout patterns.
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// Waits for the task to complete within an optional timeout period.
    /// </summary>
    /// <typeparam name="T">The type of the task result.</typeparam>
    /// <param name="task">The task to wait for.</param>
    /// <param name="timeout">The optional timeout duration. If null, zero, or negative, the method returns the original task (no timeout).</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// If timeout is null, zero, or negative: returns the original task.
    /// Otherwise: returns a task that completes when the original task completes or when the timeout expires.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="task"/> is null.</exception>
    /// <exception cref="TimeoutException">Thrown when the task doesn't complete within the specified timeout.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.</exception>
    /// <remarks>
    /// This method provides a convenient way to add optional timeouts to any task. When no timeout is needed,
    /// simply pass null and the original task is returned unchanged. This avoids the overhead of timeout monitoring
    /// when it's not required.
    /// </remarks>
    public static Task<T> WaitBetterAsync<T>(this Task<T> task, TimeSpan? timeout, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        if (!timeout.HasValue || timeout.Value <= TimeSpan.Zero)
        {
            return task;
        }

        // Cap very large timeouts to avoid ArgumentOutOfRangeException from Task.WaitAsync
        // The maximum is approximately 4294967294 milliseconds (about 49.7 days)
        var actualTimeout = timeout.Value;
        const long maxAllowedTimeoutMs = 4294967294L;
        if (actualTimeout.TotalMilliseconds > maxAllowedTimeoutMs)
        {
            actualTimeout = TimeSpan.FromMilliseconds(maxAllowedTimeoutMs);
        }

        return task.WaitAsync(actualTimeout, cancellationToken);
    }

    /// <summary>
    /// Waits for the task to complete within an optional timeout period.
    /// </summary>
    /// <param name="task">The task to wait for.</param>
    /// <param name="timeout">The optional timeout duration. If null, zero, or negative, the method returns the original task (no timeout).</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// If timeout is null, zero, or negative: returns the original task.
    /// Otherwise: returns a task that completes when the original task completes or when the timeout expires.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="task"/> is null.</exception>
    /// <exception cref="TimeoutException">Thrown when the task doesn't complete within the specified timeout.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.</exception>
    /// <remarks>
    /// This method provides a convenient way to add optional timeouts to any task. When no timeout is needed,
    /// simply pass null and the original task is returned unchanged. This avoids the overhead of timeout monitoring
    /// when it's not required.
    /// </remarks>
    public static Task WaitBetterAsync(this Task task, TimeSpan? timeout, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        if (!timeout.HasValue || timeout.Value <= TimeSpan.Zero)
        {
            return task;
        }

        // Cap very large timeouts to avoid ArgumentOutOfRangeException from Task.WaitAsync
        // The maximum is approximately 4294967294 milliseconds (about 49.7 days)
        var actualTimeout = timeout.Value;
        const long maxAllowedTimeoutMs = 4294967294L;
        if (actualTimeout.TotalMilliseconds > maxAllowedTimeoutMs)
        {
            actualTimeout = TimeSpan.FromMilliseconds(maxAllowedTimeoutMs);
        }

        return task.WaitAsync(actualTimeout, cancellationToken);
    }

#pragma warning disable CA1031 // Do not catch general exception types
    /// <summary>
    /// Executes the task asynchronously in a fire-and-forget manner without waiting for its completion.
    /// This method is intended for scenarios where you want to start a background operation without blocking
    /// the caller, and you need to handle any exceptions that may occur during execution.
    /// </summary>
    /// <param name="task">The task to execute. Must not be null.</param>
    /// <param name="onException">
    /// An action that will be invoked if the task throws an exception. The exception will be passed
    /// as a parameter to this action. This handler is called on the thread pool context (due to ConfigureAwait(false)).
    /// Must not be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="task"/> or <paramref name="onException"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is useful for starting background operations where you don't need to await the result,
    /// but you want to ensure that exceptions are handled gracefully rather than being silently swallowed
    /// or causing unobserved task exceptions.
    /// </para>
    /// <para>
    /// The method returns immediately after starting the task execution. The task runs on the thread pool
    /// context (ConfigureAwait(false) is used), ensuring it doesn't capture the synchronization context.
    /// </para>
    /// <para>
    /// Common use cases include:
    /// <list type="bullet">
    /// <item><description>Logging operations that shouldn't block the main flow</description></item>
    /// <item><description>Fire-and-forget notifications or telemetry</description></item>
    /// <item><description>Background cleanup or maintenance tasks</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Warning: Because this is an async void method, exceptions thrown before the first await
    /// cannot be caught by the caller. Always ensure that the task parameter is valid before calling this method.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example: Start a background logging operation
    /// LogAsync(message).StartAndForget(ex =>
    ///     Console.WriteLine($"Logging failed: {ex.Message}"));
    ///
    /// // Example: Send telemetry without blocking
    /// SendTelemetryAsync(data).StartAndForget(ex =>
    ///     _logger.LogError(ex, "Telemetry failed"));
    /// </code>
    /// </example>
    public async static void StartAndForget(this Task task, Action<Exception> onException)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(onException);
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            onException.Invoke(ex);
        }
    }

    /// <summary>
    /// Executes the task asynchronously in a fire-and-forget manner without waiting for its completion,
    /// and signals the completion or failure through a <see cref="TaskCompletionSource"/>.
    /// This method is intended for scenarios where you want to start a background operation without blocking
    /// the caller, and you need to track its completion status via a TaskCompletionSource.
    /// </summary>
    /// <param name="task">The task to execute. Must not be null.</param>
    /// <param name="completionSource">
    /// A <see cref="TaskCompletionSource"/> that will be completed when the task finishes.
    /// If the task completes successfully, <see cref="TaskCompletionSource.TrySetResult"/> is called.
    /// If the task throws an exception, <see cref="TaskCompletionSource.TrySetException(Exception)"/> is called with that exception.
    /// Must not be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="task"/> or <paramref name="completionSource"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is useful for starting background operations where you don't need to await the result immediately,
    /// but you want to track completion status through a TaskCompletionSource. This allows you to observe the
    /// completion or failure of the background operation at a later time.
    /// </para>
    /// <para>
    /// The method returns immediately after starting the task execution. The task runs on the thread pool
    /// context (ConfigureAwait(false) is used), ensuring it doesn't capture the synchronization context.
    /// </para>
    /// <para>
    /// Common use cases include:
    /// <list type="bullet">
    /// <item><description>Starting background operations that need to signal completion to waiting code</description></item>
    /// <item><description>Fire-and-forget operations where you want to track success/failure externally</description></item>
    /// <item><description>Coordinating multiple background tasks through their completion sources</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Warning: Because this is an async void method, exceptions thrown before the first await
    /// cannot be caught by the caller. Always ensure that the task parameter is valid before calling this method.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example: Track completion of a background operation
    /// var tcs = new TaskCompletionSource();
    /// BackgroundWorkAsync().StartAndForget(tcs);
    /// // Later, you can await tcs.Task to know when it completed
    /// await tcs.Task;
    ///
    /// // Example: Coordinate multiple background operations
    /// var operations = new[] { Op1Async(), Op2Async(), Op3Async() };
    /// var completionSources = operations.Select(_ => new TaskCompletionSource()).ToArray();
    /// for (int i = 0; i &lt; operations.Length; i++)
    /// {
    ///     operations[i].StartAndForget(completionSources[i]);
    /// }
    /// await Task.WhenAll(completionSources.Select(tcs => tcs.Task));
    /// </code>
    /// </example>
    public async static void StartAndForget(this Task task, TaskCompletionSource completionSource)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(completionSource);
        try
        {
            await task.ConfigureAwait(false);
            completionSource.TrySetResult();
        }
        catch (Exception ex)
        {
            completionSource.TrySetException(ex);
        }
    }

    /// <summary>
    /// Executes the ValueTask asynchronously in a fire-and-forget manner without waiting for its completion.
    /// This method is intended for scenarios where you want to start a background operation without blocking
    /// the caller, and you need to handle any exceptions that may occur during execution.
    /// </summary>
    /// <param name="valueTask">The ValueTask to execute. Must not be null.</param>
    /// <param name="onException">
    /// An action that will be invoked if the task throws an exception. The exception will be passed
    /// as a parameter to this action. This handler is called on the thread pool context (due to ConfigureAwait(false)).
    /// Must not be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="onException"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is useful for starting background operations where you don't need to await the result,
    /// but you want to ensure that exceptions are handled gracefully rather than being silently swallowed
    /// or causing unobserved task exceptions.
    /// </para>
    /// <para>
    /// The method returns immediately after starting the task execution. The task runs on the thread pool
    /// context (ConfigureAwait(false) is used), ensuring it doesn't capture the synchronization context.
    /// </para>
    /// <para>
    /// Common use cases include:
    /// <list type="bullet">
    /// <item><description>Logging operations that shouldn't block the main flow</description></item>
    /// <item><description>Fire-and-forget notifications or telemetry</description></item>
    /// <item><description>Background cleanup or maintenance tasks</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Warning: Because this is an async void method, exceptions thrown before the first await
    /// cannot be caught by the caller. Always ensure that the valueTask parameter is valid before calling this method.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example: Start a background logging operation
    /// LogAsync(message).StartAndForget(ex =>
    ///     Console.WriteLine($"Logging failed: {ex.Message}"));
    ///
    /// // Example: Send telemetry without blocking
    /// SendTelemetryAsync(data).StartAndForget(ex =>
    ///     _logger.LogError(ex, "Telemetry failed"));
    /// </code>
    /// </example>
    public async static void StartAndForget(this ValueTask valueTask, Action<Exception> onException)
    {
        ArgumentNullException.ThrowIfNull(onException);
        try
        {
            await valueTask.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            onException.Invoke(ex);
        }
    }

    /// <summary>
    /// Executes the ValueTask asynchronously in a fire-and-forget manner without waiting for its completion,
    /// and signals the completion or failure through a <see cref="TaskCompletionSource"/>.
    /// This method is intended for scenarios where you want to start a background operation without blocking
    /// the caller, and you need to track its completion status via a TaskCompletionSource.
    /// </summary>
    /// <param name="valueTask">The ValueTask to execute. Must not be null.</param>
    /// <param name="completionSource">
    /// A <see cref="TaskCompletionSource"/> that will be completed when the task finishes.
    /// If the task completes successfully, <see cref="TaskCompletionSource.TrySetResult"/> is called.
    /// If the task throws an exception, <see cref="TaskCompletionSource.TrySetException(Exception)"/> is called with that exception.
    /// Must not be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="completionSource"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is useful for starting background operations where you don't need to await the result immediately,
    /// but you want to track completion status through a TaskCompletionSource. This allows you to observe the
    /// completion or failure of the background operation at a later time.
    /// </para>
    /// <para>
    /// The method returns immediately after starting the task execution. The task runs on the thread pool
    /// context (ConfigureAwait(false) is used), ensuring it doesn't capture the synchronization context.
    /// </para>
    /// <para>
    /// Common use cases include:
    /// <list type="bullet">
    /// <item><description>Starting background operations that need to signal completion to waiting code</description></item>
    /// <item><description>Fire-and-forget operations where you want to track success/failure externally</description></item>
    /// <item><description>Coordinating multiple background tasks through their completion sources</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Warning: Because this is an async void method, exceptions thrown before the first await
    /// cannot be caught by the caller. Always ensure that the valueTask parameter is valid before calling this method.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example: Track completion of a background operation
    /// var tcs = new TaskCompletionSource();
    /// BackgroundWorkAsync().StartAndForget(tcs);
    /// // Later, you can await tcs.Task to know when it completed
    /// await tcs.Task;
    ///
    /// // Example: Coordinate multiple background operations
    /// var operations = new[] { Op1Async(), Op2Async(), Op3Async() };
    /// var completionSources = operations.Select(_ => new TaskCompletionSource()).ToArray();
    /// for (int i = 0; i &lt; operations.Length; i++)
    /// {
    ///     operations[i].StartAndForget(completionSources[i]);
    /// }
    /// await Task.WhenAll(completionSources.Select(tcs => tcs.Task));
    /// </code>
    /// </example>
    public async static void StartAndForget(this ValueTask valueTask, TaskCompletionSource completionSource)
    {
        ArgumentNullException.ThrowIfNull(completionSource);
        try
        {
            await valueTask.ConfigureAwait(false);
            completionSource.TrySetResult();
        }
        catch (Exception ex)
        {
            completionSource.TrySetException(ex);
        }
    }
#pragma warning restore CA1031 // Do not catch general exception types
}
