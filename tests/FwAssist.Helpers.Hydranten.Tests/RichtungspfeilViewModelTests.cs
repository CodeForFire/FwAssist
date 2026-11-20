using FwAssist.AppLogic.Services.Location;
using FwAssist.AppLogic.Services.Map;
using FwAssist.Core.Map;
using FwAssist.Testing;

namespace FwAssist.Helpers.Hydranten.Tests;

public class RichtungspfeilViewModelTests
{
    private static readonly GeoPoint Phone = new(48.1785, 11.2555);

    // About 111 m due north of the phone.
    private static readonly WaterPoint Hydrant = new(new GeoPoint(48.1795, 11.2555), WaterPointKind.Hydrant, string.Empty);

    private readonly FakeLocationSource _location = new();
    private readonly FakeCompass _compass = new();
    private readonly FakeTimeProvider _time = new();

    private RichtungspfeilViewModel Pfeil(ICompass? compass = null) =>
        new(Hydrant, _location, compass ?? _compass, _time);

    private async Task<RichtungspfeilViewModel> Running()
    {
        var pfeil = Pfeil();
        await pfeil.StartAsync();
        return pfeil;
    }

    [Fact]
    public async Task Started_it_runs_the_gps_and_the_compass_and_searches_until_the_first_fix()
    {
        var pfeil = await Running();

        Assert.True(_location.IsRunning);
        Assert.True(_compass.IsRunning);
        Assert.Equal(RichtungspfeilViewModel.SuchtText, pfeil.Status);
        Assert.False(pfeil.ShowsArrow);
        Assert.Null(pfeil.DistanceText);
    }

    [Fact]
    public async Task With_a_fix_and_a_heading_the_arrow_points_to_the_hydrant_relative_to_the_phone()
    {
        var pfeil = await Running();

        _location.Move(Phone, _time.GetUtcNow(), accuracy: 7.2);
        _compass.Turn(90);

        Assert.True(pfeil.ShowsArrow);
        Assert.Null(pfeil.Status);
        Assert.Equal(270, pfeil.ArrowAngle, precision: 0);
        Assert.Equal("111 m", pfeil.DistanceText);
        Assert.Equal("Richtung N (0°)", pfeil.DirectionText);
        Assert.Equal("GPS ± 8 m", pfeil.AccuracyText);
    }

    [Fact]
    public async Task The_arrow_follows_the_phone_as_it_moves_and_turns()
    {
        var pfeil = await Running();
        _location.Move(Phone, _time.GetUtcNow());
        _compass.Turn(0);
        Assert.Equal(0, pfeil.ArrowAngle, precision: 0);

        _location.Move(new GeoPoint(48.1795, 11.2545), _time.GetUtcNow());
        _compass.Turn(45);

        Assert.Equal(45, pfeil.ArrowAngle, precision: 0);
        Assert.Equal("74 m", pfeil.DistanceText);
        Assert.Equal("Richtung O (90°)", pfeil.DirectionText);
    }

    [Fact]
    public async Task The_arrow_needs_both_a_fix_and_a_heading()
    {
        var pfeil = await Running();

        _compass.Turn(90);
        Assert.False(pfeil.ShowsArrow);

        var other = await Running();
        _location.Move(Phone, _time.GetUtcNow());
        Assert.Null(other.Heading);
        Assert.False(other.ShowsArrow);
    }

    [Fact]
    public async Task Within_ten_metres_the_hydrant_is_reached_and_the_arrow_gives_way()
    {
        var pfeil = await Running();
        _compass.Turn(0);

        _location.Move(new GeoPoint(48.17945, 11.2555), _time.GetUtcNow());

        Assert.True(pfeil.IsArrived);
        Assert.False(pfeil.ShowsArrow);
        Assert.Equal(RichtungspfeilViewModel.AngekommenText, pfeil.Status);
    }

