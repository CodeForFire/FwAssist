using CommunityToolkit.Mvvm.ComponentModel;
using FwAssist.AppLogic.Services.Location;
using FwAssist.AppLogic.Services.Map;
using FwAssist.Core;

namespace FwAssist.Helpers.Hydranten;

/// <summary>
/// The Richtungspfeil: a large arrow from the phone's live position to one hydrant, turned by the
/// compass so it points the right way however the phone is held, with the distance still to go.
/// Unlike every other figure in the Hydrantenkarte it starts at the phone, not at the
/// Einsatzstelle, because it guides the person walking to the hydrant. The position is used here
/// and nowhere else: it is not stored and never leaves the device.
/// </summary>
public sealed partial class RichtungspfeilViewModel : ObservableObject
{
    /// <summary>Closer than this, in metres, the hydrant is reached and the arrow gives way.</summary>
    public const double AngekommenMetres = 10;

    public const string SuchtText = "Suche GPS-Signal …";

    public const string VerweigertText = "Ohne Standortfreigabe zeigt der Pfeil nichts. Freigabe in den Android-Einstellungen erteilen.";

    public const string AusgeschaltetText = "Standort ist ausgeschaltet. In den Android-Einstellungen einschalten.";

    public const string VerlorenText = "GPS-Signal verloren. Die Richtung kann veraltet sein.";

    public const string AngekommenText = "Am Hydranten.";

    public const string OhneKompassText = "Dieses Handy hat keinen Kompass. Richtung nach Himmelsrichtung gehen.";

    /// <summary>A fix older than this no longer counts as where the phone is.</summary>
    public static readonly TimeSpan VerlorenNach = TimeSpan.FromSeconds(15);

    private readonly ILocationSource _location;
    private readonly ICompass? _compass;
    private readonly TimeProvider _time;

    public RichtungspfeilViewModel(WaterPoint target, ILocationSource location, ICompass? compass = null, TimeProvider? time = null)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(location);
        Target = target;
        _location = location;
        _compass = compass;
        _time = time ?? TimeProvider.System;
    }

    /// <summary>The hydrant the arrow points to.</summary>
    public WaterPoint Target { get; }

    /// <summary>The phone's last position; null until the first fix.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Metres))]
    [NotifyPropertyChangedFor(nameof(Bearing))]
    [NotifyPropertyChangedFor(nameof(ArrowAngle))]
    [NotifyPropertyChangedFor(nameof(ShowsArrow))]
    [NotifyPropertyChangedFor(nameof(DistanceText))]
    [NotifyPropertyChangedFor(nameof(DirectionText))]
    [NotifyPropertyChangedFor(nameof(AccuracyText))]
    [NotifyPropertyChangedFor(nameof(IsArrived))]
    [NotifyPropertyChangedFor(nameof(IsLost))]
    [NotifyPropertyChangedFor(nameof(Status))]
    private GeoFix? _fix;

    /// <summary>Which way the top of the phone points, from true north; null without a compass reading.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ArrowAngle))]
    [NotifyPropertyChangedFor(nameof(ShowsArrow))]
    [NotifyPropertyChangedFor(nameof(IsLost))]
    [NotifyPropertyChangedFor(nameof(Status))]
    private double? _heading;

    /// <summary>What came of asking for the position; null while it is being asked.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Status))]
    private LocationAccess? _access;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Status))]
    private bool _hasCompass = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Status))]
    private bool _isRunning;

    /// <summary>The distance from the phone to the hydrant, in metres; null without a fix.</summary>
    public double? Metres => Fix is { } fix ? Schlauchbedarf.Metres(fix.Position, Target.Position) : null;

    /// <summary>Which way the hydrant lies from the phone, in degrees from true north; null without a fix.</summary>
    public double? Bearing => Fix is { } fix ? Peilung.Grad(fix.Position, Target.Position) : null;

    /// <summary>How far the arrow turns clockwise from the top of the screen.</summary>
    public double ArrowAngle => Bearing is { } bearing && Heading is { } heading ? Peilung.Normalize(bearing - heading) : 0;

    /// <summary>
    /// The arrow is drawn: there is a fresh fix and a heading, and the hydrant is not reached yet.
    /// A heading only ever comes from a compass that started, so it also means there is one.
    /// </summary>
    public bool ShowsArrow => Bearing is not null && Heading is not null && !IsLost && !IsArrived;

    // Stryker disable once Equality : exactly 10.000 m is not a distance a GPS fix lands on.
    public bool IsArrived => Metres <= AngekommenMetres;

    /// <summary>The last fix is older than <see cref="VerlorenNach"/>; checked whenever a reading arrives.</summary>
    public bool IsLost => Fix is { } fix && _time.GetUtcNow() - fix.Time > VerlorenNach;

    /// <summary>"120 m"; null without a fix.</summary>
    public string? DistanceText => Metres is { } metres ? $"{Formatting.Number((int)Math.Round(metres))} m" : null;

    /// <summary>"Richtung NO (45°)", which also guides without a compass; null without a fix.</summary>
    public string? DirectionText => Bearing is { } bearing
        ? $"Richtung {Peilung.Himmelsrichtung(bearing)} ({Formatting.Number((int)Math.Round(bearing) % 360)}°)"
        : null;

    /// <summary>"GPS ± 8 m"; null without a fix.</summary>
    public string? AccuracyText => Fix is { } fix ? $"GPS ± {Formatting.Number((int)Math.Ceiling(fix.AccuracyMetres))} m" : null;

    /// <summary>What the user has to know, in German; null while the arrow simply works.</summary>
    public string? Status => Access switch
    {
        LocationAccess.Denied => VerweigertText,
        LocationAccess.SwitchedOff => AusgeschaltetText,
        _ when Fix is null => IsRunning ? SuchtText : null,
        _ when IsArrived => AngekommenText,
        _ when IsLost => VerlorenText,
        _ when !HasCompass => OhneKompassText,
        _ => null,
    };

    /// <summary>Asks for the position and starts the GPS and the compass.</summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (IsRunning)
        {
            return;
        }

        IsRunning = true;
        Access = null;
        _location.FixChanged += OnFix;
        var access = await _location.StartAsync(cancellationToken);

        // Stopped while the permission dialog was open: the GPS must not run on.
        if (!IsRunning)
        {
            _location.FixChanged -= OnFix;
            _location.Stop();
            return;
        }

        Access = access;
        if (access != LocationAccess.Granted)
        {
            _location.FixChanged -= OnFix;
            IsRunning = false;
            return;
        }

        if (_compass is null)
        {
            HasCompass = false;
            return;
        }

        _compass.HeadingChanged += OnHeading;
        if (!_compass.Start())
        {
            _compass.HeadingChanged -= OnHeading;
            HasCompass = false;
        }
    }

    /// <summary>Stops the GPS and the compass. Stopping twice does nothing.</summary>
    public void Stop()
    {
        if (!IsRunning)
        {
            return;
        }

        IsRunning = false;
        _location.FixChanged -= OnFix;
        _location.Stop();
        if (_compass is not null)
        {
            _compass.HeadingChanged -= OnHeading;
            _compass.Stop();
        }
    }

    private void OnFix(object? sender, GeoFixEventArgs e) => Fix = e.Fix;

    // A compass reads many times a second, so a heading is also when a fix is found out to be old.
    private void OnHeading(object? sender, HeadingEventArgs e)
    {
        Heading = e.Degrees;
        OnPropertyChanged(nameof(IsLost));
        OnPropertyChanged(nameof(ShowsArrow));
        OnPropertyChanged(nameof(Status));
    }
}
