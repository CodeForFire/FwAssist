using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Helfer.AppLogic.Helpers;

namespace Helfer.AppLogic.ViewModels;

/// <summary>One helper's tile on the home screen.</summary>
public sealed partial class HelperTileViewModel : ObservableObject
{
    private readonly Action<string> _open;

    public HelperTileViewModel(IHelperModule module, Action<string> open)
    {
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(open);
        Id = module.Id;
        Title = module.Title;
        Summary = module.Summary;
        IconPathData = module.IconPathData;
        _open = open;
    }

    public string Id { get; }

    public string Title { get; }

    public string Summary { get; }

    public string IconPathData { get; }

    [RelayCommand]
    private void Open() => _open(Id);
}
