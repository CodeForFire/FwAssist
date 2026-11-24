using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FwAssist.AppLogic.Services.Map;
using FwAssist.Core;

namespace FwAssist.AppLogic.ViewModels;

/// <summary>
/// The Kartenpakete: one list of every pack this device holds or the catalogue offers, each row
/// with its own state and action. A pack belongs to the device and acts at once: there is nothing
/// to save. Ported from Lagebuch's Stammdaten section of the same name.
/// </summary>
public sealed partial class MapPacksSection : ObservableObject
{
    private readonly IRegionPackLibrary _library;

    // Null until the catalogue was fetched once; the installed packs are listed without it.
    private IReadOnlyList<RegionPackInfo>? _catalogue;

    // The running download, so ABBRECHEN can stop it.
    private CancellationTokenSource? _installCancel;

    public MapPacksSection(IRegionPackLibrary library)
    {
        ArgumentNullException.ThrowIfNull(library);
        _library = library;
        Rebuild();
    }

    /// <summary>Every pack by name: the installed ones and, once fetched, what the catalogue offers.</summary>
    public ObservableCollection<MapPackRow> Packs { get; } = new();

    public bool HasInstalled => Packs.Any(p => p.Installed is not null);

    /// <summary>
    /// One line on what is on the device and how many updates the catalogue offers, or how far the
    /// running download is.
    /// </summary>
    public string Summary
    {
        get
        {
            if (Packs.FirstOrDefault(p => p.IsDownloading) is { } loading)
            {
                return $"Lädt … {loading.Percent} %";
            }

            var installed = Packs.Count(p => p.Installed is not null);
            if (installed == 0)
            {
                return "Nichts installiert";
            }

            var summary = $"{Formatting.Number(installed)} installiert";
            return Packs.Count(p => p.State == MapPackState.UpdateAvailable) switch
            {
                0 => summary,
                1 => $"{summary} · 1 Update",
                var updates => $"{summary} · {Formatting.Number(updates)} Updates",
            };
        }
    }

    /// <summary>The row whose LÖSCHEN was tapped and now waits for the second tap; null otherwise.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmRemoveCommand))]
    private MapPackRow? _removing;

