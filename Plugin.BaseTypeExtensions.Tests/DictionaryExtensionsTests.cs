namespace Plugin.BaseTypeExtensions.Tests;

public class DictionaryExtensionsTests
{
    [Fact]
    public void Add_Remove_Update_ExtendDictionary()
    {
        var dictionary = new Dictionary<string, int>();
        dictionary.Add(new KeyValuePair<string, int>("a", 1));
        dictionary.Update(new KeyValuePair<string, int>("a", 2));
        dictionary.Remove(new KeyValuePair<string, int>("a", 0));

        dictionary.Should().BeEmpty();
    }

    [Fact]
    public void GetValuesForKeys_ReturnsProjectedValues()
    {
        var dictionary = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        dictionary.GetValuesForKeys(["a", "b"]).Should().Equal(1, 2);
    }

    [Fact]
    public void UpdateFrom_WithDictionary_DefaultsToDictionaryActions()
    {
        var output = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var input = new Dictionary<string, int> { ["a"] = 1, ["c"] = 3 };

        output.UpdateFrom(input: input);

        output.Should().ContainKeys("a", "c");
        output.Should().NotContainKey("b");
        output["c"].Should().Be(3);
    }

    [Fact]
    public void UpdateFrom_WithDictionary_UpdatesExistingValues()
    {
        var output = new Dictionary<string, int> { ["a"] = 1 };
        var input = new Dictionary<string, int> { ["a"] = 2 };

        output.UpdateFrom(input: input);

        output["a"].Should().Be(2);
    }

    [Fact]
    public void UpdateFrom_WithValueConversion_UpdatesDifferentTypes()
    {
        var output = new Dictionary<string, string> { ["a"] = "1" };
        var input = new Dictionary<string, int> { ["a"] = 2, ["b"] = 3 };

        output.UpdateFrom(
            input,
            areRepresentingTheSameValue: (src, dest) => src.ToString() == dest,
            fromValueInputTypeToValueOutputTypeConversion: value => value.ToString());

        output.Should().Contain(new KeyValuePair<string, string>("a", "2"));
        output.Should().Contain(new KeyValuePair<string, string>("b", "3"));
    }

    [Fact]
    public void UpdateFrom_WithKeyConversion_UpdatesDifferentKeys()
    {
        var output = new Dictionary<int, string> { [1] = "one" };
        var input = new Dictionary<string, string> { ["1"] = "one", ["2"] = "two" };

        output.UpdateFrom(
            input,
            areRepresentingTheSameKey: (src, dest) => src == dest.ToString(),
            fromKeyInputTypeToKeyOutputTypeConversion: key => int.Parse(key));

        output.Should().Contain(new KeyValuePair<int, string>(1, "one"));
        output.Should().Contain(new KeyValuePair<int, string>(2, "two"));
    }

    [Fact]
    public void UpdateFrom_WithFullCustomization_HandlesAddUpdateRemove()
    {
        var output = new List<KeyValuePair<string, string>>
        {
            new("1", "1"),
            new("b", "2"),
        };
        var input = new List<KeyValuePair<int, int>>
        {
            new(1, 2),
            new(3, 4),
        };

        var added = new List<(string Key, string Value)>();
        var updated = new List<(string Key, string Value)>();
        var removed = new List<string>();

        output.UpdateFrom(
            input,
            areRepresentingTheSameKey: (src, dest) => src.ToString() == dest,
            fromKeyInputTypeToKeyOutputTypeConversion: key => key.ToString(),
            areRepresentingTheSameValue: (src, dest) => src.ToString() == dest,
            fromValueInputTypeToValueOutputTypeConversion: value => value.ToString(),
            addAction: (k, v) => added.Add((k, v)),
            updateAction: (k, v) => updated.Add((k, v)),
            removeAction: key =>
            {
                removed.Add(key);
                return true;
            });

        added.Should().ContainSingle().Which.Should().Be(("3", "4"));
        updated.Should().ContainSingle().Which.Should().Be(("1", "2"));
        removed.Should().ContainSingle().Which.Should().Be("b");
    }

