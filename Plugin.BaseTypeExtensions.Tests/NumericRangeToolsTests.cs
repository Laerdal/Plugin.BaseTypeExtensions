namespace Plugin.BaseTypeExtensions.Tests;

public class NumericRangeToolsTests
{
    #region Generic GetRange<T> Tests

    [Fact]
    public void GetRange_GenericHandlesZeroStep()
    {
        NumericRangeTools.GetRange(0, 10, 0).Should().BeEmpty();
    }

    [Fact]
    public void GetRange_GenericProducesSequence()
    {
        NumericRangeTools.GetRange(1, 3, 1).Should().Equal(1, 2, 3);
    }

    [Fact]
    public void GetRange_GenericProducesSequenceDescending()
    {
        NumericRangeTools.GetRange(5, 1, 1).Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public void GetRange_GenericWithDecimals()
    {
        NumericRangeTools.GetRange(1.0m, 3.0m, 0.5m).Should().Equal(1.0m, 1.5m, 2.0m, 2.5m, 3.0m);
    }

    [Fact]
    public void GetRange_GenericWithFloats()
    {
        NumericRangeTools.GetRange(1.0f, 3.0f, 0.5f).Should().Equal(1.0f, 1.5f, 2.0f, 2.5f);
    }

    [Fact]
    public void GetRange_GenericWithDoubles()
    {
        NumericRangeTools.GetRange(1.0, 3.0, 0.5).Should().Equal(1.0, 1.5, 2.0, 2.5);
    }

    [Fact]
    public void GetRange_GenericWithLargeStep()
    {
        NumericRangeTools.GetRange(1, 10, 5).Should().Equal(1, 6);
    }

    [Fact]
    public void GetRange_GenericSingleValue()
    {
        NumericRangeTools.GetRange(5, 5, 1).Should().Equal(5);
    }

    [Fact]
    public void GetRange_GenericEmptyRangeStepTooLarge()
    {
        NumericRangeTools.GetRange(1, 5, 10).Should().Equal(1);
    }

    [Fact]
    public void GetRange_GenericNegativeNumbers()
    {
        NumericRangeTools.GetRange(-5, -1, 1).Should().Equal(-5, -4, -3, -2, -1);
    }

    [Fact]
    public void GetRange_GenericMixedSignNumbers()
    {
        NumericRangeTools.GetRange(-2, 2, 1).Should().Equal(-2, -1, 0, 1, 2);
    }

    // The negative-step tests cap the sequence with Take so that a regression fails the assertion instead of hanging the run.

    [Fact]
    public void GetRange_GenericNegativeStepUsesMagnitude()
    {
        NumericRangeTools.GetRange(1, 5, -1).Take(100).Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public void GetRange_GenericNegativeStepWithLargeStep()
    {
        NumericRangeTools.GetRange(1, 10, -5).Take(100).Should().Equal(1, 6);
    }

    [Fact]
    public void GetRange_GenericNegativeStepWithReversedBounds()
    {
        NumericRangeTools.GetRange(5, 1, -1).Take(100).Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public void GetRange_GenericNegativeStepWithNegativeNumbers()
    {
        NumericRangeTools.GetRange(-5, -1, -2).Take(100).Should().Equal(-5, -3, -1);
    }

    [Fact]
    public void GetRange_GenericNegativeStepWithLongs()
    {
        NumericRangeTools.GetRange(1L, 3L, -1L).Take(100).Should().Equal(1L, 2L, 3L);
    }

    [Fact]
    public void GetRange_GenericNegativeStepWithDecimals()
    {
        NumericRangeTools.GetRange(1.0m, 2.0m, -0.5m).Take(100).Should().Equal(1.0m, 1.5m, 2.0m);
    }

    [Fact]
    public void GetRange_GenericNegativeStepWithDoubles()
    {
        // The explicit type argument selects the generic overload instead of the double one.
        NumericRangeTools.GetRange<double>(1.0, 2.0, -0.5).Take(100).Should().Equal(1.0, 1.5, 2.0);
    }

    [Fact]
    public void GetRange_GenericMinValueStepThrowsInsteadOfLooping()
    {
        var act = () => NumericRangeTools.GetRange(0, 10, int.MinValue).Take(100).ToList();

        act.Should().Throw<OverflowException>();
    }

    #endregion

    #region Double GetRange Tests

    [Fact]
    public void GetRange_DoubleProducesSequence()
    {
        NumericRangeTools.GetRange(0.0, 0.6, 0.3).Should().Equal(0.0, 0.3);
    }

    [Fact]
    public void GetRange_DoubleHandlesZeroStep()
    {
        NumericRangeTools.GetRange(0.0, 10.0, 0.0).Should().BeEmpty();
    }

    [Fact]
    public void GetRange_DoubleHandlesNaNStep()
    {
        NumericRangeTools.GetRange(0.0, 10.0, double.NaN).Should().BeEmpty();
    }

    [Fact]
    public void GetRange_DoubleHandlesPositiveInfinityStep()
    {
        NumericRangeTools.GetRange(0.0, 10.0, double.PositiveInfinity).Should().BeEmpty();
    }

    [Fact]
    public void GetRange_DoubleHandlesNegativeInfinityStep()
    {
        NumericRangeTools.GetRange(0.0, 10.0, double.NegativeInfinity).Should().BeEmpty();
    }

    [Fact]
    public void GetRange_DoubleDescendingRange()
    {
        NumericRangeTools.GetRange(5.0, 1.0, 1.0).Should().Equal(5.0, 4.0, 3.0, 2.0);
    }

    [Fact]
    public void GetRange_DoubleNegativeStep()
    {
        NumericRangeTools.GetRange(0.0, 5.0, -1.0).Should().Equal(0.0, 1.0, 2.0, 3.0, 4.0);
    }

    [Fact]
    public void GetRange_DoubleNegativeStepDescending()
    {
        NumericRangeTools.GetRange(5.0, 0.0, -1.0).Should().Equal(5.0, 4.0, 3.0, 2.0, 1.0);
    }

    [Fact]
    public void GetRange_DoubleSameStartEnd()
    {
        NumericRangeTools.GetRange(5.0, 5.0, 1.0).Should().BeEmpty();
    }

    [Fact]
    public void GetRange_DoubleSmallStep()
    {
        NumericRangeTools.GetRange(0.0, 0.5, 0.1).Should().HaveCount(5)
            .And.Contain(0.0).And.Contain(0.1).And.Contain(0.4);
    }

    [Fact]
    public void GetRange_DoubleWithNaN()
    {
        NumericRangeTools.GetRange(double.NaN, 10.0, 1.0).Should().BeEmpty();
    }

    [Fact]
    public void GetRange_DoubleWithInfinity()
    {
        var result = NumericRangeTools.GetRange(0.0, double.PositiveInfinity, 1.0).Take(3).ToList();
        result.Should().HaveCount(3);
        result[0].Should().Be(0.0);
        result[1].Should().Be(1.0);
        result[2].Should().Be(2.0);
    }

    [Fact]
    public void GetRange_DoublePrecisionTest()
    {
        var result = NumericRangeTools.GetRange(0.1, 0.3, 0.1).ToList();
        result.Should().HaveCount(2);
        result[0].Should().BeApproximately(0.1, 1e-15);
        result[1].Should().BeApproximately(0.2, 1e-15);
    }

    #endregion

    #region Float GetRange Tests

    [Fact]
    public void GetRange_FloatProducesSequence()
    {
        NumericRangeTools.GetRange(0f, 0.6f, 0.3f).Should().Equal(0f, 0.3f);
    }

    [Fact]
    public void GetRange_FloatHandlesZeroStep()
    {
        NumericRangeTools.GetRange(0f, 10f, 0f).Should().BeEmpty();
    }

    [Fact]
    public void GetRange_FloatHandlesNaNStep()
    {
        NumericRangeTools.GetRange(0f, 10f, float.NaN).Should().BeEmpty();
    }

    [Fact]
    public void GetRange_FloatHandlesPositiveInfinityStep()
    {
        NumericRangeTools.GetRange(0f, 10f, float.PositiveInfinity).Should().BeEmpty();
    }

    [Fact]
    public void GetRange_FloatHandlesNegativeInfinityStep()
    {
        NumericRangeTools.GetRange(0f, 10f, float.NegativeInfinity).Should().BeEmpty();
    }

    [Fact]
    public void GetRange_FloatDescendingRange()
    {
        NumericRangeTools.GetRange(5f, 1f, 1f).Should().Equal(5f, 4f, 3f, 2f);
    }

    [Fact]
    public void GetRange_FloatNegativeStep()
    {
        NumericRangeTools.GetRange(0f, 5f, -1f).Should().Equal(0f, 1f, 2f, 3f, 4f);
    }

    [Fact]
    public void GetRange_FloatNegativeStepDescending()
    {
        NumericRangeTools.GetRange(5f, 0f, -1f).Should().Equal(5f, 4f, 3f, 2f, 1f);
    }

    [Fact]
    public void GetRange_FloatSameStartEnd()
    {
        NumericRangeTools.GetRange(5f, 5f, 1f).Should().BeEmpty();
    }

    [Fact]
    public void GetRange_FloatSmallStep()
    {
        NumericRangeTools.GetRange(0f, 0.5f, 0.1f).Should().HaveCount(5)
            .And.Contain(0f).And.Contain(0.1f).And.Contain(0.4f);
    }

    [Fact]
    public void GetRange_FloatWithNaN()
    {
        NumericRangeTools.GetRange(float.NaN, 10f, 1f).Should().BeEmpty();
    }

    [Fact]
    public void GetRange_FloatWithInfinity()
    {
        var result = NumericRangeTools.GetRange(0f, float.PositiveInfinity, 1f).Take(3).ToList();
        result.Should().HaveCount(3);
        result[0].Should().Be(0f);
        result[1].Should().Be(1f);
        result[2].Should().Be(2f);
    }

    [Fact]
    public void GetRange_FloatPrecisionTest()
    {
        var result = NumericRangeTools.GetRange(0.1f, 0.3f, 0.1f).ToList();
        result.Should().HaveCount(2);
        result[0].Should().BeApproximately(0.1f, 1e-6f);
        result[1].Should().BeApproximately(0.2f, 1e-6f);
    }

    #endregion

    #region DateTime GetRange Tests

    [Fact]
    public void GetRange_DateTimeProducesSequence()
    {
        var start = new DateTime(2024, 1, 1);
        var step = TimeSpan.FromDays(1);
        NumericRangeTools.GetRange(start, start.AddDays(2), step).Should().Equal(start, start.AddDays(1));
    }

    [Fact]
    public void GetRange_DateTimeHandlesZeroStep()
    {
        var start = new DateTime(2024, 1, 1);
        NumericRangeTools.GetRange(start, start.AddDays(10), TimeSpan.Zero).Should().BeEmpty();
    }

    [Fact]
    public void GetRange_DateTimeHandlesMinValueStep()
    {
        var start = new DateTime(2024, 1, 1);
        NumericRangeTools.GetRange(start, start.AddDays(10), TimeSpan.MinValue).Should().BeEmpty();
    }

    [Fact]
    public void GetRange_DateTimeDescendingRange()
    {
        var start = new DateTime(2024, 1, 5);
        var end = new DateTime(2024, 1, 1);
        var step = TimeSpan.FromDays(1);
        var result = NumericRangeTools.GetRange(start, end, step).ToList();

        result.Should().HaveCount(4);
        result[0].Should().Be(new DateTime(2024, 1, 5));
        result[1].Should().Be(new DateTime(2024, 1, 4));
        result[2].Should().Be(new DateTime(2024, 1, 3));
        result[3].Should().Be(new DateTime(2024, 1, 2));
    }

    [Fact]
    public void GetRange_DateTimeNegativeStep()
    {
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 1, 5);
        var step = TimeSpan.FromDays(-1);
        var result = NumericRangeTools.GetRange(start, end, step).ToList();

        result.Should().HaveCount(4);
        result[0].Should().Be(new DateTime(2024, 1, 1));
        result[1].Should().Be(new DateTime(2024, 1, 2));
        result[2].Should().Be(new DateTime(2024, 1, 3));
        result[3].Should().Be(new DateTime(2024, 1, 4));
    }

    [Fact]
    public void GetRange_DateTimeNegativeStepDescending()
    {
        var start = new DateTime(2024, 1, 5);
        var end = new DateTime(2024, 1, 1);
        var step = TimeSpan.FromDays(-1);
        var result = NumericRangeTools.GetRange(start, end, step).ToList();

        result.Should().HaveCount(4);
        result[0].Should().Be(new DateTime(2024, 1, 5));
        result[1].Should().Be(new DateTime(2024, 1, 4));
        result[2].Should().Be(new DateTime(2024, 1, 3));
        result[3].Should().Be(new DateTime(2024, 1, 2));
    }

    [Fact]
    public void GetRange_DateTimeSameStartEnd()
    {
        var start = new DateTime(2024, 1, 1);
        NumericRangeTools.GetRange(start, start, TimeSpan.FromDays(1)).Should().BeEmpty();
    }

    [Fact]
    public void GetRange_DateTimeHourlyStep()
    {
        var start = new DateTime(2024, 1, 1, 0, 0, 0);
        var end = new DateTime(2024, 1, 1, 6, 0, 0);
        var step = TimeSpan.FromHours(2);
        var result = NumericRangeTools.GetRange(start, end, step).ToList();

        result.Should().HaveCount(3);
        result[0].Should().Be(new DateTime(2024, 1, 1, 0, 0, 0));
        result[1].Should().Be(new DateTime(2024, 1, 1, 2, 0, 0));
        result[2].Should().Be(new DateTime(2024, 1, 1, 4, 0, 0));
    }

    [Fact]
    public void GetRange_DateTimeMillisecondStep()
    {
        var start = new DateTime(2024, 1, 1, 0, 0, 0, 0);
        var end = new DateTime(2024, 1, 1, 0, 0, 0, 5);
        var step = TimeSpan.FromMilliseconds(2);
        var result = NumericRangeTools.GetRange(start, end, step).ToList();

        result.Should().HaveCount(3);
        result[0].Should().Be(new DateTime(2024, 1, 1, 0, 0, 0, 0));
        result[1].Should().Be(new DateTime(2024, 1, 1, 0, 0, 0, 2));
        result[2].Should().Be(new DateTime(2024, 1, 1, 0, 0, 0, 4));
    }

    [Fact]
    public void GetRange_DateTimeMaxValueStep()
    {
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 1, 2);
        var step = TimeSpan.FromDays(1);
        var result = NumericRangeTools.GetRange(start, end, step).ToList();

        result.Should().HaveCount(1);
        result[0].Should().Be(start);
    }

    [Fact]
    public void GetRange_DateTimeLargeRange()
    {
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2025, 1, 1);
        var step = TimeSpan.FromDays(100);
        var result = NumericRangeTools.GetRange(start, end, step).ToList();

        result.Should().HaveCount(4);
        result[0].Should().Be(new DateTime(2024, 1, 1));
        result[1].Should().Be(new DateTime(2024, 4, 10));
        result[2].Should().Be(new DateTime(2024, 7, 19));
        result[3].Should().Be(new DateTime(2024, 10, 27));
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [Fact]
    public void GetRange_GenericMaxValue()
    {
        NumericRangeTools.GetRange(100, 102, 1).Should().Equal(100, 101, 102);
    }

    [Fact]
    public void GetRange_GenericMinValue()
    {
        NumericRangeTools.GetRange(int.MinValue, int.MinValue + 1, 1).Should().Equal(int.MinValue, int.MinValue + 1);
    }

    [Fact]
    public void GetRange_DoubleVerySmallStep()
    {
        var result = NumericRangeTools.GetRange(0.0, 1e-10, 1e-11).Take(5).ToList();
        result.Should().HaveCount(5);
    }

    [Fact]
    public void GetRange_FloatVerySmallStep()
    {
        var result = NumericRangeTools.GetRange(0f, 1e-6f, 1e-7f).Take(5).ToList();
        result.Should().HaveCount(5);
    }

    [Fact]
    public void GetRange_DateTimeMinDateValue()
    {
        var start = DateTime.MinValue;
        var end = DateTime.MinValue.AddDays(1);
        var step = TimeSpan.FromHours(12);
        var result = NumericRangeTools.GetRange(start, end, step).ToList();

        result.Should().HaveCount(2);
        result[0].Should().Be(DateTime.MinValue);
        result[1].Should().Be(DateTime.MinValue.AddHours(12));
    }

    [Fact]
    public void GetRange_DateTimeMaxDateValue()
    {
        var end = DateTime.MaxValue;
        var start = DateTime.MaxValue.AddDays(-1);
        var step = TimeSpan.FromHours(12);
        var result = NumericRangeTools.GetRange(start, end, step).ToList();

        result.Should().HaveCount(2);
        result[0].Should().Be(DateTime.MaxValue.AddDays(-1));
        result[1].Should().Be(DateTime.MaxValue.AddDays(-1).AddHours(12));
    }

    #endregion
}