    /// <summary>What went wrong, or what just finished; German, shown under the list.</summary>
    [ObservableProperty]
    private string? _status;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadCatalogCommand))]
    [NotifyCanExecuteChangedFor(nameof(InstallCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveCommand))]
    [NotifyCanExecuteChangedFor(nameof(ConfirmRemoveCommand))]
    private bool _isBusy;

    /// <summary>The catalogue is being fetched.</summary>
    [ObservableProperty]
    private bool _isCatalogLoading;

    /// <summary>
    /// Fetches the catalogue the first time the section is opened, so the list is complete without
    /// a button press. A failure surfaces in <see cref="Status"/>; opening the section again retries.
    /// </summary>
    public void EnsureCatalogLoaded()
    {
        if (_catalogue is null && LoadCatalogCommand.CanExecute(null))
        {
            // LoadCatalog turns every RegionPackException into Status.
            _ = LoadCatalogCommand.ExecuteAsync(null);
        }
    }

    private bool CanLoadCatalog => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanLoadCatalog))]
    private async Task LoadCatalog()
    {
        IsBusy = true;
        IsCatalogLoading = true;
        Status = null;
        try
        {
            _catalogue = await _library.GetCatalogAsync();
            Rebuild();
            if (_catalogue.Count == 0)
            {
                Status = "Die Liste enthält keine Kartenpakete.";
            }
        }
        catch (RegionPackException e)
        {
            Status = HasInstalled ? $"{e.Message} Installierte Karten funktionieren weiter." : e.Message;
        }
        finally
        {
            IsCatalogLoading = false;
            IsBusy = false;
        }
    }

    private bool CanInstall(MapPackRow? row) => !IsBusy && row is { ActionLabel: not null };

    [RelayCommand(CanExecute = nameof(CanInstall))]
    private async Task Install(MapPackRow? row)
    {
        if (row?.Offered is not { } offered)
        {
            return;
        }

        using var cancel = new CancellationTokenSource();
        _installCancel = cancel;
        IsBusy = true;
        Status = null;
        row.Progress = 0;
        row.IsDownloading = true;
        OnPropertyChanged(nameof(Summary));
        try
        {
            var progress = new RowProgress(p =>
            {
                row.Progress = p;
                OnPropertyChanged(nameof(Summary));
            });
            await _library.InstallAsync(offered, progress, cancel.Token);
            Status = $"„{offered.Name}“ ist installiert.";
        }
        catch (OperationCanceledException)
        {
            Status = "Download abgebrochen.";
        }
        catch (RegionPackException e)
        {
            Status = e.Message;
        }
        finally
        {
            _installCancel = null;
            row.IsDownloading = false;
            IsBusy = false;
            Rebuild();
        }
    }

    /// <summary>Stops the running download; the device keeps what it had before.</summary>
    [RelayCommand]
    private void CancelInstall() => _installCancel?.Cancel();

    private bool CanRemove(MapPackRow? row) => !IsBusy && row is { Installed: not null };

    /// <summary>The first tap on LÖSCHEN: the row asks again, because a pack takes long to fetch back.</summary>
    [RelayCommand(CanExecute = nameof(CanRemove))]
    private void Remove(MapPackRow? row) => Removing = row is { Installed: not null } ? row : null;

    private bool CanConfirmRemove => Removing is not null && !IsBusy;

    /// <summary>The second tap: deletes the pack from the device.</summary>
    [RelayCommand(CanExecute = nameof(CanConfirmRemove))]
    private void ConfirmRemove()
    {
        if (Removing?.Installed is not { } pack)
        {
            return;
        }

        Removing = null;
        try
        {
            _library.Remove(pack);
            Status = null;
        }
        catch (RegionPackException e)
        {
            Status = e.Message;
        }

        Rebuild();
    }

    [RelayCommand]
    private void CancelRemove() => Removing = null;

    // Lists the installed packs and the catalogue as one, by name.
    private void Rebuild()
    {
        IReadOnlyList<InstalledRegionPack> installed = [];
        try
        {
            installed = _library.GetInstalled();
        }
        catch (RegionPackException e)
        {
            Status = e.Message;
        }

        var offered = (_catalogue ?? []).ToDictionary(p => p.Slug, StringComparer.Ordinal);
        var installedSlugs = installed.Select(i => i.Info.Slug).ToHashSet(StringComparer.Ordinal);
        var rows = installed
            .Select(i => new MapPackRow(offered.GetValueOrDefault(i.Info.Slug), i))
            .Concat(offered.Values.Where(o => !installedSlugs.Contains(o.Slug)).Select(o => new MapPackRow(o, null)))
            .OrderBy(r => r.Name, Formatting.NameOrder)
            .ToList();

        Packs.Clear();
        foreach (var row in rows)
        {
            Packs.Add(row);
        }

        OnPropertyChanged(nameof(HasInstalled));
        OnPropertyChanged(nameof(Summary));
        InstallCommand.NotifyCanExecuteChanged();
        RemoveCommand.NotifyCanExecuteChanged();
        ConfirmRemoveCommand.NotifyCanExecuteChanged();
    }

    // "Stand 27.10.2026 · Version 1 · 253 MB". A build time that is no ISO timestamp is shown as it came.
    internal static string Detail(RegionPackInfo info) =>
        $"Stand {BuiltDay(info)} · Version {Formatting.Number(info.Version)} · {Size(info.SizeBytes)}";

    internal static string BuiltDay(RegionPackInfo info) =>
        DateTimeOffset.TryParse(info.BuiltAt, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var t)
            ? Formatting.Date(t)
            : info.BuiltAt;

    internal static string Size(long bytes) => $"{Formatting.Number((int)Math.Max(1, Math.Ceiling(bytes / (1024.0 * 1024.0))))} MB";

    // Reports where it is called. The library awaits without ConfigureAwait(false), so it reports
    // on the UI context already; Progress<T> would only post each report once more, after the
    // install may have finished and reset the row.
    private sealed class RowProgress(Action<double> report) : IProgress<double>
    {
        public void Report(double value) => report(value);
    }
}

