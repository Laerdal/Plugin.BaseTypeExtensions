namespace Plugin.BaseTypeExtensions.Tests;

public class ListExtensionsTests
{
    [Fact]
    public void UpdateFrom_WithCustomConversion_SyncsCollections()
    {
        var output = new List<string> { "1", "3" };
        var input = new List<int> { 1, 2 };

        output.UpdateFrom(input, (i, o) => i.ToString() == o, i => i.ToString());

        output.Should().Equal("1", "2");
    }

    [Fact]
    public void UpdateFrom_WithEquatableConversion_SyncsCollections()
    {
        var output = new List<int> { 1, 3 };
        var input = new List<int> { 1, 2 };

        output.UpdateFrom(input, i => i);

        output.Should().Equal(1, 2);
    }

    [Fact]
    public void UpdateFrom_WithComparer_SyncsCollections()
    {
        var output = new List<int> { 1, 3 };
        var input = new List<int> { 1, 2 };

        output.UpdateFrom(input, (i, o) => i == o);

        output.Should().Equal(1, 2);
    }

    [Fact]
    public void UpdateFrom_DefaultOverload_SyncsCollections()
    {
        var output = new List<int> { 1, 3 };
        var input = new List<int> { 1, 2 };

        output.UpdateFrom(input);

        output.Should().Equal(1, 2);
    }

    [Fact]
    public void Pairs_ReturnsConsecutivePairs()
    {
        var items = new List<int> { 1, 2, 3 };
        var pairs = items.Pairs().ToArray();

        pairs.Should().Equal((1, 2), (2, 3));
    }

    [Fact]
    public void Pairs_ReturnsEmptyWhenInsufficientItems()
    {
        var items = new List<int> { 1 };
        items.Pairs().Should().BeEmpty();
    }

    [Fact]
    public void UpdateFrom_WithAddedAndRemovedItems_AppliesChanges()
    {
        var list = new List<int> { 1, 2, 3 };

        list.UpdateFrom(
            addedItems: [4, 5],
            removedItems: [2]);

        list.Should().Equal(1, 3, 4, 5);
    }

    [Fact]
    public void UpdateFrom_WithNullAddedItems_OnlyRemoves()
    {
        var list = new List<int> { 1, 2, 3 };

        list.UpdateFrom(
            addedItems: [],
            removedItems: [2]);

        list.Should().Equal(1, 3);
    }

    [Fact]
    public void UpdateFrom_WithNullRemovedItems_OnlyAdds()
    {
        var list = new List<int> { 1, 2 };

        list.UpdateFrom(
            addedItems: [3, 4],
            removedItems: []);

        list.Should().Equal(1, 2, 3, 4);
    }

    [Fact]
    public void UpdateFrom_WithEmptyCollections_NoChanges()
    {
        var list = new List<int> { 1, 2 };

        list.UpdateFrom(
            addedItems: Array.Empty<int>(),
            removedItems: Array.Empty<int>());

        list.Should().Equal(1, 2);
    }

    [Fact]
    public async Task UpdateFromAsync_WithAddedAndRemovedItems_AppliesChanges()
    {
        var list = new List<int> { 1, 2, 3 };
        var added = new List<int>();
        var removed = new List<int>();

        await list.UpdateFromAsync(
            addedItems: [4, 5],
            removedItems: [2],
            addAction: async (item, ct) => { await Task.Yield(); added.Add(item); list.Add(item); },
            removeAction: async (item, ct) => { await Task.Yield(); removed.Add(item); return list.Remove(item); });

        added.Should().Equal(4, 5);
        removed.Should().ContainSingle().Which.Should().Be(2);
        list.Should().Equal(1, 3, 4, 5);
    }

    [Fact]
    public async Task UpdateFromAsync_WithDefaultActions_UsesListMethods()
    {
        var list = new List<int> { 1, 2 };

        await list.UpdateFromAsync(
            addedItems: [3],
            removedItems: [1]);

        list.Should().Equal(2, 3);
    }

    [Fact]
    public async Task UpdateFromAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        var list = new List<int> { 1, 2 };
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var action = async () => await list.UpdateFromAsync(
            addedItems: [3, 4],
            removedItems: [],
            cancellationToken: cts.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task UpdateFromAsync_WithTypeConversion_ConvertsAndSyncs()
    {
        var output = new List<string> { "1", "3" };
        var input = new List<int> { 1, 2 };
        var added = new List<string>();
        var removed = new List<string>();

        await output.UpdateFromAsync(
            input,
            areRepresentingTheSameItem: (i, o) => i.ToString() == o,
            fromInputTypeToOutputTypeConversion: i => i.ToString(),
            addAction: async (item, ct) => { await Task.Yield(); added.Add(item); },
            removeAction: async (item, ct) => { await Task.Yield(); removed.Add(item); return true; });

        added.Should().ContainSingle().Which.Should().Be("2");
        removed.Should().ContainSingle().Which.Should().Be("3");
    }

    [Fact]
    public async Task UpdateFromAsync_WithEmptyCollections_CompletesSuccessfully()
    {
        var output = new List<int>();
        var input = new List<int>();

        await output.UpdateFromAsync(
            input,
            areRepresentingTheSameItem: (i, o) => i == o,
            fromInputTypeToOutputTypeConversion: i => i,
            addAction: async (item, ct) => { await Task.Yield(); },
            removeAction: async (item, ct) => { await Task.Yield(); return true; });

        output.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateFromAsync_WithMultipleOperations_MaintainsOrder()
    {
        var output = new List<int> { 1, 5, 10 };
        var input = new List<int> { 1, 3, 5, 7 };
        var processedItems = new List<string>();

        await output.UpdateFromAsync(
            input,
            areRepresentingTheSameItem: (i, o) => i == o,
            fromInputTypeToOutputTypeConversion: i => i,
            addAction: async (item, ct) =>
            {
                await Task.Yield();
                processedItems.Add($"Add:{item}");
                output.Add(item);
            },
            removeAction: async (item, ct) =>
            {
                await Task.Yield();
                processedItems.Add($"Remove:{item}");
                return output.Remove(item);
            });

        processedItems.Should().Contain("Add:3");
        processedItems.Should().Contain("Add:7");
        processedItems.Should().Contain("Remove:10");
    }
}
