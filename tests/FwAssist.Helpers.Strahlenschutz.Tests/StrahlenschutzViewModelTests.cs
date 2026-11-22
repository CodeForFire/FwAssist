using FwAssist.AppLogic.Helpers;
using FwAssist.AppLogic.Services;
using FwAssist.AppLogic.ViewModels;

namespace FwAssist.Helpers.Strahlenschutz.Tests;

public class StrahlenschutzViewModelTests
{
    private readonly InMemoryHelperSettings _settings = new();

    [Fact]
    public void A_new_page_opens_on_tkz_with_nothing_to_show()
    {
        var vm = Page();

        Assert.True(vm.IsTkz);
        Assert.False(vm.HasResults);
        Assert.Equal("1", vm.TkzAbstand.Text);
        Assert.Equal("25", vm.Grenzwert.Text);
    }

    [Fact]
    public void A_tkz_at_one_metre_gives_one_line()
    {
        var vm = Page();

        vm.Tkz.Text = "1";

        Assert.Equal([("In 1 m", "10 µSv/h")], Lines(vm));
    }

    [Fact]
    public void A_tkz_at_another_distance_adds_that_distance()
    {
        var vm = Page();

        vm.Tkz.Text = "1";
        vm.TkzAbstand.Text = "2";

        Assert.Equal([("In 1 m", "10 µSv/h"), ("In 2 m", "2,5 µSv/h")], Lines(vm));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Tkz_needs_both_of_its_values(bool clearTkz)
    {
        var vm = Page();
        vm.Tkz.Text = "1";

        (clearTkz ? vm.Tkz : vm.TkzAbstand).Text = string.Empty;

        Assert.False(vm.HasResults);
    }

    [Fact]
    public void Abstand_gives_the_dose_rate_at_the_new_distance()
    {
        var vm = Measured(Modus.Abstand, "100", "1");

        vm.Zielabstand.Text = "2";

        Assert.Equal([("In 2 m", "25 µSv/h")], Lines(vm));
    }

    [Fact]
    public void A_typed_distance_reads_as_typed()
    {
        var vm = Measured(Modus.Abstand, "100", "1");

        vm.Zielabstand.Text = "2,5";

        Assert.Equal([("In 2,5 m", "16 µSv/h")], Lines(vm));
    }

    [Fact]
    public void From_one_thousand_microsievert_the_result_reads_in_millisievert()
    {
        var vm = Measured(Modus.Abstand, "1000", "2");

        vm.Zielabstand.Text = "1";

        Assert.Equal([("In 1 m", "4,0 mSv/h")], Lines(vm));
    }

    [Fact]
    public void Exactly_one_thousand_microsievert_reads_as_one_millisievert()
    {
        var vm = Measured(Modus.Abstand, "1000", "1");

        vm.Zielabstand.Text = "1";

        Assert.Equal([("In 1 m", "1,0 mSv/h")], Lines(vm));
    }

    [Fact]
    public void Just_below_one_thousand_microsievert_the_result_stays_in_microsievert()
    {
        var vm = Measured(Modus.Abstand, "990", "1");

        vm.Zielabstand.Text = "1";

        Assert.Equal([("In 1 m", "990 µSv/h")], Lines(vm));
    }

    [Theory]
    [InlineData("1", "3", "0,12 µSv/h")]
    [InlineData("1", "10", "0,010 µSv/h")]
    [InlineData("50", "1", "50 µSv/h")]
    [InlineData("5", "1", "5,0 µSv/h")]
    public void A_result_keeps_two_significant_figures(string dosis, string ziel, string expected)
    {
        var vm = Measured(Modus.Abstand, dosis, "1");

        vm.Zielabstand.Text = ziel;

        Assert.Equal(expected, Assert.Single(vm.Results).Value);
    }

    [Fact]
    public void A_result_below_the_smallest_shown_value_says_so()
    {
        var vm = Measured(Modus.Abstand, "0,01", "1");

        vm.Zielabstand.Text = "100";

        Assert.Equal([("In 100 m", "unter 0,01 µSv/h")], Lines(vm));
    }

    [Fact]
    public void A_missing_or_wrong_value_shows_no_result()
    {
        var vm = Measured(Modus.Abstand, "100", "1");

        Assert.False(vm.HasResults);

        vm.Zielabstand.Text = "0";

        Assert.False(vm.HasResults);
        Assert.False(vm.Zielabstand.IsValid);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Abstand_needs_the_measurement(bool clearDosis)
    {
        var vm = Measured(Modus.Abstand, "100", "1");
        vm.Zielabstand.Text = "2";

        (clearDosis ? vm.Dosisleistung : vm.Messabstand).Text = string.Empty;

        Assert.False(vm.HasResults);
    }

    [Fact]
    public void Absperrung_gives_the_distance_where_the_grenzwert_is_reached()
    {
        var vm = Measured(Modus.Absperrung, "100", "1");

        Assert.Equal([("Absperrgrenze", "2 m")], Lines(vm));
    }

    [Fact]
    public void A_changed_grenzwert_moves_the_absperrgrenze()
    {
        var vm = Measured(Modus.Absperrung, "100", "1");

        vm.Grenzwert.Text = "1";

        Assert.Equal([("Absperrgrenze", "10 m")], Lines(vm));
    }

    [Fact]
    public void A_long_absperrgrenze_reads_with_grouping()
    {
        var vm = Measured(Modus.Absperrung, "10000000", "1");

        vm.Grenzwert.Text = "1";

        Assert.Equal([("Absperrgrenze", "3.163 m")], Lines(vm));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Absperrung_needs_all_three_of_its_values(int cleared)
    {
        var vm = Measured(Modus.Absperrung, "100", "1");
        NumberField[] fields = [vm.Dosisleistung, vm.Messabstand, vm.Grenzwert];

        fields[cleared].Text = string.Empty;

        Assert.False(vm.HasResults);
    }

    [Fact]
    public void The_preset_sets_the_grenzwert_back_to_the_gefahrenbereich()
    {
        var vm = Page();
        vm.Grenzwert.Text = "3";

        vm.SetGrenzwertCommand.Execute(StrahlenschutzViewModel.GrenzwertPreset);

        Assert.Equal("25", vm.Grenzwert.Text);
    }

    [Fact]
    public void Switching_mode_shows_that_modes_result()
    {
        var vm = Measured(Modus.Tkz, "100", "1");
        vm.Tkz.Text = "1";

        vm.IsAbsperrung = true;

        Assert.Equal([("Absperrgrenze", "2 m")], Lines(vm));
        Assert.False(vm.IsTkz);
        Assert.False(vm.IsAbstand);

        vm.IsTkz = true;

        Assert.Equal([("In 1 m", "10 µSv/h")], Lines(vm));
        Assert.False(vm.IsAbsperrung);
    }

    [Fact]
    public void Switching_mode_announces_every_mode_button_and_the_hinweis()
    {
        var vm = Page();
        var announced = new List<string?>();
        vm.PropertyChanged += (_, e) => announced.Add(e.PropertyName);

        vm.IsAbstand = true;

        Assert.Contains(nameof(StrahlenschutzViewModel.IsTkz), announced);
        Assert.Contains(nameof(StrahlenschutzViewModel.IsAbstand), announced);
        Assert.Contains(nameof(StrahlenschutzViewModel.IsAbsperrung), announced);
        Assert.Contains(nameof(StrahlenschutzViewModel.Hinweis), announced);
    }

    [Fact]
    public void Clearing_a_mode_button_leaves_the_mode_alone()
    {
        var vm = Page();
        vm.IsAbstand = true;

        vm.IsAbstand = false;
        vm.IsTkz = false;
        vm.IsAbsperrung = false;

        Assert.Equal(Modus.Abstand, vm.Modus);
    }

    [Fact]
    public void Mode_and_values_survive_a_new_page_over_the_same_settings()
    {
        var first = Measured(Modus.Absperrung, "100", "1");
        first.Grenzwert.Text = "4";

        var second = Page();

        Assert.True(second.IsAbsperrung);
        Assert.Equal("4", second.Grenzwert.Text);
        Assert.Equal([("Absperrgrenze", "5 m")], Lines(second));
        Assert.Equal("Absperrung", _settings.Get("strahlenschutz.modus"));
        Assert.Equal("100", _settings.Get("strahlenschutz.dosisleistung"));
    }

    [Theory]
    [InlineData("99")]
    [InlineData("Unsinn")]
    public void A_stored_mode_that_means_nothing_opens_on_tkz(string stored)
    {
        _settings.Set("strahlenschutz.modus", stored);

        Assert.True(Page().IsTkz);
    }

    [Fact]
    public void Reset_clears_the_values_and_keeps_the_grenzwert()
    {
        var vm = Measured(Modus.Tkz, "100", "1");
        vm.Tkz.Text = "1";
        vm.TkzAbstand.Text = "3";
        vm.Zielabstand.Text = "2";
        vm.Grenzwert.Text = "4";

        vm.ResetCommand.Execute(null);

        Assert.Equal(string.Empty, vm.Tkz.Text);
        Assert.Equal("1", vm.TkzAbstand.Text);
        Assert.Equal(string.Empty, vm.Dosisleistung.Text);
        Assert.Equal(string.Empty, vm.Messabstand.Text);
        Assert.Equal(string.Empty, vm.Zielabstand.Text);
        Assert.Equal("4", vm.Grenzwert.Text);
        Assert.False(vm.HasResults);
    }

    [Fact]
    public void Each_mode_says_what_its_result_does_not_know()
    {
        var vm = Page();
        Assert.Equal("Gilt nur für ein unbeschädigtes Versandstück. TKZ 0 zählt als 0,05.", vm.Hinweis);

        vm.IsAbstand = true;
        Assert.Equal("Gilt für eine punktförmige Quelle. Nah an großen Quellen fällt die Dosisleistung langsamer.", vm.Hinweis);

        vm.IsAbsperrung = true;
        Assert.Equal(
            "Gilt für eine punktförmige Quelle. Nah an großen Quellen fällt die Dosisleistung langsamer. Gefahrenbereich mindestens 50 m.",
            vm.Hinweis);
    }

    [Fact]
    public void The_sources_are_named()
    {
        Assert.Contains("ADR 5.1.5.3.1", StrahlenschutzViewModel.Quellen, StringComparison.Ordinal);
        Assert.Contains("Taschenkarte ABC", StrahlenschutzViewModel.Quellen, StringComparison.Ordinal);
    }

    [Fact]
    public void The_module_keeps_its_values_in_the_store_it_is_given()
    {
        var page = (StrahlenschutzViewModel)new StrahlenschutzModule()
            .CreateViewModel(new HelperServices(TimeProvider.System, new NoTicker(), _settings));

        page.Tkz.Text = "2";

        Assert.Equal("2", _settings.Get("strahlenschutz.tkz"));
    }

    private static (string Label, string Value)[] Lines(StrahlenschutzViewModel vm) =>
        [.. vm.Results.Select(r => (r.Label, r.Value))];

    private StrahlenschutzViewModel Page() => new(_settings);

    private StrahlenschutzViewModel Measured(Modus modus, string dosis, string messabstand)
    {
        var vm = Page();
        vm.Modus = modus;
        vm.Dosisleistung.Text = dosis;
        vm.Messabstand.Text = messabstand;
        return vm;
    }

    private sealed class NoTicker : ITicker
    {
        public IDisposable Subscribe(Action onTick) => throw new NotSupportedException();
    }
}
