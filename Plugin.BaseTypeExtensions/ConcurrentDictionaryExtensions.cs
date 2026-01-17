using System.Collections.Concurrent;

namespace Plugin.BaseTypeExtensions;

/// <summary>
/// Extension methods for <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey, TValue}"/>.
/// </summary>
public static class ConcurrentDictionaryExtensions
{
    /// <summary>
    ///     Attempts to add the specified key and value to the dictionary with retry logic.
    ///     Retries the operation if it fails, with an optional delay between attempts.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The concurrent dictionary to add to. Cannot be null.</param>
    /// <param name="key">The key of the element to add. Cannot be null.</param>
    /// <param name="value">The value of the element to add.</param>
    /// <param name="retryCount">The number of retry attempts. Must be greater than or equal to 1. Default is 3.</param>
    /// <param name="delayBetweenRetries">The delay between retry attempts. If null, no delay is applied. Default is null.</param>
    /// <param name="cancellationToken">The cancellation token to observe. Default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <c>true</c> if the key/value pair was added successfully;
    ///     <c>false</c> if the key already exists or all retry attempts failed.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dictionary"/> or <paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="retryCount"/> is less than 1.</exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="cancellationToken"/>.</exception>
    public static async Task<bool> TryAddAsync<TKey, TValue>(
        this ConcurrentDictionary<TKey, TValue> dictionary,
        TKey key,
        TValue value,
        int retryCount = 3,
        TimeSpan? delayBetweenRetries = null,
        CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        ArgumentNullException.ThrowIfNull(key);
        if (retryCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(retryCount), retryCount, "Retry count must be at least 1.");
        }

        for (var attempt = 0; attempt < retryCount; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (dictionary.TryAdd(key, value))
            {
                return true;
            }

            if (attempt < retryCount - 1 && delayBetweenRetries.HasValue)
            {
                await Task.Delay(delayBetweenRetries.Value, cancellationToken).ConfigureAwait(false);
            }
        }

        return false;
    }

    /// <summary>
    ///     Attempts to remove the value with the specified key from the dictionary with retry logic.
    ///     Retries the operation if it fails, with an optional delay between attempts.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The concurrent dictionary to remove from. Cannot be null.</param>
    /// <param name="key">The key of the element to remove. Cannot be null.</param>
    /// <param name="retryCount">The number of retry attempts. Must be greater than or equal to 1. Default is 3.</param>
    /// <param name="delayBetweenRetries">The delay between retry attempts. If null, no delay is applied. Default is null.</param>
    /// <param name="cancellationToken">The cancellation token to observe. Default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is a tuple containing:
    ///     <list type="bullet">
    ///         <item><description><c>Success</c>: <c>true</c> if the element was removed successfully; otherwise, <c>false</c>.</description></item>
    ///         <item><description><c>Value</c>: The removed value if successful; otherwise, <c>default(TValue)</c>.</description></item>
    ///     </list>
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dictionary"/> or <paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="retryCount"/> is less than 1.</exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="cancellationToken"/>.</exception>
    public static async Task<(bool Success, TValue? Value)> TryRemoveAsync<TKey, TValue>(
        this ConcurrentDictionary<TKey, TValue> dictionary,
        TKey key,
        int retryCount = 3,
        TimeSpan? delayBetweenRetries = null,
        CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        ArgumentNullException.ThrowIfNull(key);
        if (retryCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(retryCount), retryCount, "Retry count must be at least 1.");
        }

        for (var attempt = 0; attempt < retryCount; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (dictionary.TryRemove(key, out var value))
            {
                return (true, value);
            }

            if (attempt < retryCount - 1 && delayBetweenRetries.HasValue)
            {
                await Task.Delay(delayBetweenRetries.Value, cancellationToken).ConfigureAwait(false);
            }
        }

        return (false, default);
    }

    /// <summary>
    ///     Attempts to update the value associated with the specified key with retry logic.
    ///     Retries the operation if it fails, with an optional delay between attempts.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The concurrent dictionary to update. Cannot be null.</param>
    /// <param name="key">The key of the value to update. Cannot be null.</param>
    /// <param name="newValue">The value that replaces the value of the element with <paramref name="key"/> if the comparison results in equality.</param>
    /// <param name="comparisonValue">The value that is compared to the value of the element with <paramref name="key"/>.</param>
    /// <param name="retryCount">The number of retry attempts. Must be greater than or equal to 1. Default is 3.</param>
    /// <param name="delayBetweenRetries">The delay between retry attempts. If null, no delay is applied. Default is null.</param>
    /// <param name="cancellationToken">The cancellation token to observe. Default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <c>true</c> if the value with <paramref name="key"/> was equal to
    ///     <paramref name="comparisonValue"/> and replaced with <paramref name="newValue"/>; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dictionary"/> or <paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="retryCount"/> is less than 1.</exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="cancellationToken"/>.</exception>
    public static async Task<bool> TryUpdateAsync<TKey, TValue>(
        this ConcurrentDictionary<TKey, TValue> dictionary,
        TKey key,
        TValue newValue,
        TValue comparisonValue,
        int retryCount = 3,
        TimeSpan? delayBetweenRetries = null,
        CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        ArgumentNullException.ThrowIfNull(key);
        if (retryCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(retryCount), retryCount, "Retry count must be at least 1.");
        }

        for (var attempt = 0; attempt < retryCount; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (dictionary.TryUpdate(key, newValue, comparisonValue))
            {
                return true;
            }

            if (attempt < retryCount - 1 && delayBetweenRetries.HasValue)
            {
                await Task.Delay(delayBetweenRetries.Value, cancellationToken).ConfigureAwait(false);
            }
        }

        return false;
    }

    /// <summary>
    ///     Attempts to get the value associated with the specified key with retry logic.
    ///     Retries the operation if it fails, with an optional delay between attempts.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The concurrent dictionary to get the value from. Cannot be null.</param>
    /// <param name="key">The key of the value to get. Cannot be null.</param>
    /// <param name="retryCount">The number of retry attempts. Must be greater than or equal to 1. Default is 3.</param>
    /// <param name="delayBetweenRetries">The delay between retry attempts. If null, no delay is applied. Default is null.</param>
    /// <param name="cancellationToken">The cancellation token to observe. Default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is a tuple containing:
    ///     <list type="bullet">
    ///         <item><description><c>Success</c>: <c>true</c> if the key was found; otherwise, <c>false</c>.</description></item>
    ///         <item><description><c>Value</c>: The value associated with the key if found; otherwise, <c>default(TValue)</c>.</description></item>
    ///     </list>
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dictionary"/> or <paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="retryCount"/> is less than 1.</exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="cancellationToken"/>.</exception>
    public static async Task<(bool Success, TValue? Value)> TryGetValueAsync<TKey, TValue>(
        this ConcurrentDictionary<TKey, TValue> dictionary,
        TKey key,
        int retryCount = 3,
        TimeSpan? delayBetweenRetries = null,
        CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        ArgumentNullException.ThrowIfNull(key);
        if (retryCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(retryCount), retryCount, "Retry count must be at least 1.");
        }

        for (var attempt = 0; attempt < retryCount; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (dictionary.TryGetValue(key, out var value))
            {
                return (true, value);
            }

            if (attempt < retryCount - 1 && delayBetweenRetries.HasValue)
            {
                await Task.Delay(delayBetweenRetries.Value, cancellationToken).ConfigureAwait(false);
            }
        }

        return (false, default);
    }
}
