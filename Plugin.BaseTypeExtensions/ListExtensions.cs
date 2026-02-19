using System.Runtime.CompilerServices;

namespace Plugin.BaseTypeExtensions;

/// <summary>
/// Provides extension methods for <see cref="IList{T}"/> to support advanced update and pairing operations.
/// These methods implement Layer 3 (convenience) of the UpdateFrom architecture, delegating to Layer 2 methods in EnumerableExtensions.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    ///     Updates the output list by adding and removing specified items.
    ///     This is a Layer 3 (convenience) method for IList that provides default actions.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    /// <param name="output">The list to update.</param>
    /// <param name="addedItems">The items to add to the list.</param>
    /// <param name="removedItems">The items to remove from the list.</param>
    public static void UpdateFrom<T>(
        this IList<T> output,
        IEnumerable<T> addedItems,
        IEnumerable<T> removedItems
    )
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(addedItems);
        ArgumentNullException.ThrowIfNull(removedItems);

        lock (output)
        {
            // Delegate to Layer 1 with default IList actions
            output.UpdateFrom<T, T>(addedItems, removedItems, output.Add, item => output.Remove(item));
        }
    }

    /// <summary>
    ///     Updates the output list from the input list with item comparison and type conversion.
    ///     This is a Layer 3 (convenience) method that delegates to Layer 2 with default IList actions.
    /// </summary>
    /// <typeparam name="TInput">The type of the input items.</typeparam>
    /// <typeparam name="TOutput">The type of the output items.</typeparam>
    /// <param name="output">The list to update.</param>
    /// <param name="input">The input list to synchronize with.</param>
    /// <param name="areRepresentingTheSameItem">Function to determine if items represent the same entity.</param>
    /// <param name="fromInputTypeToOutputTypeConversion">Function to convert input items to output items.</param>
    public static void UpdateFrom<TInput, TOutput>(
        this IList<TOutput> output,
        IEnumerable<TInput> input,
        Func<TInput, TOutput, bool> areRepresentingTheSameItem,
        Func<TInput, TOutput> fromInputTypeToOutputTypeConversion)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(fromInputTypeToOutputTypeConversion);

        lock (output)
        {
            // Delegate to Layer 2 with default IList actions
            output.UpdateFrom(
                input,
                areRepresentingTheSameItem,
                fromInputTypeToOutputTypeConversion,
                output.Add,
                item => output.Remove(item));
        }
    }

    /// <summary>
    ///     Updates the output list from the input list with type conversion using IEquatable for comparison.
    ///     This is a Layer 3 (convenience) method that delegates to Layer 2 with default IList actions.
    /// </summary>
    /// <typeparam name="TInput">The type of the input items.</typeparam>
    /// <typeparam name="TOutput">The type of the output items, which must be equatable with TInput.</typeparam>
    /// <param name="output">The list to update.</param>
    /// <param name="input">The input list to synchronize with.</param>
    /// <param name="fromInputTypeToOutputTypeConversion">Function to convert input items to output items.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UpdateFrom<TInput, TOutput>(
        this IList<TOutput> output,
        IEnumerable<TInput> input,
        Func<TInput, TOutput> fromInputTypeToOutputTypeConversion)
        where TOutput : IEquatable<TInput>
    {
        output.UpdateFrom(input, (i, o) => i != null && i.Equals(o), fromInputTypeToOutputTypeConversion);
    }

    /// <summary>
    ///     Updates the output list from the input list with custom item comparison.
    ///     This is a Layer 3 (convenience) method that delegates to the other overload.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    /// <param name="output">The list to update.</param>
    /// <param name="input">The input list to synchronize with.</param>
    /// <param name="areRepresentingTheSameItem">Function to determine if items represent the same entity.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UpdateFrom<T>(
        this IList<T> output,
        IEnumerable<T> input,
        Func<T, T, bool> areRepresentingTheSameItem)
    {
        output.UpdateFrom(input, areRepresentingTheSameItem, i => i);
    }

    /// <summary>
    ///     Updates the output list from the input list using default equality comparison.
    ///     This is a Layer 3 (convenience) method that delegates to the other overload.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    /// <param name="output">The list to update.</param>
    /// <param name="input">The input list to synchronize with.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UpdateFrom<T>(this IList<T> output, IEnumerable<T> input)
    {
        output.UpdateFrom(input, (equatable1, equatable2) => equatable1 != null && equatable1.Equals(equatable2), i => i);
    }

    /// <summary>
    ///     Asynchronously updates the output list by adding and removing specified items.
    ///     This is a Layer 3 (convenience) method that provides default IList actions and delegates to Layer 1.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    /// <param name="output">The list to update.</param>
    /// <param name="addedItems">The items to add to the list.</param>
    /// <param name="removedItems">The items to remove from the list.</param>
    /// <param name="addAction">Async action to perform when adding an item. If null, uses the list's Add method.</param>
    /// <param name="removeAction">Async function to perform when removing an item. If null, uses the list's Remove method.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    public static ValueTask UpdateFromAsync<T>(
        this IList<T> output,
        IEnumerable<T> addedItems,
        IEnumerable<T> removedItems,
        Func<T, CancellationToken, ValueTask>? addAction = null,
        Func<T, CancellationToken, ValueTask<bool>>? removeAction = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(addedItems);
        ArgumentNullException.ThrowIfNull(removedItems);

        var finalAddAction = addAction ?? ((item, ct) =>
        {
            output.Add(item);
            return ValueTask.CompletedTask;
        });

        Func<T, CancellationToken, ValueTask> finalRemoveAction = removeAction != null
            ? async (item, ct) => { await removeAction(item, ct).ConfigureAwait(false); }
            : (item, ct) =>
            {
                output.Remove(item);
                return ValueTask.CompletedTask;
            };

        // Delegate to Layer 1 async
        return output.UpdateFromAsync<T, T>(
            addedItems,
            removedItems,
            finalAddAction,
            finalRemoveAction,
            cancellationToken);
    }

    /// <summary>
    ///     Asynchronously updates the output list from the input list with item comparison and type conversion.
    ///     This is a Layer 3 (convenience) method that delegates to Layer 2 async.
    /// </summary>
    /// <typeparam name="TInput">The type of the input items.</typeparam>
    /// <typeparam name="TOutput">The type of the output items.</typeparam>
    /// <param name="output">The list to update.</param>
    /// <param name="input">The input list to synchronize with.</param>
    /// <param name="areRepresentingTheSameItem">Function to determine if items represent the same entity.</param>
    /// <param name="fromInputTypeToOutputTypeConversion">Function to convert input items to output items.</param>
    /// <param name="addAction">Async action to perform when adding an item.</param>
    /// <param name="removeAction">Async action to perform when removing an item.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    public static ValueTask UpdateFromAsync<TInput, TOutput>(
        this IList<TOutput> output,
        IEnumerable<TInput> input,
        Func<TInput, TOutput, bool> areRepresentingTheSameItem,
        Func<TInput, TOutput> fromInputTypeToOutputTypeConversion,
        Func<TOutput, CancellationToken, ValueTask> addAction,
        Func<TOutput, CancellationToken, ValueTask<bool>> removeAction,
        CancellationToken cancellationToken = default
    )
    {
        // Delegate to Layer 2 async which handles diff calculation
        return output.UpdateFromAsync(
            input,
            areRepresentingTheSameItem,
            async (item, ct) =>
            {
                var converted = fromInputTypeToOutputTypeConversion(item);
                await addAction(converted, ct).ConfigureAwait(false);
            },
            async (item, ct) => { await removeAction(item, ct).ConfigureAwait(false); },
            cancellationToken);
    }

    /// <summary>
    ///     Returns pairs of consecutive elements from the list.
    /// </summary>
    /// <remarks>Returns an empty sequence if the input has fewer than two elements.</remarks>
    public static IEnumerable<ValueTuple<T, T>> Pairs<T>(this IList<T> input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var count = input.Count;
        if (count < 2)
        {
            return [];
        }

        return input.Take(count - 1).Select((o1, i) => (o1, input.ElementAt(i + 1)));
    }
}
