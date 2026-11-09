using FwAssist.AppLogic.Helpers;
using FwAssist.AppLogic.Services;

namespace FwAssist.Helpers.Schaummittel.Tests;

public class SchaummittelViewModelTests
{
    private readonly InMemoryHelperSettings _settings = new();

    private static string? Line(SchaummittelViewModel vm, string label) =>
        vm.Results.SingleOrDefault(r => r.Label == label)?.Value;

    private SchaummittelViewModel FilledVorrat()
    {
        var vm = new SchaummittelViewModel(_settings);
        vm.Verschaeumungszahl.Text = "10";
        vm.SchaummittelVorrat.Text = "100";
        return vm;
    }

    private SchaummittelViewModel FilledRaum()
    {
        var vm = new SchaummittelViewModel(_settings) { IsRaum = true };
        vm.Verschaeumungszahl.Text = "10";
        vm.Laenge.Text = "10";
        vm.Breite.Text = "5";
        vm.Hoehe.Text = "3";
        return vm;
    }

    [Fact]
    public void Starts_on_vorrat_at_three_percent_with_no_result()
    {
        var vm = new SchaummittelViewModel(_settings);

        Assert.True(vm.IsVorrat);
        Assert.False(vm.IsRaum);
        Assert.Equal("3", vm.Zumischrate.Text);
        Assert.Equal("0", vm.Zuschlag.Text);
        Assert.Empty(vm.Verschaeumungszahl.Text);
        Assert.False(vm.HasResults);
    }

    [Fact]
    public void Vorrat_shows_what_the_supplies_make()
    {
        var vm = FilledVorrat();

        Assert.True(vm.HasResults);
        Assert.Equal("33,3 m³", Line(vm, "Schaum"));
        Assert.Equal("3.333 l", Line(vm, "Gemisch"));
        Assert.Equal("100 l", Line(vm, "Schaummittel"));
        Assert.Equal("3.234 l", Line(vm, "Wasser"));
        Assert.Equal("Reicht bis das Schaummittel leer ist.", vm.ResultNote);
    }

    [Fact]
    public void Vorrat_names_the_water_when_the_tank_runs_out_first()
    {
        var vm = FilledVorrat();

        vm.WasserVorrat.Text = "970";

        Assert.Equal("30 l", Line(vm, "Schaummittel"));
        Assert.Equal("Reicht bis das Wasser leer ist.", vm.ResultNote);
    }

    [Fact]
    public void Raum_shows_what_the_room_needs_and_names_the_zuschlag()
    {
        var vm = FilledRaum();

        Assert.Equal("150,0 m³", Line(vm, "Raum"));
        Assert.Equal("150,0 m³", Line(vm, "Schaum"));
        Assert.Equal("15.000 l", Line(vm, "Gemisch"));
        Assert.Equal("450 l", Line(vm, "Schaummittel"));
        Assert.Equal("14.550 l", Line(vm, "Wasser"));
        Assert.Equal("Mit 0 % Zuschlag für Verluste.", vm.ResultNote);
    }

    [Fact]
    public void A_zuschlag_raises_the_need_and_the_note_says_so()
    {
        var vm = FilledRaum();

        vm.Zuschlag.Text = "50";

        Assert.Equal("225,0 m³", Line(vm, "Schaum"));
        Assert.Equal("Mit 50 % Zuschlag für Verluste.", vm.ResultNote);
    }

    [Fact]
    public void An_empty_zuschlag_counts_as_none()
    {
        var vm = FilledRaum();

        vm.Zuschlag.Text = string.Empty;

        Assert.True(vm.HasResults);
        Assert.Equal("Mit 0 % Zuschlag für Verluste.", vm.ResultNote);
    }

    [Fact]
    public void A_decimal_comma_works()
    {
        var vm = FilledRaum();

        vm.Hoehe.Text = "2,5";

        Assert.Equal("125,0 m³", Line(vm, "Raum"));
    }

    [Fact]
    public void A_missing_field_shows_no_result_and_no_error()
    {
        var vm = FilledRaum();

        vm.Breite.Text = string.Empty;

        Assert.False(vm.HasResults);
        Assert.Null(vm.Breite.Error);
    }

