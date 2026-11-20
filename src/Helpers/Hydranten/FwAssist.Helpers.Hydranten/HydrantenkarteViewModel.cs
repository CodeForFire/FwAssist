using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FwAssist.AppLogic.Helpers;
using FwAssist.AppLogic.Services;
using FwAssist.AppLogic.Services.Location;
using FwAssist.AppLogic.Services.Map;
using FwAssist.AppLogic.ViewModels;
using FwAssist.Core.Map;

namespace FwAssist.Helpers.Hydranten;

/// <summary>
/// The Hydranten on the offline map, the Einsatzstelle the user pins on it, and the hydrants
/// nearest to that pin with the B-Schläuche each takes. Every distance starts at the Einsatzstelle,
/// never at the phone. The pin is kept in <see cref="IHelperSettings"/>, so it survives the process
/// being killed during an Einsatz. The one exception is the <see cref="RichtungspfeilViewModel"/>,
/// which guides from the phone's live position to the chosen hydrant.
/// </summary>
public sealed partial class HydrantenkarteViewModel : ObservableObject, IHelperPage
{
    /// <summary>Where the Einsatzstelle is kept, as "lat,lon" in invariant culture; empty for none.</summary>
    public const string EinsatzstelleKey = HydrantenModule.HelperId + ".einsatzstelle";

    /// <summary>How many hydrants the list of the nearest offers.</summary>
    public const int NearestCount = 5;

    /// <summary>Close enough that every hydrant around the pin shows as a flag with its numbers.</summary>
    public const int CloseZoom = 17;

    public const string NoMapText = "Keine Karte auf diesem Gerät. Kartenpakete unter Einstellungen laden.";

    public const string NoPackForEinsatzstelleText = "Kein Kartenpaket für die Einsatzstelle. Kartenpakete unter Einstellungen laden.";

    public const string PlacingText = "Einsatzstelle setzen: tippe auf die Karte.";

    public const string StandortSuchtText = "Suche Standort …";

    public const string StandortVerweigertText = "Ohne Standortfreigabe kann die Karte den eigenen Standort nicht zeigen. Freigabe in den Android-Einstellungen erteilen.";

    public const string StandortAusText = "Standort ist ausgeschaltet. In den Android-Einstellungen einschalten.";

    private readonly IRegionPackLibrary? _maps;
    private readonly IHelperSettings? _settings;
    private readonly ILocationSource? _location;
    private readonly ICompass? _compass;
    private readonly TimeProvider _time;
    private readonly IAttentionSignal? _signal;
    private IReadOnlyList<InstalledRegionPack> _packs = [];

    // Counts the loads, so a slow one that finishes after a newer one does not overwrite it.
    private int _generation;

    // Counts the locates. One that ended while its start was still awaited (a fix arrived, the page
    // was left, the Richtungspfeil took over) finds a newer count when the start returns.
    private int _locateRun;

    // Whether the source has to be stopped once a start that was still awaited returns: yes after a
    // fix or leaving the page, no when the Richtungspfeil took the source over.
    private bool _stopWhenStarted;

    public HydrantenkarteViewModel(
        IRegionPackLibrary? maps,
        IHelperSettings? settings = null,
        ILocationSource? location = null,
        ICompass? compass = null,
        TimeProvider? time = null,
        IAttentionSignal? signal = null)
    {
        _maps = maps;
        _settings = settings;
        _location = location;
        _compass = compass;
        _time = time ?? TimeProvider.System;
        _signal = signal;
        _einsatzstelle = Rehydrate(settings?.Get(EinsatzstelleKey));
        _isPlacing = _einsatzstelle is null;
        if (maps is not null)
        {
            maps.InstalledChanged += (_, _) => Reload();
        }

        Reload();
        if (_einsatzstelle is { } einsatzstelle)
        {
            Map.ChangeViewCommand.Execute(new MapViewChange(einsatzstelle, CloseZoom));
        }
    }

    public MapViewport Map { get; } = new();

