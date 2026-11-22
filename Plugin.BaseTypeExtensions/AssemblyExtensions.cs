using System.Reflection;

namespace Plugin.BaseTypeExtensions;

/// <summary>
/// Provides extension methods for <see cref="Assembly"/> operations, including manifest resource extraction.
/// </summary>
public static class AssemblyExtensions
{
    /// <summary>
    /// Defines behavior when a target file already exists during resource extraction.
    /// </summary>
    public enum FileAlreadyExistsBehavior
    {
        /// <summary>
        /// Overwrite the existing file with the new content.
        /// </summary>
        Overwrite,

        /// <summary>
        /// Skip extraction and return the existing file.
        /// </summary>
        Skip,

        /// <summary>
        /// Rename the new file to avoid conflict by appending a counter.
        /// </summary>
        Rename,

        /// <summary>
        /// Throw an exception if the file already exists.
        /// </summary>
        Fail
    }

    /// <summary>
    /// Extracts a manifest resource from an assembly and saves it to a specified directory.
    /// </summary>
    /// <param name="assembly">The assembly containing the manifest resource.</param>
    /// <param name="manifestResourceName">The name of the manifest resource to extract.</param>
    /// <param name="targetDirectory">The directory where the resource should be saved.</param>
    /// <param name="filename">The filename for the extracted resource. If null, a random filename will be generated.</param>
    /// <param name="fileAlreadyExistsBehavior">Behavior when the target file already exists.</param>
    /// <returns>A <see cref="FileInfo"/> object representing the extracted file.</returns>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    /// <exception cref="ArgumentException">Thrown when manifestResourceName is empty or whitespace, or when filename is not a plain file name (it has a directory part).</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the target directory cannot be created or accessed.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the manifest resource cannot be found.</exception>
    /// <exception cref="IOException">Thrown when file operations fail or when fileAlreadyExistsBehavior is Fail and file exists.</exception>
    public static FileInfo MoveManifestResourceToDirectory(this Assembly assembly, string manifestResourceName, string targetDirectory, string? filename = null, FileAlreadyExistsBehavior fileAlreadyExistsBehavior = FileAlreadyExistsBehavior.Fail)
    {
        // Validate input parameters
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(manifestResourceName);
        ArgumentNullException.ThrowIfNull(targetDirectory);

        if (string.IsNullOrWhiteSpace(manifestResourceName))
        {
            throw new ArgumentException("Resource name cannot be empty or whitespace.", nameof(manifestResourceName));
        }

        // Generate filename if not provided
        filename ??= $"{DateTime.Now.Ticks}_{Path.GetRandomFileName()}";
        ThrowIfNotPlainFileName(filename);

        // Ensure target directory exists
        if (!Directory.Exists(targetDirectory))
        {
            try
            {
                Directory.CreateDirectory(targetDirectory);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or DirectoryNotFoundException or IOException)
            {
                throw new DirectoryNotFoundException($"Cannot create or access target directory '{targetDirectory}'. {ex.Message}", ex);
            }
        }

        var outputFilePath = Path.Join(targetDirectory, filename);

        // Handle existing file based on specified behavior
        if (File.Exists(outputFilePath))
        {
            switch (fileAlreadyExistsBehavior)
            {
                case FileAlreadyExistsBehavior.Overwrite:
                    // Continue with existing path - file will be overwritten
                    break;

                case FileAlreadyExistsBehavior.Skip:
                    // Return existing file without modification
                    return new FileInfo(outputFilePath);

                case FileAlreadyExistsBehavior.Rename:
                    // Generate unique filename
                    var counter = 1;
                    var originalFilename = filename;
                    var extension = Path.GetExtension(originalFilename);
                    var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalFilename);

                    do
                    {
                        filename = $"{nameWithoutExtension}_{counter}{extension}";
                        outputFilePath = Path.Join(targetDirectory, filename);
                        counter++;

                        // Prevent infinite loop in case of extreme edge cases
                        if (counter > 1000)
                        {
                            throw new IOException($"Unable to generate unique filename after {counter} attempts in target directory '{targetDirectory}'.");
                        }
                    } while (File.Exists(outputFilePath));
                    break;

                case FileAlreadyExistsBehavior.Fail:
                default:
                    throw new IOException($"File '{outputFilePath}' already exists and fileAlreadyExistsBehavior is set to Fail.");
            }
        }

        try
        {
            // COPY BYTES
            using var inputStream = assembly.GetResourceStream(manifestResourceName);
            using var outputStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write);

