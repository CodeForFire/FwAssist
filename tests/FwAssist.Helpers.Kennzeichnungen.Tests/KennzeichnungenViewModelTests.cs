using FwAssist.AppLogic.Services;

namespace FwAssist.Helpers.Kennzeichnungen.Tests;

public class KennzeichnungenViewModelTests
{
    private readonly InMemoryHelperSettings _settings = new();

    [Fact]
    public void Starts_on_the_warntafeln()
    {
        var vm = new KennzeichnungenViewModel(_settings);

        Assert.True(vm.IsWarntafel);
        Assert.False(vm.IsGefahrzettel);
        Assert.False(vm.IsGhs);
        Assert.Same(Kennzeichenkatalog.Warntafeln, vm.Entries);
        Assert.False(vm.IsSearching);
    }

    [Fact]
    public void Choosing_a_group_shows_only_that_group()
    {
        var vm = new KennzeichnungenViewModel(_settings) { IsGefahrzettel = true };
        Assert.Same(Kennzeichenkatalog.Gefahrzettel, vm.Entries);
        Assert.False(vm.IsWarntafel);

        vm.IsGhs = true;
        Assert.Same(Kennzeichenkatalog.Ghs, vm.Entries);
        Assert.False(vm.IsGefahrzettel);
    }

    [Fact]
    public void Choosing_rohrleitung_or_gasflasche_shows_that_group_with_its_note()
    {
        var vm = new KennzeichnungenViewModel(_settings) { IsRohrleitung = true };
        Assert.Same(Kennzeichenkatalog.Rohrleitungen, vm.Entries);
        Assert.True(vm.HasGroupNote);
        Assert.Contains("Beschriftung am Rohr", vm.GroupNote, StringComparison.Ordinal);

        vm.IsGasflasche = true;
        Assert.Same(Kennzeichenkatalog.Gasflaschen, vm.Entries);
        Assert.True(vm.IsGasflasche);
        Assert.False(vm.IsRohrleitung);
        Assert.Contains("Gefahrgutaufkleber", vm.GroupNote, StringComparison.Ordinal);
    }

    [Fact]
    public void The_pictogram_groups_carry_no_note()
    {
        var vm = new KennzeichnungenViewModel(_settings) { IsGhs = true };

        Assert.False(vm.HasGroupNote);
        Assert.Equal(string.Empty, vm.GroupNote);
    }

    [Fact]
    public void A_search_hides_the_group_note()
    {
        var vm = new KennzeichnungenViewModel(_settings) { IsGasflasche = true, Query = "Ammoniak" };

        Assert.False(vm.HasGroupNote);
        Assert.Equal("flasche-gelb", Assert.Single(vm.Entries).PictureKey);
    }

    [Fact]
    public void Choosing_a_group_notifies_its_note()
    {
        var vm = new KennzeichnungenViewModel(_settings);
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.IsGasflasche = true;

        Assert.Contains(nameof(vm.GroupNote), changed);
        Assert.Contains(nameof(vm.HasGroupNote), changed);
        Assert.Contains(nameof(vm.IsGasflasche), changed);
        Assert.Contains(nameof(vm.IsRohrleitung), changed);
    }

    [Fact]
    public void A_radio_button_losing_its_check_changes_nothing()
    {
        var vm = new KennzeichnungenViewModel(_settings) { IsGhs = true };

        vm.IsWarntafel = false;
        vm.IsGefahrzettel = false;
        vm.IsRohrleitung = false;
        vm.IsGasflasche = false;

        Assert.True(vm.IsGhs);
    }