    [Fact]
    public void Each_mode_needs_only_its_own_fields()
    {
        var vm = FilledRaum();

        vm.SchaummittelVorrat.Text = "kaputt";

        Assert.True(vm.HasResults);

        vm.IsRaum = false;

        Assert.False(vm.HasResults);
    }

    [Fact]
    public void Switching_mode_recalculates()
    {
        var vm = FilledVorrat();
        vm.Laenge.Text = "1";
        vm.Breite.Text = "1";
        vm.Hoehe.Text = "1";

        vm.IsVorrat = false;

        Assert.True(vm.IsRaum);
        Assert.Equal("1,0 m³", Line(vm, "Raum"));
    }

    [Theory]
    [InlineData("abc", "Zahl eingeben.")]
    [InlineData("0", "Von 0,1 bis 50 % möglich.")]
    [InlineData("0,09", "Von 0,1 bis 50 % möglich.")]
    [InlineData("50,1", "Von 0,1 bis 50 % möglich.")]
    [InlineData("300", "Von 0,1 bis 50 % möglich.")]
    public void A_bad_zumischrate_says_why_and_shows_no_result(string text, string error)
    {
        var vm = FilledVorrat();

        vm.Zumischrate.Text = text;

        Assert.Equal(error, vm.Zumischrate.Error);
        Assert.False(vm.HasResults);
    }

    [Theory]
    [InlineData("0,1")]
    [InlineData("50")]
    public void The_ends_of_the_zumischrate_range_are_allowed(string text)
    {
        var vm = FilledVorrat();

        vm.Zumischrate.Text = text;

        Assert.Null(vm.Zumischrate.Error);
        Assert.True(vm.HasResults);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("1001")]
    public void A_bad_verschaeumungszahl_shows_no_result(string text)
    {
        var vm = FilledVorrat();

        vm.Verschaeumungszahl.Text = text;

        Assert.Equal("Von 1 bis 1.000 möglich.", vm.Verschaeumungszahl.Error);
        Assert.False(vm.HasResults);
    }

    [Fact]
    public void A_bad_optional_water_shows_no_result()
    {
        var vm = FilledVorrat();

        vm.WasserVorrat.Text = "abc";

        Assert.False(vm.HasResults);
    }

    [Fact]
    public void A_bad_zuschlag_shows_no_result()
    {
        var vm = FilledRaum();

        vm.Zuschlag.Text = "501";

        Assert.False(vm.HasResults);
    }

    [Fact]
    public void Correcting_a_bad_value_clears_its_error()
    {
        var vm = FilledVorrat();
        vm.Zumischrate.Text = "abc";

        vm.Zumischrate.Text = "3";

        Assert.Null(vm.Zumischrate.Error);
        Assert.True(vm.HasResults);
    }

    [Fact]
    public void A_bad_value_tells_the_view_to_show_the_error()
    {
        var vm = FilledVorrat();
        var changed = new List<string?>();
        vm.Zumischrate.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.Zumischrate.Text = "abc";

        Assert.Contains(nameof(vm.Zumischrate.Error), changed);
        Assert.Contains(nameof(vm.Zumischrate.IsValid), changed);
        Assert.False(vm.Zumischrate.IsValid);
    }

    [Fact]
    public void A_null_from_the_binding_reads_as_empty()
    {
        var vm = FilledVorrat();

        foreach (var field in new[]
        {
            vm.Zumischrate, vm.Verschaeumungszahl, vm.SchaummittelVorrat, vm.WasserVorrat,
            vm.Laenge, vm.Breite, vm.Hoehe, vm.Zuschlag,
        })
        {
            field.Text = null;

            Assert.Equal(string.Empty, field.Text);
            Assert.Null(field.Error);
        }

        Assert.False(vm.HasResults);
    }