            inputStream.CopyTo(outputStream);
            outputStream.Flush();
        }
        catch (FileNotFoundException)
        {
            // Let FileNotFoundException pass through unchanged - this is the most appropriate exception
            // for when a resource cannot be found
            throw;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            // Clean up partial file if creation failed
            if (File.Exists(outputFilePath))
            {
                try
                {
                    File.Delete(outputFilePath);
                }
                catch (IOException)
                {
                    // Ignore cleanup errors
                }
            }
            throw new IOException($"Error writing resource '{manifestResourceName}' to file '{outputFilePath}'. {ex.Message}", ex);
        }

        // GENERATE OUTPUT
        return new FileInfo(outputFilePath);
    }



    /// <summary>
    /// Gets or sets the cache directory used for resource extraction.
    /// Overridden in the Maui project to use <c>FileSystem.CacheDirectory</c>.
    /// In unit tests, it uses <see cref="Path.GetTempPath()"/> to ensure compatibility.
    /// </summary>
    public static string CacheDirectory { get; set; } = Path.GetTempPath();

    /// <summary>
    /// Extracts a manifest resource from an assembly and saves it to a specified directory asynchronously.
    /// </summary>
    /// <param name="assembly">The assembly containing the manifest resource.</param>
    /// <param name="manifestResourceName">The name of the manifest resource to extract.</param>
    /// <param name="targetDirectory">The directory where the resource should be saved.</param>
    /// <param name="filename">The filename for the extracted resource. If null, a random filename will be generated.</param>
    /// <param name="fileAlreadyExistsBehavior">Behavior when the target file already exists.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="Task{FileInfo}"/> representing the asynchronous operation that returns a <see cref="FileInfo"/> object for the extracted file.</returns>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    /// <exception cref="ArgumentException">Thrown when manifestResourceName is empty or whitespace, or when filename is not a plain file name (it has a directory part).</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the target directory cannot be created or accessed.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the manifest resource cannot be found.</exception>
    /// <exception cref="IOException">Thrown when file operations fail or when fileAlreadyExistsBehavior is Fail and file exists.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
    public static async Task<FileInfo> MoveManifestResourceToDirectoryAsync(this Assembly assembly, string manifestResourceName, string targetDirectory, string? filename = null, FileAlreadyExistsBehavior fileAlreadyExistsBehavior = FileAlreadyExistsBehavior.Fail, CancellationToken cancellationToken = default)
    {
        // Validate input parameters
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(manifestResourceName);
        ArgumentNullException.ThrowIfNull(targetDirectory);

        if (string.IsNullOrWhiteSpace(manifestResourceName))
        {
            throw new ArgumentException("Resource name cannot be empty or whitespace.", nameof(manifestResourceName));
        }

        cancellationToken.ThrowIfCancellationRequested();

        // Generate filename if not provided
        filename ??= $"{DateTime.Now.Ticks}_{Path.GetRandomFileName()}";
        ThrowIfNotPlainFileName(filename);

        // Ensure target directory exists
        if (!Directory.Exists(targetDirectory))
        {
            try
            {
                Directory.CreateDirectory(targetDirectory);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or DirectoryNotFoundException or IOException)
            {
                throw new DirectoryNotFoundException($"Cannot create or access target directory '{targetDirectory}'. {ex.Message}", ex);
            }
        }

        var outputFilePath = Path.Join(targetDirectory, filename);

        // Handle existing file based on specified behavior
        if (File.Exists(outputFilePath))
        {
            switch (fileAlreadyExistsBehavior)
            {
                case FileAlreadyExistsBehavior.Overwrite:
                    // Continue with existing path - file will be overwritten
                    break;

                case FileAlreadyExistsBehavior.Skip:
                    // Return existing file without modification
                    return new FileInfo(outputFilePath);

                case FileAlreadyExistsBehavior.Rename:
                    // Generate unique filename
                    var counter = 1;
                    var originalFilename = filename;
                    var extension = Path.GetExtension(originalFilename);
                    var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalFilename);

                    do
                    {
                        filename = $"{nameWithoutExtension}_{counter}{extension}";
                        outputFilePath = Path.Join(targetDirectory, filename);
                        counter++;

                        // Prevent infinite loop in case of extreme edge cases
                        if (counter > 1000)
                        {
                            throw new IOException($"Unable to generate unique filename after {counter} attempts in target directory '{targetDirectory}'.");
                        }
                    } while (File.Exists(outputFilePath));
                    break;

                case FileAlreadyExistsBehavior.Fail:
                default:
                    throw new IOException($"File '{outputFilePath}' already exists and fileAlreadyExistsBehavior is set to Fail.");
            }
        }

        try
        {
            // COPY BYTES ASYNCHRONOUSLY
            using var inputStream = assembly.GetResourceStream(manifestResourceName);
            using var outputStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write);

            await inputStream.CopyToAsync(outputStream, cancellationToken).ConfigureAwait(false);
            await outputStream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (FileNotFoundException)
        {
            // Let FileNotFoundException pass through unchanged - this is the most appropriate exception
            // for when a resource cannot be found
            throw;
        }
        catch (OperationCanceledException)
        {
            // Clean up partial file if operation was cancelled
            if (File.Exists(outputFilePath))
            {
                try
                {
                    File.Delete(outputFilePath);
                }
                catch (IOException)
                {
                    // Ignore cleanup errors
                }
            }
            throw;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            // Clean up partial file if creation failed
            if (File.Exists(outputFilePath))
            {
                try
                {
                    File.Delete(outputFilePath);
                }
                catch (IOException)
                {
                    // Ignore cleanup errors
                }
            }
            throw new IOException($"Error writing resource '{manifestResourceName}' to file '{outputFilePath}'. {ex.Message}", ex);
        }

        // GENERATE OUTPUT
        return new FileInfo(outputFilePath);
    }

    /// <summary>
    /// Extracts a manifest resource from an assembly and saves it to the cache directory.
    /// </summary>
    /// <param name="assembly">The assembly containing the manifest resource.</param>
    /// <param name="manifestResourceName">The name of the manifest resource to extract.</param>
    /// <param name="filename">The filename for the extracted resource. If null, a random filename will be generated.</param>
    /// <param name="fileAlreadyExistsBehavior">Behavior when the target file already exists.</param>
    /// <returns>A <see cref="FileInfo"/> object representing the extracted file.</returns>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    /// <exception cref="ArgumentException">Thrown when manifestResourceName is empty or whitespace, or when filename is not a plain file name (it has a directory part).</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the cache directory cannot be created or accessed.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the manifest resource cannot be found.</exception>
    /// <exception cref="IOException">Thrown when file operations fail or when fileAlreadyExistsBehavior is Fail and file exists.</exception>
    public static FileInfo MoveManifestResourceToCache(this Assembly assembly, string manifestResourceName, string? filename = null, FileAlreadyExistsBehavior fileAlreadyExistsBehavior = FileAlreadyExistsBehavior.Fail)
    {
        return MoveManifestResourceToDirectory(assembly, manifestResourceName, CacheDirectory, filename, fileAlreadyExistsBehavior);
    }

    /// <summary>
    /// Extracts a manifest resource from an assembly and saves it to the cache directory asynchronously.
    /// </summary>
    /// <param name="assembly">The assembly containing the manifest resource.</param>
    /// <param name="manifestResourceName">The name of the manifest resource to extract.</param>
    /// <param name="filename">The filename for the extracted resource. If null, a random filename will be generated.</param>
    /// <param name="fileAlreadyExistsBehavior">Behavior when the target file already exists.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="Task{FileInfo}"/> representing the asynchronous operation that returns a <see cref="FileInfo"/> object for the extracted file.</returns>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    /// <exception cref="ArgumentException">Thrown when manifestResourceName is empty or whitespace, or when filename is not a plain file name (it has a directory part).</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the cache directory cannot be created or accessed.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the manifest resource cannot be found.</exception>
    /// <exception cref="IOException">Thrown when file operations fail or when fileAlreadyExistsBehavior is Fail and file exists.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
    public static async Task<FileInfo> MoveManifestResourceToCacheAsync(this Assembly assembly, string manifestResourceName, string? filename = null, FileAlreadyExistsBehavior fileAlreadyExistsBehavior = FileAlreadyExistsBehavior.Fail, CancellationToken cancellationToken = default)
    {
        return await MoveManifestResourceToDirectoryAsync(assembly, manifestResourceName, CacheDirectory, filename, fileAlreadyExistsBehavior, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Rejects names that would resolve outside the target directory: a rooted path (which
    /// <see cref="Path.Combine(string, string)"/> would let replace the directory) or one with a directory part such as <c>..</c>.
    /// </summary>
    private static void ThrowIfNotPlainFileName(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename) || Path.GetFileName(filename) != filename)
        {
            throw new ArgumentException($"Filename '{filename}' must be a plain file name without any directory part.", nameof(filename));
        }
    }
}
