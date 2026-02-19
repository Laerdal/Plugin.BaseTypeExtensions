using System.Collections;
using System.Collections.Concurrent;

namespace Plugin.BaseTypeExtensions.Tests;

public class EnumerableExtensionsTests
{
    [Fact]
    public void NullIfEmpty_WorksForNonGeneric()
    {
        IEnumerable? value = new[] { 1 };
        value.NullIfEmpty().Should().BeSameAs(value);
        ((IEnumerable?)Array.Empty<int>()).NullIfEmpty().Should().BeNull();
        IEnumerable? nullValue = null;
        nullValue.NullIfEmpty().Should().BeNull();
    }

    [Fact]
    public void NullIfEmpty_WorksForGeneric()
    {
        IEnumerable<int>? value = [1];
        value.NullIfEmpty().Should().BeSameAs(value);
        ((IEnumerable<int>?)Array.Empty<int>()).NullIfEmpty().Should().BeNull();
        IEnumerable<int>? nullValue = null;
        nullValue.NullIfEmpty().Should().BeNull();
    }

    [Fact]
    public void Enqueue_LimitsQueueSize()
    {
        var queue = new ConcurrentQueue<int>(Enumerable.Range(0, 3));
        queue.Enqueue(3, max: 3);

        queue.Should().HaveCount(3);
        queue.Should().ContainInOrder(1, 2, 3);
    }

    [Fact]
    public void GetOrDefault_ReturnsDefaultsForInvalidIndex()
    {
        var numbers = new[] { 1, 2, 3 };
        numbers.GetOrDefault(-1).Should().Be(default);
        numbers.GetOrDefault(10, defaultValue: 42).Should().Be(42);
    }

    [Fact]
    public void GetOrDefault_ReturnsItemForValidIndex()
    {
        var numbers = new[] { 1, 2, 3 };
        numbers.GetOrDefault(1).Should().Be(2);
    }

    [Fact]
    public void UpdateFrom_AddsAndRemovesItems()
    {
        var output = new List<int> { 1, 3 };
        var input = new List<int> { 1, 2 };
        var added = new List<int>();
        var removed = new List<int>();

        output.UpdateFrom(
            input,
            areRepresentingTheSameItem: (i, o) => i == o,
            addAction: added.Add,
            removeAction: removed.Add);

        added.Should().ContainSingle().Which.Should().Be(2);
        removed.Should().ContainSingle().Which.Should().Be(3);
    }

    [Fact]
    public void UpdateFrom_WithAddedAndRemovedItems_AppliesChanges()
    {
        var output = new List<int> { 1, 2, 3 };
        var added = new List<int>();
        var removed = new List<int>();

        output.UpdateFrom(
            addedItems: [4, 5],
            removedItems: [2],
            addAction: added.Add,
            removeAction: removed.Add);

        added.Should().Equal(4, 5);
        removed.Should().ContainSingle().Which.Should().Be(2);
    }

    [Fact]
    public void UpdateFrom_WithNullAddedItems_OnlyRemoves()
    {
        var output = new List<int> { 1, 2, 3 };
        var removed = new List<int>();

        output.UpdateFrom(
            addedItems: [],
            removedItems: [2],
            addAction: _ => throw new InvalidOperationException("Should not add"),
            removeAction: removed.Add);

        removed.Should().ContainSingle().Which.Should().Be(2);
    }

    [Fact]
    public void UpdateFrom_WithNullRemovedItems_OnlyAdds()
    {
        var output = new List<int> { 1, 2 };
        var added = new List<int>();

        output.UpdateFrom(
            addedItems: [3, 4],
            removedItems: [],
            addAction: added.Add,
            removeAction: _ => throw new InvalidOperationException("Should not remove"));

        added.Should().Equal(3, 4);
    }

    [Fact]
    public void UpdateFrom_WithUpdateAction_AddsUpdatesAndRemoves()
    {
        var output = new List<string> { "a:1", "b:2", "c:3" };
        var input = new[] { "a:10", "b:2", "d:4" };
        var added = new List<string>();
        var updated = new List<(string output, string input)>();
        var removed = new List<string>();

        output.UpdateFrom(
            input,
            areRepresentingTheSameItem: (i, o) => i.Split(':')[0] == o.Split(':')[0],
            areRepresentingTheSameValue: (i, o) => i == o,
            addAction: added.Add,
            updateAction: (o, i) => updated.Add((o, i)),
            removeAction: removed.Add);

        added.Should().ContainSingle().Which.Should().Be("d:4");
        updated.Should().ContainSingle().Which.Should().Be(("a:1", "a:10"));
        removed.Should().ContainSingle().Which.Should().Be("c:3");
    }

