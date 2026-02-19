using System.Collections;
using System.Collections.Concurrent;

namespace Plugin.BaseTypeExtensions;

/// <summary>
/// Provides extension methods for working with <see cref="IEnumerable"/> and <see cref="IEnumerable{T}"/>.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>Coalesces the input to null if it is empty.</summary>
    /// <param name="input">The input sequence to coalesce. Can be null.</param>
    /// <returns>The input sequence that was passed in if it is not empty; otherwise, null.</returns>
    public static IEnumerable? NullIfEmpty(this IEnumerable? input)
    {
        return input == null || !input.Cast<object?>().Any()
            ? null
            : input;
    }

    /// <summary>Coalesces the input to null if it is empty.</summary>
    /// <param name="input">The input sequence to coalesce. Can be null.</param>
    /// <returns>The input sequence that was passed in if it is not empty; otherwise, null.</returns>
    public static IEnumerable<T?>? NullIfEmpty<T>(this IEnumerable<T?>? input)
    {
        return input == null || !input.Any()
            ? null
            : input;
    }

    /// <summary>
    ///     Enqueues an item to the queue and ensures the queue does not exceed the specified maximum size.
    /// </summary>
    /// <param name="queue">The queue to enqueue the item to. Cannot be null.</param>
    /// <param name="obj">The item to enqueue. Can be null.</param>
    /// <param name="max">The maximum size of the queue.</param>
    public static void Enqueue<T>(this ConcurrentQueue<T> queue, T obj, int max)
    {
        ArgumentNullException.ThrowIfNull(queue);

        queue.Enqueue(obj);
        while (queue.Count > max)
        {
            queue.TryDequeue(out var _);
        }
    }

    /// <summary>
    ///     Gets the element at the specified index or returns the default value if the index is out of range.
    /// </summary>
    /// <param name="enumerable">The enumerable to get the element from. Can be null.</param>
    /// <param name="index">The index of the element to get. If the index is out of range or if the enumerable is null, the default value is returned.</param>
    /// <param name="defaultValue">The default value to return if the index is out of range. Can be null.</param>
    public static T? GetOrDefault<T>(this IEnumerable<T?>? enumerable, int index, T? defaultValue = default)
    {
        if (enumerable is null || index < 0)
        {
            return defaultValue;
        }

        try
        {
            var enumeratedList = enumerable as IList<T?> ?? enumerable.ToList();
            if (index >= enumeratedList.Count)
            {
                return defaultValue;
            }

            return enumeratedList[index];
        }
        catch (ArgumentOutOfRangeException)
        {
            return defaultValue;
        }
        catch (InvalidCastException)
        {
            return defaultValue;
        }
    }

    /// <summary>
    ///     Updates the output collection by adding and removing specified items.
    ///     This is a Layer 1 (core) method that executes actions on explicit item lists.
    /// </summary>
    /// <typeparam name="TAdd">The type of items to add.</typeparam>
    /// <typeparam name="TRemove">The type of items to remove.</typeparam>
    /// <param name="output">The collection to update.</param>
    /// <param name="addedItems">The items to add to the collection.</param>
    /// <param name="removedItems">The items to remove from the collection.</param>
    /// <param name="addAction">Action to perform when adding an item. This parameter is required.</param>
    /// <param name="removeAction">Action to perform when removing an item. This parameter is required.</param>
    public static void UpdateFrom<TAdd, TRemove>(
        this IEnumerable<TRemove> output,
        IEnumerable<TAdd> addedItems,
        IEnumerable<TRemove> removedItems,
        Action<TAdd> addAction,
        Action<TRemove> removeAction
    )
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(addedItems);
        ArgumentNullException.ThrowIfNull(removedItems);
        ArgumentNullException.ThrowIfNull(addAction);
        ArgumentNullException.ThrowIfNull(removeAction);

        // Add items
        foreach (var item in addedItems)
        {
            addAction(item);
        }

        // Remove items
        foreach (var item in removedItems)
        {
            removeAction(item);
        }
    }

    /// <summary>
    ///     Updates the output collection by adding, updating, and removing specified items.
    ///     This is a Layer 1 (core) method that executes actions on explicit item lists.
    /// </summary>
    /// <typeparam name="TAdd">The type of items to add.</typeparam>
    /// <typeparam name="TUpdate">The type of items to update.</typeparam>
    /// <typeparam name="TRemove">The type of items to remove.</typeparam>
    /// <param name="output">The collection to update.</param>
    /// <param name="addedItems">The items to add to the collection.</param>
    /// <param name="updatedItems">The items to update in the collection.</param>
    /// <param name="removedItems">The items to remove from the collection.</param>
    /// <param name="addAction">Action to perform when adding an item. This parameter is required.</param>
    /// <param name="updateAction">Action to perform when updating an item. This parameter is required.</param>
    /// <param name="removeAction">Action to perform when removing an item. This parameter is required.</param>
    public static void UpdateFrom<TAdd, TUpdate, TRemove>(
        this IEnumerable<TRemove> output,
        IEnumerable<TAdd> addedItems,
        IEnumerable<TUpdate> updatedItems,
        IEnumerable<TRemove> removedItems,
        Action<TAdd> addAction,
        Action<TUpdate> updateAction,
        Action<TRemove> removeAction
    )
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(addedItems);
        ArgumentNullException.ThrowIfNull(updatedItems);
        ArgumentNullException.ThrowIfNull(removedItems);
        ArgumentNullException.ThrowIfNull(addAction);
        ArgumentNullException.ThrowIfNull(updateAction);
        ArgumentNullException.ThrowIfNull(removeAction);

        // Add items
        foreach (var item in addedItems)
        {
            addAction(item);
        }

        // Update items
        foreach (var item in updatedItems)
        {
            updateAction(item);
        }

        // Remove items
        foreach (var item in removedItems)
        {
            removeAction(item);
        }
    }

    /// <summary>
    ///     Updates the output collection from the input collection with item comparison and actions for adding and removing items.
    ///     This is a Layer 2 (diff-calculating) method that calculates differences and delegates to Layer 1.
    /// </summary>
    /// <typeparam name="TInput">The type of the input items.</typeparam>
    /// <typeparam name="TOutput">The type of the output items.</typeparam>
    /// <param name="output">The collection to update.</param>
    /// <param name="input">The collection to use as source for updates.</param>
    /// <param name="areRepresentingTheSameItem">Function to determine if an input item and output item represent the same entity.</param>
    /// <param name="addAction">Action to perform when adding an item.</param>
    /// <param name="removeAction">Action to perform when removing an item.</param>
    public static void UpdateFrom<TInput, TOutput>(
        this IEnumerable<TOutput> output,
        IEnumerable<TInput> input,
        Func<TInput, TOutput, bool> areRepresentingTheSameItem,
        Action<TInput> addAction,
        Action<TOutput> removeAction
    )
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(areRepresentingTheSameItem);
        ArgumentNullException.ThrowIfNull(addAction);
        ArgumentNullException.ThrowIfNull(removeAction);

        var inputList = input as TInput[] ?? input.ToArray();

        lock (output)
        {
            // Take a fresh snapshot of the output inside the lock
            var outputList = new List<TOutput>(output);

            var toBeAdded = new List<TInput>();
            var toBeRemoved = new List<TOutput>();

            // Find items to add (present in input but not in output)
            foreach (var inputItem in inputList)
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
                foreach (var inputItem in inputList)
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

            // Delegate to Layer 1 for execution
            output.UpdateFrom(toBeAdded, toBeRemoved, addAction, removeAction);
        }
    }

    /// <summary>
    ///     Updates the output collection from the input collection with item comparison and actions for adding, updating, and removing items.
    ///     This is a Layer 2 (diff-calculating) method that calculates differences and delegates to Layer 1.
    /// </summary>
    /// <typeparam name="TInput">The type of the input items.</typeparam>
    /// <typeparam name="TOutput">The type of the output items.</typeparam>
    /// <param name="output">The collection to update.</param>
    /// <param name="input">The collection to use as source for updates.</param>
    /// <param name="areRepresentingTheSameItem">Function to determine if an input item and output item represent the same entity.</param>
    /// <param name="areRepresentingTheSameValue">Function to determine if an input item and output item have the same value.</param>
    /// <param name="addAction">Action to perform when adding an item.</param>
    /// <param name="updateAction">Action to perform when updating an item.</param>
    /// <param name="removeAction">Action to perform when removing an item.</param>
    public static void UpdateFrom<TInput, TOutput>(
        this IEnumerable<TOutput> output,
        IEnumerable<TInput> input,
        Func<TInput, TOutput, bool> areRepresentingTheSameItem,
        Func<TInput, TOutput, bool> areRepresentingTheSameValue,
        Action<TInput> addAction,
        Action<TOutput, TInput> updateAction,
        Action<TOutput> removeAction
    )
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(areRepresentingTheSameItem);
        ArgumentNullException.ThrowIfNull(areRepresentingTheSameValue);
        ArgumentNullException.ThrowIfNull(addAction);
        ArgumentNullException.ThrowIfNull(updateAction);
        ArgumentNullException.ThrowIfNull(removeAction);

        var inputList = input as TInput[] ?? input.ToArray();

        lock (output)
        {
            // Take a fresh snapshot of the output inside the lock
            var outputList = new List<TOutput>(output);

            var toBeAdded = new List<TInput>();
            var toBeUpdated = new List<(TOutput outputItem, TInput inputItem)>();
            var toBeRemoved = new List<TOutput>();

            // Find items to add or update (present in input)
            foreach (var inputItem in inputList)
            {
                var found = false;
                TOutput? matchedOutputItem = default;

                foreach (var outputItem in outputList)
                {
                    if (areRepresentingTheSameItem(inputItem, outputItem))
                    {
                        found = true;
                        matchedOutputItem = outputItem;
                        break;
                    }
                }

                if (!found)
                {
                    toBeAdded.Add(inputItem);
                }
                else if (matchedOutputItem != null && !areRepresentingTheSameValue(inputItem, matchedOutputItem))
                {
                    toBeUpdated.Add((matchedOutputItem, inputItem));
                }
            }

            // Find items to remove (present in output but not in input)
            foreach (var outputItem in outputList)
            {
                var found = false;
                foreach (var inputItem in inputList)
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

            // Delegate to Layer 1 for execution
            output.UpdateFrom(
                toBeAdded,
                toBeUpdated,
                toBeRemoved,
                addAction,
                tuple => updateAction(tuple.outputItem, tuple.inputItem),
                removeAction
            );
        }
    }

    /// <summary>
    ///     Updates the output collection from the input collection with item comparison, type conversion, and actions for adding and removing items.
    ///     This is a Layer 2 (diff-calculating) method that calculates differences, converts types, and delegates to Layer 1.
    /// </summary>
    /// <typeparam name="TInput">The type of the input items.</typeparam>
    /// <typeparam name="TOutput">The type of the output items.</typeparam>
    /// <param name="output">The collection to update.</param>
    /// <param name="input">The collection to use as source for updates.</param>
    /// <param name="areRepresentingTheSameItem">Function to determine if an input item and output item represent the same entity.</param>
    /// <param name="fromInputTypeToOutputTypeConversion">Function to convert input items to output items.</param>
    /// <param name="addAction">Action to perform when adding an item.</param>
    /// <param name="removeAction">Action to perform when removing an item.</param>
    public static void UpdateFrom<TInput, TOutput>(
        this IEnumerable<TOutput> output,
        IEnumerable<TInput> input,
        Func<TInput, TOutput, bool> areRepresentingTheSameItem,
        Func<TInput, TOutput> fromInputTypeToOutputTypeConversion,
        Action<TOutput> addAction,
        Action<TOutput> removeAction
    )
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(areRepresentingTheSameItem);
        ArgumentNullException.ThrowIfNull(fromInputTypeToOutputTypeConversion);
        ArgumentNullException.ThrowIfNull(addAction);
        ArgumentNullException.ThrowIfNull(removeAction);

        var inputList = input as TInput[] ?? input.ToArray();

        lock (output)
        {
            // Take a fresh snapshot of the output inside the lock
            var outputList = new List<TOutput>(output);

            var toBeAdded = new List<TInput>();
            var toBeRemoved = new List<TOutput>();

            // Find items to add (present in input but not in output)
            foreach (var inputItem in inputList)
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
                foreach (var inputItem in inputList)
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

            // Convert items and delegate to Layer 1 for execution
            var convertedAdded = toBeAdded.Select(fromInputTypeToOutputTypeConversion).ToList();
            output.UpdateFrom<TOutput, TOutput>(convertedAdded, toBeRemoved, addAction, removeAction);
        }
    }

    /// <summary>
    ///     Picks a random element from the collection.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the source collection is empty.</exception>
    public static T PickRandom<T>(this IEnumerable<T> source)
    {
        var sourceArray = source as T[] ?? source.ToArray();
        if (sourceArray.Length == 0)
        {
            throw new InvalidOperationException("Cannot pick a random element from an empty collection");
        }

        return sourceArray.PickRandom(1).Single();
    }

    /// <summary>
    ///     Picks a specified number of random elements from the collection.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if requested count exceeds the source collection count.</exception>
    public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
    {
        return source.Shuffle().Take(count);
    }

    /// <summary>
    ///     Shuffles the elements of the collection using Fisher-Yates algorithm.
    /// </summary>
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        var sourceArray = source.ToArray();

        // Fisher-Yates shuffle algorithm
        for (var i = sourceArray.Length - 1; i > 0; i--)
        {
#pragma warning disable CA5394 // Do not use insecure randomness
            var j = Random.Shared.Next(i + 1);
#pragma warning restore CA5394 // Do not use insecure randomness
            (sourceArray[j], sourceArray[i]) = (sourceArray[i], sourceArray[j]);
        }

        return sourceArray;
    }

    /// <summary>
    ///     Asynchronously updates the output collection by adding and removing specified items.
    ///     This is a Layer 1 (core) method that executes actions on explicit item lists.
    /// </summary>
    /// <typeparam name="TAdd">The type of items to add.</typeparam>
    /// <typeparam name="TRemove">The type of items to remove.</typeparam>
    /// <param name="output">The collection to update.</param>
    /// <param name="addedItems">The items to add to the collection.</param>
    /// <param name="removedItems">The items to remove from the collection.</param>
    /// <param name="addAction">Async action to perform when adding an item. This parameter is required.</param>
    /// <param name="removeAction">Async action to perform when removing an item. This parameter is required.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    public static async ValueTask UpdateFromAsync<TAdd, TRemove>(
        this IEnumerable<TRemove> output,
        IEnumerable<TAdd> addedItems,
        IEnumerable<TRemove> removedItems,
        Func<TAdd, CancellationToken, ValueTask> addAction,
        Func<TRemove, CancellationToken, ValueTask> removeAction,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(addedItems);
        ArgumentNullException.ThrowIfNull(removedItems);
        ArgumentNullException.ThrowIfNull(addAction);
        ArgumentNullException.ThrowIfNull(removeAction);

        // Add items
        foreach (var item in addedItems)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await addAction(item, cancellationToken).ConfigureAwait(false);
        }

        // Remove items
        foreach (var item in removedItems)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await removeAction(item, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     Asynchronously updates the output collection by adding, updating, and removing specified items.
    ///     This is a Layer 1 (core) method that executes actions on explicit item lists.
    /// </summary>
    /// <typeparam name="TAdd">The type of items to add.</typeparam>
    /// <typeparam name="TUpdate">The type of items to update.</typeparam>
    /// <typeparam name="TRemove">The type of items to remove.</typeparam>
    /// <param name="output">The collection to update.</param>
    /// <param name="addedItems">The items to add to the collection.</param>
    /// <param name="updatedItems">The items to update in the collection.</param>
    /// <param name="removedItems">The items to remove from the collection.</param>
    /// <param name="addAction">Async action to perform when adding an item. This parameter is required.</param>
    /// <param name="updateAction">Async action to perform when updating an item. This parameter is required.</param>
    /// <param name="removeAction">Async action to perform when removing an item. This parameter is required.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    public static async ValueTask UpdateFromAsync<TAdd, TUpdate, TRemove>(
        this IEnumerable<TRemove> output,
        IEnumerable<TAdd> addedItems,
        IEnumerable<TUpdate> updatedItems,
        IEnumerable<TRemove> removedItems,
        Func<TAdd, CancellationToken, ValueTask> addAction,
        Func<TUpdate, CancellationToken, ValueTask> updateAction,
        Func<TRemove, CancellationToken, ValueTask> removeAction,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(addedItems);
        ArgumentNullException.ThrowIfNull(updatedItems);
        ArgumentNullException.ThrowIfNull(removedItems);
        ArgumentNullException.ThrowIfNull(addAction);
        ArgumentNullException.ThrowIfNull(updateAction);
        ArgumentNullException.ThrowIfNull(removeAction);

        // Add items
        foreach (var item in addedItems)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await addAction(item, cancellationToken).ConfigureAwait(false);
        }

        // Update items
        foreach (var item in updatedItems)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await updateAction(item, cancellationToken).ConfigureAwait(false);
        }

        // Remove items
        foreach (var item in removedItems)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await removeAction(item, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     Asynchronously updates the output collection from the input collection with item comparison and actions for adding and removing items.
    ///     This is a Layer 2 (diff-calculating) method that calculates differences and delegates to Layer 1.
    /// </summary>
    /// <typeparam name="TInput">The type of the input items.</typeparam>
    /// <typeparam name="TOutput">The type of the output items.</typeparam>
    /// <param name="output">The collection to update.</param>
    /// <param name="input">The collection to use as source for updates.</param>
    /// <param name="areRepresentingTheSameItem">Function to determine if an input item and output item represent the same entity.</param>
    /// <param name="addAction">Async action to perform when adding an item.</param>
    /// <param name="removeAction">Async action to perform when removing an item.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    public static async ValueTask UpdateFromAsync<TInput, TOutput>(
        this IEnumerable<TOutput> output,
        IEnumerable<TInput> input,
        Func<TInput, TOutput, bool> areRepresentingTheSameItem,
        Func<TInput, CancellationToken, ValueTask> addAction,
        Func<TOutput, CancellationToken, ValueTask> removeAction,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(areRepresentingTheSameItem);
        ArgumentNullException.ThrowIfNull(addAction);
        ArgumentNullException.ThrowIfNull(removeAction);

        var inputList = input as TInput[] ?? input.ToArray();

        // Take a snapshot of the output
        var outputList = new List<TOutput>(output);

        var toBeAdded = new List<TInput>();
        var toBeRemoved = new List<TOutput>();

        // Find items to add (present in input but not in output)
        foreach (var inputItem in inputList)
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
            foreach (var inputItem in inputList)
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

        // Delegate to Layer 1 for execution
        await output.UpdateFromAsync(toBeAdded, toBeRemoved, addAction, removeAction, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Asynchronously updates the output collection from the input collection with item comparison and actions for adding, updating, and removing items.
    ///     This is a Layer 2 (diff-calculating) method that calculates differences and delegates to Layer 1.
    /// </summary>
    /// <typeparam name="TInput">The type of the input items.</typeparam>
    /// <typeparam name="TOutput">The type of the output items.</typeparam>
    /// <param name="output">The collection to update.</param>
    /// <param name="input">The collection to use as source for updates.</param>
    /// <param name="areRepresentingTheSameItem">Function to determine if an input item and output item represent the same entity.</param>
    /// <param name="areRepresentingTheSameValue">Function to determine if an input item and output item have the same value.</param>
    /// <param name="addAction">Async action to perform when adding an item.</param>
    /// <param name="updateAction">Async action to perform when updating an item.</param>
    /// <param name="removeAction">Async action to perform when removing an item.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    public static async ValueTask UpdateFromAsync<TInput, TOutput>(
        this IEnumerable<TOutput> output,
        IEnumerable<TInput> input,
        Func<TInput, TOutput, bool> areRepresentingTheSameItem,
        Func<TInput, TOutput, bool> areRepresentingTheSameValue,
        Func<TInput, CancellationToken, ValueTask> addAction,
        Func<TOutput, TInput, CancellationToken, ValueTask> updateAction,
        Func<TOutput, CancellationToken, ValueTask> removeAction,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(areRepresentingTheSameItem);
        ArgumentNullException.ThrowIfNull(areRepresentingTheSameValue);
        ArgumentNullException.ThrowIfNull(addAction);
        ArgumentNullException.ThrowIfNull(updateAction);
        ArgumentNullException.ThrowIfNull(removeAction);

        var inputList = input as TInput[] ?? input.ToArray();

        // Take a snapshot of the output
        var outputList = new List<TOutput>(output);

        var toBeAdded = new List<TInput>();
        var toBeUpdated = new List<(TOutput outputItem, TInput inputItem)>();
        var toBeRemoved = new List<TOutput>();

        // Find items to add or update (present in input)
        foreach (var inputItem in inputList)
        {
            var found = false;
            TOutput? matchedOutputItem = default;

            foreach (var outputItem in outputList)
            {
                if (areRepresentingTheSameItem(inputItem, outputItem))
                {
                    found = true;
                    matchedOutputItem = outputItem;
                    break;
                }
            }

            if (!found)
            {
                toBeAdded.Add(inputItem);
            }
            else if (matchedOutputItem != null && !areRepresentingTheSameValue(inputItem, matchedOutputItem))
            {
                toBeUpdated.Add((matchedOutputItem, inputItem));
            }
        }

        // Find items to remove (present in output but not in input)
        foreach (var outputItem in outputList)
        {
            var found = false;
            foreach (var inputItem in inputList)
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

        // Delegate to Layer 1 for execution
        await output.UpdateFromAsync(
            toBeAdded,
            toBeUpdated,
            toBeRemoved,
            addAction,
            (tuple, ct) => updateAction(tuple.outputItem, tuple.inputItem, ct),
            removeAction,
            cancellationToken
        ).ConfigureAwait(false);
    }
}
