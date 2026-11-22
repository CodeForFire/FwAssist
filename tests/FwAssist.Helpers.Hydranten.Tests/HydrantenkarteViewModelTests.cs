using FwAssist.AppLogic.Services;
using FwAssist.AppLogic.Services.Location;
using FwAssist.AppLogic.Services.Map;
using FwAssist.AppLogic.Tests;
using FwAssist.Core.Map;
using FwAssist.Testing;

namespace FwAssist.Helpers.Hydranten.Tests;

public class HydrantenkarteViewModelTests
{
    // Inside the fake pack's bounds (10.99–11.41 E, 48.08–48.29 N).
    private static readonly GeoPoint Marktplatz = new(48.1785, 11.2555);

    private static readonly RegionPackInfo Ffb = FakeRegionPackLibrary.Pack("ffb", "Landkreis Fürstenfeldbruck");

    private readonly FakeRegionPackLibrary _library = new();
    private readonly InMemoryHelperSettings _settings = new();

    private static WaterPoint Hydrant(double lat, double lon, string type = "Unterflur") =>
        new(new GeoPoint(lat, lon), WaterPointKind.Hydrant, string.Empty)
        {
            Details = [new WaterPointDetail(WaterPointDetail.TypeLabel, type)],
        };

    // Due north of the Marktplatz by about 11, 22, ... metres per step of 0.0001 degrees.
    private static WaterPoint North(int steps, string type = "Unterflur") =>
        Hydrant(Marktplatz.Latitude + (steps * 0.0001), Marktplatz.Longitude, type);

    private async Task<HydrantenkarteViewModel> Open(params WaterPoint[] points)
    {
        _library.Preinstall(Ffb);
        _library.WaterPoints["ffb"] = points;
        var vm = new HydrantenkarteViewModel(_library, _settings);
        await vm.Loading;
        return vm;
    }

    [Fact]
    public void Without_a_map_library_the_user_is_told_to_load_kartenpakete()
    {
        var vm = new HydrantenkarteViewModel(null);

        Assert.False(vm.HasMap);
        Assert.Equal(HydrantenkarteViewModel.NoMapText, vm.Status);
        Assert.Null(vm.Map.TileSource);
    }

    [Fact]
    public void Without_an_installed_pack_the_user_is_told_to_load_kartenpakete()
    {
        var vm = new HydrantenkarteViewModel(_library, _settings);

        Assert.Equal(HydrantenkarteViewModel.NoMapText, vm.Status);
    }

    [Fact]
    public async Task With_a_pack_and_no_einsatzstelle_the_user_is_asked_to_tap_one()
    {
        var vm = await Open(North(1));

        Assert.True(vm.HasMap);
        Assert.True(vm.IsPlacing);
        Assert.Equal(HydrantenkarteViewModel.PlacingText, vm.Status);
        Assert.Empty(vm.EinsatzstelleMarks);
        Assert.False(vm.NaechsteHydrantenCommand.CanExecute(null));
    }

    [Fact]
    public async Task Only_hydrants_are_shown_not_saugstellen_or_loeschteiche()
    {
        var saugstelle = new WaterPoint(Marktplatz, WaterPointKind.Saugstelle, "Musterbach");
        var teich = new WaterPoint(Marktplatz, WaterPointKind.Loeschteich, string.Empty);

        var vm = await Open(North(1), saugstelle, teich);

        Assert.Equal(WaterPointKind.Hydrant, Assert.Single(vm.Hydranten).Kind);
    }

    [Fact]
    public async Task A_tap_while_placing_sets_the_einsatzstelle_keeps_it_and_moves_the_map_there()
    {
        var vm = await Open(North(1));

        vm.TapCommand.Execute(Marktplatz);

        Assert.Equal(Marktplatz, vm.Einsatzstelle);
        Assert.False(vm.IsPlacing);
        Assert.Null(vm.Status);
        Assert.Equal(Marktplatz, Assert.Single(vm.EinsatzstelleMarks));
        Assert.Equal(Marktplatz, vm.Map.Center);
        Assert.True(vm.Map.Zoom >= HydrantenkarteViewModel.CloseZoom);
        Assert.Equal(HydrantenkarteViewModel.Persisted(Marktplatz), _settings.Get(HydrantenkarteViewModel.EinsatzstelleKey));
    }

