using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Helfer.AppLogic.Services;

namespace Helfer.AppLogic.ViewModels;

/// <summary>Which version this is, whose project it is, and where its source lives.</summary>
public sealed partial class AboutViewModel : ObservableObject
{
    public const string PageTitle = "Über FwAssist";

    private readonly IExternalLinks _links;

    public AboutViewModel(IExternalLinks links, string appVersion)
    {
        ArgumentNullException.ThrowIfNull(links);
        ArgumentNullException.ThrowIfNull(appVersion);

        _links = links;
        VersionText = appVersion;
    }

    public string VersionText { get; }

    public string RepositoryText { get; init; } = "https://www.github.com/codeforfire/helfer";

    /// <summary>Why the link did not open, in German; <see langword="null"/> when it did.</summary>
    [ObservableProperty]
    private string? _error;

    /// <summary>Drops a failure from an earlier visit.</summary>
    public void Refresh() => Error = null;

    [RelayCommand]
    private void OpenRepository()
    {
        Error = HttpUrlValidator.TryGetHttpUri(RepositoryText, out var uri) && _links.TryOpen(uri)
            ? null
            : "Link konnte nicht geöffnet werden.";
    }
}
