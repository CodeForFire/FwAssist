using FwAssist.AppLogic.Helpers;
using FwAssist.AppLogic.Services;

namespace FwAssist.Helpers.Funkalphabet.Tests;

public class FunkalphabetViewModelTests
{
    private readonly InMemoryHelperSettings _settings = new();

    [Fact]
    public void Starts_on_the_old_table_with_nothing_spelled()
    {
        var vm = new FunkalphabetViewModel(_settings);

        Assert.Equal("BOS (DIN 5009:1983)", vm.TableName);
        Assert.True(vm.IsAlt);
        Assert.False(vm.IsDin2022);
        Assert.False(vm.HasSpelling);
        Assert.Empty(vm.Spelled);
        Assert.Equal(Buchstabiertafel.Alt.Entries, vm.Entries);
    }

    [Fact]
    public void Typing_spells_the_text()
    {
        var vm = new FunkalphabetViewModel(_settings) { Text = "ab" };

        Assert.True(vm.HasSpelling);
        Assert.Equal(["Anton", "Berta"], vm.Spelled.Select(c => c.Word));
    }

    [Fact]
    public void Clearing_the_text_clears_the_spelling()
    {
        var vm = new FunkalphabetViewModel(_settings) { Text = "ab" };

        vm.Text = string.Empty;

        Assert.False(vm.HasSpelling);
        Assert.Empty(vm.Spelled);
    }

    [Fact]
    public void A_null_from_the_binding_spells_nothing()
    {
        var vm = new FunkalphabetViewModel(_settings) { Text = "ab" };

        vm.Text = null;

        Assert.False(vm.HasSpelling);
        Assert.Empty(vm.Spelled);
    }

    [Fact]
    public void Switching_the_table_respells_the_text_and_changes_the_list()
    {
        var vm = new FunkalphabetViewModel(_settings) { Text = "ab" };

        vm.IsDin2022 = true;

        Assert.Equal("DIN 5009:2022", vm.TableName);
        Assert.True(vm.IsDin2022);
        Assert.False(vm.IsAlt);
        Assert.Equal(["Aachen", "Berlin"], vm.Spelled.Select(c => c.Word));
        Assert.Equal(Buchstabiertafel.Din2022.Entries, vm.Entries);
    }

    [Fact]
    public void Unchecking_a_table_leaves_the_choice_alone()
    {
        var vm = new FunkalphabetViewModel(_settings) { IsNato = true };

        vm.IsAlt = false;
        vm.IsDin2022 = false;
        Assert.True(vm.IsNato);

        vm.IsNato = false;
        Assert.True(vm.IsNato);
    }

    [Fact]
    public void Switching_to_nato_respells_the_text_and_changes_the_list()
    {
        var vm = new FunkalphabetViewModel(_settings) { Text = "ab" };
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.IsNato = true;

        Assert.Equal("NATO", vm.TableName);
        Assert.True(vm.IsNato);
        Assert.False(vm.IsAlt);
        Assert.False(vm.IsDin2022);
        Assert.Equal("Alfa - Bravo", vm.SpelledText);
        Assert.Equal(Buchstabiertafel.Nato.Entries, vm.Entries);
        Assert.Contains(nameof(vm.IsNato), changed);
        Assert.Contains(nameof(vm.SpelledText), changed);
    }

    [Fact]
    public void The_nato_choice_is_stored_and_restored()
    {
        _ = new FunkalphabetViewModel(_settings) { IsNato = true };

        Assert.Equal("nato", _settings.Get("funkalphabet.table"));
        Assert.True(new FunkalphabetViewModel(_settings).IsNato);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("a", "Anton")]
    [InlineData("hlf", "Heinrich - Ludwig - Friedrich")]
    [InlineData("ab c", "Anton - Berta / Cäsar")]
    [InlineData("  a   b  ", "Anton / Berta")]
    public void The_spelling_reads_as_one_line(string text, string expected)
    {
        var vm = new FunkalphabetViewModel(_settings) { Text = text };

        Assert.Equal(expected, vm.SpelledText);
    }

