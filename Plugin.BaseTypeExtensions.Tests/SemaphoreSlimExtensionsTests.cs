namespace Plugin.BaseTypeExtensions.Tests;

public class SemaphoreSlimExtensionsTests
{
    #region WaitBetterAsync(TimeSpan?) tests

    [Fact]
    public async Task WaitBetterAsync_TimeSpan_NullTimeout_CallsOriginalWaitAsync()
    {
        // Arrange
        using var semaphore = new SemaphoreSlim(1, 1);

        // Act
        var result = await semaphore.WaitBetterAsync((TimeSpan?)null);

        // Assert
        result.Should().BeTrue();
        semaphore.CurrentCount.Should().Be(0);
    }

    [Fact]
    public async Task WaitBetterAsync_TimeSpan_ZeroTimeout_CallsOriginalWaitAsync()
    {
        // Arrange
        using var semaphore = new SemaphoreSlim(1, 1);

        // Act
        var result = await semaphore.WaitBetterAsync(TimeSpan.Zero);

        // Assert
        result.Should().BeTrue();
        semaphore.CurrentCount.Should().Be(0);
    }

    [Fact]
    public async Task WaitBetterAsync_TimeSpan_NegativeTimeout_CallsOriginalWaitAsync()
    {
        // Arrange
        using var semaphore = new SemaphoreSlim(1, 1);

        // Act
        var result = await semaphore.WaitBetterAsync(TimeSpan.FromMilliseconds(-1));

        // Assert
        result.Should().BeTrue();
        semaphore.CurrentCount.Should().Be(0);
    }

    [Fact]
    public async Task WaitBetterAsync_TimeSpan_ValidTimeout_EntersSemaphore()
    {
        // Arrange
        using var semaphore = new SemaphoreSlim(1, 1);

        // Act
        var result = await semaphore.WaitBetterAsync(TimeSpan.FromSeconds(1));

        // Assert
        result.Should().BeTrue();
        semaphore.CurrentCount.Should().Be(0);
    }

    [Fact]
    public async Task WaitBetterAsync_TimeSpan_ValidTimeout_TimesOut()
    {
        // Arrange
        using var semaphore = new SemaphoreSlim(0, 1); // No permits available

        // Act
        var result = await semaphore.WaitBetterAsync(TimeSpan.FromMilliseconds(50));

        // Assert
        result.Should().BeFalse();
        semaphore.CurrentCount.Should().Be(0);
    }

