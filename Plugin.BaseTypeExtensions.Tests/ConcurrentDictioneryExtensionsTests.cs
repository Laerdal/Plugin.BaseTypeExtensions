using System.Collections.Concurrent;
using FluentAssertions;

namespace Plugin.BaseTypeExtensions.Tests;

public class ConcurrentDictionaryExtensionsTests
{
    #region TryAddAsync Tests

    [Fact]
    public async Task TryAddAsync_WithNewKey_ReturnsTrue()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        const string key = "test";
        const int value = 42;

        // Act
        var result = await dictionary.TryAddAsync(key, value);

        // Assert
        result.Should().BeTrue();
        dictionary.Should().ContainKey(key);
        dictionary[key].Should().Be(value);
    }

    [Fact]
    public async Task TryAddAsync_WithExistingKey_ReturnsFalse()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        const string key = "test";
        dictionary.TryAdd(key, 1);

        // Act
        var result = await dictionary.TryAddAsync(key, 42);

        // Assert
        result.Should().BeFalse();
        dictionary[key].Should().Be(1); // Original value unchanged
    }

    [Fact]
    public async Task TryAddAsync_WithRetries_ReturnsTrue()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        const string key = "test";
        const int value = 42;

        // Act
        var result = await dictionary.TryAddAsync(key, value, retryCount: 5);

        // Assert
        result.Should().BeTrue();
        dictionary[key].Should().Be(value);
    }

    [Fact]
    public async Task TryAddAsync_WithExistingKeyAndRetries_ReturnsFalse()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        const string key = "test";
        dictionary.TryAdd(key, 1);

        // Act
        var result = await dictionary.TryAddAsync(key, 42, retryCount: 3);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task TryAddAsync_WithDelay_WaitsBeforeRetrying()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        const string key = "test";
        dictionary.TryAdd(key, 1);
        var delay = TimeSpan.FromMilliseconds(100);
        var startTime = DateTime.UtcNow;

        // Act
        var result = await dictionary.TryAddAsync(key, 42, retryCount: 3, delayBetweenRetries: delay);

        // Assert
        var elapsed = DateTime.UtcNow - startTime;
        result.Should().BeFalse();
        elapsed.Should().BeGreaterOrEqualTo(TimeSpan.FromMilliseconds(200)); // 2 delays (3 attempts = 2 delays)
    }

    [Fact]
    public async Task TryAddAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await dictionary.TryAddAsync("test", 42, cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task TryAddAsync_WithCancellationDuringRetries_ThrowsOperationCanceledException()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        dictionary.TryAdd("test", 1); // Ensure first attempt fails
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        // Act
        Func<Task> act = async () => await dictionary.TryAddAsync("test", 42, retryCount: 10, delayBetweenRetries: TimeSpan.FromMilliseconds(100), cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task TryAddAsync_WithNullDictionary_ThrowsArgumentNullException()
    {
        // Arrange
        ConcurrentDictionary<string, int>? dictionary = null;

        // Act
        Func<Task> act = async () => await dictionary!.TryAddAsync("test", 42);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("dictionary");
    }

    [Fact]
    public async Task TryAddAsync_WithNullKey_ThrowsArgumentNullException()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();

        // Act
        Func<Task> act = async () => await dictionary.TryAddAsync(null!, 42);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("key");
    }

    [Fact]
    public async Task TryAddAsync_WithInvalidRetryCount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();

        // Act
        Func<Task> act = async () => await dictionary.TryAddAsync("test", 42, retryCount: 0);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("retryCount");
    }

    [Fact]
    public async Task TryAddAsync_WithNegativeRetryCount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();

        // Act
        Func<Task> act = async () => await dictionary.TryAddAsync("test", 42, retryCount: -1);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("retryCount");
    }

    #endregion

    #region TryRemoveAsync Tests

    [Fact]
    public async Task TryRemoveAsync_WithExistingKey_ReturnsTrueAndValue()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        const string key = "test";
        const int value = 42;
        dictionary.TryAdd(key, value);

        // Act
        var result = await dictionary.TryRemoveAsync(key);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().Be(value);
        dictionary.Should().NotContainKey(key);
    }

    [Fact]
    public async Task TryRemoveAsync_WithNonExistingKey_ReturnsFalseAndDefault()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();

        // Act
        var result = await dictionary.TryRemoveAsync("nonexistent");

        // Assert
        result.Success.Should().BeFalse();
        result.Value.Should().Be(default(int));
    }

    [Fact]
    public async Task TryRemoveAsync_WithRetries_ReturnsTrue()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        const string key = "test";
        const int value = 42;
        dictionary.TryAdd(key, value);

        // Act
        var result = await dictionary.TryRemoveAsync(key, retryCount: 5);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public async Task TryRemoveAsync_WithNonExistingKeyAndRetries_ReturnsFalse()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();

        // Act
        var result = await dictionary.TryRemoveAsync("nonexistent", retryCount: 3);

        // Assert
        result.Success.Should().BeFalse();
        result.Value.Should().Be(default(int));
    }

    [Fact]
    public async Task TryRemoveAsync_WithDelay_WaitsBeforeRetrying()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        var delay = TimeSpan.FromMilliseconds(100);
        var startTime = DateTime.UtcNow;

        // Act
        var result = await dictionary.TryRemoveAsync("nonexistent", retryCount: 3, delayBetweenRetries: delay);

        // Assert
        var elapsed = DateTime.UtcNow - startTime;
        result.Success.Should().BeFalse();
        elapsed.Should().BeGreaterOrEqualTo(TimeSpan.FromMilliseconds(200));
    }

    [Fact]
    public async Task TryRemoveAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await dictionary.TryRemoveAsync("test", cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task TryRemoveAsync_WithNullDictionary_ThrowsArgumentNullException()
    {
        // Arrange
        ConcurrentDictionary<string, int>? dictionary = null;

        // Act
        Func<Task> act = async () => await dictionary!.TryRemoveAsync("test");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("dictionary");
    }

    [Fact]
    public async Task TryRemoveAsync_WithNullKey_ThrowsArgumentNullException()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();

        // Act
        Func<Task> act = async () => await dictionary.TryRemoveAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("key");
    }

    [Fact]
    public async Task TryRemoveAsync_WithInvalidRetryCount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();

        // Act
        Func<Task> act = async () => await dictionary.TryRemoveAsync("test", retryCount: 0);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("retryCount");
    }

    #endregion

    #region TryUpdateAsync Tests

    [Fact]
    public async Task TryUpdateAsync_WithMatchingComparisonValue_ReturnsTrue()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        const string key = "test";
        dictionary.TryAdd(key, 1);

        // Act
        var result = await dictionary.TryUpdateAsync(key, newValue: 42, comparisonValue: 1);

        // Assert
        result.Should().BeTrue();
        dictionary[key].Should().Be(42);
    }

    [Fact]
    public async Task TryUpdateAsync_WithNonMatchingComparisonValue_ReturnsFalse()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        const string key = "test";
        dictionary.TryAdd(key, 1);

        // Act
        var result = await dictionary.TryUpdateAsync(key, newValue: 42, comparisonValue: 99);

        // Assert
        result.Should().BeFalse();
        dictionary[key].Should().Be(1); // Original value unchanged
    }

    [Fact]
    public async Task TryUpdateAsync_WithNonExistingKey_ReturnsFalse()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();

        // Act
        var result = await dictionary.TryUpdateAsync("nonexistent", newValue: 42, comparisonValue: 1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task TryUpdateAsync_WithRetries_ReturnsTrue()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        const string key = "test";
        dictionary.TryAdd(key, 1);

        // Act
        var result = await dictionary.TryUpdateAsync(key, newValue: 42, comparisonValue: 1, retryCount: 5);

        // Assert
        result.Should().BeTrue();
        dictionary[key].Should().Be(42);
    }

    [Fact]
    public async Task TryUpdateAsync_WithNonMatchingValueAndRetries_ReturnsFalse()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        const string key = "test";
        dictionary.TryAdd(key, 1);

        // Act
        var result = await dictionary.TryUpdateAsync(key, newValue: 42, comparisonValue: 99, retryCount: 3);

        // Assert
        result.Should().BeFalse();
        dictionary[key].Should().Be(1);
    }

    [Fact]
    public async Task TryUpdateAsync_WithDelay_WaitsBeforeRetrying()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        const string key = "test";
        dictionary.TryAdd(key, 1);
        var delay = TimeSpan.FromMilliseconds(100);
        var startTime = DateTime.UtcNow;

        // Act
        var result = await dictionary.TryUpdateAsync(key, newValue: 42, comparisonValue: 99, retryCount: 3, delayBetweenRetries: delay);

        // Assert
        var elapsed = DateTime.UtcNow - startTime;
        result.Should().BeFalse();
        elapsed.Should().BeGreaterOrEqualTo(TimeSpan.FromMilliseconds(200));
    }

    [Fact]
    public async Task TryUpdateAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await dictionary.TryUpdateAsync("test", newValue: 42, comparisonValue: 1, cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task TryUpdateAsync_WithNullDictionary_ThrowsArgumentNullException()
    {
        // Arrange
        ConcurrentDictionary<string, int>? dictionary = null;

        // Act
        Func<Task> act = async () => await dictionary!.TryUpdateAsync("test", newValue: 42, comparisonValue: 1);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("dictionary");
    }

    [Fact]
    public async Task TryUpdateAsync_WithNullKey_ThrowsArgumentNullException()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();

        // Act
        Func<Task> act = async () => await dictionary.TryUpdateAsync(null!, newValue: 42, comparisonValue: 1);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("key");
    }

    [Fact]
    public async Task TryUpdateAsync_WithInvalidRetryCount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();

        // Act
        Func<Task> act = async () => await dictionary.TryUpdateAsync("test", newValue: 42, comparisonValue: 1, retryCount: 0);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("retryCount");
    }

    #endregion

    #region TryGetValueAsync Tests

    [Fact]
    public async Task TryGetValueAsync_WithExistingKey_ReturnsTrueAndValue()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        const string key = "test";
        const int value = 42;
        dictionary.TryAdd(key, value);

        // Act
        var result = await dictionary.TryGetValueAsync(key);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public async Task TryGetValueAsync_WithNonExistingKey_ReturnsFalseAndDefault()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();

        // Act
        var result = await dictionary.TryGetValueAsync("nonexistent");

        // Assert
        result.Success.Should().BeFalse();
        result.Value.Should().Be(default(int));
    }

    [Fact]
    public async Task TryGetValueAsync_WithRetries_ReturnsTrue()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        const string key = "test";
        const int value = 42;
        dictionary.TryAdd(key, value);

        // Act
        var result = await dictionary.TryGetValueAsync(key, retryCount: 5);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public async Task TryGetValueAsync_WithNonExistingKeyAndRetries_ReturnsFalse()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();

        // Act
        var result = await dictionary.TryGetValueAsync("nonexistent", retryCount: 3);

        // Assert
        result.Success.Should().BeFalse();
        result.Value.Should().Be(default(int));
    }

    [Fact]
    public async Task TryGetValueAsync_WithDelay_WaitsBeforeRetrying()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        var delay = TimeSpan.FromMilliseconds(100);
        var startTime = DateTime.UtcNow;

        // Act
        var result = await dictionary.TryGetValueAsync("nonexistent", retryCount: 3, delayBetweenRetries: delay);

        // Assert
        var elapsed = DateTime.UtcNow - startTime;
        result.Success.Should().BeFalse();
        elapsed.Should().BeGreaterOrEqualTo(TimeSpan.FromMilliseconds(200));
    }

    [Fact]
    public async Task TryGetValueAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await dictionary.TryGetValueAsync("test", cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task TryGetValueAsync_WithNullDictionary_ThrowsArgumentNullException()
    {
        // Arrange
        ConcurrentDictionary<string, int>? dictionary = null;

        // Act
        Func<Task> act = async () => await dictionary!.TryGetValueAsync("test");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("dictionary");
    }

    [Fact]
    public async Task TryGetValueAsync_WithNullKey_ThrowsArgumentNullException()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();

        // Act
        Func<Task> act = async () => await dictionary.TryGetValueAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("key");
    }

    [Fact]
    public async Task TryGetValueAsync_WithInvalidRetryCount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();

        // Act
        Func<Task> act = async () => await dictionary.TryGetValueAsync("test", retryCount: 0);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("retryCount");
    }

    [Fact]
    public async Task TryGetValueAsync_WithNullableReferenceType_ReturnsNullValue()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, string?>();
        const string key = "test";
        dictionary.TryAdd(key, null);

        // Act
        var result = await dictionary.TryGetValueAsync(key);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    #endregion

    #region Edge Cases and Integration Tests

    [Fact]
    public async Task TryAddAsync_WithSingleRetryCount_AttemptsOnce()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        const string key = "test";

        // Act
        var result = await dictionary.TryAddAsync(key, 42, retryCount: 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TryRemoveAsync_WithNullableValueType_ReturnsCorrectly()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int?>();
        const string key = "test";
        dictionary.TryAdd(key, null);

        // Act
        var result = await dictionary.TryRemoveAsync(key);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task TryUpdateAsync_WithComplexReferenceType_UpdatesCorrectly()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, TestObject>();
        const string key = "test";
        var oldValue = new TestObject { Id = 1, Name = "Old" };
        var newValue = new TestObject { Id = 2, Name = "New" };
        dictionary.TryAdd(key, oldValue);

        // Act
        var result = await dictionary.TryUpdateAsync(key, newValue, oldValue);

        // Assert
        result.Should().BeTrue();
        dictionary[key].Should().Be(newValue);
        dictionary[key].Id.Should().Be(2);
        dictionary[key].Name.Should().Be("New");
    }

    [Fact]
    public async Task TryAddAsync_WithoutDelay_CompletesQuickly()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        dictionary.TryAdd("test", 1);
        var startTime = DateTime.UtcNow;

        // Act
        var result = await dictionary.TryAddAsync("test", 42, retryCount: 3, delayBetweenRetries: null);

        // Assert
        var elapsed = DateTime.UtcNow - startTime;
        result.Should().BeFalse();
        elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(100)); // Should be fast without delays
    }

    [Fact]
    public async Task AllMethods_WithDefaultCancellationToken_CompleteSuccessfully()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        dictionary.TryAdd("key1", 1);

        // Act & Assert - All should complete without throwing
        var addResult = await dictionary.TryAddAsync("key2", 2);
        var getValue = await dictionary.TryGetValueAsync("key1");
        var updateResult = await dictionary.TryUpdateAsync("key1", 10, 1);
        var removeResult = await dictionary.TryRemoveAsync("key2");

        addResult.Should().BeTrue();
        getValue.Success.Should().BeTrue();
        updateResult.Should().BeTrue();
        removeResult.Success.Should().BeTrue();
    }

    private class TestObject
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }

    #endregion
}