    [Fact]
    public void The_group_survives_a_process_death()
    {
        _ = new KennzeichnungenViewModel(_settings) { IsGefahrzettel = true };

        var revived = new KennzeichnungenViewModel(_settings);

        Assert.True(revived.IsGefahrzettel);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Unbekannt")]
    [InlineData("7")]
    public void An_unreadable_stored_group_falls_back_to_the_warntafeln(string stored)
    {
        _settings.Set(KennzeichnungenModule.HelperId + ".gruppe", stored);

        Assert.True(new KennzeichnungenViewModel(_settings).IsWarntafel);
    }

    [Fact]
    public void A_search_finds_entries_in_every_group()
    {
        var vm = new KennzeichnungenViewModel(_settings) { Query = "Gasflasche" };

        Assert.True(vm.IsSearching);
        Assert.Equal("TREFFER IN ALLEN GRUPPEN", vm.ListTitle);
        Assert.Contains(vm.Entries, e => e.PictureKey == "adr-2.2");
        Assert.Contains(vm.Entries, e => e.PictureKey == "ghs04");
    }

    [Theory]
    [InlineData("atzend")]
    [InlineData("ÄTZEND")]
    [InlineData(" ätzend ")]
    public void A_search_ignores_case_umlauts_and_spaces_around_it(string query)
    {
        var vm = new KennzeichnungenViewModel(_settings) { Query = query };

        Assert.Contains(vm.Entries, e => e.PictureKey == "adr-8");
    }

    [Fact]
    public void A_search_matches_the_code()
    {
        var vm = new KennzeichnungenViewModel(_settings) { Query = "GHS07" };

        Assert.Equal("ghs07", Assert.Single(vm.Entries).PictureKey);
    }

    [Fact]
    public void Clearing_the_search_returns_to_the_chosen_group()
    {
        var vm = new KennzeichnungenViewModel(_settings) { IsGhs = true, Query = "Flamme" };

        vm.ClearCommand.Execute(null);

        Assert.Equal(string.Empty, vm.Query);
        Assert.Same(Kennzeichenkatalog.Ghs, vm.Entries);
    }

    [Fact]
    public void A_typed_gefahrnummer_is_read_on_screen()
    {
        var vm = new KennzeichnungenViewModel(_settings) { Query = "336" };

        Assert.True(vm.HasGefahrnummer);
        Assert.Equal("leicht entzündbarer flüssiger Stoff, giftig", vm.GefahrnummerMeaning);
        Assert.Equal(3, vm.GefahrnummerNotes.Count);
        Assert.StartsWith("6  Giftigkeit", vm.GefahrnummerNotes[2], StringComparison.Ordinal);
        Assert.False(vm.HasNoHits);
        Assert.False(vm.HasEntries);
    }

    [Fact]
    public void A_group_and_a_search_with_hits_have_entries()
    {
        Assert.True(new KennzeichnungenViewModel(_settings).HasEntries);
        Assert.True(new KennzeichnungenViewModel(_settings) { Query = "Flamme" }.HasEntries);
    }

    [Fact]
    public void An_x_number_carries_the_water_warning_last()
    {
        var vm = new KennzeichnungenViewModel(_settings) { Query = "X423" };

        Assert.Equal([Gefahrnummern.SpecialNote, Gefahrnummern.WaterNote], vm.GefahrnummerNotes);
    }

    [Fact]
    public void An_unlisted_number_warns_to_check_the_plate()
    {
        var vm = new KennzeichnungenViewModel(_settings) { Query = "57" };

        Assert.Equal(Gefahrnummern.UnlistedNote, vm.GefahrnummerMeaning);
    }

    [Fact]
    public void A_word_is_no_gefahrnummer()
    {
        var vm = new KennzeichnungenViewModel(_settings) { Query = "Flamme" };

        Assert.False(vm.HasGefahrnummer);
        Assert.Equal(string.Empty, vm.GefahrnummerMeaning);
        Assert.Empty(vm.GefahrnummerNotes);
    }

    [Fact]
    public void A_search_without_hits_says_so()
    {
        var vm = new KennzeichnungenViewModel(_settings) { Query = "Quarkspeise" };

        Assert.True(vm.HasNoHits);
        Assert.Empty(vm.Entries);
    }

    [Fact]
    public void Typing_notifies_everything_the_screen_reads()
    {
        var vm = new KennzeichnungenViewModel(_settings);
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.Query = "33";

        Assert.Contains(nameof(vm.Entries), changed);
        Assert.Contains(nameof(vm.ListTitle), changed);
        Assert.Contains(nameof(vm.HasGefahrnummer), changed);
        Assert.Contains(nameof(vm.GefahrnummerMeaning), changed);
        Assert.Contains(nameof(vm.GefahrnummerNotes), changed);
        Assert.Contains(nameof(vm.HasNoHits), changed);
        Assert.Contains(nameof(vm.HasEntries), changed);
    }
}
