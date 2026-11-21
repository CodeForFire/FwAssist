using System.Globalization;

namespace FwAssist.Helpers.Strahlenschutz.Tests;

public class StrahlenRechnerTests
{
    [Fact]
    public void One_tkz_is_ten_microsievert_per_hour_at_one_metre() =>
        Assert.Equal(10m, StrahlenRechner.Tkz(1m, 1m));

    [Fact]
    public void A_tkz_of_zero_counts_as_the_most_it_can_stand_for() =>
        Assert.Equal(0.5m, StrahlenRechner.Tkz(0m, 1m));

    [Fact]
    public void A_tkz_above_the_zero_band_counts_as_given() =>
        Assert.Equal(0.6m, StrahlenRechner.Tkz(0.06m, 1m));

    [Fact]
    public void A_tkz_falls_with_the_square_of_the_distance() =>
        Assert.Equal(2.5m, StrahlenRechner.Tkz(1m, 2m));

    [Fact]
    public void A_tkz_result_rounds_up() =>
        Assert.Equal(3.7m, StrahlenRechner.Tkz(3.3m, 3m));

    [Fact]
    public void Twice_the_distance_is_a_quarter_of_the_dose_rate() =>
        Assert.Equal(25m, StrahlenRechner.Abstand(100m, 1m, 2m));

    [Fact]
    public void Half_the_distance_is_four_times_the_dose_rate() =>
        Assert.Equal(400m, StrahlenRechner.Abstand(100m, 2m, 1m));

    [Fact]
    public void A_dose_rate_at_another_distance_rounds_up() =>
        Assert.Equal(1.2m, StrahlenRechner.Abstand(10m, 1m, 3m));

    [Fact]
    public void Arithmetic_noise_does_not_round_up_a_whole_step() =>
        Assert.Equal(25m, StrahlenRechner.Abstand(225m, 1m, 3m));

    [Fact]
    public void The_limit_is_reached_where_the_square_law_says() =>
        Assert.Equal(2m, StrahlenRechner.Absperrgrenze(100m, 1m, 25m));

    [Fact]
    public void The_measuring_distance_scales_the_absperrgrenze() =>
        Assert.Equal(20m, StrahlenRechner.Absperrgrenze(2500m, 2m, 25m));

    [Fact]
    public void An_absperrgrenze_between_metres_rounds_up_to_the_next_metre() =>
        Assert.Equal(4m, StrahlenRechner.Absperrgrenze(100m, 1m, 10m));

    [Fact]
    public void A_dose_rate_already_below_the_limit_still_gives_a_whole_metre() =>
        Assert.Equal(1m, StrahlenRechner.Absperrgrenze(10m, 1m, 25m));

    [Fact]
    public void A_dose_rate_at_the_limit_gives_the_measuring_distance() =>
        Assert.Equal(3m, StrahlenRechner.Absperrgrenze(25m, 3m, 25m));

    [Theory]
    [InlineData("0.123", "0.13")]
    [InlineData("0.12", "0.12")]
    [InlineData("12", "12")]
    [InlineData("10", "10")]
    [InlineData("99.1", "100")]
    [InlineData("1234", "1300")]
    [InlineData("0.0001234", "0.00013")]
    [InlineData("5", "5")]
    public void Dose_rates_round_up_to_two_significant_figures(string value, string expected) =>
        Assert.Equal(Parse(expected), StrahlenRechner.RoundUpSignificant(Parse(value)));

    [Fact]
    public void Nothing_rounds_to_nothing() =>
        Assert.Equal(0m, StrahlenRechner.RoundUpSignificant(0m));

    private static decimal Parse(string value) => decimal.Parse(value, CultureInfo.InvariantCulture);
}
