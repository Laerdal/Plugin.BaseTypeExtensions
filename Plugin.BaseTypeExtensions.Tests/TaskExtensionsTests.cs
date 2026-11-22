namespace Plugin.BaseTypeExtensions.Tests;

public class TaskExtensionsTests
{
    /// <summary>
    /// Upper bound for waits that only run out when a test is failing (a hang), never in normal runs.
    /// </summary>
    private static readonly TimeSpan SafetyTimeout = TimeSpan.FromSeconds(10);

    [Fact]
    public void IsDone_ReflectsTaskStatus()
    {
        Task.CompletedTask.IsCompleted.Should().BeTrue();
        Task.FromCanceled(new CancellationToken(true)).IsCompleted.Should().BeTrue();
        Task.FromException(new InvalidOperationException()).IsCompleted.Should().BeTrue();

        var tcs = new TaskCompletionSource();
        tcs.Task.IsCompleted.Should().BeFalse();
    }

    #region WaitAsync<T> tests

    [Fact]
    public async Task WaitAsync_Generic_NullTimeout_ReturnsSameTask()
    {
        // Arrange
        var task = Task.FromResult(42);

        // Act
        var result = task.WaitBetterAsync(null);

        // Assert
        result.Should().BeSameAs(task);
        (await result).Should().Be(42);
    }

    [Fact]
    public async Task WaitAsync_Generic_ZeroTimeout_ReturnsSameTask()
    {
        // Arrange
        var task = Task.FromResult(42);

        // Act
        var result = task.WaitBetterAsync(TimeSpan.Zero);

        // Assert
        result.Should().BeSameAs(task);
        (await result).Should().Be(42);
    }

    [Fact]
    public async Task WaitAsync_Generic_NegativeTimeout_ReturnsSameTask()
    {
        // Arrange
        var task = Task.FromResult(42);

        // Act
        var result = task.WaitBetterAsync(TimeSpan.FromMilliseconds(-1));

        // Assert
        result.Should().BeSameAs(task);
        (await result).Should().Be(42);
    }

    [Fact]
    public async Task WaitAsync_Generic_ValidTimeout_CompletesSuccessfully()
    {
        // Arrange
        var task = Task.FromResult(42);

        // Act
        var result = await task.WaitBetterAsync(TimeSpan.FromSeconds(1));

        // Assert
        result.Should().Be(42);
    }

    [Fact]
    public async Task WaitAsync_Generic_ValidTimeout_ThrowsTimeoutException()
    {
        // Arrange
        var tcs = new TaskCompletionSource<int>();

        // Act & Assert
        Func<Task> act = () => tcs.Task.WaitBetterAsync(TimeSpan.FromMilliseconds(50));
        await act.Should().ThrowAsync<TimeoutException>();
    }

