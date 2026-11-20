using CommunityToolkit.Mvvm.ComponentModel;
using FwAssist.AppLogic.Helpers;
using FwAssist.AppLogic.Services;
using FwAssist.AppLogic.Services.Map;

namespace FwAssist.AppLogic.ViewModels;

/// <summary>
/// Which helpers get an icon of their own on the phone's home screen, the order of the helpers on
/// the overview, and the Kartenpakete on the device.
/// </summary>
public sealed partial class SettingsViewModel : ObservableObject
{
    public const string PageTitle = "Einstellungen";

    public SettingsViewModel(IReadOnlyList<IHelperModule> modules, ILauncherIcons launcherIcons, IRegionPackLibrary? maps = null, HelperOrderSection? order = null)
    {
        ArgumentNullException.ThrowIfNull(modules);
        ArgumentNullException.ThrowIfNull(launcherIcons);
        LauncherIcons = modules.Select(m => new LauncherIconToggleViewModel(m.Id, m.Title, launcherIcons, this)).ToList();
        Maps = maps is null ? null : new MapPacksSection(maps);
        Order = order;
    }

    public IReadOnlyList<LauncherIconToggleViewModel> LauncherIcons { get; }

    /// <summary>The order of the helpers on the overview, or null where the shell keeps none.</summary>
    public HelperOrderSection? Order { get; }

    /// <summary>
    /// The Kartenpakete, or null where the platform offers none. The list of packs on offer is
    /// fetched only when the user asks for it, so opening Einstellungen never goes online.
    /// </summary>
    public MapPacksSection? Maps { get; }

    /// <summary>Why the last switch did not take, in German; <see langword="null"/> when it did.</summary>
    [ObservableProperty]
    private string? _error;

    /// <summary>Reads the icons' state again: the launcher may have changed it while we were away.</summary>
    public void Refresh()
    {
        Error = null;
        foreach (var toggle in LauncherIcons)
        {
            toggle.Refresh();
        }
    }
}
