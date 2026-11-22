namespace Plugin.BaseTypeExtensions.Tests;

public class CancellationTokenExtensionsTests
{
    [Fact]
    public void WithTimeout_TimeSpan_CreatesCombinedToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var originalToken = cts.Token;
        var timeout = TimeSpan.FromMilliseconds(100);

        // Act
        var result = originalToken.WithTimeout(timeout);

        // Assert
        result.Should().NotBe(originalToken);
        result.CanBeCanceled.Should().BeTrue();
    }

    [Fact]
    public void WithTimeout_Milliseconds_CreatesCombinedToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var originalToken = cts.Token;
        var timeoutMs = 100;

        // Act
        var result = originalToken.WithTimeout(timeoutMs);

        // Assert
        result.Should().NotBe(originalToken);
        result.CanBeCanceled.Should().BeTrue();
    }

    [Fact]
    public void WithTimeout_NegativeTimeSpan_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var token = CancellationToken.None;
        var negativeTimeout = TimeSpan.FromMilliseconds(-100);

        // Act & Assert
        var act = () => token.WithTimeout(negativeTimeout);
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Timeout cannot be negative*")
            .And.ParamName.Should().Be("timeout");
    }

    [Fact]
    public void WithTimeout_NegativeMilliseconds_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var token = CancellationToken.None;

        // Act & Assert
        var act = () => token.WithTimeout(-100);
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Timeout cannot be negative*")
            .And.ParamName.Should().Be("timeout");
    }

    [Fact]
    public void WithTimeout_ZeroTimeSpan_ReturnsAlreadyCanceledToken()
    {
        // Arrange
        var token = CancellationToken.None;

        // Act
        var result = token.WithTimeout(TimeSpan.Zero);

        // Assert
        result.IsCancellationRequested.Should().BeTrue();
    }

    [Fact]
    public void WithTimeout_AlreadyCanceledToken_ReturnsSameToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var canceledToken = cts.Token;

        // Act
        var result = canceledToken.WithTimeout(TimeSpan.FromSeconds(1));

        // Assert
        result.Should().Be(canceledToken);
        result.IsCancellationRequested.Should().BeTrue();
    }

    [Fact]
    public void AsTask_ReturnsTaskThatCompletesOnCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act
        var task = token.AsTask();
        cts.Cancel();

        // Assert
        task.IsCanceled.Should().BeTrue();
    }

    [Fact]
    public void AsTask_NeverCompletesForNonCanceledToken()
    {
        // Arrange
        var token = CancellationToken.None;

        // Act
        var task = token.AsTask();

        // Assert
        task.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void AsTask_AlreadyCanceledToken_ReturnsCompletedTask()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var token = cts.Token;

        // Act
        var task = token.AsTask();

        // Assert
        task.IsCompleted.Should().BeTrue();
        task.IsCanceled.Should().BeTrue();
    }

    [Fact]
    public void ThrowIfCanceledAfter_ThrowsWhenCanceled()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        cts.Cancel();

        // Act & Assert
        var act = () => token.ThrowIfCanceledAfter(TimeSpan.FromMilliseconds(100));
        act.Should().Throw<OperationCanceledException>();
    }

    [Fact]
    public void ThrowIfCanceledAfter_NegativeTimeout_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var token = CancellationToken.None;

        // Act & Assert
        var act = () => token.ThrowIfCanceledAfter(TimeSpan.FromMilliseconds(-100));
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Timeout cannot be negative*")
            .And.ParamName.Should().Be("timeout");
    }

    [Fact]
    public void ThrowIfCanceledAfter_ZeroTimeout_ThrowsOperationCanceledException()
    {
        // Arrange
        var token = CancellationToken.None;

        // Act & Assert
        var act = () => token.ThrowIfCanceledAfter(TimeSpan.Zero);
        act.Should().Throw<OperationCanceledException>()
            .WithMessage("*Timeout expired*");
    }

    [Fact]
    public void ThrowIfCanceledAfter_TimeoutExpires_ThrowsOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act & Assert
        var act = () => token.ThrowIfCanceledAfter(TimeSpan.FromMilliseconds(10));
        act.Should().Throw<OperationCanceledException>()
            .WithMessage("*Timeout expired*");
    }

    [Fact]
    public void WaitHandle_ReturnsTrueWhenCanceled()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        cts.Cancel();

        // Act
        var result = token.WaitHandle(100);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void WaitHandle_ReturnsFalseOnTimeout()
    {
        // Arrange
        var token = CancellationToken.None;

        // Act
        var result = token.WaitHandle(50);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void WaitHandle_NegativeTimeout_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var token = CancellationToken.None;

        // Act & Assert
        var act = () => token.WaitHandle(-100);
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Timeout cannot be negative*")
            .And.ParamName.Should().Be("timeoutMs");
    }

    [Fact]
    public void WaitHandle_ZeroTimeout_ReturnsFalseImmediately()
    {
        // Arrange
        var token = CancellationToken.None;

        // Act
        var result = token.WaitHandle(0);

        // Assert
        result.Should().BeFalse();
    }
}