    [Fact]
    public async Task WaitAsync_Generic_WithCancellationToken_RespectsToken()
    {
        // Arrange
        var tcs = new TaskCompletionSource<int>();
        using var cts = new CancellationTokenSource();

        // Act
        var waitTask = tcs.Task.WaitBetterAsync(TimeSpan.FromSeconds(10), cts.Token);
        cts.Cancel();

        // Assert
        Func<Task> act = () => waitTask;
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task WaitAsync_Generic_NoTimeout_RespectsToken(int? timeoutMilliseconds)
    {
        // Arrange
        var tcs = new TaskCompletionSource<int>();
        using var cts = new CancellationTokenSource();
        TimeSpan? timeout = timeoutMilliseconds is { } ms ? TimeSpan.FromMilliseconds(ms) : null;

        // Act
        var waitTask = tcs.Task.WaitBetterAsync(timeout, cts.Token);
        cts.Cancel();

        // Assert: bounded, so a regression fails instead of hanging
        Func<Task> act = () => waitTask.WaitAsync(TimeSpan.FromSeconds(5));
        await act.Should().ThrowAsync<OperationCanceledException>();
        tcs.Task.IsCompleted.Should().BeFalse("only the wait is cancelled, not the awaited task");
    }

    [Fact]
    public async Task WaitAsync_Generic_NullTimeout_CancelableTokenNotCancelled_ReturnsResult()
    {
        // Arrange
        var tcs = new TaskCompletionSource<int>();
        using var cts = new CancellationTokenSource();

        // Act
        var waitTask = tcs.Task.WaitBetterAsync(null, cts.Token);
        tcs.SetResult(42);

        // Assert
        (await waitTask).Should().Be(42);
    }

    #endregion

    #region WaitAsync (non-generic) tests

    [Fact]
    public async Task WaitAsync_NonGeneric_NullTimeout_ReturnsSameTask()
    {
        // Arrange
        var task = Task.CompletedTask;

        // Act
        var result = task.WaitBetterAsync(null);

        // Assert
        result.Should().BeSameAs(task);
        await result; // Should complete successfully
    }

    [Fact]
    public async Task WaitAsync_NonGeneric_ZeroTimeout_ReturnsSameTask()
    {
        // Arrange
        var task = Task.CompletedTask;

        // Act
        var result = task.WaitBetterAsync(TimeSpan.Zero);

        // Assert
        result.Should().BeSameAs(task);
        await result; // Should complete successfully
    }

    [Fact]
    public async Task WaitAsync_NonGeneric_NegativeTimeout_ReturnsSameTask()
    {
        // Arrange
        var task = Task.CompletedTask;

        // Act
        var result = task.WaitBetterAsync(TimeSpan.FromMilliseconds(-1));

        // Assert
        result.Should().BeSameAs(task);
        await result; // Should complete successfully
    }

    [Fact]
    public async Task WaitAsync_NonGeneric_ValidTimeout_CompletesSuccessfully()
    {
        // Arrange
        var task = Task.Delay(10); // Short delay

        // Act & Assert
        await task.WaitBetterAsync(TimeSpan.FromSeconds(1)); // Should not throw
    }

    [Fact]
    public async Task WaitAsync_NonGeneric_ValidTimeout_ThrowsTimeoutException()
    {
        // Arrange
        var tcs = new TaskCompletionSource();

        // Act & Assert
        Func<Task> act = () => tcs.Task.WaitBetterAsync(TimeSpan.FromMilliseconds(50));
        await act.Should().ThrowAsync<TimeoutException>();
    }

    [Fact]
    public async Task WaitAsync_NonGeneric_WithCancellationToken_RespectsToken()
    {
        // Arrange
        var tcs = new TaskCompletionSource();
        using var cts = new CancellationTokenSource();

        // Act
        var waitTask = tcs.Task.WaitBetterAsync(TimeSpan.FromSeconds(10), cts.Token);
        cts.Cancel();

        // Assert
        Func<Task> act = () => waitTask;
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task WaitAsync_NonGeneric_NoTimeout_RespectsToken(int? timeoutMilliseconds)
    {
        // Arrange
        var tcs = new TaskCompletionSource();
        using var cts = new CancellationTokenSource();
        TimeSpan? timeout = timeoutMilliseconds is { } ms ? TimeSpan.FromMilliseconds(ms) : null;

        // Act
        var waitTask = tcs.Task.WaitBetterAsync(timeout, cts.Token);
        cts.Cancel();

        // Assert: bounded, so a regression fails instead of hanging
        Func<Task> act = () => waitTask.WaitAsync(TimeSpan.FromSeconds(5));
        await act.Should().ThrowAsync<OperationCanceledException>();
        tcs.Task.IsCompleted.Should().BeFalse("only the wait is cancelled, not the awaited task");
    }

    [Fact]
    public async Task WaitAsync_NonGeneric_NullTimeout_CancelableTokenNotCancelled_Completes()
    {
        // Arrange
        var tcs = new TaskCompletionSource();
        using var cts = new CancellationTokenSource();

        // Act
        var waitTask = tcs.Task.WaitBetterAsync(null, cts.Token);
        tcs.SetResult();

        // Assert
        await waitTask.WaitAsync(TimeSpan.FromSeconds(5));
        waitTask.IsCompletedSuccessfully.Should().BeTrue();
    }

    #endregion

    #region Integration and edge case tests

    [Fact]
    public async Task WaitAsync_LargeTimeout_DoesNotThrow()
    {
        // Arrange
        var task = Task.FromResult(42);
        var largeTimeout = TimeSpan.FromDays(365); // Very large timeout

        // Act & Assert - should not throw, even with very large timeouts
        var result = await task.WaitBetterAsync(largeTimeout);
        result.Should().Be(42);
    }

    [Fact]
    public async Task WaitAsync_CancelledTask_PropagatesCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var cancelledTask = Task.FromCanceled<int>(cts.Token);

        // Act & Assert
        Func<Task> act = () => cancelledTask.WaitBetterAsync(TimeSpan.FromSeconds(1));
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task WaitAsync_FaultedTask_PropagatesException()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");
        var faultedTask = Task.FromException<int>(exception);

        // Act & Assert
        Func<Task> act = () => faultedTask.WaitBetterAsync(TimeSpan.FromSeconds(1));
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Test exception");
    }

    #endregion

    #region StartAndForget tests

    [Fact]
    public async Task StartAndForget_SuccessfulTask_CompletesWithoutException()
    {
        // Arrange
        var taskCompleted = false;
        var exceptionHandled = false;
        var taskCompletionSignal = new TaskCompletionSource();
        var task = Task.Run(async () =>
        {
            await Task.Delay(10);
            taskCompleted = true;
            taskCompletionSignal.SetResult();
        });

        // Act
        task.StartAndForget(ex => exceptionHandled = true);

        // Wait for the task to complete (with timeout)
        var completed = await Task.WhenAny(taskCompletionSignal.Task, Task.Delay(1000));
        completed.Should().Be(taskCompletionSignal.Task, "Task should complete within timeout");

        // Assert
        taskCompleted.Should().BeTrue();
        exceptionHandled.Should().BeFalse();
    }

    [Fact]
    public async Task StartAndForget_TaskThatThrows_InvokesExceptionHandler()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        Exception? capturedEx = null;
        var handlerInvoked = new TaskCompletionSource();
        var task = Task.Run(async () =>
        {
            await Task.Delay(10);
            throw expectedException;
        });

        // Act
        task.StartAndForget(ex =>
        {
            capturedEx = ex;
            handlerInvoked.SetResult();
        });

        // Wait for the exception handler to be invoked (with timeout)
        var completed = await Task.WhenAny(handlerInvoked.Task, Task.Delay(1000));
        completed.Should().Be(handlerInvoked.Task, "Exception handler should be invoked within timeout");

        // Assert
        capturedEx.Should().NotBeNull();
        capturedEx.Should().BeOfType<InvalidOperationException>();
        capturedEx!.Message.Should().Be("Test exception");
    }

