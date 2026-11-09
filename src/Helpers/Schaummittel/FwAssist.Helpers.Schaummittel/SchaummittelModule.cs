using CommunityToolkit.Mvvm.ComponentModel;
using FwAssist.AppLogic.Helpers;
using FwAssist.AppLogic.Services;

namespace FwAssist.Helpers.Schaummittel;

public sealed class SchaummittelModule : IHelperModule
{
    public const string HelperId = "schaummittel";

    public string Id => HelperId;

    public string Title => "Schaummittel";

    public string Summary => "Schaum aus dem Vorrat, Bedarf für einen Raum.";

    // Drawn for this app: three foam bubbles, filled even-odd so each ring stays hollow. The launcher
    // icon in the Android head draws the same path.
    public string IconPathData => "F0 M9,8 A7,7 0 1 0 9,22 A7,7 0 1 0 9,8 Z M9,10 A5,5 0 1 1 9,20 A5,5 0 1 1 9,10 Z M18,2 A4,4 0 1 0 18,10 A4,4 0 1 0 18,2 Z M18,4 A2,2 0 1 1 18,8 A2,2 0 1 1 18,4 Z M20,15.5 A2.5,2.5 0 1 0 20,20.5 A2.5,2.5 0 1 0 20,15.5 Z";

    public ObservableObject CreateViewModel(HelperServices services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return new SchaummittelViewModel(services.Settings ?? new InMemoryHelperSettings());
    }
}
