using CommunityToolkit.Mvvm.ComponentModel;
using FwAssist.AppLogic.Helpers;
using FwAssist.AppLogic.Services;

namespace FwAssist.Helpers.Kennzeichnungen;

public sealed class KennzeichnungenModule : IHelperModule
{
    public const string HelperId = "kennzeichnungen";

    public string Id => HelperId;

    public string Title => "Kennzeichnungen";

    public string Summary => "Warntafeln, Gefahrzettel und GHS-Piktogramme erkennen.";

    // Drawn for this app: a diamond standing on its tip with an exclamation mark cut out of it,
    // filled even-odd. The launcher icon in the Android head draws the same path.
    public string IconPathData => "F0 M12,1 L23,12 L12,23 L1,12 Z M11,6 H13 V14 H11 Z M11,16 H13 V18 H11 Z";

    public ObservableObject CreateViewModel(HelperServices services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return new KennzeichnungenViewModel(services.Settings ?? new InMemoryHelperSettings());
    }
}