    [Fact]
    public async Task StartAndForget_ExceptionHandlerReceivesCorrectException()
    {
        // Arrange
        var testMessage = "Specific error message";
        Exception? receivedException = null;
        var handlerInvoked = new TaskCompletionSource();
        Func<int> throwingFunc = () => throw new ArgumentException(testMessage);
        var task = Task.Run(throwingFunc);

        // Act
        task.StartAndForget(ex =>
        {
            receivedException = ex;
            handlerInvoked.SetResult();
        });

        // Wait for the exception handler to be invoked (with timeout)
        var completed = await Task.WhenAny(handlerInvoked.Task, Task.Delay(1000));
        completed.Should().Be(handlerInvoked.Task, "Exception handler should be invoked within timeout");

        // Assert
        receivedException.Should().NotBeNull();
        receivedException.Should().BeOfType<ArgumentException>();
        receivedException!.Message.Should().Contain(testMessage);
    }

    [Fact]
    public async Task StartAndForget_MultipleExceptions_EachIsHandledSeparately()
    {
        // Arrange
        var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();
        var tcs1 = new TaskCompletionSource();
        var tcs2 = new TaskCompletionSource();
        var tcs3 = new TaskCompletionSource();
        var task1 = Task.Run(() => throw new InvalidOperationException("Error 1"));
        var task2 = Task.Run(() => throw new ArgumentException("Error 2"));
        var task3 = Task.Run(() => throw new NotSupportedException("Error 3"));

        // Act
        task1.StartAndForget(ex => { exceptions.Add(ex); tcs1.SetResult(); });
        task2.StartAndForget(ex => { exceptions.Add(ex); tcs2.SetResult(); });
        task3.StartAndForget(ex => { exceptions.Add(ex); tcs3.SetResult(); });

        // Wait for all handlers to be invoked (with timeout)
        var allCompleted = Task.WhenAll(tcs1.Task, tcs2.Task, tcs3.Task);
        var completed = await Task.WhenAny(allCompleted, Task.Delay(1000));
        completed.Should().Be(allCompleted, "All exception handlers should be invoked within timeout");

        // Assert
        exceptions.Should().HaveCount(3);
        exceptions.Should().Contain(ex => ex is InvalidOperationException && ex.Message == "Error 1");
        exceptions.Should().Contain(ex => ex is ArgumentException && ex.Message.Contains("Error 2"));
        exceptions.Should().Contain(ex => ex is NotSupportedException && ex.Message == "Error 3");
    }

