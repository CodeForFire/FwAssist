using FwAssist.AppLogic.Helpers;
using FwAssist.AppLogic.Services;

namespace FwAssist.Helpers.Sandsack.Tests;

public class SandsackViewModelTests
{
    private readonly InMemoryHelperSettings _settings = new();

    [Fact]
    public void A_new_page_has_nothing_to_show()
    {
        var vm = Page();

        Assert.False(vm.HasResults);
        Assert.Equal(string.Empty, vm.Laenge.Text);
        Assert.Equal(string.Empty, vm.Hoehe.Text);
        Assert.Equal(string.Empty, vm.Helfer.Text);
    }

    [Fact]
    public void Length_and_height_give_sacks_and_sand()
    {
        var vm = Page();

        vm.Laenge.Text = "100";
        vm.Hoehe.Text = "1";

        Assert.Equal(
            [("Sandsäcke", "16.000"), ("Sand", "300,0 m³"), ("Sand, nass", "600,0 t")],
            Lines(vm));
    }

    [Fact]
    public void Helfer_add_the_working_time()
    {
        var vm = Page();

        vm.Laenge.Text = "100";
        vm.Hoehe.Text = "1";
        vm.Helfer.Text = "10";

        Assert.Equal(("Arbeitszeit", "80 h"), Lines(vm)[^1]);
    }

    [Fact]
    public void A_time_with_minutes_shows_them()
    {
        var vm = Page();

        vm.Laenge.Text = "10";
        vm.Hoehe.Text = "0,5";
        vm.Helfer.Text = "4";

        // 450 sacks, twice at 40 an hour, by 4 Helfer: 5,625 h, so 5 h 45 min.
        Assert.Equal(("Arbeitszeit", "5 h 45 min"), Lines(vm)[^1]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Sacks_need_both_length_and_height(bool clearLaenge)
    {
        var vm = Page();
        vm.Laenge.Text = "100";
        vm.Hoehe.Text = "1";

        (clearLaenge ? vm.Laenge : vm.Hoehe).Text = string.Empty;

        Assert.False(vm.HasResults);
    }

    [Fact]
    public void The_hint_names_the_row_used_and_the_base()
    {
        var vm = Page();

        vm.Hoehe.Text = "0,8";

        Assert.StartsWith("Gerechnet für 1,0 m Höhe. Basisbreite mindestens 1,6 m.", vm.Hinweis, StringComparison.Ordinal);
    }

    [Fact]
    public void Without_a_height_the_hint_still_says_how_to_build()
    {
        var vm = Page();

        Assert.StartsWith("Dichter Verbund", vm.Hinweis, StringComparison.Ordinal);
        Assert.Contains("40 Säcke je Helfer und Stunde", vm.Hinweis, StringComparison.Ordinal);
    }

    [Fact]
    public void A_dam_above_two_metres_is_refused_with_its_range()
    {
        var vm = Page();

        vm.Hoehe.Text = "2,5";

        Assert.Equal("Von 0,1 bis 2,0 m möglich.", vm.Hoehe.Error);
        Assert.False(vm.HasResults);
    }

    [Fact]
    public void No_helfer_is_refused_with_its_range()
    {
        var vm = Page();

        vm.Helfer.Text = "0";

        Assert.Equal("Von 1 bis 1.000 möglich.", vm.Helfer.Error);
    }

    [Fact]
    public void Reset_clears_every_field()
    {
        var vm = Page();
        vm.Laenge.Text = "100";
        vm.Hoehe.Text = "1";
        vm.Helfer.Text = "10";

        vm.ResetCommand.Execute(null);

        Assert.Equal(string.Empty, vm.Laenge.Text);
        Assert.Equal(string.Empty, vm.Hoehe.Text);
        Assert.Equal(string.Empty, vm.Helfer.Text);
        Assert.False(vm.HasResults);
    }

    [Fact]
    public void The_values_come_back_after_the_process_was_killed()
    {
        var first = Page();
        first.Laenge.Text = "100";
        first.Hoehe.Text = "1";
        first.Helfer.Text = "10";

        var second = Page();

        Assert.Equal(Lines(first), Lines(second));
    }

    [Fact]
    public void The_module_keeps_its_values_in_the_store_it_is_given()
    {
        var page = (SandsackViewModel)new SandsackModule()
            .CreateViewModel(new HelperServices(TimeProvider.System, new NoTicker(), _settings));

        page.Laenge.Text = "25";

        Assert.Equal("25", _settings.Get("sandsack.laenge"));
    }

    [Fact]
    public void The_sources_are_named_on_screen() =>
        Assert.Contains("Taschenkarte Deichverteidigung", SandsackViewModel.Quellen, StringComparison.Ordinal);

    private static (string Label, string Value)[] Lines(SandsackViewModel vm) =>
        [.. vm.Results.Select(r => (r.Label, r.Value))];

    private SandsackViewModel Page() => new(_settings);

    private sealed class NoTicker : ITicker
    {
        public IDisposable Subscribe(Action onTick) => throw new NotSupportedException();
    }
}
