using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FwAssist.AppLogic.Helpers;

namespace FwAssist.AppLogic.ViewModels;

/// <summary>The list of helpers, one tile each, in the order the composition root lists them.</summary>
public sealed partial class HomeViewModel : ObservableObject
{
    private readonly Action _openSettings;
    private readonly Action _openAbout;

    public HomeViewModel(
        IReadOnlyList<IHelperModule> modules,
        Action<string> open,
        Action openSettings,
        Action openAbout)
    {
        ArgumentNullException.ThrowIfNull(modules);
        ArgumentNullException.ThrowIfNull(open);
        ArgumentNullException.ThrowIfNull(openSettings);
        ArgumentNullException.ThrowIfNull(openAbout);

        _openSettings = openSettings;
        _openAbout = openAbout;
        Tiles = modules.Select(m => new HelperTileViewModel(m, open)).ToList();
    }

    public IReadOnlyList<HelperTileViewModel> Tiles { get; }

    [RelayCommand]
    private void OpenSettings() => _openSettings();

    [RelayCommand]
    private void OpenAbout() => _openAbout();
}