    [Fact]
    public async Task StartAndForget_CompletedTask_HandlerNotInvoked()
    {
        // Arrange
        var handlerInvoked = false;
        var task = Task.CompletedTask;

        // Act
        task.StartAndForget(ex => handlerInvoked = true);

        // Wait to ensure handler would have been called if there was an exception
        await Task.Delay(50);

        // Assert
        handlerInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task StartAndForget_CancelledTask_InvokesExceptionHandler()
    {
        // Arrange
        Exception? capturedEx = null;
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var handlerInvoked = new TaskCompletionSource();
        var task = Task.Run(() => 
        {
            cts.Token.ThrowIfCancellationRequested();
        }, cts.Token);

        // Act
        task.StartAndForget(ex =>
        {
            capturedEx = ex;
            handlerInvoked.SetResult();
        });

        // Wait for exception handler to be invoked (with timeout)
        var completed = await Task.WhenAny(handlerInvoked.Task, Task.Delay(1000));
        completed.Should().Be(handlerInvoked.Task, "Exception handler should be invoked within timeout");

        // Assert
        capturedEx.Should().NotBeNull();
        capturedEx.Should().BeAssignableTo<OperationCanceledException>("TaskCanceledException derives from OperationCanceledException");
    }

    [Fact]
    public async Task StartAndForget_ExceptionInHandler_DoesNotPropagateToCallerContext()
    {
        // Arrange
        var task = Task.Run(() => throw new InvalidOperationException("Original exception"));
        var handlerExecuted = false;
        var handlerExecutedTcs = new TaskCompletionSource();

        // Act - exception in handler should not crash the test
        task.StartAndForget(ex =>
        {
            handlerExecuted = true;
            handlerExecutedTcs.SetResult();
            // This exception should be handled by the async void context
            throw new Exception("Exception in handler");
        });

        // Wait for handler to execute deterministically
        await Task.WhenAny(handlerExecutedTcs.Task, Task.Delay(1000));

        // Assert - we can only verify the handler was called
        // The exception in the handler goes to the sync context but doesn't crash the test
        handlerExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task StartAndForget_LongRunningTask_DoesNotBlockCaller()
    {
        // Arrange - the task cannot finish until the test releases it, so StartAndForget can
        // only return while the task is still running if it does not wait for the task
        var taskStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var task = Task.Run(async () =>
        {
            taskStarted.SetResult();
            await release.Task;
        });

        try
        {
            // Act - an implementation that blocks on the task would never return, so bound the call
            await Task.Run(() => task.StartAndForget(ex => { })).WaitAsync(SafetyTimeout);

            // Assert
            await taskStarted.Task.WaitAsync(SafetyTimeout);
            task.IsCompleted.Should().BeFalse("the task is held until the test releases it");
        }
        finally
        {
            release.SetResult();
        }
    }

    #endregion

    #region StartAndForget with TaskCompletionSource tests

    [Fact]
    public async Task StartAndForget_WithTCS_SuccessfulTask_CompletesTaskCompletionSource()
    {
        // Arrange
        var taskCompleted = false;
        var task = Task.Run(async () =>
        {
            await Task.Delay(10);
            taskCompleted = true;
        });
        var tcs = new TaskCompletionSource();

        // Act
        task.StartAndForget(tcs);

        // Wait for the task to complete
        await tcs.Task;

        // Assert
        taskCompleted.Should().BeTrue();
        tcs.Task.IsCompletedSuccessfully.Should().BeTrue();
    }

    [Fact]
    public async Task StartAndForget_WithTCS_TaskThatThrows_SetsException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        var task = Task.Run(async () =>
        {
            await Task.Delay(10);
            throw expectedException;
        });
        var tcs = new TaskCompletionSource();

        // Act
        task.StartAndForget(tcs);

        // Wait for the task to complete
        Func<Task> awaitCompletion = async () => await tcs.Task;
        await awaitCompletion.Should().ThrowAsync<InvalidOperationException>();

        // Assert
        tcs.Task.IsFaulted.Should().BeTrue();
        var thrownException = await tcs.Task.ContinueWith(t => t.Exception?.InnerException);
        thrownException.Should().BeOfType<InvalidOperationException>();
        thrownException!.Message.Should().Be("Test exception");
    }

    [Fact]
    public async Task StartAndForget_WithTCS_ExceptionPropagatedCorrectly()
    {
        // Arrange
        var testMessage = "Specific error message";
        Func<int> throwingFunc = () => throw new ArgumentException(testMessage);
        var task = Task.Run(throwingFunc);
        var tcs = new TaskCompletionSource();

        // Act
        task.StartAndForget(tcs);

        // Wait for exception to be set
        Func<Task> awaitCompletion = async () => await tcs.Task;
        await awaitCompletion.Should().ThrowAsync<ArgumentException>();

        // Assert
        tcs.Task.IsFaulted.Should().BeTrue();
        tcs.Task.Exception.Should().NotBeNull();
        tcs.Task.Exception!.InnerException.Should().BeOfType<ArgumentException>();
        tcs.Task.Exception.InnerException!.Message.Should().Contain(testMessage);
    }

    [Fact]
    public async Task StartAndForget_WithTCS_MultipleOperations_EachCompletesIndependently()
    {
        // Arrange
        var tcs1 = new TaskCompletionSource();
        var tcs2 = new TaskCompletionSource();
        var tcs3 = new TaskCompletionSource();
        
        var task1 = Task.Run(async () => { await Task.Delay(10); });
        var task2 = Task.Run(() => throw new InvalidOperationException("Error 2"));
        var task3 = Task.Run(async () => { await Task.Delay(20); });

        // Act
        task1.StartAndForget(tcs1);
        task2.StartAndForget(tcs2);
        task3.StartAndForget(tcs3);

        // Wait for all to complete
        await Task.Delay(200);

        // Assert
        tcs1.Task.IsCompletedSuccessfully.Should().BeTrue();
        tcs2.Task.IsFaulted.Should().BeTrue();
        tcs3.Task.IsCompletedSuccessfully.Should().BeTrue();
    }

    [Fact]
    public async Task StartAndForget_WithTCS_CompletedTask_SignalsImmediately()
    {
        // Arrange
        var task = Task.CompletedTask;
        var tcs = new TaskCompletionSource();

        // Act
        task.StartAndForget(tcs);

        // Wait a short time
        await Task.Delay(50);

        // Assert
        tcs.Task.IsCompletedSuccessfully.Should().BeTrue();
    }

    [Fact]
    public async Task StartAndForget_WithTCS_CancelledTask_SetsException()
    {
        // Arrange
        Task task;
        using (var cts = new CancellationTokenSource())
        {
            cts.Cancel();
            task = Task.Run(() =>
            {
                cts.Token.ThrowIfCancellationRequested();
            }, cts.Token);
        }
        var tcs = new TaskCompletionSource();

        // Act
        task.StartAndForget(tcs);

        // Wait for exception to be set
        Func<Task> awaitCompletion = async () => await tcs.Task;
        await awaitCompletion.Should().ThrowAsync<OperationCanceledException>();

        // Assert
        tcs.Task.IsFaulted.Should().BeTrue();
        tcs.Task.Exception.Should().NotBeNull();
        tcs.Task.Exception!.InnerException.Should().BeAssignableTo<OperationCanceledException>();
    }

    [Fact]
    public async Task StartAndForget_WithTCS_DoesNotBlockCaller()
    {
        // Arrange - the task cannot finish until the test releases it, so StartAndForget can
        // only return while the task is still running if it does not wait for the task
        var taskStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var task = Task.Run(async () =>
        {
            taskStarted.SetResult();
            await release.Task;
        });
        var tcs = new TaskCompletionSource();

        try
        {
            // Act - an implementation that blocks on the task would never return, so bound the call
            await Task.Run(() => task.StartAndForget(tcs)).WaitAsync(SafetyTimeout);

            // Assert
            await taskStarted.Task.WaitAsync(SafetyTimeout);
            task.IsCompleted.Should().BeFalse("the task is held until the test releases it");
            tcs.Task.IsCompleted.Should().BeFalse("task is still running");
        }
        finally
        {
            release.SetResult();
        }
    }

    [Fact]
    public async Task StartAndForget_WithTCS_CanAwaitCompletion()
    {
        // Arrange
        var completed = false;
        var task = Task.Run(async () =>
        {
            await Task.Delay(50);
            completed = true;
        });
        var tcs = new TaskCompletionSource();

        // Act
        task.StartAndForget(tcs);
        await tcs.Task; // Wait for completion

        // Assert
        completed.Should().BeTrue();
        tcs.Task.IsCompletedSuccessfully.Should().BeTrue();
    }

    #endregion
}