    [Fact]
    public void The_quick_buttons_fill_the_fields()
    {
        var vm = new SchaummittelViewModel(_settings);

        vm.SetZumischrateCommand.Execute("6");
        vm.SetVerschaeumungszahlCommand.Execute("75");

        Assert.Equal("6", vm.Zumischrate.Text);
        Assert.Equal("75", vm.Verschaeumungszahl.Text);
    }

    [Fact]
    public void One_keystroke_calculates_once()
    {
        var vm = FilledVorrat();
        var changes = 0;
        vm.PropertyChanged += (_, e) => changes += e.PropertyName == nameof(vm.Results) ? 1 : 0;

        vm.SchaummittelVorrat.Text = "200";

        Assert.Equal(1, changes);
    }

    [Fact]
    public void Reset_clears_the_amounts_and_keeps_the_foam_settings()
    {
        var vm = FilledRaum();
        vm.WasserVorrat.Text = "970";
        vm.SchaummittelVorrat.Text = "100";
        vm.Zuschlag.Text = "50";
        vm.Zumischrate.Text = "6";

        vm.ResetCommand.Execute(null);

        Assert.Empty(vm.Laenge.Text);
        Assert.Empty(vm.Breite.Text);
        Assert.Empty(vm.Hoehe.Text);
        Assert.Empty(vm.SchaummittelVorrat.Text);
        Assert.Empty(vm.WasserVorrat.Text);
        Assert.Equal("0", vm.Zuschlag.Text);
        Assert.Equal("6", vm.Zumischrate.Text);
        Assert.Equal("10", vm.Verschaeumungszahl.Text);
        Assert.False(vm.HasResults);
    }

    [Fact]
    public void Everything_typed_survives_a_process_death()
    {
        var before = FilledRaum();
        before.Zumischrate.Text = "6";
        before.Zuschlag.Text = "50";
        before.SchaummittelVorrat.Text = "100";
        before.WasserVorrat.Text = "970";

        var after = new SchaummittelViewModel(_settings);

        Assert.True(after.IsRaum);
        Assert.Equal("6", after.Zumischrate.Text);
        Assert.Equal("10", after.Verschaeumungszahl.Text);
        Assert.Equal("50", after.Zuschlag.Text);
        Assert.Equal("10", after.Laenge.Text);
        Assert.Equal("5", after.Breite.Text);
        Assert.Equal("3", after.Hoehe.Text);
        Assert.Equal("100", after.SchaummittelVorrat.Text);
        Assert.Equal("970", after.WasserVorrat.Text);
        Assert.Equal(before.Results, after.Results);
    }

    [Fact]
    public void Switching_mode_survives_a_process_death_in_both_directions()
    {
        var vm = new SchaummittelViewModel(_settings) { IsRaum = true };
        Assert.True(new SchaummittelViewModel(_settings).IsRaum);

        vm.IsRaum = false;

        Assert.False(new SchaummittelViewModel(_settings).IsRaum);
    }

    [Fact]
    public void A_corrupt_stored_value_shows_the_normal_error()
    {
        _settings.Set("schaummittel.zumischrate", "kaputt");

        var vm = new SchaummittelViewModel(_settings);

        Assert.Equal("kaputt", vm.Zumischrate.Text);
        Assert.Equal("Zahl eingeben.", vm.Zumischrate.Error);
    }

    [Fact]
    public void The_module_builds_a_page_that_works_without_a_settings_store()
    {
        var module = new SchaummittelModule();

        var page = module.CreateViewModel(new HelperServices(TimeProvider.System, new NoTicker()));

        Assert.IsType<SchaummittelViewModel>(page);
        Assert.Equal("schaummittel", module.Id);
    }

    [Fact]
    public void The_module_keeps_its_values_in_the_store_it_is_given()
    {
        var page = (SchaummittelViewModel)new SchaummittelModule()
            .CreateViewModel(new HelperServices(TimeProvider.System, new NoTicker(), _settings));

        page.Verschaeumungszahl.Text = "75";

        Assert.Equal("75", _settings.Get("schaummittel.verschaeumungszahl"));
    }

    private sealed class NoTicker : ITicker
    {
        public IDisposable Subscribe(Action onTick) => throw new NotSupportedException();
    }
}
