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
        Assert.Contains(nameof(vm.IsUnNummer), changed);
        Assert.Contains(nameof(vm.IsRohrleitung), changed);
    }

    // The UN-Nummer group has no pictures, so the heading over the list must go when it is chosen.
    [Fact]
    public void Choosing_the_un_nummer_group_notifies_that_the_list_is_empty()
    {
        var vm = new KennzeichnungenViewModel(_settings);
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.IsUnNummer = true;

        Assert.Contains(nameof(vm.HasEntries), changed);
        Assert.False(vm.HasEntries);
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

    [Fact]
    public void The_un_nummer_group_shows_its_hint_and_no_pictures()
    {
        var vm = new KennzeichnungenViewModel(_settings) { IsUnNummer = true };

        Assert.True(vm.IsUnNummer);
        Assert.False(vm.IsWarntafel);
        Assert.Empty(vm.Entries);
        Assert.False(vm.HasEntries);
        Assert.True(vm.HasGroupNote);
        Assert.Contains("UN-Nummer", vm.GroupNote, StringComparison.Ordinal);
        Assert.False(vm.HasStoffe);
        Assert.False(vm.HasNoHits);
        Assert.True(new KennzeichnungenViewModel(_settings).IsUnNummer);
    }

    [Theory]
    [InlineData("1203")]
    [InlineData("UN 1203")]
    public void A_un_nummer_shows_its_stoff_and_no_gefahrnummer(string query)
    {
        var vm = new KennzeichnungenViewModel(_settings) { Query = query };

        Assert.Equal("BENZIN oder OTTOKRAFTSTOFF", Assert.Single(vm.Stoffe).Benennung);
        Assert.Equal("UN 1203", vm.StoffeTitle);
        Assert.True(vm.HasStoffe);
        Assert.False(vm.HasGefahrnummer);
        Assert.False(vm.HasNoHits);
    }

    [Fact]
    public void A_un_nummer_with_several_rows_counts_them()
    {
        var vm = new KennzeichnungenViewModel(_settings) { Query = "1202" };

        Assert.Equal(3, vm.Stoffe.Count);
        Assert.Equal("UN 1202: 3 EINTRAGUNGEN", vm.StoffeTitle);
    }

    [Fact]
    public void A_gefahrnummer_lists_no_stoffe()
    {
        var vm = new KennzeichnungenViewModel(_settings) { Query = "33" };

        Assert.True(vm.HasGefahrnummer);
        Assert.False(vm.HasStoffe);
        Assert.Equal(string.Empty, vm.StoffeTitle);
    }

    [Fact]
    public void A_word_finds_stoffe_too_and_counts_them()
    {
        var vm = new KennzeichnungenViewModel(_settings) { Query = "ammoniak" };

        Assert.True(vm.Stoffe.Count > 1);
        Assert.Equal(vm.Stoffe.Count + " STOFFE IN TABELLE A", vm.StoffeTitle);
        Assert.False(vm.HasNoHits);
    }

    [Fact]
    public void An_unknown_un_nummer_says_nothing_was_found()
    {
        var vm = new KennzeichnungenViewModel(_settings) { Query = "9999" };

        Assert.True(vm.HasNoHits);
        Assert.Empty(vm.Stoffe);
    }

    // "trocken" finds exactly 50 rows and "verbindung" 51: the limit itself is still shown whole.
    [Fact]
    public void Exactly_fifty_stoffe_are_shown_whole()
    {
        var vm = new KennzeichnungenViewModel(_settings) { Query = "trocken" };

        Assert.Equal(KennzeichnungenViewModel.MaxStoffe, vm.Stoffe.Count);
        Assert.False(vm.HasMoreStoffe);
        Assert.Equal(string.Empty, vm.MoreStoffeNote);
        Assert.Equal("50 STOFFE IN TABELLE A", vm.StoffeTitle);
    }

    [Fact]
    public void More_than_fifty_stoffe_show_the_first_fifty_and_say_so()
    {
        var vm = new KennzeichnungenViewModel(_settings) { Query = "verbindung" };

        Assert.Equal(KennzeichnungenViewModel.MaxStoffe, vm.Stoffe.Count);
        Assert.True(vm.HasMoreStoffe);
        Assert.Contains("ersten 50", vm.MoreStoffeNote, StringComparison.Ordinal);
        Assert.Equal("51 STOFFE IN TABELLE A", vm.StoffeTitle);
    }

    [Fact]
    public void The_stoffe_are_looked_up_once_per_query()
    {
        var vm = new KennzeichnungenViewModel(_settings) { Query = "ammoniak" };

        Assert.Same(vm.Stoffe, vm.Stoffe);
    }

    [Fact]
    public void A_new_query_replaces_the_stoffe_and_tells_the_view()
    {
        var vm = new KennzeichnungenViewModel(_settings) { Query = "1203" };
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.Query = "1005";

        Assert.Equal("1005", Assert.Single(vm.Stoffe).UnNummer);
        Assert.Contains(nameof(vm.Stoffe), changed);
        Assert.Contains(nameof(vm.HasStoffe), changed);
        Assert.Contains(nameof(vm.StoffeTitle), changed);
        Assert.Contains(nameof(vm.HasMoreStoffe), changed);
        Assert.Contains(nameof(vm.MoreStoffeNote), changed);
    }

    [Fact]
    public void Clearing_the_search_empties_the_stoffe()
    {
        var vm = new KennzeichnungenViewModel(_settings) { Query = "1203" };

        vm.ClearCommand.Execute(null);

        Assert.Empty(vm.Stoffe);
        Assert.False(vm.HasStoffe);
    }
}