    [Fact]
    public void UpdateFrom_WithAddedAndRemovedItems_AppliesChanges()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };

        dict.UpdateFrom(
            addedItems: [new KeyValuePair<string, int>("c", 3)],
            removedItems: [new KeyValuePair<string, int>("a", 1)]);

        dict.Should().ContainKeys("b", "c");
        dict.Should().NotContainKey("a");
        dict["c"].Should().Be(3);
    }

    [Fact]
    public void UpdateFrom_WithAddedAndRemovedItems_UsingDefaultActions()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1 };

        dict.UpdateFrom(
            addedItems: [new KeyValuePair<string, int>("b", 2)],
            removedItems: [new KeyValuePair<string, int>("a", 0)]);

        dict.Should().ContainKey("b");
        dict.Should().NotContainKey("a");
    }

    [Fact]
    public void UpdateFrom_WithNullAddedItems_OnlyRemoves()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };

        dict.UpdateFrom(
            addedItems: [],
            removedItems: [new KeyValuePair<string, int>("a", 1)]);

        dict.Should().ContainKey("b");
        dict.Should().NotContainKey("a");
    }

    [Fact]
    public void UpdateFrom_WithNullRemovedItems_OnlyAdds()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1 };

        dict.UpdateFrom(
            addedItems: [new KeyValuePair<string, int>("b", 2)],
            removedItems: []);

        dict.Should().ContainKeys("a", "b");
    }

    [Fact]
    public void UpdateFrom_WithNonDictionary_RequiresExplicitActions()
    {
        var kvps = new List<KeyValuePair<string, int>>
        {
            new("a", 1)
        };

        var action = () => kvps.UpdateFrom(
            addedItems: [new KeyValuePair<string, int>("b", 2)],
            removedItems: [],
            addAction: null,
            removeAction: null);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateFromAsync_WithAddedAndRemovedItems_AppliesChanges()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var added = new List<(string, int)>();
        var removed = new List<string>();

        await dict.UpdateFromAsync(
            addedItems: [new KeyValuePair<string, int>("c", 3)],
            removedItems: [new KeyValuePair<string, int>("a", 1)],
            addAction: async (k, v, ct) => { await Task.Yield(); added.Add((k, v)); dict.Add(k, v); },
            removeAction: async (k, ct) => { await Task.Yield(); removed.Add(k); return dict.Remove(k); });

        added.Should().ContainSingle().Which.Should().Be(("c", 3));
        removed.Should().ContainSingle().Which.Should().Be("a");
        dict.Should().ContainKeys("b", "c");
    }

    [Fact]
    public async Task UpdateFromAsync_WithDictionary_UsesDefaultActions()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1 };

        await dict.UpdateFromAsync(
            addedItems: [new KeyValuePair<string, int>("b", 2)],
            removedItems: [new KeyValuePair<string, int>("a", 1)]);

        dict.Should().ContainKey("b");
        dict.Should().NotContainKey("a");
    }

    [Fact]
    public async Task UpdateFromAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1 };
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var action = async () => await dict.UpdateFromAsync(
            addedItems: [new KeyValuePair<string, int>("b", 2)],
            removedItems: [],
            cancellationToken: cts.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task UpdateFromAsync_WithFullCustomization_HandlesAddUpdateRemove()
    {
        var output = new Dictionary<string, string> { ["1"] = "1", ["b"] = "2" };
        var input = new Dictionary<int, int> { [1] = 2, [3] = 4 };
        var added = new List<(string Key, string Value)>();
        var updated = new List<(string Key, string Value)>();
        var removed = new List<string>();

        await output.UpdateFromAsync(
            input,
            areRepresentingTheSameKey: (src, dest) => src.ToString() == dest,
            fromKeyInputTypeToKeyOutputTypeConversion: key => key.ToString(),
            areRepresentingTheSameValue: (src, dest) => src.ToString() == dest,
            fromValueInputTypeToValueOutputTypeConversion: value => value.ToString(),
            addAction: async (k, v, ct) => { await Task.Yield(); added.Add((k, v)); },
            updateAction: async (k, v, ct) => { await Task.Yield(); updated.Add((k, v)); },
            removeAction: async (key, ct) => { await Task.Yield(); removed.Add(key); return true; });

        added.Should().ContainSingle().Which.Should().Be(("3", "4"));
        updated.Should().ContainSingle().Which.Should().Be(("1", "2"));
        removed.Should().ContainSingle().Which.Should().Be("b");
    }

    [Fact]
    public async Task UpdateFromAsync_WithEmptyCollections_CompletesSuccessfully()
    {
        var output = new Dictionary<string, int>();
        var input = new Dictionary<string, int>();

        await output.UpdateFromAsync(
            input,
            areRepresentingTheSameKey: (src, dest) => src == dest,
            fromKeyInputTypeToKeyOutputTypeConversion: k => k,
            areRepresentingTheSameValue: (src, dest) => src == dest,
            fromValueInputTypeToValueOutputTypeConversion: v => v);

        output.Should().BeEmpty();
    }
}
