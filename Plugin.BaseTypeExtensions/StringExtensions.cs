using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Plugin.BaseTypeExtensions;

/// <summary>
/// Provides extension methods for string manipulation and utility functions.
/// </summary>
public static partial class StringExtensions
{
    // Common separators for string case conversion methods
    private static readonly char[] _caseSeparators = [' ', '_', '-'];

    /// <summary>
    /// Returns null if the string is null, empty, or contains only whitespace; otherwise returns the string as-is.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <returns>null if the string is a "dud" (null/empty/whitespace), otherwise the original string.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? NullIfEmptyOrWhiteSpace(this string? input)
    {
        return string.IsNullOrWhiteSpace(input) ? null : input;
    }

    /// <summary>
    /// Removes common special characters from a string, keeping only letters, digits, and spaces.
    /// </summary>
    /// <param name="input">The string to clean.</param>
    /// <returns>A string with special characters removed, or an empty string if input is null.</returns>
    public static string RemoveSpecialCharacters(this string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var result = new StringBuilder(input.Length);
        foreach (var c in input.Where(IsKeptByRemoveSpecialCharacters))
        {
            result.Append(c);
        }
        return result.ToString();
    }

    private static bool IsKeptByRemoveSpecialCharacters(char c) => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '-';

    #region String Validation & Checks

    /// <summary>
    /// Determines whether the string is a valid email address.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>true if the string is a valid email address; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidEmail(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        try
        {
            // Use built-in email validation via MailAddress
            var addr = new System.Net.Mail.MailAddress(input);
            return addr.Address == input;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// Determines whether the string is a valid URL.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>true if the string is a valid URL; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidUrl(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        return Uri.IsWellFormedUriString(input, UriKind.Absolute);
    }

