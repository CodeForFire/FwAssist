using CommunityToolkit.Mvvm.ComponentModel;
using Helfer.AppLogic.Helpers;
using Helfer.AppLogic.Services;

namespace Helfer.Helpers.Funkalphabet;

public sealed class FunkalphabetModule : IHelperModule
{
    public const string HelperId = "funkalphabet";

    public string Id => HelperId;

    public string Title => "Funkalphabet";

    public string Summary => "Wörter für den Funk buchstabieren.";

    // Drawn for this app: a handheld radio with an antenna, a display and a speaker slot, filled
    // even-odd so both stay hollow. The launcher icon in the Android head draws the same path.
    public string IconPathData => "F0 M7,1 H9 V7 H17 A2,2 0 0 1 19,9 V21 A2,2 0 0 1 17,23 H7 A2,2 0 0 1 5,21 V9 A2,2 0 0 1 7,7 Z M8,10 V14 H16 V10 Z M8,16 V18 H16 V16 Z";

    public ObservableObject CreateViewModel(HelperServices services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return new FunkalphabetViewModel(services.Settings ?? new InMemoryHelperSettings());
    }
}
