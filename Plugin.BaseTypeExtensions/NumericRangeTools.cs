using System.Numerics;

namespace Plugin.BaseTypeExtensions;

/// <summary>
/// Provides utility methods for numeric range conversions, percentage calculations, and value mapping operations.
/// </summary>
/// <remarks>
/// This class contains helper methods for converting between absolute and relative values,
/// percentage calculations, and generating numeric ranges. All methods are optimized for
/// performance and support generic numeric types where applicable.
/// </remarks>
public static class NumericRangeTools
{

    #region Range

    /// <summary>
    /// Generates a sequence of numbers from start to end with a specified step.
    /// </summary>
    /// <typeparam name="T">The numeric type that implements INumber&lt;T&gt;.</typeparam>
    /// <param name="start">The start of the range.</param>
    /// <param name="end">The end of the range.</param>
    /// <param name="step">The step size between each number in the range.</param>
    /// <returns>An IEnumerable of T values representing the range.</returns>
    /// <remarks>
    /// This method generates a sequence of numbers starting from `start` to `end`, inclusive, with the specified `step`.
    /// If `step` is zero, an empty sequence is returned.
    /// The sequence will include both `start` and `end` if they are reachable with the given step.
    /// </remarks>
    public static IEnumerable<T> GetRange<T>(T start, T end, T step) where T : INumber<T>
    {
        if (step == T.Zero)
        {
            yield break;
        }

        var minimum = T.Min(start, end);
        var maximum = T.Max(start, end);
        var current = minimum;

        while (current <= maximum)
        {
            yield return current;
            current += step;
        }
    }

    /// <summary>
    /// Generates a sequence of numbers from start to end with a specified step.
    /// </summary>
    /// <param name="start">The start of the range.</param>
    /// <param name="end">The end of the range.</param>
    /// <param name="step">The step size between each number in the range.</param>
    /// <returns>An IEnumerable of double values representing the range.</returns>
    /// <remarks>
    /// This method generates a sequence of numbers starting from `start` and stopping before `end`, using the magnitude of
    /// `step` to move between values. If `step` is zero, NaN, or infinite, an empty sequence is returned.
    /// </remarks>
    public static IEnumerable<double> GetRange(double start, double end, double step)
    {
        if (!double.IsFinite(step) || Math.Abs(step) < double.Epsilon)
        {
            return Array.Empty<double>();
        }

        return Iterate();

        IEnumerable<double> Iterate()
        {
            var isAscending = end >= start;
            var increment = Math.Abs(step);

            if (!isAscending)
            {
                increment = -increment;
            }

            for (var current = start;
                 isAscending ? current < end : current > end;
                 current += increment)
            {
                yield return current;
            }
        }
    }

    /// <summary>
    /// Generates a sequence of floating-point numbers from start to end with a specified step.
    /// </summary>
    /// <param name="start">The start of the range.</param>
    /// <param name="end">The end of the range.</param>
    /// <param name="step">The step size between each number in the range.</param>
    /// <returns>An IEnumerable of float values representing the range.</returns>
    /// <remarks>
    /// This method generates a sequence of floating-point numbers starting from `start` and stopping before `end`, using the magnitude of
    /// `step` to move between values. If `step` is zero, NaN, or infinite, an empty sequence is returned.
    /// </remarks>
    public static IEnumerable<float> GetRange(float start, float end, float step)
    {
        if (!float.IsFinite(step) || MathF.Abs(step) < float.Epsilon)
        {
            return Array.Empty<float>();
        }

        return Iterate();

        IEnumerable<float> Iterate()
        {
            var isAscending = end >= start;
            var increment = MathF.Abs(step);

            if (!isAscending)
            {
                increment = -increment;
            }

            for (var current = start;
                 isAscending ? current < end : current > end;
                 current += increment)
            {
                yield return current;
            }
        }
    }

    /// <summary>
    /// Generates a sequence of DateTime values from start to end with a specified step.
    /// </summary>
    /// <param name="start">The start of the range.</param>
    /// <param name="end">The end of the range.</param>
    /// <param name="step">The step size between each DateTime in the range.</param>
    /// <returns>An IEnumerable of DateTime values representing the range.</returns>
    /// <remarks>
    /// This method generates a sequence of DateTime values starting from `start` and stopping before `end`, using the magnitude of
    /// `step` to move between values. If `step` is zero or equals <see cref="TimeSpan.MinValue"/>, an empty sequence is returned.
    /// </remarks>
    public static IEnumerable<DateTime> GetRange(DateTime start, DateTime end, TimeSpan step)
    {
        if (step == TimeSpan.Zero || step == TimeSpan.MinValue)
        {
            return Array.Empty<DateTime>();
        }

        return Iterate();

        IEnumerable<DateTime> Iterate()
        {
            var isAscending = end >= start;
            var tickStep = step.Ticks;
            var increment = Math.Abs(tickStep);

            if (increment == 0)
            {
                yield break;
            }

            if (!isAscending)
            {
                increment = -increment;
            }

            for (var current = start;
                 isAscending ? current < end : current > end;
                 current = current.AddTicks(increment))
            {
                yield return current;
            }
        }
    }

    #endregion

}