/// <summary>Where a Kartenpaket stands on this device.</summary>
public enum MapPackState
{
    /// <summary>The catalogue offers it; the device does not hold it.</summary>
    NotInstalled,

    /// <summary>The device holds it, and the catalogue offers another edition.</summary>
    UpdateAvailable,

    /// <summary>The device holds it, and there is nothing newer to fetch, or no catalogue to ask.</summary>
    Installed,
}

/// <summary>One Kartenpaket: on this device, in the catalogue, or both.</summary>
public sealed partial class MapPackRow : ObservableObject
{
    private readonly RegionPackInfo _shown;

    /// <param name="offered">The catalogue's edition, or null when the catalogue is not loaded or lacks the pack.</param>
    /// <param name="installed">The device's copy, or null when it has none.</param>
    public MapPackRow(RegionPackInfo? offered, InstalledRegionPack? installed)
    {
        Offered = offered;
        Installed = installed;
        _shown = installed?.Info ?? offered ?? throw new ArgumentException("A pack row needs an installed or an offered pack.", nameof(offered));
    }

    public RegionPackInfo? Offered { get; }

    public InstalledRegionPack? Installed { get; }

    public string Slug => _shown.Slug;

    public string Name => _shown.Name;

    /// <summary>The installed edition when there is one, else the one on offer.</summary>
    public string Detail => MapPacksSection.Detail(_shown);

    public MapPackState State =>
        Installed is null ? MapPackState.NotInstalled :
        Offered is null || IsSameEdition(Installed.Info, Offered) ? MapPackState.Installed :
        MapPackState.UpdateAvailable;

    /// <summary>The device holds this pack and nothing newer is on offer.</summary>
    public bool IsCurrent => State == MapPackState.Installed;

    public bool HasUpdate => State == MapPackState.UpdateAvailable;

    /// <summary>The download button's label, or null when there is nothing to fetch.</summary>
    public string? ActionLabel => State switch
    {
        MapPackState.NotInstalled => "HERUNTERLADEN",
        MapPackState.UpdateAvailable => "AKTUALISIEREN",
        _ => null,
    };

    public bool ShowsAction => ActionLabel is not null && !IsDownloading;

    public bool ShowsRemove => Installed is not null && !IsDownloading;

    public bool ShowsDetail => !IsDownloading;

    /// <summary>Why AKTUALISIEREN is offered, so an update with the same version number is not a riddle; null otherwise.</summary>
    public string? UpdateReason =>
        State != MapPackState.UpdateAvailable || Installed?.Info is not { } installed || Offered is not { } offered ? null :
        installed.Version != offered.Version ? $"Neue Version {Formatting.Number(offered.Version)} – installiert ist Version {Formatting.Number(installed.Version)}." :
        $"Neu gebaut am {MapPacksSection.BuiltDay(offered)} – installiert ist der Stand vom {MapPacksSection.BuiltDay(installed)}.";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowsAction))]
    [NotifyPropertyChangedFor(nameof(ShowsRemove))]
    [NotifyPropertyChangedFor(nameof(ShowsDetail))]
    private bool _isDownloading;

    /// <summary>0..1 while this pack downloads.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Percent))]
    [NotifyPropertyChangedFor(nameof(ProgressText))]
    private double _progress;

    public int Percent => (int)Math.Floor(Math.Clamp(Progress, 0, 1) * 100);

    /// <summary>"41 % von 109 MB" while the pack downloads.</summary>
    public string ProgressText => $"{Percent} % von {MapPacksSection.Size((Offered ?? _shown).SizeBytes)}";

    // Exactly this edition: the same version and the same file.
    private static bool IsSameEdition(RegionPackInfo installed, RegionPackInfo offered) =>
        installed.Version == offered.Version
        && string.Equals(installed.Sha256, offered.Sha256, StringComparison.Ordinal);
}