    [Fact]
    public void Reading_the_spelling_twice_does_not_spell_twice()
    {
        var vm = new FunkalphabetViewModel(_settings) { Text = "ab" };

        Assert.Same(vm.Spelled, vm.Spelled);
    }

    [Fact]
    public void Clearing_empties_the_field_and_the_spelling()
    {
        var vm = new FunkalphabetViewModel(_settings) { Text = "ab" };

        vm.ClearCommand.Execute(null);

        Assert.Equal(string.Empty, vm.Text);
        Assert.False(vm.HasSpelling);
        Assert.Empty(vm.Spelled);
    }

    [Fact]
    public void Switching_back_returns_to_the_old_table()
    {
        var vm = new FunkalphabetViewModel(_settings) { Text = "a" };

        vm.IsDin2022 = true;
        vm.IsAlt = true;

        Assert.Equal(["Anton"], vm.Spelled.Select(c => c.Word));
    }

    [Fact]
    public void Switching_the_table_tells_the_view_about_everything_that_changed()
    {
        var vm = new FunkalphabetViewModel(_settings);
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.IsDin2022 = true;

        Assert.Contains(nameof(vm.TableName), changed);
        Assert.Contains(nameof(vm.IsAlt), changed);
        Assert.Contains(nameof(vm.IsDin2022), changed);
        Assert.Contains(nameof(vm.Spelled), changed);
        Assert.Contains(nameof(vm.Entries), changed);
    }

    [Fact]
    public void Typing_tells_the_view_about_the_spelling()
    {
        var vm = new FunkalphabetViewModel(_settings);
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.Text = "a";

        Assert.Contains(nameof(vm.Spelled), changed);
        Assert.Contains(nameof(vm.HasSpelling), changed);
    }

    [Fact]
    public void The_table_choice_survives_a_process_death_in_both_directions()
    {
        var vm = new FunkalphabetViewModel(_settings) { IsDin2022 = true };
        Assert.True(new FunkalphabetViewModel(_settings).IsDin2022);

        vm.IsAlt = true;

        Assert.True(new FunkalphabetViewModel(_settings).IsAlt);
    }

    [Fact]
    public void The_table_choice_is_stored_under_its_permanent_key()
    {
        var vm = new FunkalphabetViewModel(_settings) { IsDin2022 = true };
        Assert.Equal("din2022", _settings.Get("funkalphabet.table"));

        vm.IsAlt = true;

        Assert.Equal("alt", _settings.Get("funkalphabet.table"));
    }

    [Fact]
    public void A_corrupt_stored_table_falls_back_to_alt()
    {
        _settings.Set("funkalphabet.table", "kaputt");

        var vm = new FunkalphabetViewModel(_settings);

        Assert.True(vm.IsAlt);
        Assert.Equal(Buchstabiertafel.Alt.Entries, vm.Entries);
    }

    [Fact]
    public void A_restored_table_spells_with_that_table()
    {
        _settings.Set("funkalphabet.table", "din2022");

        var vm = new FunkalphabetViewModel(_settings) { Text = "a" };

        Assert.Equal("DIN 5009:2022", vm.TableName);
        Assert.Equal(Buchstabiertafel.Din2022.Spell("a"), vm.Spelled);
    }

    [Fact]
    public void The_view_model_needs_a_settings_store() =>
        Assert.Throws<ArgumentNullException>(() => new FunkalphabetViewModel(null!));

    [Fact]
    public void The_module_builds_a_page_that_works_without_a_settings_store()
    {
        var page = new FunkalphabetModule().CreateViewModel(new HelperServices(TimeProvider.System, new NoTicker()));

        Assert.IsType<FunkalphabetViewModel>(page);
    }

    [Fact]
    public void The_module_keeps_the_table_in_the_store_it_is_given()
    {
        var page = (FunkalphabetViewModel)new FunkalphabetModule()
            .CreateViewModel(new HelperServices(TimeProvider.System, new NoTicker(), _settings));

        page.IsDin2022 = true;

        Assert.Equal("din2022", _settings.Get("funkalphabet.table"));
    }

    private sealed class NoTicker : ITicker
    {
        public IDisposable Subscribe(Action onTick) => throw new NotSupportedException();
    }
}
