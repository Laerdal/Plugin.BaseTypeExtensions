namespace Plugin.BaseTypeExtensions.Tests;

public class ExceptionExtensionsTests
{
    [Fact]
    public void GetFullMessage_SimpleException_ReturnsMessage()
    {
        // Arrange
        var message = "Test exception message";
        var exception = new InvalidOperationException(message);

        // Act
        var result = exception.GetFullMessage();

        // Assert
        result.Should().Be(message);
    }

    [Fact]
    public void GetFullMessage_ExceptionWithInnerException_ReturnsChainedMessages()
    {
        // Arrange
        var innerMessage = "Inner exception message";
        var outerMessage = "Outer exception message";
        var innerException = new ArgumentException(innerMessage);
        var outerException = new InvalidOperationException(outerMessage, innerException);

        // Act
        var result = outerException.GetFullMessage();

        // Assert
        result.Should().Be($"{outerMessage} --> {innerMessage}");
    }

    [Fact]
    public void GetStackTraceWithoutSystemCalls_FiltersSystemCalls()
    {
        // Arrange
        Exception exception;
        try
        {
            throw new InvalidOperationException("Test with stack trace");
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        // Act
        var result = exception.GetStackTraceWithoutSystemCalls();

        // Assert
        result.Should().Contain("GetStackTraceWithoutSystemCalls_FiltersSystemCalls");
        result.Should().NotContain("System.");
    }

    [Fact]
    public void GetInnerExceptions_ReturnsAllInnerExceptions()
    {
        // Arrange
        var innermost = new ArgumentException("Innermost");
        var middle = new InvalidOperationException("Middle", innermost);
        var outer = new ApplicationException("Outer", middle);

        // Act
        var result = outer.GetInnerExceptions().ToArray();

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().BeSameAs(middle);
        result[1].Should().BeSameAs(innermost);
    }

    [Fact]
    public void GetInnermostException_ReturnsDeepestException()
    {
        // Arrange
        var innermost = new ArgumentException("Innermost");
        var middle = new InvalidOperationException("Middle", innermost);
        var outer = new ApplicationException("Outer", middle);

        // Act
        var result = outer.GetInnermostException();

        // Assert
        result.Should().BeSameAs(innermost);
    }

    [Fact]
    public void GetInnerException_FindsSpecificType()
    {
        // Arrange
        var innermost = new ArgumentException("Innermost");
        var middle = new InvalidOperationException("Middle", innermost);
        var outer = new ApplicationException("Outer", middle);

        // Act
        var result = outer.GetInnerException<ArgumentException>();

        // Assert
        result.Should().BeSameAs(innermost);
    }

    [Fact]
    public void HasInnerException_ReturnsTrueWhenTypeExists()
    {
        // Arrange
        var innermost = new ArgumentException("Innermost");
        var outer = new InvalidOperationException("Outer", innermost);

        // Act
        var result = outer.HasInnerException<ArgumentException>();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetExceptionData_ReturnsAllData()
    {
        // Arrange
        var exception = new InvalidOperationException("Test with data");
        exception.Data.Add("Key1", "Value1");
        exception.Data.Add("Key2", 42);

        // Act
        var result = exception.GetExceptionData();

        // Assert
        result.Should().ContainKey("Key1").WhoseValue.Should().Be("Value1");
        result.Should().ContainKey("Key2").WhoseValue.Should().Be(42);
    }

    [Fact]
    public void IsFatal_ReturnsTrueForFatalExceptions()
    {
        // Arrange
        var fatalException = new OutOfMemoryException();
        var normalException = new ArgumentException();

        // Act & Assert
        fatalException.IsFatal().Should().BeTrue();
        normalException.IsFatal().Should().BeFalse();
    }

    [Fact]
    public void ToLogString_ReturnsFormattedString()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");
        exception.Data.Add("Key1", "Value1");

        // Act
        var result = exception.ToLogString();

        // Assert
        result.Should().Contain("InvalidOperationException");
        result.Should().Contain("Test exception");
        result.Should().Contain("Key1");
        result.Should().Contain("Value1");
    }
}
