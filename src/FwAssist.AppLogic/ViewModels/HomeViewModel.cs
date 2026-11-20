using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FwAssist.AppLogic.Helpers;

namespace FwAssist.AppLogic.ViewModels;

/// <summary>The list of helpers, one tile each, in the order chosen in Einstellungen.</summary>
public sealed partial class HomeViewModel : ObservableObject
{
    private readonly Action _openSettings;
    private readonly Action _openAbout;
    private readonly ObservableCollection<HelperTileViewModel> _tiles;

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
        _tiles = new(modules.Select(m => new HelperTileViewModel(m, open)));
        Tiles = new(_tiles);
    }

    public ReadOnlyObservableCollection<HelperTileViewModel> Tiles { get; }

    /// <summary>Puts the tiles in the given order of helper ids. A tile whose id is not given keeps its place after the others.</summary>
    public void Arrange(IReadOnlyList<string> ids)
    {
        ArgumentNullException.ThrowIfNull(ids);
        var place = 0;

        // Lazy, so each id is looked up after the moves before it, as the tiles stand then.
        foreach (var from in ids.Select(IndexOf).Where(from => from >= 0))
        {
            if (from != place)
            {
                _tiles.Move(from, place);
            }

            place++;
        }
    }

    [RelayCommand]
    private void OpenSettings() => _openSettings();

    [RelayCommand]
    private void OpenAbout() => _openAbout();

    private int IndexOf(string id)
    {
        for (var i = 0; i < _tiles.Count; i++)
        {
            if (string.Equals(_tiles[i].Id, id, StringComparison.Ordinal))
            {
                return i;
            }
        }

        return -1;
    }
}