    /// <summary>Every hydrant of the installed packs; Saugstellen and the like are left out.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NaechsteHydrantenCommand))]
    private IReadOnlyList<WaterPoint> _hydranten = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Status))]
    [NotifyPropertyChangedFor(nameof(HasEinsatzstelle))]
    [NotifyPropertyChangedFor(nameof(EinsatzstelleMarks))]
    [NotifyPropertyChangedFor(nameof(IsMoving))]
    [NotifyCanExecuteChangedFor(nameof(NaechsteHydrantenCommand))]
    [NotifyCanExecuteChangedFor(nameof(EinsatzstelleVerschiebenCommand))]
    [NotifyCanExecuteChangedFor(nameof(EinsatzstelleLoeschenCommand))]
    [NotifyCanExecuteChangedFor(nameof(VerschiebenAbbrechenCommand))]
    private GeoPoint? _einsatzstelle;

    /// <summary>The next tap on the map sets the Einsatzstelle.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Status))]
    [NotifyPropertyChangedFor(nameof(IsMoving))]
    [NotifyCanExecuteChangedFor(nameof(NaechsteHydrantenCommand))]
    [NotifyCanExecuteChangedFor(nameof(EinsatzstelleVerschiebenCommand))]
    [NotifyCanExecuteChangedFor(nameof(VerschiebenAbbrechenCommand))]
    private bool _isPlacing;

    /// <summary>The hydrant whose card is open; null for none.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelected))]
    [NotifyCanExecuteChangedFor(nameof(PfeilStartenCommand))]
    private WaterPoint? _selected;

    /// <summary>The Richtungspfeil while it guides to a hydrant; null otherwise.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPfeil))]
    [NotifyCanExecuteChangedFor(nameof(PfeilStartenCommand))]
    [NotifyCanExecuteChangedFor(nameof(StandortZeigenCommand))]
    private RichtungspfeilViewModel? _pfeil;

    /// <summary>Where the phone was when MEIN STANDORT last found it; null before that and after leaving the page.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StandortMarks))]
    private GeoPoint? _standort;

    /// <summary>MEIN STANDORT waits for a fix.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StandortZeigenCommand))]
    private bool _isLocating;

    /// <summary>What MEIN STANDORT has to say, in German; null when it has nothing to say.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Status))]
    private string? _standortMeldung;

    /// <summary>The hydrants nearest the Einsatzstelle, closest first; empty until asked for.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNearest))]
    private IReadOnlyList<HydrantRow> _nearest = [];

    /// <summary>Why the hydrants could not be read, in German; null when they could.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Status))]
    private string? _loadError;

    public bool HasEinsatzstelle => Einsatzstelle is not null;

    /// <summary>The next tap moves an Einsatzstelle already set; ABBRECHEN leaves it where it was.</summary>
    public bool IsMoving => IsPlacing && HasEinsatzstelle;

    /// <summary>The Einsatzstelle as a list of one, for the pin layer over the map; empty without one.</summary>
    public IReadOnlyList<GeoPoint> EinsatzstelleMarks => Einsatzstelle is { } e ? [e] : [];

    public bool HasSelected => Selected is not null;

    /// <summary>The phone's located position as a list of one, for the dot over the map; empty without one.</summary>
    public IReadOnlyList<GeoPoint> StandortMarks => Standort is { } s ? [s] : [];

    public bool HasPfeil => Pfeil is not null;

    /// <summary>The platform can tell where the phone is, so the Richtungspfeil can be offered.</summary>
    public bool CanGuide => _location is not null;

    public bool HasNearest => Nearest.Count > 0;

    /// <summary>At least one Kartenpaket is installed.</summary>
    public bool HasMap => _packs.Count > 0;

    /// <summary>What the user has to do next, or what is missing, in German; null when all is well.</summary>
    public string? Status =>
        LoadError
        ?? StandortMeldung
        ?? (!HasMap ? NoMapText
            : Einsatzstelle is { } e && !_packs.Any(p => p.Info.Covers(e)) ? NoPackForEinsatzstelleText
            : IsPlacing ? PlacingText
            : null);

    /// <summary>The last load of the hydrants, for tests to await.</summary>
    internal Task Loading { get; private set; } = Task.CompletedTask;

    /// <summary>A stored Einsatzstelle, or null when there is none or the value is damaged.</summary>
    public static GeoPoint? Rehydrate(string? stored)
    {
        var parts = stored?.Split(',');
        return parts is { Length: 2 }
            && double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude)
            && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude)
                ? GeoPoint.TryCreate(latitude, longitude)
                : null;
    }

    /// <summary>A point as <see cref="Rehydrate"/> reads it back.</summary>
    public static string Persisted(GeoPoint point)
    {
        ArgumentNullException.ThrowIfNull(point);
        return string.Create(CultureInfo.InvariantCulture, $"{point.Latitude:R},{point.Longitude:R}");
    }

    /// <summary>A tap on the map: sets the Einsatzstelle while placing, otherwise opens the hydrant under it.</summary>
    [RelayCommand]
    private void Tap(GeoPoint? at)
    {
        if (at is null)
        {
            return;
        }

        if (IsPlacing)
        {
            Einsatzstelle = at;
            IsPlacing = false;
            Nearest = [];
            Selected = null;
            _settings?.Set(EinsatzstelleKey, Persisted(at));
            Map.ChangeViewCommand.Execute(new MapViewChange(at, Math.Max(Map.Zoom, CloseZoom)));
            return;
        }

        Selected = WaterPointLayer.At(Hydranten, at, Map.Zoom, Map.Rotation, Selected);
    }

    private bool CanVerschieben => HasEinsatzstelle && !IsPlacing;

    /// <summary>The next tap moves the Einsatzstelle.</summary>
    [RelayCommand(CanExecute = nameof(CanVerschieben))]
    private void EinsatzstelleVerschieben() => IsPlacing = true;

    /// <summary>Leaves the Einsatzstelle where it was.</summary>
    [RelayCommand(CanExecute = nameof(IsMoving))]
    private void VerschiebenAbbrechen() => IsPlacing = false;

    [RelayCommand(CanExecute = nameof(HasEinsatzstelle))]
    private void EinsatzstelleLoeschen()
    {
        Einsatzstelle = null;
        IsPlacing = true;
        Nearest = [];
        _settings?.Set(EinsatzstelleKey, string.Empty);
    }

    private bool CanNaechste => HasEinsatzstelle && !IsPlacing && Hydranten.Count > 0;

    /// <summary>Lists the <see cref="NearestCount"/> hydrants closest to the Einsatzstelle.</summary>
    [RelayCommand(CanExecute = nameof(CanNaechste))]
    private void NaechsteHydranten()
    {
        if (Einsatzstelle is not { } from)
        {
            return;
        }

        Nearest = Hydranten
            .Select(h => new HydrantRow(h, (int)Math.Round(Schlauchbedarf.Metres(from, h.Position))))
            .OrderBy(r => r.Metres)
            .Take(NearestCount)
            .ToList();
    }

    /// <summary>The user's choice from the list: its card opens and the map moves to it.</summary>
    [RelayCommand]
    private void Waehle(HydrantRow? row)
    {
        if (row is null)
        {
            return;
        }

        Selected = row.Point;
        Nearest = [];
        Map.ChangeViewCommand.Execute(new MapViewChange(row.Point.Position, Math.Max(Map.Zoom, CloseZoom)));
    }

    private bool CanPfeilStarten => _location is not null && Selected is not null && Pfeil is null;

    /// <summary>Opens the Richtungspfeil to the open hydrant; asks for the location permission the first time.</summary>
    [RelayCommand(CanExecute = nameof(CanPfeilStarten))]
    private async Task PfeilStarten()
    {
        if (_location is null || Selected is not { } target)
        {
            return;
        }

        // The arrow takes the one position source over; a locate still waiting gives it up.
        CancelLocate(stop: false);
        var pfeil = new RichtungspfeilViewModel(target, _location, _compass, _time, _signal);
        Pfeil = pfeil;
        await pfeil.StartAsync();
    }

    /// <summary>Closes the Richtungspfeil and stops the GPS and the compass.</summary>
    [RelayCommand]
    private void PfeilBeenden()
    {
        Pfeil?.Stop();
        Pfeil = null;
    }

    /// <summary>Leaving the page stops the GPS and the compass: they must not run on unseen.</summary>
    public void Leave()
    {
        PfeilBeenden();
        CancelLocate(stop: true);
        Standort = null;
    }

    private bool CanStandortZeigen => _location is not null && !IsLocating && Pfeil is null;

    /// <summary>
    /// MEIN STANDORT: one fix, the map moves there and the dot marks it, and the GPS goes off again.
    /// Asks for the location permission the first time.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanStandortZeigen))]
    private async Task StandortZeigen()
    {
        if (_location is not { } location)
        {
            return;
        }

        var run = ++_locateRun;
        _stopWhenStarted = false;
        IsLocating = true;
        StandortMeldung = StandortSuchtText;
        location.FixChanged += OnStandortFix;
        var access = await location.StartAsync();

        if (run != _locateRun)
        {
            // Ended while the start was awaited. A source that started after it was told to stop
            // would run on, so it is stopped now, unless the Richtungspfeil has it.
            if (access == LocationAccess.Granted && _stopWhenStarted)
            {
                location.Stop();
            }

            return;
        }

        if (access != LocationAccess.Granted)
        {
            location.FixChanged -= OnStandortFix;
            IsLocating = false;
            StandortMeldung = access == LocationAccess.Denied ? StandortVerweigertText : StandortAusText;
        }
    }

    private void OnStandortFix(object? sender, GeoFixEventArgs e)
    {
        Standort = e.Fix.Position;
        Map.ChangeViewCommand.Execute(new MapViewChange(e.Fix.Position, Math.Max(Map.Zoom, CloseZoom)));
        CancelLocate(stop: true);
    }

    // Ends a locate: no more fixes, no message. With stop the source is switched off, now and, if
    // its start is still awaited, once that returns.
    private void CancelLocate(bool stop)
    {
        if (!IsLocating || _location is not { } location)
        {
            return;
        }

        _locateRun++;
        _stopWhenStarted = stop;
        location.FixChanged -= OnStandortFix;
        IsLocating = false;
        StandortMeldung = null;
        if (stop)
        {
            location.Stop();
        }
    }

    [RelayCommand]
    private void ListeSchliessen() => Nearest = [];

    [RelayCommand]
    private void KarteSchliessen() => Selected = null;

    // Shows the installed packs and starts reading their hydrants.
    private void Reload()
    {
        try
        {
            _packs = _maps?.GetInstalled() ?? [];
            LoadError = null;
        }
        catch (RegionPackException e)
        {
            _packs = [];
            LoadError = e.Message;
        }

        Map.ShowAll(_packs, centerOnPacks: Einsatzstelle is null);
        OnPropertyChanged(nameof(HasMap));
        OnPropertyChanged(nameof(Status));
        Loading = _maps is null ? Task.CompletedTask : LoadHydrantsAsync(_maps, _packs, ++_generation);
    }

    private async Task LoadHydrantsAsync(IRegionPackLibrary maps, IReadOnlyList<InstalledRegionPack> packs, int generation)
    {
        var found = new List<WaterPoint>();
        string? error = null;
        foreach (var pack in packs)
        {
            try
            {
                found.AddRange((await maps.GetWaterPointsAsync(pack)).Where(p => p.Kind == WaterPointKind.Hydrant));
            }
            catch (RegionPackException e)
            {
                error ??= e.Message;
            }
        }

        if (generation != _generation)
        {
            return;
        }

        Hydranten = found;
        Selected = null;
        Nearest = [];

        // Reload already cleared the error of the last load; a folder it could not read stays said.
        if (error is not null)
        {
            LoadError = error;
        }
    }
}