    [Fact]
    public async Task WaitBetterAsync_TimeSpan_WithCancellationToken_RespectsToken()
    {
        // Arrange
        using var semaphore = new SemaphoreSlim(0, 1); // No permits available
        using var cts = new CancellationTokenSource();

        // Act
        var waitTask = semaphore.WaitBetterAsync(TimeSpan.FromSeconds(10), cts.Token);
        cts.Cancel();

        // Assert
        Func<Task> act = () => waitTask;
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void WaitBetterAsync_TimeSpan_NullSemaphore_ThrowsArgumentNullException()
    {
        // Arrange
        SemaphoreSlim semaphore = null!;

        // Act & Assert
        Func<Task> act = () => semaphore.WaitBetterAsync((TimeSpan?)null);
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region WaitBetterAsync(int?) tests

    [Fact]
    public async Task WaitBetterAsync_Int_NullTimeout_CallsOriginalWaitAsync()
    {
        // Arrange
        using var semaphore = new SemaphoreSlim(1, 1);

        // Act
        var result = await semaphore.WaitBetterAsync((int?)null);

        // Assert
        result.Should().BeTrue();
        semaphore.CurrentCount.Should().Be(0);
    }

    [Fact]
    public async Task WaitBetterAsync_Int_ZeroTimeout_CallsOriginalWaitAsync()
    {
        // Arrange
        using var semaphore = new SemaphoreSlim(1, 1);

        // Act
        var result = await semaphore.WaitBetterAsync(0);

        // Assert
        result.Should().BeTrue();
        semaphore.CurrentCount.Should().Be(0);
    }

    [Fact]
    public async Task WaitBetterAsync_Int_NegativeTimeout_CallsOriginalWaitAsync()
    {
        // Arrange
        using var semaphore = new SemaphoreSlim(1, 1);

        // Act
        var result = await semaphore.WaitBetterAsync(-1);

        // Assert
        result.Should().BeTrue();
        semaphore.CurrentCount.Should().Be(0);
    }

    [Fact]
    public async Task WaitBetterAsync_Int_ValidTimeout_EntersSemaphore()
    {
        // Arrange
        using var semaphore = new SemaphoreSlim(1, 1);

        // Act
        var result = await semaphore.WaitBetterAsync(1000);

        // Assert
        result.Should().BeTrue();
        semaphore.CurrentCount.Should().Be(0);
    }

    [Fact]
    public async Task WaitBetterAsync_Int_ValidTimeout_TimesOut()
    {
        // Arrange
        using var semaphore = new SemaphoreSlim(0, 1); // No permits available

        // Act
        var result = await semaphore.WaitBetterAsync(50);

        // Assert
        result.Should().BeFalse();
        semaphore.CurrentCount.Should().Be(0);
    }

    [Fact]
    public async Task WaitBetterAsync_Int_WithCancellationToken_RespectsToken()
    {
        // Arrange
        using var semaphore = new SemaphoreSlim(0, 1); // No permits available
        using var cts = new CancellationTokenSource();

        // Act
        var waitTask = semaphore.WaitBetterAsync(10000, cts.Token);
        cts.Cancel();

        // Assert
        Func<Task> act = () => waitTask;
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void WaitBetterAsync_Int_NullSemaphore_ThrowsArgumentNullException()
    {
        // Arrange
        SemaphoreSlim semaphore = null!;

        // Act & Assert
        Func<Task> act = () => semaphore.WaitBetterAsync((int?)null);
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region Integration and edge case tests

    [Fact]
    public async Task WaitBetterAsync_LargeTimeout_DoesNotThrow()
    {
        // Arrange
        using var semaphore = new SemaphoreSlim(1, 1);
        var largeTimeout = TimeSpan.FromDays(365); // Very large timeout

        // Act & Assert - should not throw, even with very large timeouts
        var result = await semaphore.WaitBetterAsync(largeTimeout);
        result.Should().BeTrue();
        semaphore.CurrentCount.Should().Be(0);
    }

    [Fact]
    public async Task WaitBetterAsync_LargeTimeoutInt_DoesNotThrow()
    {
        // Arrange
        using var semaphore = new SemaphoreSlim(1, 1);
        var largeTimeout = int.MaxValue; // Very large timeout

        // Act & Assert - should not throw, even with very large timeouts
        var result = await semaphore.WaitBetterAsync(largeTimeout);
        result.Should().BeTrue();
        semaphore.CurrentCount.Should().Be(0);
    }

    [Fact]
    public async Task WaitBetterAsync_SemaphoreRelease_AllowsEntry()
    {
        // Arrange
        using var semaphore = new SemaphoreSlim(0, 1); // No permits initially

        // Act
        var waitTask = semaphore.WaitBetterAsync(TimeSpan.FromSeconds(1));

        // Release the semaphore after a short delay
        _ = Task.Run(async () =>
        {
            await Task.Delay(50);
            semaphore.Release();
        });

        var result = await waitTask;

        // Assert
        result.Should().BeTrue();
        semaphore.CurrentCount.Should().Be(0);
    }

    [Fact]
    public async Task WaitBetterAsync_MultipleConcurrentWaits_HandlesCorrectly()
    {
        // Arrange
        using var semaphore = new SemaphoreSlim(2, 2); // Two permits

        // Act
        var task1 = semaphore.WaitBetterAsync(TimeSpan.FromSeconds(1));
        var task2 = semaphore.WaitBetterAsync(TimeSpan.FromSeconds(1));
        var task3 = semaphore.WaitBetterAsync(TimeSpan.FromMilliseconds(50)); // Should timeout

        var results = await Task.WhenAll(task1, task2, task3);

        // Assert
        results[0].Should().BeTrue(); // First two should succeed
        results[1].Should().BeTrue();
        results[2].Should().BeFalse(); // Third should timeout
        semaphore.CurrentCount.Should().Be(0);
    }

    #endregion
}
