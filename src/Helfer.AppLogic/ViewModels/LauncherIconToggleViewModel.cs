using CommunityToolkit.Mvvm.ComponentModel;
using Helfer.AppLogic.Services;

namespace Helfer.AppLogic.ViewModels;

/// <summary>The switch for one helper's own launcher icon.</summary>
public sealed partial class LauncherIconToggleViewModel : ObservableObject
{
    private readonly string _helperId;
    private readonly ILauncherIcons _launcherIcons;
    private readonly SettingsViewModel _owner;
    private bool _refreshing;

    public LauncherIconToggleViewModel(string helperId, string title, ILauncherIcons launcherIcons, SettingsViewModel owner)
    {
        ArgumentNullException.ThrowIfNull(helperId);
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(launcherIcons);
        ArgumentNullException.ThrowIfNull(owner);
        _helperId = helperId;
        _launcherIcons = launcherIcons;
        _owner = owner;
        Title = title;
        _isEnabled = launcherIcons.IsEnabled(helperId);
    }

    public string Title { get; }

    [ObservableProperty]
    private bool _isEnabled;

    internal void Refresh()
    {
        _refreshing = true;
        try
        {
            IsEnabled = _launcherIcons.IsEnabled(_helperId);
        }
        finally
        {
            _refreshing = false;
        }
    }

    partial void OnIsEnabledChanged(bool value)
    {
        if (_refreshing)
        {
            return;
        }

        if (_launcherIcons.TrySetEnabled(_helperId, value))
        {
            _owner.Error = null;
            return;
        }

        _owner.Error = $"Das Symbol für {Title} konnte nicht geändert werden.";
        Refresh();
    }
}
