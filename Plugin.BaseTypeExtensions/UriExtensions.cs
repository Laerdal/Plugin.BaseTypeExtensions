using System.Runtime.CompilerServices;
using System.Text;
using System.Web;

namespace Plugin.BaseTypeExtensions;

/// <summary>
/// Provides extension methods for <see cref="Uri"/> manipulation and utility functions.
/// </summary>
public static class UriExtensions
{
    #region Path Operations

    /// <summary>
    /// Appends a path segment to the URI.
    /// </summary>
    /// <param name="uri">The base URI.</param>
    /// <param name="path">The path segment to append.</param>
    /// <returns>A new URI with the appended path.</returns>
    /// <exception cref="ArgumentNullException">Thrown when uri or path is null.</exception>
    public static Uri AppendPath(this Uri uri, string path)
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(path);

        if (string.IsNullOrWhiteSpace(path))
        {
            return uri;
        }

        var trimmedPath = path.Trim('/');
        var baseUri = uri.ToString().TrimEnd('/');

        return new Uri($"{baseUri}/{trimmedPath}");
    }

    #endregion

    #region Query Operations

    /// <summary>
    /// Appends a query parameter to the URI.
    /// </summary>
    /// <param name="uri">The base URI.</param>
    /// <param name="key">The parameter key.</param>
    /// <param name="value">The parameter value.</param>
    /// <returns>A new URI with the appended query parameter.</returns>
    /// <exception cref="ArgumentNullException">Thrown when uri, key, or value is null.</exception>
    public static Uri AppendQuery(this Uri uri, string key, string value)
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        var uriBuilder = new UriBuilder(uri);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query[key] = value;
        uriBuilder.Query = query.ToString();

        return uriBuilder.Uri;
    }

    /// <summary>
    /// Appends multiple query parameters to the URI.
    /// </summary>
    /// <param name="uri">The base URI.</param>
    /// <param name="parameters">The parameters to append.</param>
    /// <returns>A new URI with the appended query parameters.</returns>
    /// <exception cref="ArgumentNullException">Thrown when uri or parameters is null.</exception>
    public static Uri AppendQuery(this Uri uri, Dictionary<string, string> parameters)
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(parameters);

        if (parameters.Count == 0)
        {
            return uri;
        }

        var uriBuilder = new UriBuilder(uri);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        foreach (var kvp in parameters)
        {
            query[kvp.Key] = kvp.Value;
        }

        uriBuilder.Query = query.ToString();
        return uriBuilder.Uri;
    }

    /// <summary>
    /// Removes all query parameters from the URI.
    /// </summary>
    /// <param name="uri">The URI to modify.</param>
    /// <returns>A new URI without query parameters.</returns>
    /// <exception cref="ArgumentNullException">Thrown when uri is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Uri RemoveQuery(this Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);

        var uriBuilder = new UriBuilder(uri)
        {
            Query = string.Empty
        };

        return uriBuilder.Uri;
    }

    /// <summary>
    /// Removes a specific query parameter from the URI.
    /// </summary>
    /// <param name="uri">The URI to modify.</param>
    /// <param name="key">The parameter key to remove.</param>
    /// <returns>A new URI without the specified query parameter.</returns>
    /// <exception cref="ArgumentNullException">Thrown when uri or key is null.</exception>
    public static Uri RemoveQuery(this Uri uri, string key)
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(key);

        var uriBuilder = new UriBuilder(uri);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query.Remove(key);
        uriBuilder.Query = query.ToString();

        return uriBuilder.Uri;
    }

    #endregion

    #region Query Parameter Access

    /// <summary>
    /// Gets all query parameters from the URI as a dictionary.
    /// </summary>
    /// <param name="uri">The URI to parse.</param>
    /// <returns>A dictionary containing all query parameters.</returns>
    /// <exception cref="ArgumentNullException">Thrown when uri is null.</exception>
    public static Dictionary<string, string> GetQueryParameters(this Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var query = HttpUtility.ParseQueryString(uri.Query);

        foreach (var key in query.AllKeys.OfType<string>())
        {
            result[key] = query[key] ?? string.Empty;
        }

        return result;
    }

    /// <summary>
    /// Gets a specific query parameter value from the URI.
    /// </summary>
    /// <param name="uri">The URI to parse.</param>
    /// <param name="key">The parameter key to retrieve.</param>
    /// <param name="defaultValue">The default value to return if the parameter is not found.</param>
    /// <returns>The parameter value or the default value if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when uri or key is null.</exception>
    public static string GetQueryParameter(this Uri uri, string key, string defaultValue = "")
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(key);

        var query = HttpUtility.ParseQueryString(uri.Query);
        return query[key] ?? defaultValue;
    }

    /// <summary>
    /// Determines whether the URI contains the specified query parameter.
    /// </summary>
    /// <param name="uri">The URI to check.</param>
    /// <param name="key">The parameter key to look for.</param>
    /// <returns>True if the parameter exists; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when uri or key is null.</exception>
    public static bool HasQueryParameter(this Uri uri, string key)
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(key);

        var query = HttpUtility.ParseQueryString(uri.Query);
        return query.AllKeys.Contains(key);
    }

    #endregion

    #region URI Component Modification

    /// <summary>
    /// Sets the scheme of the URI.
    /// </summary>
    /// <param name="uri">The URI to modify.</param>
    /// <param name="scheme">The new scheme (e.g., "https", "http").</param>
    /// <returns>A new URI with the specified scheme.</returns>
    /// <exception cref="ArgumentNullException">Thrown when uri or scheme is null.</exception>
    /// <exception cref="ArgumentException">Thrown when scheme is empty or whitespace.</exception>
    public static Uri SetScheme(this Uri uri, string scheme)
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(scheme);

        if (string.IsNullOrWhiteSpace(scheme))
        {
            throw new ArgumentException("Scheme cannot be empty or whitespace.", nameof(scheme));
        }

        var uriBuilder = new UriBuilder(uri)
        {
            Scheme = scheme
        };

        return uriBuilder.Uri;
    }

    /// <summary>
    /// Sets the port of the URI.
    /// </summary>
    /// <param name="uri">The URI to modify.</param>
    /// <param name="port">The new port number.</param>
    /// <returns>A new URI with the specified port.</returns>
    /// <exception cref="ArgumentNullException">Thrown when uri is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when port is not between 1 and 65535.</exception>
    public static Uri SetPort(this Uri uri, int port)
    {
        ArgumentNullException.ThrowIfNull(uri);

        if (port < 1 || port > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535.");
        }

        var uriBuilder = new UriBuilder(uri)
        {
            Port = port
        };

        return uriBuilder.Uri;
    }

    #endregion

    #region URI Validation and Information

    /// <summary>
    /// Determines whether the URI points to localhost.
    /// </summary>
    /// <param name="uri">The URI to check.</param>
    /// <returns>True if the URI is localhost; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when uri is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLocalhost(this Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);

        return uri.IsLoopback ||
               string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the URI uses a secure scheme (HTTPS).
    /// </summary>
    /// <param name="uri">The URI to check.</param>
    /// <returns>True if the URI uses HTTPS; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when uri is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSecure(this Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);

        return string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
