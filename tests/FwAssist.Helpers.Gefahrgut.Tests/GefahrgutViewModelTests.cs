using FwAssist.AppLogic.Helpers;
using FwAssist.AppLogic.Services;

namespace FwAssist.Helpers.Gefahrgut.Tests;

public class GefahrgutViewModelTests
{
    [Fact]
    public void Starts_empty_with_the_hint()
    {
        var vm = new GefahrgutViewModel();

        Assert.False(vm.IsSearching);
        Assert.Empty(vm.Results);
        Assert.False(vm.HasResults);
        Assert.False(vm.HasNoHits);
        Assert.False(vm.HasMore);
        Assert.Equal(string.Empty, vm.ResultTitle);
        Assert.Equal(string.Empty, vm.MoreNote);
    }

    [Fact]
    public void A_un_nummer_shows_its_stoff()
    {
        var vm = new GefahrgutViewModel { Query = "UN 1203" };

        var benzin = Assert.Single(vm.Results);
        Assert.Equal("BENZIN oder OTTOKRAFTSTOFF", benzin.Benennung);
        Assert.Equal("UN 1203", vm.ResultTitle);
        Assert.True(vm.HasResults);
        Assert.False(vm.HasNoHits);
    }

    [Fact]
    public void A_un_nummer_with_several_rows_counts_them()
    {
        var vm = new GefahrgutViewModel { Query = "1202" };

        Assert.Equal(3, vm.Results.Count);
        Assert.Equal("UN 1202: 3 EINTRAGUNGEN", vm.ResultTitle);
    }

    [Fact]
    public void A_name_search_counts_its_hits()
    {
        var vm = new GefahrgutViewModel { Query = "ammoniak" };

        Assert.True(vm.Results.Count > 1);
        Assert.Equal(vm.Results.Count + " TREFFER", vm.ResultTitle);
    }

    [Fact]
    public void An_unknown_number_says_nothing_was_found()
    {
        var vm = new GefahrgutViewModel { Query = "9999" };

        Assert.True(vm.HasNoHits);
        Assert.False(vm.HasResults);
        Assert.Empty(vm.Results);
    }

    [Fact]
    public void A_wide_search_shows_the_first_fifty_and_says_there_are_more()
    {
        var vm = new GefahrgutViewModel { Query = "STOFF" };

        Assert.Equal(GefahrgutViewModel.MaxResults, vm.Results.Count);
        Assert.True(vm.HasMore);
        Assert.Contains("ersten 50", vm.MoreNote, StringComparison.Ordinal);
        Assert.EndsWith(" TREFFER", vm.ResultTitle, StringComparison.Ordinal);
    }

    [Fact]
    public void A_new_query_replaces_the_hits_and_tells_the_view()
    {
        var vm = new GefahrgutViewModel { Query = "1203" };
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.Query = "1005";

        Assert.Equal("1005", Assert.Single(vm.Results).UnNummer);
        Assert.Contains(nameof(vm.Results), changed);
        Assert.Contains(nameof(vm.ResultTitle), changed);
        Assert.Contains(nameof(vm.HasNoHits), changed);
    }

    [Fact]
    public void Clear_empties_the_search()
    {
        var vm = new GefahrgutViewModel { Query = "1203" };

        vm.ClearCommand.Execute(null);

        Assert.Equal(string.Empty, vm.Query);
        Assert.Empty(vm.Results);
        Assert.False(vm.IsSearching);
    }

    [Fact]
    public void The_module_creates_the_view_model()
    {
        var module = new GefahrgutModule();

        Assert.Equal("gefahrgut", module.Id);
        Assert.IsType<GefahrgutViewModel>(module.CreateViewModel(new HelperServices(TimeProvider.System, new NoTicker())));
        Assert.Throws<ArgumentNullException>(() => module.CreateViewModel(null!));
    }

    private sealed class NoTicker : ITicker
    {
        public IDisposable Subscribe(Action onTick) => throw new NotSupportedException();
    }
}
