using System.Globalization;

namespace FwAssist.Helpers.Sandsack.Tests;

public class SandsackRechnerTests
{
    [Theory]
    [InlineData("0.1", "0.5")]
    [InlineData("0.5", "0.5")]
    [InlineData("0.51", "1.0")]
    [InlineData("1.0", "1.0")]
    [InlineData("1.2", "1.5")]
    [InlineData("1.5", "1.5")]
    [InlineData("1.6", "2.0")]
    [InlineData("2.0", "2.0")]
    public void A_height_counts_as_the_next_row_of_the_table_at_or_above_it(string hoehe, string stufe) =>
        Assert.Equal(Parse(stufe), SandsackRechner.Stufe(Parse(hoehe)).HoeheMeter);

    [Fact]
    public void A_dam_higher_than_the_table_is_not_calculated() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => SandsackRechner.Stufe(2.01m));

    [Fact]
    public void The_highest_dam_is_the_tables_last_row() =>
        Assert.Equal(2.0m, SandsackRechner.MaxHoeheMeter);

    [Theory]
    [InlineData("0.5", 45, "1")]
    [InlineData("1.0", 160, "3")]
    [InlineData("1.5", 350, "6")]
    [InlineData("2.0", 580, "9.6")]
    public void One_metre_of_dam_takes_the_taschenkartes_figures(string hoehe, int saecke, string sand)
    {
        var stufe = SandsackRechner.Stufe(Parse(hoehe));

        Assert.Equal(saecke, SandsackRechner.Saecke(1m, stufe));
        Assert.Equal(Parse(sand), SandsackRechner.SandKubikmeter(1m, stufe));
    }

    [Fact]
    public void Sacks_scale_with_the_length() =>
        Assert.Equal(16000m, SandsackRechner.Saecke(100m, SandsackRechner.Stufe(1m)));

    [Fact]
    public void Part_of_a_sack_counts_as_a_whole_one() =>
        Assert.Equal(68m, SandsackRechner.Saecke(1.5m, SandsackRechner.Stufe(0.5m)));

    [Fact]
    public void Sand_rounds_up_to_a_tenth() =>
        Assert.Equal(3.1m, SandsackRechner.SandKubikmeter(1.01m, SandsackRechner.Stufe(1m)));

    // 1,333333334 m × 45 is 60,000000030 sacks: the part of a sack is real and needs a sack.
    [Fact]
    public void A_tiny_real_part_of_a_sack_still_counts_as_a_whole_one() =>
        Assert.Equal(61m, SandsackRechner.Saecke(1.333333334m, SandsackRechner.Stufe(0.5m)));

    [Fact]
    public void A_tiny_real_part_of_a_tenth_of_sand_still_counts() =>
        Assert.Equal(1.1m, SandsackRechner.SandKubikmeter(1.00000001m, SandsackRechner.Stufe(0.5m)));

    [Fact]
    public void A_tiny_real_part_of_a_quarter_hour_still_counts() =>
        Assert.Equal(TimeSpan.FromMinutes(30), SandsackRechner.Arbeitszeit(10.0000001m, 2m));

    [Fact]
    public void An_amount_just_below_a_whole_tenth_is_not_rounded_past_it() =>
        Assert.Equal(1m, SandsackRechner.SandKubikmeter(1m / 3m * 3m, SandsackRechner.Stufe(0.5m)));

    [Fact]
    public void Dry_sand_weighs_one_point_two_to_one_point_six_tonnes_a_cubic_metre() =>
        Assert.Equal((360m, 480m), SandsackRechner.SandTrockenTonnen(300m));

    [Fact]
    public void Both_ends_of_the_dry_weight_round_up_to_a_tenth() =>
        Assert.Equal((3.8m, 5m), SandsackRechner.SandTrockenTonnen(3.1m));

    [Fact]
    public void Wet_sand_weighs_two_thirds_more_than_the_lightest_dry_sand() =>
        Assert.Equal(67m, SandsackRechner.NassMehrgewichtProzent);

    [Fact]
    public void Wet_sand_weighs_two_tonnes_a_cubic_metre() =>
        Assert.Equal(6.2m, SandsackRechner.SandTonnen(3.1m));

    [Fact]
    public void Each_sack_is_filled_and_carried_then_laid_at_forty_an_hour() =>
        Assert.Equal(TimeSpan.FromHours(80), SandsackRechner.Arbeitszeit(16000m, 10m));

    [Fact]
    public void Time_rounds_up_to_the_quarter_hour() =>
        Assert.Equal(TimeSpan.FromMinutes(75), SandsackRechner.Arbeitszeit(205m, 10m));

    [Fact]
    public void An_exact_quarter_hour_stays() =>
        Assert.Equal(TimeSpan.FromMinutes(15), SandsackRechner.Arbeitszeit(10m, 2m));

    [Fact]
    public void Part_of_a_helfer_does_not_count() =>
        Assert.Equal(SandsackRechner.Arbeitszeit(400m, 2m), SandsackRechner.Arbeitszeit(400m, 2.9m));

    [Fact]
    public void Less_than_one_whole_helfer_is_refused() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => SandsackRechner.Arbeitszeit(10m, 0.5m));

    [Theory]
    [InlineData("1", "2")]
    [InlineData("0.75", "1.5")]
    [InlineData("0.77", "1.6")]
    public void The_base_is_at_least_twice_the_height(string hoehe, string basis) =>
        Assert.Equal(Parse(basis), SandsackRechner.Basisbreite(Parse(hoehe)));

    [Fact]
    public void Each_row_of_the_table_is_higher_and_needs_more_than_the_one_before()
    {
        var stufen = Sandsackvorgaben.Dammstufen;
        for (var i = 1; i < stufen.Count; i++)
        {
            Assert.True(stufen[i].HoeheMeter > stufen[i - 1].HoeheMeter);
            Assert.True(stufen[i].SaeckeJeMeter > stufen[i - 1].SaeckeJeMeter);
            Assert.True(stufen[i].SandKubikmeterJeMeter > stufen[i - 1].SandKubikmeterJeMeter);
        }
    }

    private static decimal Parse(string value) => decimal.Parse(value, CultureInfo.InvariantCulture);
}