    [Fact]
    public void UpdateFrom_WithUpdateAction_DoesNotUpdateWhenValuesMatch()
    {
        var output = new List<int> { 1, 2, 3 };
        var input = new[] { 1, 2, 3 };
        var updated = new List<(int output, int input)>();

        output.UpdateFrom(
            input,
            areRepresentingTheSameItem: (i, o) => i == o,
            areRepresentingTheSameValue: (i, o) => i == o,
            addAction: _ => { },
            updateAction: (o, i) => updated.Add((o, i)),
            removeAction: _ => { });

        updated.Should().BeEmpty();
    }

    [Fact]
    public void UpdateFrom_WithTypeConversion_ConvertsAndSyncs()
    {
        var output = new List<string> { "1", "3" };
        var input = new[] { 1, 2 };
        var added = new List<string>();
        var removed = new List<string>();

        output.UpdateFrom(
            input,
            areRepresentingTheSameItem: (i, o) => i.ToString() == o,
            fromInputTypeToOutputTypeConversion: i => i.ToString(),
            addAction: added.Add,
            removeAction: removed.Add);

        added.Should().ContainSingle().Which.Should().Be("2");
        removed.Should().ContainSingle().Which.Should().Be("3");
    }

    [Fact]
    public async Task UpdateFromAsync_WithAddedAndRemovedItems_AppliesChanges()
    {
        var output = new List<int> { 1, 2, 3 };
        var added = new List<int>();
        var removed = new List<int>();

        await output.UpdateFromAsync(
            addedItems: [4, 5],
            removedItems: [2],
            addAction: async (item, ct) => { await Task.Yield(); added.Add(item); },
            removeAction: async (item, ct) => { await Task.Yield(); removed.Add(item); });

        added.Should().Equal(4, 5);
        removed.Should().ContainSingle().Which.Should().Be(2);
    }

    [Fact]
    public async Task UpdateFromAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        var output = new List<int> { 1, 2, 3 };
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var action = async () => await output.UpdateFromAsync(
            addedItems: [4, 5],
            removedItems: [],
            addAction: async (item, ct) => { await Task.Delay(100, ct); },
            removeAction: async (item, ct) => { await Task.Delay(100, ct); },
            cancellationToken: cts.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task UpdateFromAsync_WithComparison_AddsAndRemoves()
    {
        var output = new List<int> { 1, 3 };
        var input = new[] { 1, 2 };
        var added = new List<int>();
        var removed = new List<int>();

        await output.UpdateFromAsync(
            input,
            areRepresentingTheSameItem: (i, o) => i == o,
            addAction: async (item, ct) => { await Task.Yield(); added.Add(item); },
            removeAction: async (item, ct) => { await Task.Yield(); removed.Add(item); });

        added.Should().ContainSingle().Which.Should().Be(2);
        removed.Should().ContainSingle().Which.Should().Be(3);
    }

    [Fact]
    public async Task UpdateFromAsync_WithUpdateAction_AddsUpdatesAndRemoves()
    {
        var output = new List<string> { "a:1", "b:2", "c:3" };
        var input = new[] { "a:10", "b:2", "d:4" };
        var added = new List<string>();
        var updated = new List<(string output, string input)>();
        var removed = new List<string>();

        await output.UpdateFromAsync(
            input,
            areRepresentingTheSameItem: (i, o) => i.Split(':')[0] == o.Split(':')[0],
            areRepresentingTheSameValue: (i, o) => i == o,
            addAction: async (item, ct) => { await Task.Yield(); added.Add(item); },
            updateAction: async (o, i, ct) => { await Task.Yield(); updated.Add((o, i)); },
            removeAction: async (item, ct) => { await Task.Yield(); removed.Add(item); });

        added.Should().ContainSingle().Which.Should().Be("d:4");
        updated.Should().ContainSingle().Which.Should().Be(("a:1", "a:10"));
        removed.Should().ContainSingle().Which.Should().Be("c:3");
    }

    [Fact]
    public async Task UpdateFromAsync_WithEmptyCollections_CompletesSuccessfully()
    {
        var output = new List<int>();
        var input = Array.Empty<int>();

        await output.UpdateFromAsync(
            input,
            areRepresentingTheSameItem: (i, o) => i == o,
            addAction: async (item, ct) => { await Task.Yield(); },
            removeAction: async (item, ct) => { await Task.Yield(); });

        // Should complete without errors
        output.Should().BeEmpty();
    }

    [Fact]
    public void PickRandom_ReturnsElement()
    {
        var single = new[] { 42 };
        single.PickRandom().Should().Be(42);

        Action empty = () => Array.Empty<int>().PickRandom();
        empty.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PickRandom_WithCount_ReturnsExpectedNumber()
    {
        var numbers = Enumerable.Range(0, 5).ToArray();
        var result = numbers.PickRandom(3).ToArray();

        result.Should().HaveCount(3);
        result.Should().OnlyContain(x => numbers.Contains(x));
    }

    [Fact]
    public void Shuffle_PreservesElements()
    {
        var numbers = Enumerable.Range(0, 5).ToArray();
        var shuffled = numbers.Shuffle().ToArray();

        shuffled.Should().BeEquivalentTo(numbers);
    }
}
