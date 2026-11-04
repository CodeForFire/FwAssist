namespace Helfer.Core.Tests;

public class FormattingTests
{
    [Theory]
    [InlineData(0, "00:00")]
    [InlineData(59, "00:59")]
    [InlineData(247, "04:07")]
    [InlineData(3599, "59:59")]
    [InlineData(3600, "1:00:00")]
    [InlineData(3847, "1:04:07")]
    [InlineData(36000, "10:00:00")]
    public void Duration_shows_minutes_and_seconds_and_hours_only_from_one_hour_on(int seconds, string expected) =>
        Assert.Equal(expected, Formatting.Duration(TimeSpan.FromSeconds(seconds)));

    [Fact]
    public void Duration_truncates_to_the_second_and_never_rounds_up() =>
        Assert.Equal("00:04", Formatting.Duration(TimeSpan.FromMilliseconds(4999)));

    [Fact]
    public void Duration_reads_a_negative_span_as_zero() =>
        Assert.Equal("00:00", Formatting.Duration(TimeSpan.FromSeconds(-5)));

    [Fact]
    public void Number_groups_thousands_with_a_dot() =>
        Assert.Equal("1.234", Formatting.Number(1234));

    [Fact]
    public void Timestamp_and_time_of_day_use_german_order()
    {
        var t = new DateTimeOffset(2026, 11, 3, 14, 5, 0, TimeSpan.Zero);
        Assert.Equal("03.11.2026 14:05", Formatting.Timestamp(t));
        Assert.Equal("14:05", Formatting.TimeOfDay(t));
    }
}