    [Fact]
    public async Task A_fix_older_than_15_seconds_is_reported_lost_at_the_next_compass_reading()
    {
        var pfeil = await Running();
        _location.Move(Phone, _time.GetUtcNow());
        _compass.Turn(0);

        _time.Advance(TimeSpan.FromSeconds(15));
        _compass.Turn(0);
        Assert.False(pfeil.IsLost);

        _time.Advance(TimeSpan.FromSeconds(1));
        var changed = new List<string?>();
        pfeil.PropertyChanged += (_, e) => changed.Add(e.PropertyName);
        _compass.Turn(0);

        Assert.True(pfeil.IsLost);
        Assert.False(pfeil.ShowsArrow);
        Assert.Equal(RichtungspfeilViewModel.VerlorenText, pfeil.Status);
        Assert.Contains(nameof(RichtungspfeilViewModel.IsLost), changed);
        Assert.Contains(nameof(RichtungspfeilViewModel.Status), changed);
        Assert.Contains(nameof(RichtungspfeilViewModel.ShowsArrow), changed);
    }

    [Fact]
    public async Task A_refused_permission_says_how_to_grant_it_and_starts_nothing()
    {
        _location.Access = LocationAccess.Denied;

        var pfeil = await Running();

        Assert.Equal(RichtungspfeilViewModel.VerweigertText, pfeil.Status);
        Assert.False(pfeil.IsRunning);
        Assert.False(_compass.IsRunning);
        _location.Move(Phone, _time.GetUtcNow());
        Assert.Null(pfeil.Fix);
    }

    [Fact]
    public async Task Location_switched_off_says_to_switch_it_on()
    {
        _location.Access = LocationAccess.SwitchedOff;

        var pfeil = await Running();

        Assert.Equal(RichtungspfeilViewModel.AusgeschaltetText, pfeil.Status);
        Assert.False(_compass.IsRunning);
    }

    [Fact]
    public async Task Without_a_compass_it_names_the_himmelsrichtung_instead()
    {
        _compass.Present = false;
        var pfeil = await Running();

        _location.Move(Phone, _time.GetUtcNow());
        _compass.Turn(90);

        Assert.False(pfeil.HasCompass);
        Assert.Null(pfeil.Heading);
        Assert.False(pfeil.ShowsArrow);
        Assert.Equal("Richtung N (0°)", pfeil.DirectionText);
        Assert.Equal(RichtungspfeilViewModel.OhneKompassText, pfeil.Status);
    }

    [Fact]
    public async Task A_platform_without_a_compass_service_is_the_same()
    {
        var pfeil = new RichtungspfeilViewModel(Hydrant, _location, null, _time);

        await pfeil.StartAsync();
        _location.Move(Phone, _time.GetUtcNow());

        Assert.False(pfeil.HasCompass);
        Assert.Equal(RichtungspfeilViewModel.OhneKompassText, pfeil.Status);
    }

    [Fact]
    public async Task Stopping_stops_the_gps_and_the_compass_and_ignores_what_still_arrives()
    {
        var pfeil = await Running();

        pfeil.Stop();
        pfeil.Stop();
        _location.Move(Phone, _time.GetUtcNow());
        _compass.Turn(90);

        Assert.False(_location.IsRunning);
        Assert.False(_compass.IsRunning);
        Assert.Equal(1, _location.Stops);
        Assert.Null(pfeil.Fix);
        Assert.Null(pfeil.Heading);
        Assert.Null(pfeil.Status);
    }

    [Fact]
    public async Task Stopped_while_the_permission_dialog_is_open_the_gps_does_not_run_on()
    {
        _location.PermissionGate = new TaskCompletionSource();
        var pfeil = Pfeil();
        var starting = pfeil.StartAsync();

        pfeil.Stop();
        _location.PermissionGate.SetResult();
        await starting;

        Assert.False(_location.IsRunning);
        Assert.False(_compass.IsRunning);
        Assert.False(pfeil.IsRunning);
        _location.Move(Phone, _time.GetUtcNow());
        Assert.Null(pfeil.Fix);
    }

    [Fact]
    public async Task Starting_twice_asks_once()
    {
        var pfeil = await Running();

        await pfeil.StartAsync();

        Assert.Equal(1, _location.Starts);
    }
}
