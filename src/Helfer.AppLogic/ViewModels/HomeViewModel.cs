using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Helfer.AppLogic.Helpers;

namespace Helfer.AppLogic.ViewModels;

/// <summary>The list of helpers, one tile each, in the order the composition root lists them.</summary>
public sealed partial class HomeViewModel : ObservableObject
{
    private readonly Action _openSettings;

    public HomeViewModel(
        IReadOnlyList<IHelperModule> modules,
        Action<string> open,
        Action openSettings,
        string appVersion)
    {
        ArgumentNullException.ThrowIfNull(modules);
        ArgumentNullException.ThrowIfNull(open);
        ArgumentNullException.ThrowIfNull(openSettings);
        ArgumentNullException.ThrowIfNull(appVersion);

        _openSettings = openSettings;
        Tiles = modules.Select(m => new HelperTileViewModel(m, open)).ToList();
        VersionText = $"Version {appVersion}";
    }

    public IReadOnlyList<HelperTileViewModel> Tiles { get; }

    public string VersionText { get; }

    [RelayCommand]
    private void OpenSettings() => _openSettings();
}
