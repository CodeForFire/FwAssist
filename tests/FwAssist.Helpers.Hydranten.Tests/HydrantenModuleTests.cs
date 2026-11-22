using FwAssist.AppLogic.Helpers;
using FwAssist.AppLogic.Services;
using FwAssist.Core.Map;
using FwAssist.Testing;

namespace FwAssist.Helpers.Hydranten.Tests;

// The Einsatzstelle is where a real Einsatz is, so it lives in the settings Android never backs up.
public class HydrantenModuleTests
{
    private const string Key = HydrantenkarteViewModel.EinsatzstelleKey;

    private static readonly GeoPoint Marktplatz = new(48.1785, 11.2555);
    private static readonly GeoPoint Bahnhof = new(48.1702, 11.2489);

    private readonly InMemoryHelperSettings _backedUp = new();
    private readonly InMemoryHelperSettings _deviceOnly = new();

    private HydrantenkarteViewModel Open() =>
        Assert.IsType<HydrantenkarteViewModel>(new HydrantenModule().CreateViewModel(
            new HelperServices(new FakeTimeProvider(), new ManualTicker(), _backedUp, DeviceOnlySettings: _deviceOnly)));

    private static string? Shown(HydrantenkarteViewModel vm) =>
        vm.Einsatzstelle is { } at ? HydrantenkarteViewModel.Persisted(at) : null;

    [Fact]
    public void The_einsatzstelle_comes_from_the_device_only_settings()
    {
        _deviceOnly.Set(Key, HydrantenkarteViewModel.Persisted(Marktplatz));

        Assert.Equal(HydrantenkarteViewModel.Persisted(Marktplatz), Shown(Open()));
    }

    [Fact]
    public void Clearing_the_einsatzstelle_writes_only_the_device_only_settings()
    {
        _deviceOnly.Set(Key, HydrantenkarteViewModel.Persisted(Marktplatz));
        var vm = Open();

        vm.EinsatzstelleLoeschenCommand.Execute(null);

        Assert.Equal(string.Empty, _deviceOnly.Get(Key));
        Assert.Null(_backedUp.Get(Key));
    }

    [Fact]
    public void An_einsatzstelle_from_before_the_update_moves_out_of_the_backed_up_settings()
    {
        _backedUp.Set(Key, HydrantenkarteViewModel.Persisted(Marktplatz));

        var vm = Open();

        Assert.Equal(HydrantenkarteViewModel.Persisted(Marktplatz), Shown(vm));
        Assert.Equal(HydrantenkarteViewModel.Persisted(Marktplatz), _deviceOnly.Get(Key));
        Assert.Equal(string.Empty, _backedUp.Get(Key));
    }

    [Fact]
    public void A_device_only_einsatzstelle_wins_over_an_old_copy_which_is_still_removed()
    {
        _backedUp.Set(Key, HydrantenkarteViewModel.Persisted(Marktplatz));
        _deviceOnly.Set(Key, HydrantenkarteViewModel.Persisted(Bahnhof));

        var vm = Open();

        Assert.Equal(HydrantenkarteViewModel.Persisted(Bahnhof), Shown(vm));
        Assert.Equal(string.Empty, _backedUp.Get(Key));
    }
}
