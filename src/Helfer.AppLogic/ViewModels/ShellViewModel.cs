using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Helfer.AppLogic.Helpers;
using Helfer.AppLogic.Services;

namespace Helfer.AppLogic.ViewModels;

/// <summary>
/// The one screen the app has: a title bar and whatever page is current. The home screen, the
/// settings and every helper are pages of this shell.
/// <para>
/// Two ways back, on purpose. The arrow in the title bar always goes to the home screen. The
/// system Back gesture leaves the app when the helper was opened from its own launcher icon:
/// someone who tapped "Stoppuhr" on their home screen expects Back to return there, not to a list
/// of tools they never saw.
/// </para>
/// </summary>
public sealed partial class ShellViewModel : ObservableObject
{
    private readonly Dictionary<string, IHelperModule> _modules;
    private readonly Dictionary<string, ObservableObject> _opened = new(StringComparer.Ordinal);
    private readonly HelperServices _services;
    private readonly SettingsViewModel _settings;
    private bool _openedFromLauncher;

    public ShellViewModel(
        IReadOnlyList<IHelperModule> modules,
        HelperServices services,
        ILauncherIcons launcherIcons,
        string appVersion)
    {
        ArgumentNullException.ThrowIfNull(modules);
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(launcherIcons);
        ArgumentNullException.ThrowIfNull(appVersion);

        _modules = modules.ToDictionary(m => m.Id, StringComparer.Ordinal);
        _services = services;
        Home = new HomeViewModel(modules, Open, OpenSettings, appVersion);
        _settings = new SettingsViewModel(modules, launcherIcons);
        _current = Home;
        _title = HomeTitle;
    }

    /// <summary>The app's name in the title bar of the home screen.</summary>
    public const string HomeTitle = "FwAssist";

    public HomeViewModel Home { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHome))]
    private ObservableObject _current;

    [ObservableProperty]
    private string _title;

    public bool IsHome => ReferenceEquals(Current, Home);

    /// <summary>Opens a helper from its tile on the home screen.</summary>
    public void Open(string helperId)
    {
        ArgumentNullException.ThrowIfNull(helperId);
        if (TryShow(helperId))
        {
            _openedFromLauncher = false;
        }
    }

    /// <summary>
    /// Opens a helper because its own launcher icon was tapped. An id no module claims — an icon
    /// left over from a helper a later version removed — lands on the home screen instead.
    /// </summary>
    public void OpenFromLauncher(string helperId)
    {
        ArgumentNullException.ThrowIfNull(helperId);
        if (TryShow(helperId))
        {
            _openedFromLauncher = true;
        }
        else
        {
            GoHome();
        }
    }

    [RelayCommand]
    public void GoHome()
    {
        Current = Home;
        Title = HomeTitle;
    }

    /// <summary>
    /// The system Back gesture. <see langword="true"/> when the shell handled it; <see langword="false"/>
    /// tells the platform to do its own thing, which on Android sends the app to the background.
    /// </summary>
    public bool HandleSystemBack()
    {
        if (IsHome || _openedFromLauncher)
        {
            return false;
        }

        GoHome();
        return true;
    }

    private void OpenSettings()
    {
        _openedFromLauncher = false;
        _settings.Refresh();
        Current = _settings;
        Title = SettingsViewModel.PageTitle;
    }

    private bool TryShow(string helperId)
    {
        if (!TryGetModule(helperId, out var module))
        {
            return false;
        }

        if (!_opened.TryGetValue(helperId, out var page))
        {
            page = module.CreateViewModel(_services);
            _opened.Add(helperId, page);
        }

        Current = page;
        Title = module.Title;
        return true;
    }

    private bool TryGetModule(string? helperId, [MaybeNullWhen(false)] out IHelperModule module)
    {
        module = null;
        return helperId is not null && _modules.TryGetValue(helperId, out module);
    }
}