    /// <summary>
    /// Determines whether the string represents a numeric value.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <param name="formatProvider"></param>
    /// <returns>true if the string is numeric; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNumeric(this string? input, IFormatProvider? formatProvider = null)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        return double.TryParse(input, NumberStyles.Any, formatProvider ?? CultureInfo.InvariantCulture, out _);
    }

    /// <summary>
    /// Determines whether the string represents an integer value.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>true if the string is an integer; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInteger(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        return int.TryParse(input, out _);
    }

    #endregion

    #region String Transformations

    /// <summary>
    /// Converts the string to title case (first letter of each word capitalized).
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The string in title case, or an empty string if input is null.</returns>
    public static string ToTitleCase(this string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

#pragma warning disable CA1308 // Avoid ToLowerInvariant - this is required by TextInfo.ToTitleCase
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLowerInvariant());
#pragma warning restore CA1308
    }

    /// <summary>
    /// Converts the string to camelCase.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The string in camelCase, or an empty string if input is null.</returns>
    public static string ToCamelCase(this string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var words = input.Split(_caseSeparators, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return string.Empty;
        }

        var result = new StringBuilder();
#pragma warning disable CA1308 // Avoid ToLowerInvariant - this is intentional for camelCase
        result.Append(words[0].ToLowerInvariant());
#pragma warning restore CA1308

        for (var i = 1; i < words.Length; i++)
        {
            result.Append(char.ToUpperInvariant(words[i][0]));
            if (words[i].Length > 1)
            {
#pragma warning disable CA1308 // Avoid ToLowerInvariant - this is intentional for camelCase
                result.Append(words[i][1..].ToLowerInvariant());
#pragma warning restore CA1308
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Converts the string to PascalCase.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The string in PascalCase, or an empty string if input is null.</returns>
    public static string ToPascalCase(this string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var words = input.Split(_caseSeparators, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return string.Empty;
        }

        var result = new StringBuilder();
        foreach (var word in words)
        {
            result.Append(char.ToUpperInvariant(word[0]));
            if (word.Length > 1)
            {
#pragma warning disable CA1308 // Avoid ToLowerInvariant - this is intentional for PascalCase
                result.Append(word[1..].ToLowerInvariant());
#pragma warning restore CA1308
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Converts the string to kebab-case.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The string in kebab-case, or an empty string if input is null.</returns>
    public static string ToKebabCase(this string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var words = input.Split(_caseSeparators, StringSplitOptions.RemoveEmptyEntries);
#pragma warning disable CA1308 // Avoid ToLowerInvariant - this is intentional for kebab-case
        return string.Join("-", words.Select(w => w.ToLowerInvariant()));
#pragma warning restore CA1308
    }

    /// <summary>
    /// Converts the string to snake_case.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The string in snake_case, or an empty string if input is null.</returns>
    public static string ToSnakeCase(this string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var words = input.Split(_caseSeparators, StringSplitOptions.RemoveEmptyEntries);
#pragma warning disable CA1308 // Avoid ToLowerInvariant - this is intentional for snake_case
        return string.Join("_", words.Select(w => w.ToLowerInvariant()));
#pragma warning restore CA1308
    }

    #endregion

    #region String Truncation & Padding

    /// <summary>
    /// Truncates the string to the specified maximum length, optionally adding a suffix.
    /// </summary>
    /// <param name="input">The string to truncate.</param>
    /// <param name="maxLength">The maximum length of the result string.</param>
    /// <param name="suffix">The suffix to append if truncation occurs.</param>
    /// <returns>The truncated string, or an empty string if input is null.</returns>
    public static string Truncate(this string? input, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(input) || maxLength <= 0)
        {
            return string.Empty;
        }

        if (suffix is null)
        {
            suffix = string.Empty;
        }

        if (input.Length <= maxLength)
        {
            return input;
        }

        if (maxLength <= suffix.Length)
        {
            return suffix[..maxLength];
        }

        return input[..(maxLength - suffix.Length)] + suffix;
    }

    /// <summary>
    /// Truncates the string with ellipsis if it exceeds the maximum length.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <param name="maxLength">The maximum length before ellipsis.</param>
    /// <returns>The string with ellipsis if too long, or an empty string if input is null.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string EllipsisIfTooLong(this string? input, int maxLength)
    {
        return input.Truncate(maxLength, "...");
    }

    /// <summary>
    /// Pads the string on both sides to center it within the specified total width.
    /// </summary>
    /// <param name="input">The string to pad.</param>
    /// <param name="totalWidth">The total width of the result string.</param>
    /// <param name="paddingChar">The character to use for padding.</param>
    /// <returns>The centered string, or an empty string if input is null.</returns>
    public static string PadBoth(this string? input, int totalWidth, char paddingChar = ' ')
    {
        if (string.IsNullOrEmpty(input))
        {
            return new string(paddingChar, Math.Max(0, totalWidth));
        }

        if (input.Length >= totalWidth)
        {
            return input;
        }

        var padding = totalWidth - input.Length;
        var leftPadding = padding / 2;
        var rightPadding = padding - leftPadding;

        return new string(paddingChar, leftPadding) + input + new string(paddingChar, rightPadding);
    }

    #endregion

    #region String Safety & Encoding

    /// <summary>
    /// HTML-encodes the string for safe display in HTML.
    /// </summary>
    /// <param name="input">The string to encode.</param>
    /// <returns>The HTML-encoded string, or an empty string if input is null.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string EscapeHtml(this string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        return HttpUtility.HtmlEncode(input);
    }

    /// <summary>
    /// HTML-decodes the string.
    /// </summary>
    /// <param name="input">The string to decode.</param>
    /// <returns>The HTML-decoded string, or an empty string if input is null.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string UnescapeHtml(this string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        return HttpUtility.HtmlDecode(input);
    }

    /// <summary>
    /// Converts the string to Base64 encoding.
    /// </summary>
    /// <param name="input">The string to encode.</param>
    /// <param name="encoding">The encoding to use. Defaults to UTF-8.</param>
    /// <returns>The Base64-encoded string, or an empty string if input is null.</returns>
    public static string ToBase64(this string? input, Encoding? encoding = null)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        encoding ??= Encoding.UTF8;
        var bytes = encoding.GetBytes(input);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Converts the Base64-encoded string back to its original form.
    /// </summary>
    /// <param name="input">The Base64-encoded string to decode.</param>
    /// <param name="encoding">The encoding to use. Defaults to UTF-8.</param>
    /// <returns>The decoded string, or an empty string if input is null or invalid Base64.</returns>
    public static string FromBase64(this string? input, Encoding? encoding = null)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        try
        {
            encoding ??= Encoding.UTF8;
            var bytes = Convert.FromBase64String(input);
            return encoding.GetString(bytes);
        }
        catch (FormatException)
        {
            return string.Empty;
        }
    }

    #endregion

    #region String Manipulation

    /// <summary>
    /// Removes all whitespace characters from the string.
    /// </summary>
    /// <param name="input">The string to process.</param>
    /// <returns>The string with all whitespace removed, or an empty string if input is null.</returns>
    public static string RemoveWhitespace(this string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        return Regex.Replace(input, @"\s+", string.Empty);
    }

    /// <summary>
    /// Collapses multiple consecutive whitespace characters into single spaces.
    /// </summary>
    /// <param name="input">The string to process.</param>
    /// <returns>The string with collapsed whitespace, or an empty string if input is null.</returns>
    public static string CollapseWhitespace(this string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        return Regex.Replace(input.Trim(), @"\s+", " ");
    }

    /// <summary>
    /// Reverses the characters in the string.
    /// </summary>
    /// <param name="input">The string to reverse.</param>
    /// <returns>The reversed string, or an empty string if input is null.</returns>
    public static string Reverse(this string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var chars = input.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }

    /// <summary>
    /// Removes all digits from the string.
    /// </summary>
    /// <param name="input">The string to process.</param>
    /// <returns>The string with digits removed, or an empty string if input is null.</returns>
    public static string RemoveDigits(this string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        return Regex.Replace(input, @"\d", string.Empty);
    }

    /// <summary>
    /// Extracts only the digits from the string.
    /// </summary>
    /// <param name="input">The string to process.</param>
    /// <returns>A string containing only digits, or an empty string if input is null or contains no digits.</returns>
    public static string OnlyDigits(this string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        return Regex.Replace(input, @"\D", string.Empty);
    }

    /// <summary>
    /// Extracts only the letters from the string.
    /// </summary>
    /// <param name="input">The string to process.</param>
    /// <returns>A string containing only letters, or an empty string if input is null or contains no letters.</returns>
    public static string OnlyLetters(this string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        return Regex.Replace(input, @"[^a-zA-Z]", string.Empty);
    }

    #endregion

    #region String Comparison

    /// <summary>
    /// Determines whether the string contains the specified value, ignoring case.
    /// </summary>
    /// <param name="input">The string to search in.</param>
    /// <param name="value">The value to search for.</param>
    /// <returns>true if the value is found; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsIgnoreCase(this string? input, string value)
    {
        if (input is null || value is null)
        {
            return false;
        }

        return input.Contains(value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the string starts with the specified value, ignoring case.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <param name="value">The value to compare.</param>
    /// <returns>true if the string starts with the value; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWithIgnoreCase(this string? input, string value)
    {
        if (input is null || value is null)
        {
            return false;
        }

        return input.StartsWith(value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the string ends with the specified value, ignoring case.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <param name="value">The value to compare.</param>
    /// <returns>true if the string ends with the value; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EndsWithIgnoreCase(this string? input, string value)
    {
        if (input is null || value is null)
        {
            return false;
        }

        return input.EndsWith(value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the string equals the specified value, ignoring case.
    /// </summary>
    /// <param name="input">The string to compare.</param>
    /// <param name="value">The value to compare.</param>
    /// <returns>true if the strings are equal; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsIgnoreCase(this string? input, string? value)
    {
        return string.Equals(input, value, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region String Counts & Analysis

    /// <summary>
    /// Counts the number of occurrences of a substring in the string.
    /// </summary>
    /// <param name="input">The string to search in.</param>
    /// <param name="substring">The substring to count.</param>
    /// <returns>The number of occurrences, or 0 if input or substring is null/empty.</returns>
    public static int CountOccurrences(this string? input, string substring)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(substring))
        {
            return 0;
        }

        var count = 0;
        var index = 0;
        while ((index = input.IndexOf(substring, index, StringComparison.Ordinal)) != -1)
        {
            count++;
            index += substring.Length;
        }

        return count;
    }

    /// <summary>
    /// Counts the number of occurrences of a character in the string.
    /// </summary>
    /// <param name="input">The string to search in.</param>
    /// <param name="character">The character to count.</param>
    /// <returns>The number of occurrences, or 0 if input is null.</returns>
    public static int CountOccurrences(this string? input, char character)
    {
        if (string.IsNullOrEmpty(input))
        {
            return 0;
        }

        return input.Count(c => c == character);
    }

    /// <summary>
    /// Counts the number of words in the string.
    /// </summary>
    /// <param name="input">The string to analyze.</param>
    /// <returns>The number of words, or 0 if input is null or empty.</returns>
    public static int WordCount(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return 0;
        }

        return input.Split(_caseSeparators, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    /// <summary>
    /// Counts the number of lines in the string.
    /// </summary>
    /// <param name="input">The string to analyze.</param>
    /// <returns>The number of lines, or 0 if input is null.</returns>
    public static int LineCount(this string? input)
    {
        if (input is null)
        {
            return 0;
        }

        if (input.Length == 0)
        {
            return 1;
        }

        return input.CountOccurrences('\n') + 1;
    }

    #endregion

    #region String Extraction

    /// <summary>
    /// Extracts all numbers from the string and concatenates them.
    /// </summary>
    /// <param name="input">The string to extract numbers from.</param>
    /// <returns>A string containing all extracted numbers, or an empty string if input is null or contains no numbers.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ExtractNumbers(this string? input)
    {
        return input.OnlyDigits();
    }

    /// <summary>
    /// Extracts the substring between two markers.
    /// </summary>
    /// <param name="input">The string to search in.</param>
    /// <param name="start">The start marker.</param>
    /// <param name="end">The end marker.</param>
    /// <returns>The substring between the markers, or an empty string if not found or input is null.</returns>
    public static string Between(this string? input, string start, string end)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end))
        {
            return string.Empty;
        }

        var startIndex = input.IndexOf(start, StringComparison.Ordinal);
        if (startIndex == -1)
        {
            return string.Empty;
        }

        startIndex += start.Length;
        var endIndex = input.IndexOf(end, startIndex, StringComparison.Ordinal);
        if (endIndex == -1)
        {
            return string.Empty;
        }

        return input[startIndex..endIndex];
    }

    /// <summary>
    /// Extracts the substring after the specified marker.
    /// </summary>
    /// <param name="input">The string to search in.</param>
    /// <param name="marker">The marker to search for.</param>
    /// <returns>The substring after the marker, or an empty string if not found or input is null.</returns>
    public static string After(this string? input, string marker)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(marker))
        {
            return string.Empty;
        }

        var index = input.IndexOf(marker, StringComparison.Ordinal);
        if (index == -1)
        {
            return string.Empty;
        }

        return input[(index + marker.Length)..];
    }

    /// <summary>
    /// Extracts the substring before the specified marker.
    /// </summary>
    /// <param name="input">The string to search in.</param>
    /// <param name="marker">The marker to search for.</param>
    /// <returns>The substring before the marker, or an empty string if not found or input is null.</returns>
    public static string Before(this string? input, string marker)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(marker))
        {
            return string.Empty;
        }

        var index = input.IndexOf(marker, StringComparison.Ordinal);
        if (index == -1)
        {
            return string.Empty;
        }

        return input[..index];
    }

    #endregion

    #region String Splitting & Joining

    /// <summary>
    /// Splits the string by the specified separators and trims each result.
    /// </summary>
    /// <param name="input">The string to split.</param>
    /// <param name="separators">The separator characters.</param>
    /// <returns>An array of trimmed strings, or an empty array if input is null.</returns>
    public static string[] SplitAndTrim(this string? input, params char[] separators)
    {
        if (string.IsNullOrEmpty(input))
        {
            return [];
        }

        return input.Split(separators).Select(s => s.Trim()).ToArray();
    }

    /// <summary>
    /// Splits the string by the specified separators and removes empty entries.
    /// </summary>
    /// <param name="input">The string to split.</param>
    /// <param name="separators">The separator characters.</param>
    /// <returns>An array of non-empty strings, or an empty array if input is null.</returns>
    public static string[] SplitAndRemoveEmpty(this string? input, params char[] separators)
    {
        if (string.IsNullOrEmpty(input))
        {
            return [];
        }

        return input.Split(separators, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Joins the string values with the specified separator.
    /// </summary>
    /// <param name="values">The string values to join.</param>
    /// <param name="separator">The separator to use.</param>
    /// <returns>The joined string, or an empty string if values is null or empty.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string JoinWith(this IEnumerable<string> values, string separator)
    {
        if (values is null)
        {
            return string.Empty;
        }

        return string.Join(separator, values);
    }

    #endregion

    #region String Repeat & Generate

    /// <summary>
    /// Repeats the string the specified number of times.
    /// </summary>
    /// <param name="input">The string to repeat.</param>
    /// <param name="count">The number of times to repeat.</param>
    /// <returns>The repeated string, or an empty string if input is null or count is 0 or negative.</returns>
    public static string Repeat(this string? input, int count)
    {
        if (string.IsNullOrEmpty(input) || count <= 0)
        {
            return string.Empty;
        }

        if (count == 1)
        {
            return input;
        }

        var result = new StringBuilder(input.Length * count);
        for (var i = 0; i < count; i++)
        {
            result.Append(input);
        }

        return result.ToString();
    }

    /// <summary>
    /// Generates a random string of the specified length using the allowed characters.
    /// </summary>
    /// <param name="length">The length of the random string.</param>
    /// <param name="allowedChars">The characters to choose from.</param>
    /// <returns>A random string of the specified length.</returns>
#pragma warning disable CA5394 // Do not use insecure randomness
    public static string RandomString(int length, string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789")
    {
        if (length <= 0 || string.IsNullOrEmpty(allowedChars))
        {
            return string.Empty;
        }

        var random = new Random();
        var result = new StringBuilder(length);

        for (var i = 0; i < length; i++)
        {
            result.Append(allowedChars[random.Next(allowedChars.Length)]);
        }

        return result.ToString();
    }
#pragma warning restore CA5394

    #endregion
}
