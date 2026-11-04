using CommunityToolkit.Mvvm.ComponentModel;
using Helfer.AppLogic.Helpers;
using Helfer.AppLogic.Services;

namespace Helfer.AppLogic.ViewModels;

/// <summary>Which helpers get an icon of their own on the phone's home screen.</summary>
public sealed partial class SettingsViewModel : ObservableObject
{
    public const string PageTitle = "Einstellungen";

    public SettingsViewModel(IReadOnlyList<IHelperModule> modules, ILauncherIcons launcherIcons)
    {
        ArgumentNullException.ThrowIfNull(modules);
        ArgumentNullException.ThrowIfNull(launcherIcons);
        LauncherIcons = modules.Select(m => new LauncherIconToggleViewModel(m.Id, m.Title, launcherIcons, this)).ToList();
    }

    public IReadOnlyList<LauncherIconToggleViewModel> LauncherIcons { get; }

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
