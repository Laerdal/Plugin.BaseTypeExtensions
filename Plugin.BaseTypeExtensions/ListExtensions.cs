namespace Plugin.BaseTypeExtensions;

/// <summary>
/// Provides extension methods for <see cref="IList{T}"/> to support advanced update and pairing operations.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    ///     Updates the output list by adding and removing specified items.
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
            // Add items
            foreach (var item in addedItems)
            {
                output.Add(item);
            }

            // Remove items
            foreach (var item in removedItems)
            {
                output.Remove(item);
            }
        }
    }

    /// <summary>
    ///     Updates the output list from the input list with item comparison and type conversion.
    /// </summary>
    public static void UpdateFrom<TInput, TOutput>(this IList<TOutput> output, IList<TInput> input, Func<TInput, TOutput, bool> areRepresentingTheSameItem, Func<TInput, TOutput> fromInputTypeToOutputTypeConversion)
    {
        ArgumentNullException.ThrowIfNull(fromInputTypeToOutputTypeConversion);

        lock (output)
        {
            output.UpdateFrom(input, areRepresentingTheSameItem, i => output.Add(fromInputTypeToOutputTypeConversion.Invoke(i)), o => output.Remove(o));
        }
    }

    /// <summary>
    ///     Updates the output list from the input list with item comparison and type conversion.
    /// </summary>
    public static void UpdateFrom<TInput, TOutput>(this IList<TOutput> output, IList<TInput> input, Func<TInput, TOutput> fromInputTypeToOutputTypeConversion) where TOutput : IEquatable<TInput>
    {
        lock (output)
        {
            output.UpdateFrom(input, (i, o) => i != null && i.Equals(o), i => output.Add(fromInputTypeToOutputTypeConversion.Invoke(i)), o => output.Remove(o));
        }
    }

    /// <summary>
    ///     Updates the output list from the input list with item comparison.
    /// </summary>
    public static void UpdateFrom<T>(this IList<T> output, IList<T> input, Func<T, T, bool> areRepresentingTheSameItem)
    {
        output.UpdateFrom(input, areRepresentingTheSameItem, i => i);
    }

    /// <summary>
    ///     Updates the output list from the input list with item comparison.
    /// </summary>
    public static void UpdateFrom<T>(this IList<T> output, IList<T> input)
    {
        output.UpdateFrom(input, (equatable1, equatable2) => equatable1 != null && equatable1.Equals(equatable2), i => i);
    }

    /// <summary>
    ///     Asynchronously updates the output list by adding and removing specified items.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    /// <param name="output">The list to update.</param>
    /// <param name="addedItems">The items to add to the list.</param>
    /// <param name="removedItems">The items to remove from the list.</param>
    /// <param name="addAction">Async action to perform when adding an item. If null, uses the list's Add method.</param>
    /// <param name="removeAction">Async function to perform when removing an item. If null, uses the list's Remove method.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    public static async Task UpdateFromAsync<T>(
        this IList<T> output,
        IEnumerable<T> addedItems,
        IEnumerable<T> removedItems,
        Func<T, CancellationToken, Task>? addAction = null,
        Func<T, CancellationToken, Task<bool>>? removeAction = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(addedItems);
        ArgumentNullException.ThrowIfNull(removedItems);

        var finalAddAction = addAction ?? ((item, ct) =>
        {
            output.Add(item);
            return Task.CompletedTask;
        });

        var finalRemoveAction = removeAction ?? ((item, ct) => Task.FromResult(output.Remove(item)));

        // Add items
        foreach (var item in addedItems)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await finalAddAction(item, cancellationToken).ConfigureAwait(false);
        }

        // Remove items
        foreach (var item in removedItems)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await finalRemoveAction(item, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     Asynchronously updates the output list from the input list with item comparison and type conversion.
    /// </summary>
    public static async Task UpdateFromAsync<TInput, TOutput>(
        this IList<TOutput> output,
        IList<TInput> input,
        Func<TInput, TOutput, bool> areRepresentingTheSameItem,
        Func<TInput, TOutput> fromInputTypeToOutputTypeConversion,
        Func<TOutput, CancellationToken, Task> addAction,
        Func<TOutput, CancellationToken, Task<bool>> removeAction,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(areRepresentingTheSameItem);
        ArgumentNullException.ThrowIfNull(fromInputTypeToOutputTypeConversion);
        ArgumentNullException.ThrowIfNull(addAction);
        ArgumentNullException.ThrowIfNull(removeAction);

        var outputList = new List<TOutput>(output);
        var toBeAdded = new List<TInput>();
        var toBeRemoved = new List<TOutput>();

        // Find items to add (present in input but not in output)
        foreach (var inputItem in input)
        {
            var found = false;
            foreach (var outputItem in outputList)
            {
                if (areRepresentingTheSameItem(inputItem, outputItem))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                toBeAdded.Add(inputItem);
            }
        }

        // Find items to remove (present in output but not in input)
        foreach (var outputItem in outputList)
        {
            var found = false;
            foreach (var inputItem in input)
            {
                if (areRepresentingTheSameItem(inputItem, outputItem))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                toBeRemoved.Add(outputItem);
            }
        }

        // Apply changes
        foreach (var itemToBeAdded in toBeAdded)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var convertedItem = fromInputTypeToOutputTypeConversion(itemToBeAdded);
            await addAction(convertedItem, cancellationToken).ConfigureAwait(false);
        }

        foreach (var itemToBeRemoved in toBeRemoved)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await removeAction(itemToBeRemoved, cancellationToken).ConfigureAwait(false);
        }
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
