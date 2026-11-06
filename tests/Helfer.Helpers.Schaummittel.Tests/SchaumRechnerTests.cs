namespace Helfer.Helpers.Schaummittel.Tests;

public class SchaumRechnerTests
{
    [Fact]
    public void Vorrat_turns_the_schaummittel_into_gemisch_and_foam()
    {
        var result = SchaumRechner.Vorrat(schaummittelLiter: 100m, wasserLiter: null, zumischProzent: 3m, verschaeumungszahl: 10m);

        Assert.Equal(3333m, result.GemischLiter);
        Assert.Equal(33.3m, result.SchaumKubikmeter);
        Assert.Equal(100m, result.SchaummittelLiter);
        Assert.Equal(3234m, result.WasserLiter);
        Assert.Equal(Limit.Schaummittel, result.Limit);
    }

    [Fact]
    public void Vorrat_rounds_the_foam_down_and_the_water_needed_up()
    {
        var result = SchaumRechner.Vorrat(1m, null, 3m, 1m);

        // 33,33 l Gemisch: 0,0333 m³ of foam is not yet 0,1 m³; 32,33 l of water needs 33 l.
        Assert.Equal(0m, result.SchaumKubikmeter);
        Assert.Equal(33m, result.GemischLiter);
        Assert.Equal(33m, result.WasserLiter);
    }

    [Fact]
    public void Vorrat_is_not_limited_by_water_there_is_enough_of()
    {
        var result = SchaumRechner.Vorrat(30m, 2000m, 3m, 10m);

        Assert.Equal(1000m, result.GemischLiter);
        Assert.Equal(970m, result.WasserLiter);
        Assert.Equal(Limit.Schaummittel, result.Limit);
    }

    [Fact]
    public void Vorrat_is_limited_by_a_small_tank()
    {
        var result = SchaumRechner.Vorrat(100m, 970m, 3m, 10m);

        Assert.Equal(1000m, result.GemischLiter);
        Assert.Equal(10m, result.SchaumKubikmeter);
        Assert.Equal(30m, result.SchaummittelLiter);
        Assert.Equal(970m, result.WasserLiter);
        Assert.Equal(Limit.Wasser, result.Limit);
    }

    [Fact]
    public void Vorrat_calls_the_schaummittel_the_limit_when_both_run_out_together()
    {
        var result = SchaumRechner.Vorrat(30m, 970m, 3m, 10m);

        Assert.Equal(Limit.Schaummittel, result.Limit);
    }

    [Fact]
    public void Raum_needs_its_volume_in_foam()
    {
        var result = SchaumRechner.Raum(10m, 5m, 3m, zuschlagProzent: 0m, zumischProzent: 3m, verschaeumungszahl: 10m);

        Assert.Equal(150m, result.VolumenKubikmeter);
        Assert.Equal(150m, result.SchaumKubikmeter);
        Assert.Equal(15000m, result.GemischLiter);
        Assert.Equal(450m, result.SchaummittelLiter);
        Assert.Equal(14550m, result.WasserLiter);
    }

    [Fact]
    public void Raum_adds_the_zuschlag_to_the_foam_but_not_to_the_room()
    {
        var result = SchaumRechner.Raum(10m, 5m, 3m, 50m, 3m, 10m);

        Assert.Equal(150m, result.VolumenKubikmeter);
        Assert.Equal(225m, result.SchaumKubikmeter);
        Assert.Equal(22500m, result.GemischLiter);
        Assert.Equal(675m, result.SchaummittelLiter);
        Assert.Equal(21825m, result.WasserLiter);
    }

    [Fact]
    public void Raum_rounds_everything_it_needs_up()
    {
        var result = SchaumRechner.Raum(1m, 1m, 1m, 0m, 3m, 75m);

        // 13,33 l Gemisch, 0,4 l Schaummittel, 12,93 l Wasser.
        Assert.Equal(14m, result.GemischLiter);
        Assert.Equal(1m, result.SchaummittelLiter);
        Assert.Equal(13m, result.WasserLiter);
    }

    [Fact]
    public void Raum_rounds_the_foam_up_to_a_tenth()
    {
        var result = SchaumRechner.Raum(1.01m, 1m, 1m, 0m, 3m, 10m);

        Assert.Equal(1.1m, result.SchaumKubikmeter);
    }

    [Theory]
    [InlineData(10, 3)]
    [InlineData(100, 6)]
    [InlineData(70, 1)]
    public void Division_noise_does_not_add_a_litre(int schaummittelLiter, int zumischProzent)
    {
        // The Gemisch repeats, and multiplying back must land on the supply, not one litre above it.
        var result = SchaumRechner.Vorrat(schaummittelLiter, null, zumischProzent, 10m);

        Assert.Equal(schaummittelLiter, result.SchaummittelLiter);
    }

    [Fact]
    public void The_largest_inputs_the_fields_allow_do_not_overflow()
    {
        var vorrat = SchaumRechner.Vorrat(1000000m, null, 0.1m, 1000m);
        var raum = SchaumRechner.Raum(1000m, 1000m, 1000m, 500m, 50m, 1m);

        Assert.True(vorrat.SchaumKubikmeter > 0m);
        Assert.True(raum.GemischLiter > 0m);
    }
}