    [Fact]
    public async Task The_einsatzstelle_survives_the_process_being_killed()
    {
        var first = await Open(North(1));
        first.TapCommand.Execute(Marktplatz);

        var again = new HydrantenkarteViewModel(_library, _settings);

        Assert.Equal(Marktplatz, again.Einsatzstelle);
        Assert.False(again.IsPlacing);
        Assert.Equal(Marktplatz, again.Map.Center);
        Assert.Equal(HydrantenkarteViewModel.CloseZoom, again.Map.Zoom);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("48.1785")]
    [InlineData("48.1785,11.2555,3")]
    [InlineData("abc,11.2555")]
    [InlineData("48.1785,xyz")]
    [InlineData("91,11")]
    [InlineData("48,181")]
    [InlineData("NaN,11")]
    public void A_damaged_stored_einsatzstelle_means_none(string? stored) =>
        Assert.Null(HydrantenkarteViewModel.Rehydrate(stored));

    [Fact]
    public void A_stored_einsatzstelle_reads_back_exactly_whatever_the_culture()
    {
        var point = new GeoPoint(48.178512345678, -11.255598765432);

        Assert.Equal(point, HydrantenkarteViewModel.Rehydrate(HydrantenkarteViewModel.Persisted(point)));
        Assert.DoesNotContain(";", HydrantenkarteViewModel.Persisted(point), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Naechste_hydranten_lists_the_five_closest_closest_first_with_metres_and_b_schlaeuche()
    {
        var vm = await Open(North(30), North(2), North(10, "Überflur"), North(50), North(5), North(20), North(40));
        vm.TapCommand.Execute(Marktplatz);

        Assert.True(vm.NaechsteHydrantenCommand.CanExecute(null));
        vm.NaechsteHydrantenCommand.Execute(null);

        Assert.True(vm.HasNearest);
        Assert.Equal(HydrantenkarteViewModel.NearestCount, vm.Nearest.Count);
        Assert.Equal([22, 56, 111, 222, 334], vm.Nearest.Select(r => r.Metres));
        Assert.Equal([2, 3, 6, 12, 17], vm.Nearest.Select(r => r.BSchlaeuche));
        Assert.Equal("Überflurhydrant", vm.Nearest[2].Bauart);
        Assert.Equal("111 m Luftlinie · 6 B-Schläuche", vm.Nearest[2].Text);
    }

    [Fact]
    public void A_hydrant_within_20_metres_needs_one_b_schlauch_and_says_so_in_the_singular()
    {
        var row = new HydrantRow(North(1), 11);

        Assert.Equal("11 m Luftlinie · 1 B-Schlauch", row.Text);
    }

    [Fact]
    public async Task Choosing_a_hydrant_from_the_list_opens_its_card_closes_the_list_and_moves_the_map_to_it()
    {
        var far = North(40);
        var vm = await Open(North(2), far);
        vm.TapCommand.Execute(Marktplatz);
        vm.NaechsteHydrantenCommand.Execute(null);

        vm.WaehleCommand.Execute(vm.Nearest[1]);

        Assert.Same(far, vm.Selected);
        Assert.True(vm.HasSelected);
        Assert.False(vm.HasNearest);
        Assert.Equal(far.Position, vm.Map.Center);
    }

    [Fact]
    public async Task Choosing_nothing_changes_nothing()
    {
        var vm = await Open(North(2));
        vm.TapCommand.Execute(Marktplatz);
        vm.NaechsteHydrantenCommand.Execute(null);

        vm.WaehleCommand.Execute(null);
        vm.TapCommand.Execute(null);

        Assert.Null(vm.Selected);
        Assert.True(vm.HasNearest);
    }

    [Fact]
    public async Task The_list_and_the_card_can_be_closed()
    {
        var vm = await Open(North(2));
        vm.TapCommand.Execute(Marktplatz);
        vm.NaechsteHydrantenCommand.Execute(null);
        vm.WaehleCommand.Execute(vm.Nearest[0]);
        vm.NaechsteHydrantenCommand.Execute(null);

        vm.ListeSchliessenCommand.Execute(null);
        vm.KarteSchliessenCommand.Execute(null);

        Assert.False(vm.HasNearest);
        Assert.False(vm.HasSelected);
    }

    [Fact]
    public async Task A_tap_on_a_hydrant_after_placing_opens_its_card_and_a_tap_beside_it_closes_it()
    {
        var hydrant = North(30);
        var vm = await Open(hydrant);
        vm.TapCommand.Execute(Marktplatz);

        vm.TapCommand.Execute(hydrant.Position);
        Assert.Same(hydrant, vm.Selected);
        Assert.Equal(Marktplatz, vm.Einsatzstelle);

        vm.TapCommand.Execute(Marktplatz);
        Assert.Null(vm.Selected);
    }

    [Fact]
    public async Task Verschieben_lets_the_next_tap_move_the_einsatzstelle_and_clears_the_old_list()
    {
        var vm = await Open(North(2));
        vm.TapCommand.Execute(Marktplatz);
        vm.NaechsteHydrantenCommand.Execute(null);
        Assert.False(vm.VerschiebenAbbrechenCommand.CanExecute(null));

        vm.EinsatzstelleVerschiebenCommand.Execute(null);
        Assert.True(vm.IsMoving);
        Assert.False(vm.EinsatzstelleVerschiebenCommand.CanExecute(null));
        Assert.False(vm.NaechsteHydrantenCommand.CanExecute(null));
        var moved = new GeoPoint(48.18, 11.26);
        vm.TapCommand.Execute(moved);

        Assert.Equal(moved, vm.Einsatzstelle);
        Assert.False(vm.IsMoving);
        Assert.False(vm.HasNearest);
        Assert.Equal(HydrantenkarteViewModel.Persisted(moved), _settings.Get(HydrantenkarteViewModel.EinsatzstelleKey));
    }

    [Fact]
    public async Task Moving_can_be_called_off_and_the_einsatzstelle_stays()
    {
        var vm = await Open(North(2));
        vm.TapCommand.Execute(Marktplatz);
        vm.EinsatzstelleVerschiebenCommand.Execute(null);

        vm.VerschiebenAbbrechenCommand.Execute(null);

        Assert.False(vm.IsPlacing);
        Assert.Equal(Marktplatz, vm.Einsatzstelle);
    }

    [Fact]
    public async Task Loeschen_removes_the_einsatzstelle_for_good_and_asks_for_a_new_one()
    {
        var vm = await Open(North(2));
        Assert.False(vm.EinsatzstelleLoeschenCommand.CanExecute(null));
        vm.TapCommand.Execute(Marktplatz);
        vm.NaechsteHydrantenCommand.Execute(null);

        vm.EinsatzstelleLoeschenCommand.Execute(null);

        Assert.Null(vm.Einsatzstelle);
        Assert.True(vm.IsPlacing);
        Assert.False(vm.IsMoving);
        Assert.False(vm.HasNearest);
        Assert.Equal(HydrantenkarteViewModel.PlacingText, vm.Status);
        Assert.Null(new HydrantenkarteViewModel(_library, _settings).Einsatzstelle);
    }

    [Fact]
    public async Task An_einsatzstelle_outside_every_pack_says_that_a_pack_is_missing()
    {
        var vm = await Open(North(2));

        vm.TapCommand.Execute(new GeoPoint(52.52, 13.40));

        Assert.Equal(HydrantenkarteViewModel.NoPackForEinsatzstelleText, vm.Status);
    }

    [Fact]
    public async Task Without_hydrants_there_is_nothing_to_list()
    {
        var vm = await Open();
        vm.TapCommand.Execute(Marktplatz);

        Assert.False(vm.NaechsteHydrantenCommand.CanExecute(null));
    }

    [Fact]
    public async Task Water_points_that_cannot_be_read_say_why()
    {
        _library.WaterFailure = new RegionPackException("Die Wasserentnahmestellen des Kartenpakets sind beschädigt.");

        var vm = await Open(North(2));

        Assert.Equal("Die Wasserentnahmestellen des Kartenpakets sind beschädigt.", vm.Status);
        Assert.Empty(vm.Hydranten);
    }

    [Fact]
    public async Task A_pack_installed_under_einstellungen_shows_on_the_open_map_at_once()
    {
        var vm = new HydrantenkarteViewModel(_library, _settings);
        Assert.False(vm.HasMap);
        _library.WaterPoints["ffb"] = [North(2)];

        await _library.InstallAsync(Ffb);
        await vm.Loading;

        Assert.True(vm.HasMap);
        Assert.NotNull(vm.Map.TileSource);
        Assert.Single(vm.Hydranten);
        Assert.Equal(HydrantenkarteViewModel.PlacingText, vm.Status);
    }

    [Fact]
    public async Task A_pack_removed_under_einstellungen_leaves_the_map()
    {
        var vm = await Open(North(2));

        _library.Remove(_library.GetInstalled()[0]);
        await vm.Loading;

        Assert.False(vm.HasMap);
        Assert.Empty(vm.Hydranten);
        Assert.Equal(HydrantenkarteViewModel.NoMapText, vm.Status);
    }

    [Fact]
    public async Task A_slow_load_that_finishes_after_a_newer_one_does_not_win()
    {
        _library.Preinstall(Ffb);
        _library.WaterPoints["ffb"] = [North(2)];
        var gate = new TaskCompletionSource<IReadOnlyList<WaterPoint>>();
        var slow = new GatedLibrary(_library, gate.Task);
        var vm = new HydrantenkarteViewModel(slow, _settings);
        var first = vm.Loading;

        slow.Gate = null;
        _library.WaterPoints["ffb"] = [North(2), North(3)];
        slow.RaiseChanged();
        await vm.Loading;
        gate.SetResult([North(9), North(10), North(11)]);
        await first;

        Assert.Equal(2, vm.Hydranten.Count);
    }

    [Fact]
    public void A_folder_that_cannot_be_read_says_why()
    {
        var vm = new HydrantenkarteViewModel(new BrokenLibrary(), _settings);

        Assert.Equal("Die installierten Kartenpakete sind nicht lesbar.", vm.Status);
        Assert.False(vm.HasMap);
    }

    [Fact]
    public async Task Without_a_location_service_no_richtungspfeil_is_offered()
    {
        var vm = await Open(North(2));
        vm.TapCommand.Execute(North(2).Position);

        Assert.False(vm.CanGuide);
        Assert.False(vm.PfeilStartenCommand.CanExecute(null));
    }

    [Fact]
    public async Task The_richtungspfeil_opens_on_the_chosen_hydrant_and_closes_with_beenden()
    {
        var hydrant = North(30);
        _library.Preinstall(Ffb);
        _library.WaterPoints["ffb"] = [hydrant];
        var location = new FakeLocationSource();
        var compass = new FakeCompass();
        var vm = new HydrantenkarteViewModel(_library, _settings, location, compass, new FakeTimeProvider());
        await vm.Loading;
        vm.TapCommand.Execute(Marktplatz);
        Assert.True(vm.CanGuide);
        Assert.False(vm.PfeilStartenCommand.CanExecute(null));
        vm.NaechsteHydrantenCommand.Execute(null);
        vm.WaehleCommand.Execute(vm.Nearest[0]);

        await vm.PfeilStartenCommand.ExecuteAsync(null);

        var pfeil = Assert.IsType<RichtungspfeilViewModel>(vm.Pfeil);
        Assert.True(vm.HasPfeil);
        Assert.Same(hydrant, pfeil.Target);
        Assert.True(location.IsRunning);
        Assert.True(compass.IsRunning);
        Assert.False(vm.PfeilStartenCommand.CanExecute(null));

        vm.PfeilBeendenCommand.Execute(null);

        Assert.Null(vm.Pfeil);
        Assert.False(location.IsRunning);
        Assert.False(compass.IsRunning);
    }

    [Fact]
    public async Task Leaving_the_page_stops_the_gps_and_the_compass()
    {
        var hydrant = North(30);
        _library.Preinstall(Ffb);
        _library.WaterPoints["ffb"] = [hydrant];
        var location = new FakeLocationSource();
        var compass = new FakeCompass();
        var vm = new HydrantenkarteViewModel(_library, _settings, location, compass);
        await vm.Loading;
        vm.TapCommand.Execute(Marktplatz);
        vm.TapCommand.Execute(hydrant.Position);
        await vm.PfeilStartenCommand.ExecuteAsync(null);

        vm.Leave();

        Assert.Null(vm.Pfeil);
        Assert.False(location.IsRunning);
        Assert.False(compass.IsRunning);
    }

    [Fact]
    public async Task Leaving_the_page_without_a_pfeil_changes_nothing()
    {
        var vm = await Open(North(2));
        vm.TapCommand.Execute(Marktplatz);

        vm.Leave();

        Assert.Equal(Marktplatz, vm.Einsatzstelle);
        Assert.Null(vm.Pfeil);
    }

    private async Task<(HydrantenkarteViewModel Vm, FakeLocationSource Location)> OpenWithLocation(params WaterPoint[] points)
    {
        _library.Preinstall(Ffb);
        _library.WaterPoints["ffb"] = points;
        var location = new FakeLocationSource();
        var vm = new HydrantenkarteViewModel(_library, _settings, location, new FakeCompass(), new FakeTimeProvider());
        await vm.Loading;
        return (vm, location);
    }

    [Fact]
    public async Task Mein_standort_centres_on_the_first_fix_marks_it_and_switches_the_gps_off()
    {
        var (vm, location) = await OpenWithLocation(North(2));
        var here = new GeoPoint(48.20, 11.30);

        var locating = vm.StandortZeigenCommand.ExecuteAsync(null);
        await locating;
        Assert.True(vm.IsLocating);
        Assert.Equal(HydrantenkarteViewModel.StandortSuchtText, vm.Status);
        Assert.False(vm.StandortZeigenCommand.CanExecute(null));
        location.Move(here, DateTimeOffset.UnixEpoch);

        Assert.Equal(here, vm.Standort);
        Assert.Equal(here, Assert.Single(vm.StandortMarks));
        Assert.Equal(here, vm.Map.Center);
        Assert.True(vm.Map.Zoom >= HydrantenkarteViewModel.CloseZoom);
        Assert.False(vm.IsLocating);
        Assert.False(location.IsRunning);
        Assert.Equal(1, location.Stops);
        Assert.Equal(HydrantenkarteViewModel.PlacingText, vm.Status);
        Assert.True(vm.StandortZeigenCommand.CanExecute(null));
    }

    [Fact]
    public async Task A_later_fix_does_not_move_the_map_again()
    {
        var (vm, location) = await OpenWithLocation(North(2));
        await vm.StandortZeigenCommand.ExecuteAsync(null);
        location.Move(new GeoPoint(48.20, 11.30), DateTimeOffset.UnixEpoch);
        vm.Map.ChangeViewCommand.Execute(new MapViewChange(Marktplatz, 14));

        location.Move(new GeoPoint(48.21, 11.31), DateTimeOffset.UnixEpoch);

        Assert.Equal(Marktplatz, vm.Map.Center);
        Assert.Equal(new GeoPoint(48.20, 11.30), vm.Standort);
    }

    [Theory]
    [InlineData(LocationAccess.Denied, HydrantenkarteViewModel.StandortVerweigertText)]
    [InlineData(LocationAccess.SwitchedOff, HydrantenkarteViewModel.StandortAusText)]
    public async Task Without_a_position_mein_standort_says_why_and_moves_nothing(LocationAccess access, string text)
    {
        var (vm, location) = await OpenWithLocation(North(2));
        location.Access = access;
        var centre = vm.Map.Center;

        await vm.StandortZeigenCommand.ExecuteAsync(null);
        location.Move(new GeoPoint(48.20, 11.30), DateTimeOffset.UnixEpoch);

        Assert.Equal(text, vm.Status);
        Assert.False(vm.IsLocating);
        Assert.Null(vm.Standort);
        Assert.Equal(centre, vm.Map.Center);
        Assert.False(location.IsRunning);
    }

    [Fact]
    public void Without_a_location_service_there_is_no_mein_standort() =>
        Assert.False(new HydrantenkarteViewModel(_library, _settings).StandortZeigenCommand.CanExecute(null));

    [Fact]
    public async Task Leaving_the_page_while_locating_switches_the_gps_off_and_the_dot_goes_with_it()
    {
        var (vm, location) = await OpenWithLocation(North(2));
        await vm.StandortZeigenCommand.ExecuteAsync(null);
        location.Move(new GeoPoint(48.20, 11.30), DateTimeOffset.UnixEpoch);
        await vm.StandortZeigenCommand.ExecuteAsync(null);
        Assert.True(location.IsRunning);

        vm.Leave();
        location.Move(new GeoPoint(48.21, 11.31), DateTimeOffset.UnixEpoch);

        Assert.False(location.IsRunning);
        Assert.False(vm.IsLocating);
        Assert.Null(vm.Standort);
        Assert.Empty(vm.StandortMarks);
        Assert.Null(vm.StandortMeldung);
    }

    [Fact]
    public async Task Left_while_the_permission_dialog_is_open_the_gps_does_not_run_on()
    {
        var (vm, location) = await OpenWithLocation(North(2));
        location.PermissionGate = new TaskCompletionSource();
        var centre = vm.Map.Center;
        var locating = vm.StandortZeigenCommand.ExecuteAsync(null);

        vm.Leave();
        location.PermissionGate.SetResult();
        await locating;
        location.Move(new GeoPoint(48.20, 11.30), DateTimeOffset.UnixEpoch);

        Assert.False(location.IsRunning);
        Assert.Equal(centre, vm.Map.Center);
        Assert.Null(vm.Standort);
    }

    [Fact]
    public async Task The_richtungspfeil_takes_the_gps_over_from_a_locate_still_waiting()
    {
        var hydrant = North(30);
        var (vm, location) = await OpenWithLocation(hydrant);
        vm.TapCommand.Execute(Marktplatz);
        vm.TapCommand.Execute(hydrant.Position);
        await vm.StandortZeigenCommand.ExecuteAsync(null);
        var stops = location.Stops;

        await vm.PfeilStartenCommand.ExecuteAsync(null);
        location.Move(new GeoPoint(48.20, 11.30), DateTimeOffset.UnixEpoch);

        Assert.False(vm.IsLocating);
        Assert.Null(vm.Standort);
        Assert.Equal(stops, location.Stops);
        Assert.True(location.IsRunning);
        Assert.NotNull(vm.Pfeil?.Fix);
        Assert.False(vm.StandortZeigenCommand.CanExecute(null));
    }

    [Fact]
    public async Task The_richtungspfeil_opened_during_the_permission_dialog_keeps_the_gps()
    {
        var hydrant = North(30);
        var (vm, location) = await OpenWithLocation(hydrant);
        vm.TapCommand.Execute(Marktplatz);
        vm.TapCommand.Execute(hydrant.Position);
        location.PermissionGate = new TaskCompletionSource();
        var locating = vm.StandortZeigenCommand.ExecuteAsync(null);

        var arrow = vm.PfeilStartenCommand.ExecuteAsync(null);
        location.PermissionGate.SetResult();
        await locating;
        await arrow;

        Assert.True(location.IsRunning);
        Assert.Equal(0, location.Stops);
    }

    // The fake library, with the water points of a load held back until a test lets them go.
    private sealed class GatedLibrary(FakeRegionPackLibrary inner, Task<IReadOnlyList<WaterPoint>>? gate) : IRegionPackLibrary
    {
        public event EventHandler? InstalledChanged;

        public Task<IReadOnlyList<WaterPoint>>? Gate { get; set; } = gate;

        public void RaiseChanged() => InstalledChanged?.Invoke(this, EventArgs.Empty);

        public Task<IReadOnlyList<RegionPackInfo>> GetCatalogAsync(CancellationToken cancellationToken = default) => inner.GetCatalogAsync(cancellationToken);

        public IReadOnlyList<InstalledRegionPack> GetInstalled() => inner.GetInstalled();

        public Task<InstalledRegionPack> InstallAsync(RegionPackInfo pack, IProgress<double>? progress = null, CancellationToken cancellationToken = default) =>
            inner.InstallAsync(pack, progress, cancellationToken);

        public Task<IReadOnlyList<WaterPoint>> GetWaterPointsAsync(InstalledRegionPack pack, CancellationToken cancellationToken = default) =>
            Gate ?? inner.GetWaterPointsAsync(pack, cancellationToken);

        public void Remove(InstalledRegionPack pack) => inner.Remove(pack);
    }

    private sealed class BrokenLibrary : IRegionPackLibrary
    {
        public event EventHandler? InstalledChanged
        {
            add { }
            remove { }
        }

        public Task<IReadOnlyList<RegionPackInfo>> GetCatalogAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public IReadOnlyList<InstalledRegionPack> GetInstalled() => throw new RegionPackException("Die installierten Kartenpakete sind nicht lesbar.");

        public Task<InstalledRegionPack> InstallAsync(RegionPackInfo pack, IProgress<double>? progress = null, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<IReadOnlyList<WaterPoint>> GetWaterPointsAsync(InstalledRegionPack pack, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public void Remove(InstalledRegionPack pack) => throw new NotSupportedException();
    }
}
