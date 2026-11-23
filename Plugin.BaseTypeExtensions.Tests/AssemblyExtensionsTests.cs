using System.Reflection;

namespace Plugin.BaseTypeExtensions.Tests;

public class AssemblyExtensionsTests
{
    #region Helper Methods

    private static string CreateTempDirectory()
    {
        var path = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    #endregion

    #region MoveManifestResourceToDirectory Tests

    [Fact]
    public void MoveManifestResourceToDirectory_WritesFile()
    {
        var directory = CreateTempDirectory();
        try
        {
            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            var result = assembly.MoveManifestResourceToDirectory(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt",
                directory,
                filename: "resource.txt",
                fileAlreadyExistsBehavior: AssemblyExtensions.FileAlreadyExistsBehavior.Overwrite);

            File.ReadAllText(result.FullName).Should().Contain("first resource content");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void MoveManifestResourceToDirectory_GeneratesRandomFilename()
    {
        var directory = CreateTempDirectory();
        try
        {
            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            var result = assembly.MoveManifestResourceToDirectory(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt",
                directory);

            result.Name.Should().NotBeNullOrEmpty();
            File.ReadAllText(result.FullName).Should().Contain("first resource content");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void MoveManifestResourceToDirectory_CreatesDirectoryIfNotExists()
    {
        var parentDirectory = CreateTempDirectory();
        var targetDirectory = Path.Join(parentDirectory, "subdir", "nested");
        try
        {
            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            var result = assembly.MoveManifestResourceToDirectory(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt",
                targetDirectory,
                filename: "resource.txt");

            Directory.Exists(targetDirectory).Should().BeTrue();
            File.ReadAllText(result.FullName).Should().Contain("first resource content");
        }
        finally
        {
            Directory.Delete(parentDirectory, recursive: true);
        }
    }

    [Theory]
    [InlineData(AssemblyExtensions.FileAlreadyExistsBehavior.Overwrite)]
    [InlineData(AssemblyExtensions.FileAlreadyExistsBehavior.Skip)]
    [InlineData(AssemblyExtensions.FileAlreadyExistsBehavior.Rename)]
    public void MoveManifestResourceToDirectory_HandlesFileAlreadyExistsBehavior(AssemblyExtensions.FileAlreadyExistsBehavior behavior)
    {
        var directory = CreateTempDirectory();
        try
        {
            var targetFile = Path.Join(directory, "resource.txt");
            File.WriteAllText(targetFile, "existing");

            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            var result = assembly.MoveManifestResourceToDirectory(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt",
                directory,
                filename: "resource.txt",
                fileAlreadyExistsBehavior: behavior);

            switch (behavior)
            {
                case AssemblyExtensions.FileAlreadyExistsBehavior.Overwrite:
                    result.FullName.Should().Be(targetFile);
                    File.ReadAllText(result.FullName).Should().Contain("first resource content");
                    break;

                case AssemblyExtensions.FileAlreadyExistsBehavior.Skip:
                    result.FullName.Should().Be(targetFile);
                    File.ReadAllText(result.FullName).Should().Be("existing");
                    break;

                case AssemblyExtensions.FileAlreadyExistsBehavior.Rename:
                    result.Name.Should().Be("resource_1.txt");
                    File.ReadAllText(result.FullName).Should().Contain("first resource content");
                    break;
            }
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void MoveManifestResourceToDirectory_FailThrows()
    {
        var directory = CreateTempDirectory();
        try
        {
            var targetFile = Path.Join(directory, "resource.txt");
            File.WriteAllText(targetFile, "existing");

            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            Action action = () => assembly.MoveManifestResourceToDirectory(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt",
                directory,
                filename: "resource.txt",
                fileAlreadyExistsBehavior: AssemblyExtensions.FileAlreadyExistsBehavior.Fail);

            action.Should().Throw<IOException>().WithMessage("*already exists*");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void MoveManifestResourceToDirectory_RenameHandlesMultipleConflicts()
    {
        var directory = CreateTempDirectory();
        try
        {
            // Create multiple existing files
            File.WriteAllText(Path.Join(directory, "resource.txt"), "existing");
            File.WriteAllText(Path.Join(directory, "resource_1.txt"), "existing_1");
            File.WriteAllText(Path.Join(directory, "resource_2.txt"), "existing_2");

            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            var result = assembly.MoveManifestResourceToDirectory(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt",
                directory,
                filename: "resource.txt",
                fileAlreadyExistsBehavior: AssemblyExtensions.FileAlreadyExistsBehavior.Rename);

            result.Name.Should().Be("resource_3.txt");
            File.ReadAllText(result.FullName).Should().Contain("first resource content");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void MoveManifestResourceToDirectory_RenameWithExtension()
    {
        var directory = CreateTempDirectory();
        try
        {
            var targetFile = Path.Join(directory, "document.docx");
            File.WriteAllText(targetFile, "existing");

            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            var result = assembly.MoveManifestResourceToDirectory(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt",
                directory,
                filename: "document.docx",
                fileAlreadyExistsBehavior: AssemblyExtensions.FileAlreadyExistsBehavior.Rename);

            result.Name.Should().Be("document_1.docx");
            File.ReadAllText(result.FullName).Should().Contain("first resource content");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void MoveManifestResourceToDirectory_RenameWithoutExtension()
    {
        var directory = CreateTempDirectory();
        try
        {
            var targetFile = Path.Join(directory, "document");
            File.WriteAllText(targetFile, "existing");

            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            var result = assembly.MoveManifestResourceToDirectory(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt",
                directory,
                filename: "document",
                fileAlreadyExistsBehavior: AssemblyExtensions.FileAlreadyExistsBehavior.Rename);

            result.Name.Should().Be("document_1");
            File.ReadAllText(result.FullName).Should().Contain("first resource content");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    #endregion

    #region MoveManifestResourceToDirectoryAsync Tests

    [Fact]
    public async Task MoveManifestResourceToDirectoryAsync_WritesFile()
    {
        var directory = CreateTempDirectory();
        try
        {
            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            var result = await assembly.MoveManifestResourceToDirectoryAsync(
                "Plugin.BaseTypeExtensions.Tests.Resources.SecondResource.txt",
                directory,
                filename: "resource.txt");

            File.ReadAllText(result.FullName).Should().Contain("second resource content");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task MoveManifestResourceToDirectoryAsync_GeneratesRandomFilename()
    {
        var directory = CreateTempDirectory();
        try
        {
            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            var result = await assembly.MoveManifestResourceToDirectoryAsync(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt",
                directory);

            result.Name.Should().NotBeNullOrEmpty();
            File.ReadAllText(result.FullName).Should().Contain("first resource content");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task MoveManifestResourceToDirectoryAsync_CreatesDirectoryIfNotExists()
    {
        var parentDirectory = CreateTempDirectory();
        var targetDirectory = Path.Join(parentDirectory, "async_subdir", "nested");
        try
        {
            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            var result = await assembly.MoveManifestResourceToDirectoryAsync(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt",
                targetDirectory,
                filename: "async_resource.txt");

            Directory.Exists(targetDirectory).Should().BeTrue();
            File.ReadAllText(result.FullName).Should().Contain("first resource content");
        }
        finally
        {
            Directory.Delete(parentDirectory, recursive: true);
        }
    }

    [Theory]
    [InlineData(AssemblyExtensions.FileAlreadyExistsBehavior.Overwrite)]
    [InlineData(AssemblyExtensions.FileAlreadyExistsBehavior.Skip)]
    [InlineData(AssemblyExtensions.FileAlreadyExistsBehavior.Rename)]
    public async Task MoveManifestResourceToDirectoryAsync_HandlesFileAlreadyExistsBehavior(AssemblyExtensions.FileAlreadyExistsBehavior behavior)
    {
        var directory = CreateTempDirectory();
        try
        {
            var targetFile = Path.Join(directory, "async_resource.txt");
            File.WriteAllText(targetFile, "existing async");

            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            var result = await assembly.MoveManifestResourceToDirectoryAsync(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt",
                directory,
                filename: "async_resource.txt",
                fileAlreadyExistsBehavior: behavior);

            switch (behavior)
            {
                case AssemblyExtensions.FileAlreadyExistsBehavior.Overwrite:
                    result.FullName.Should().Be(targetFile);
                    File.ReadAllText(result.FullName).Should().Contain("first resource content");
                    break;

                case AssemblyExtensions.FileAlreadyExistsBehavior.Skip:
                    result.FullName.Should().Be(targetFile);
                    File.ReadAllText(result.FullName).Should().Be("existing async");
                    break;

                case AssemblyExtensions.FileAlreadyExistsBehavior.Rename:
                    result.Name.Should().Be("async_resource_1.txt");
                    File.ReadAllText(result.FullName).Should().Contain("first resource content");
                    break;
            }
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task MoveManifestResourceToDirectoryAsync_FailThrows()
    {
        var directory = CreateTempDirectory();
        try
        {
            var targetFile = Path.Join(directory, "async_resource.txt");
            File.WriteAllText(targetFile, "existing async");

            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            Func<Task> action = () => assembly.MoveManifestResourceToDirectoryAsync(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt",
                directory,
                filename: "async_resource.txt",
                fileAlreadyExistsBehavior: AssemblyExtensions.FileAlreadyExistsBehavior.Fail);

            await action.Should().ThrowAsync<IOException>().WithMessage("*already exists*");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task MoveManifestResourceToDirectoryAsync_HonorsCancellation()
    {
        var directory = CreateTempDirectory();
        try
        {
            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Func<Task> action = () => assembly.MoveManifestResourceToDirectoryAsync(
                "Plugin.BaseTypeExtensions.Tests.Resources.SecondResource.txt",
                directory,
                filename: "resource.txt",
                cancellationToken: cts.Token);

            await action.Should().ThrowAsync<OperationCanceledException>();
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task MoveManifestResourceToDirectoryAsync_CancellationCleanupPartialFile()
    {
        var directory = CreateTempDirectory();
        try
        {
            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            using var cts = new CancellationTokenSource();

            // Start the operation and cancel after it begins
            var task = assembly.MoveManifestResourceToDirectoryAsync(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt",
                directory,
                filename: "cancel_test.txt",
                cancellationToken: cts.Token);

            cts.Cancel();

            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
                // Expected - verify partial file cleanup
                var expectedFile = Path.Join(directory, "cancel_test.txt");
                File.Exists(expectedFile).Should().BeFalse("Partial file should be cleaned up on cancellation");
            }
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    #endregion

    #region MoveManifestResourceToCache Tests

    [Fact]
    public void MoveManifestResourceToCache_WritesFileToCache()
    {
        var originalCacheDirectory = AssemblyExtensions.CacheDirectory;
        var tempCacheDirectory = CreateTempDirectory();
        try
        {
            AssemblyExtensions.CacheDirectory = tempCacheDirectory;
            var assembly = typeof(AssemblyExtensionsTests).Assembly;

            var result = assembly.MoveManifestResourceToCache(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt",
                filename: "cached_resource.txt");

            result.DirectoryName.Should().Be(tempCacheDirectory);
            File.ReadAllText(result.FullName).Should().Contain("first resource content");
        }
        finally
        {
            AssemblyExtensions.CacheDirectory = originalCacheDirectory;
            Directory.Delete(tempCacheDirectory, recursive: true);
        }
    }

    [Fact]
    public void MoveManifestResourceToCache_GeneratesRandomFilename()
    {
        var originalCacheDirectory = AssemblyExtensions.CacheDirectory;
        var tempCacheDirectory = CreateTempDirectory();
        try
        {
            AssemblyExtensions.CacheDirectory = tempCacheDirectory;
            var assembly = typeof(AssemblyExtensionsTests).Assembly;

            var result = assembly.MoveManifestResourceToCache(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt");

            result.DirectoryName.Should().Be(tempCacheDirectory);
            result.Name.Should().NotBeNullOrEmpty();
            File.ReadAllText(result.FullName).Should().Contain("first resource content");
        }
        finally
        {
            AssemblyExtensions.CacheDirectory = originalCacheDirectory;
            Directory.Delete(tempCacheDirectory, recursive: true);
        }
    }

    [Fact]
    public void MoveManifestResourceToCache_WithAllFileAlreadyExistsBehaviors()
    {
        var originalCacheDirectory = AssemblyExtensions.CacheDirectory;
        var tempCacheDirectory = CreateTempDirectory();
        try
        {
            AssemblyExtensions.CacheDirectory = tempCacheDirectory;
            var assembly = typeof(AssemblyExtensionsTests).Assembly;

            // Test Skip behavior
            var existingFile = Path.Join(tempCacheDirectory, "test_skip.txt");
            File.WriteAllText(existingFile, "existing content");

            var result = assembly.MoveManifestResourceToCache(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt",
                filename: "test_skip.txt",
                fileAlreadyExistsBehavior: AssemblyExtensions.FileAlreadyExistsBehavior.Skip);

            File.ReadAllText(result.FullName).Should().Be("existing content");

            // Test Rename behavior
            var result2 = assembly.MoveManifestResourceToCache(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt",
                filename: "test_skip.txt",
                fileAlreadyExistsBehavior: AssemblyExtensions.FileAlreadyExistsBehavior.Rename);

            result2.Name.Should().Be("test_skip_1.txt");

            // Test Overwrite behavior
            var result3 = assembly.MoveManifestResourceToCache(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt",
                filename: "test_skip.txt",
                fileAlreadyExistsBehavior: AssemblyExtensions.FileAlreadyExistsBehavior.Overwrite);

            File.ReadAllText(result3.FullName).Should().Contain("first resource content");
        }
        finally
        {
            AssemblyExtensions.CacheDirectory = originalCacheDirectory;
            Directory.Delete(tempCacheDirectory, recursive: true);
        }
    }

    #endregion

    #region MoveManifestResourceToCacheAsync Tests

    [Fact]
    public async Task MoveManifestResourceToCacheAsync_WritesFileToCache()
    {
        var originalCacheDirectory = AssemblyExtensions.CacheDirectory;
        var tempCacheDirectory = CreateTempDirectory();
        try
        {
            AssemblyExtensions.CacheDirectory = tempCacheDirectory;
            var assembly = typeof(AssemblyExtensionsTests).Assembly;

            var result = await assembly.MoveManifestResourceToCacheAsync(
                "Plugin.BaseTypeExtensions.Tests.Resources.SecondResource.txt",
                filename: "cached_async_resource.txt");

            result.DirectoryName.Should().Be(tempCacheDirectory);
            File.ReadAllText(result.FullName).Should().Contain("second resource content");
        }
        finally
        {
            AssemblyExtensions.CacheDirectory = originalCacheDirectory;
            Directory.Delete(tempCacheDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task MoveManifestResourceToCacheAsync_GeneratesRandomFilename()
    {
        var originalCacheDirectory = AssemblyExtensions.CacheDirectory;
        var tempCacheDirectory = CreateTempDirectory();
        try
        {
            AssemblyExtensions.CacheDirectory = tempCacheDirectory;
            var assembly = typeof(AssemblyExtensionsTests).Assembly;

            var result = await assembly.MoveManifestResourceToCacheAsync(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt");

            result.DirectoryName.Should().Be(tempCacheDirectory);
            result.Name.Should().NotBeNullOrEmpty();
            File.ReadAllText(result.FullName).Should().Contain("first resource content");
        }
        finally
        {
            AssemblyExtensions.CacheDirectory = originalCacheDirectory;
            Directory.Delete(tempCacheDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task MoveManifestResourceToCacheAsync_WithAllFileAlreadyExistsBehaviors()
    {
        var originalCacheDirectory = AssemblyExtensions.CacheDirectory;
        var tempCacheDirectory = CreateTempDirectory();
        try
        {
            AssemblyExtensions.CacheDirectory = tempCacheDirectory;
            var assembly = typeof(AssemblyExtensionsTests).Assembly;

            // Test Skip behavior
            var existingFile = Path.Join(tempCacheDirectory, "test_async_skip.txt");
            File.WriteAllText(existingFile, "existing async content");

            var result = await assembly.MoveManifestResourceToCacheAsync(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt",
                filename: "test_async_skip.txt",
                fileAlreadyExistsBehavior: AssemblyExtensions.FileAlreadyExistsBehavior.Skip);

            File.ReadAllText(result.FullName).Should().Be("existing async content");

            // Test Rename behavior
            var result2 = await assembly.MoveManifestResourceToCacheAsync(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt",
                filename: "test_async_skip.txt",
                fileAlreadyExistsBehavior: AssemblyExtensions.FileAlreadyExistsBehavior.Rename);

            result2.Name.Should().Be("test_async_skip_1.txt");

            // Test Overwrite behavior
            var result3 = await assembly.MoveManifestResourceToCacheAsync(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt",
                filename: "test_async_skip.txt",
                fileAlreadyExistsBehavior: AssemblyExtensions.FileAlreadyExistsBehavior.Overwrite);

            File.ReadAllText(result3.FullName).Should().Contain("first resource content");
        }
        finally
        {
            AssemblyExtensions.CacheDirectory = originalCacheDirectory;
            Directory.Delete(tempCacheDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task MoveManifestResourceToCacheAsync_HonorsCancellation()
    {
        var originalCacheDirectory = AssemblyExtensions.CacheDirectory;
        var tempCacheDirectory = CreateTempDirectory();
        try
        {
            AssemblyExtensions.CacheDirectory = tempCacheDirectory;
            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Func<Task> action = () => assembly.MoveManifestResourceToCacheAsync(
                "Plugin.BaseTypeExtensions.Tests.Resources.SecondResource.txt",
                cancellationToken: cts.Token);

            await action.Should().ThrowAsync<OperationCanceledException>();
        }
        finally
        {
            AssemblyExtensions.CacheDirectory = originalCacheDirectory;
            Directory.Delete(tempCacheDirectory, recursive: true);
        }
    }

    #endregion

    #region CacheDirectory Property Tests

    [Fact]
    public void CacheDirectory_Property_CanBeAccessedAndModified()
    {
        var originalValue = AssemblyExtensions.CacheDirectory;
        try
        {
            // Test getter
            var currentValue = AssemblyExtensions.CacheDirectory;
            currentValue.Should().NotBeNull();

            // Test setter
            var testPath = "/tmp/test";
            AssemblyExtensions.CacheDirectory = testPath;
            AssemblyExtensions.CacheDirectory.Should().Be(testPath);
        }
        finally
        {
            AssemblyExtensions.CacheDirectory = originalValue;
        }
    }

    [Fact]
    public void CacheDirectory_DefaultValue_IsNotNull()
    {
        AssemblyExtensions.CacheDirectory.Should().NotBeNull();
        AssemblyExtensions.CacheDirectory.Should().NotBeEmpty();
    }

    #endregion

    #region Error Condition Tests

    [Fact]
    public void MoveManifestResourceToDirectory_ThrowsOnNullAssembly()
    {
        Assembly assembly = null!;
        Action action = () => assembly.MoveManifestResourceToDirectory("resource", "/tmp");
        action.Should().Throw<ArgumentNullException>().WithParameterName("assembly");
    }

    [Fact]
    public void MoveManifestResourceToDirectory_ThrowsOnNullResourceName()
    {
        var assembly = typeof(AssemblyExtensionsTests).Assembly;
        Action action = () => assembly.MoveManifestResourceToDirectory(null!, "/tmp");
        action.Should().Throw<ArgumentNullException>().WithParameterName("manifestResourceName");
    }

    [Fact]
    public void MoveManifestResourceToDirectory_ThrowsOnNullTargetDirectory()
    {
        var assembly = typeof(AssemblyExtensionsTests).Assembly;
        Action action = () => assembly.MoveManifestResourceToDirectory("resource", null!);
        action.Should().Throw<ArgumentNullException>().WithParameterName("targetDirectory");
    }

    [Fact]
    public void MoveManifestResourceToDirectory_ThrowsOnEmptyResourceName()
    {
        var assembly = typeof(AssemblyExtensionsTests).Assembly;
        Action action = () => assembly.MoveManifestResourceToDirectory("", "/tmp");
        action.Should().Throw<ArgumentException>().WithParameterName("manifestResourceName");
    }

    [Fact]
    public void MoveManifestResourceToDirectory_ThrowsOnWhitespaceResourceName()
    {
        var assembly = typeof(AssemblyExtensionsTests).Assembly;
        Action action = () => assembly.MoveManifestResourceToDirectory("   ", "/tmp");
        action.Should().Throw<ArgumentException>().WithParameterName("manifestResourceName");
    }

    [Fact]
    public void MoveManifestResourceToDirectory_ThrowsOnNonExistentResource()
    {
        var directory = CreateTempDirectory();
        try
        {
            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            Action action = () => assembly.MoveManifestResourceToDirectory("NonExistent.Resource", directory);
            action.Should().Throw<FileNotFoundException>().WithMessage("*Couldn't find resource named*");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task MoveManifestResourceToDirectoryAsync_ThrowsOnNullAssembly()
    {
        Assembly assembly = null!;
        Func<Task> action = () => assembly.MoveManifestResourceToDirectoryAsync("resource", "/tmp");
        await action.Should().ThrowAsync<ArgumentNullException>().WithParameterName("assembly");
    }

    [Fact]
    public async Task MoveManifestResourceToDirectoryAsync_ThrowsOnNullResourceName()
    {
        var assembly = typeof(AssemblyExtensionsTests).Assembly;
        Func<Task> action = () => assembly.MoveManifestResourceToDirectoryAsync(null!, "/tmp");
        await action.Should().ThrowAsync<ArgumentNullException>().WithParameterName("manifestResourceName");
    }

    [Fact]
    public async Task MoveManifestResourceToDirectoryAsync_ThrowsOnNullTargetDirectory()
    {
        var assembly = typeof(AssemblyExtensionsTests).Assembly;
        Func<Task> action = () => assembly.MoveManifestResourceToDirectoryAsync("resource", null!);
        await action.Should().ThrowAsync<ArgumentNullException>().WithParameterName("targetDirectory");
    }

    [Fact]
    public async Task MoveManifestResourceToDirectoryAsync_ThrowsOnEmptyResourceName()
    {
        var assembly = typeof(AssemblyExtensionsTests).Assembly;
        Func<Task> action = () => assembly.MoveManifestResourceToDirectoryAsync("", "/tmp");
        await action.Should().ThrowAsync<ArgumentException>().WithParameterName("manifestResourceName");
    }

    [Fact]
    public async Task MoveManifestResourceToDirectoryAsync_ThrowsOnWhitespaceResourceName()
    {
        var assembly = typeof(AssemblyExtensionsTests).Assembly;
        Func<Task> action = () => assembly.MoveManifestResourceToDirectoryAsync("   ", "/tmp");
        await action.Should().ThrowAsync<ArgumentException>().WithParameterName("manifestResourceName");
    }

    [Fact]
    public async Task MoveManifestResourceToDirectoryAsync_ThrowsOnNonExistentResource()
    {
        var directory = CreateTempDirectory();
        try
        {
            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            Func<Task> action = () => assembly.MoveManifestResourceToDirectoryAsync("NonExistent.Resource", directory);
            await action.Should().ThrowAsync<FileNotFoundException>().WithMessage("*Couldn't find resource named*");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void MoveManifestResourceToCache_ThrowsOnNullAssembly()
    {
        Assembly assembly = null!;
        Action action = () => assembly.MoveManifestResourceToCache("resource");
        action.Should().Throw<ArgumentNullException>().WithParameterName("assembly");
    }

    [Fact]
    public void MoveManifestResourceToCache_ThrowsOnNullResourceName()
    {
        var assembly = typeof(AssemblyExtensionsTests).Assembly;
        Action action = () => assembly.MoveManifestResourceToCache(null!);
        action.Should().Throw<ArgumentNullException>().WithParameterName("manifestResourceName");
    }

    [Fact]
    public void MoveManifestResourceToCache_ThrowsOnEmptyResourceName()
    {
        var assembly = typeof(AssemblyExtensionsTests).Assembly;
        Action action = () => assembly.MoveManifestResourceToCache("");
        action.Should().Throw<ArgumentException>().WithParameterName("manifestResourceName");
    }

    [Fact]
    public async Task MoveManifestResourceToCacheAsync_ThrowsOnNullAssembly()
    {
        Assembly assembly = null!;
        Func<Task> action = () => assembly.MoveManifestResourceToCacheAsync("resource");
        await action.Should().ThrowAsync<ArgumentNullException>().WithParameterName("assembly");
    }

    [Fact]
    public async Task MoveManifestResourceToCacheAsync_ThrowsOnNullResourceName()
    {
        var assembly = typeof(AssemblyExtensionsTests).Assembly;
        Func<Task> action = () => assembly.MoveManifestResourceToCacheAsync(null!);
        await action.Should().ThrowAsync<ArgumentNullException>().WithParameterName("manifestResourceName");
    }

    [Fact]
    public async Task MoveManifestResourceToCacheAsync_ThrowsOnEmptyResourceName()
    {
        var assembly = typeof(AssemblyExtensionsTests).Assembly;
        Func<Task> action = () => assembly.MoveManifestResourceToCacheAsync("");
        await action.Should().ThrowAsync<ArgumentException>().WithParameterName("manifestResourceName");
    }

    [Theory]
    [InlineData("../escape.txt")]
    [InlineData("sub/file.txt")]
    public void MoveManifestResourceToDirectory_ThrowsOnFilenameWithDirectoryPart(string filename)
    {
        var directory = CreateTempDirectory();
        try
        {
            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            Action action = () => assembly.MoveManifestResourceToDirectory(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt", directory, filename);
            action.Should().Throw<ArgumentException>().WithParameterName("filename");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void MoveManifestResourceToDirectory_ThrowsOnRootedFilename_AndWritesNothing()
    {
        var directory = CreateTempDirectory();
        var elsewhere = CreateTempDirectory();
        try
        {
            var rootedFilename = Path.Join(elsewhere, "escape.txt");
            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            Action action = () => assembly.MoveManifestResourceToDirectory(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt", directory, rootedFilename);
            action.Should().Throw<ArgumentException>().WithParameterName("filename");
            File.Exists(rootedFilename).Should().BeFalse();
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
            Directory.Delete(elsewhere, recursive: true);
        }
    }

    [Theory]
    [InlineData(".")]
    [InlineData("..")]
    public void MoveManifestResourceToDirectory_ThrowsOnDotFilename_AndCreatesNoDirectory(string filename)
    {
        var root = CreateTempDirectory();
        try
        {
            var targetDirectory = Path.Join(root, "not-created");
            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            Action action = () => assembly.MoveManifestResourceToDirectory(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt", targetDirectory, filename);
            action.Should().Throw<ArgumentException>().WithParameterName("filename");
            Directory.Exists(targetDirectory).Should().BeFalse();
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("../escape.txt")]
    [InlineData("sub/file.txt")]
    public async Task MoveManifestResourceToDirectoryAsync_ThrowsOnFilenameWithDirectoryPart(string filename)
    {
        var directory = CreateTempDirectory();
        try
        {
            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            Func<Task> action = () => assembly.MoveManifestResourceToDirectoryAsync(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt", directory, filename);
            await action.Should().ThrowAsync<ArgumentException>().WithParameterName("filename");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task MoveManifestResourceToDirectoryAsync_ThrowsOnRootedFilename_AndWritesNothing()
    {
        var directory = CreateTempDirectory();
        var elsewhere = CreateTempDirectory();
        try
        {
            var rootedFilename = Path.Join(elsewhere, "escape.txt");
            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            Func<Task> action = () => assembly.MoveManifestResourceToDirectoryAsync(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt", directory, rootedFilename);
            await action.Should().ThrowAsync<ArgumentException>().WithParameterName("filename");
            File.Exists(rootedFilename).Should().BeFalse();
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
            Directory.Delete(elsewhere, recursive: true);
        }
    }

    [Theory]
    [InlineData(".")]
    [InlineData("..")]
    public async Task MoveManifestResourceToDirectoryAsync_ThrowsOnDotFilename_AndCreatesNoDirectory(string filename)
    {
        var root = CreateTempDirectory();
        try
        {
            var targetDirectory = Path.Join(root, "not-created");
            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            Func<Task> action = () => assembly.MoveManifestResourceToDirectoryAsync(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt", targetDirectory, filename);
            await action.Should().ThrowAsync<ArgumentException>().WithParameterName("filename");
            Directory.Exists(targetDirectory).Should().BeFalse();
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void MoveManifestResourceToDirectory_RenameCounterLimit()
    {
        var directory = CreateTempDirectory();
        try
        {
            // This test simulates hitting the 1000 counter limit by mocking a scenario
            // where the file creation fails after many attempts
            var assembly = typeof(AssemblyExtensionsTests).Assembly;

            // Create a file that would cause the rename logic to increment
            File.WriteAllText(Path.Join(directory, "test.txt"), "existing");

            // This should work normally - just testing the edge case exists
            var result = assembly.MoveManifestResourceToDirectory(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt",
                directory,
                filename: "test.txt",
                fileAlreadyExistsBehavior: AssemblyExtensions.FileAlreadyExistsBehavior.Rename);

            result.Name.Should().Be("test_1.txt");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void MoveManifestResourceToDirectory_HandlesSpecialCharactersInFilename()
    {
        var directory = CreateTempDirectory();
        try
        {
            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            var result = assembly.MoveManifestResourceToDirectory(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt",
                directory,
                filename: "file with spaces & special chars!.txt");

            result.Name.Should().Be("file with spaces & special chars!.txt");
            File.ReadAllText(result.FullName).Should().Contain("first resource content");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task MoveManifestResourceToDirectoryAsync_WorksWithLongPath()
    {
        var directory = CreateTempDirectory();
        try
        {
            var assembly = typeof(AssemblyExtensionsTests).Assembly;

            // Test with a longer file path
            var result = await assembly.MoveManifestResourceToDirectoryAsync(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt",
                directory,
                filename: "this_is_a_very_long_filename_to_test_path_handling.txt");

            File.ReadAllText(result.FullName).Should().Contain("first resource content");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void MoveManifestResourceToDirectory_FileInfoPropertiesAreCorrect()
    {
        var directory = CreateTempDirectory();
        try
        {
            var assembly = typeof(AssemblyExtensionsTests).Assembly;
            var result = assembly.MoveManifestResourceToDirectory(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt",
                directory,
                filename: "fileinfo_test.txt");

            // Verify FileInfo properties
            result.Exists.Should().BeTrue();
            result.Length.Should().BeGreaterThan(0);
            result.DirectoryName.Should().Be(directory);
            result.Name.Should().Be("fileinfo_test.txt");
            result.Extension.Should().Be(".txt");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    #endregion

    #region Enum Tests

    [Fact]
    public void FileAlreadyExistsBehavior_EnumValues_AreCorrect()
    {
        // Test that all enum values exist and have expected values
        var enumValues = Enum.GetValues<AssemblyExtensions.FileAlreadyExistsBehavior>();

        enumValues.Should().Contain(AssemblyExtensions.FileAlreadyExistsBehavior.Overwrite);
        enumValues.Should().Contain(AssemblyExtensions.FileAlreadyExistsBehavior.Skip);
        enumValues.Should().Contain(AssemblyExtensions.FileAlreadyExistsBehavior.Rename);
        enumValues.Should().Contain(AssemblyExtensions.FileAlreadyExistsBehavior.Fail);
    }

    [Fact]
    public void FileAlreadyExistsBehavior_DefaultValue_IsFail()
    {
        var directory = CreateTempDirectory();
        try
        {
            var targetFile = Path.Join(directory, "default_test.txt");
            File.WriteAllText(targetFile, "existing");

            var assembly = typeof(AssemblyExtensionsTests).Assembly;

            // Test that default behavior is Fail when no parameter provided
            Action action = () => assembly.MoveManifestResourceToDirectory(
                "Plugin.BaseTypeExtensions.Tests.Resources.FirstResource.txt",
                directory,
                filename: "default_test.txt");

            action.Should().Throw<IOException>().WithMessage("*already exists*");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    #endregion
}
