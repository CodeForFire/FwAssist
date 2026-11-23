using CommunityToolkit.Mvvm.ComponentModel;
using FwAssist.AppLogic.Helpers;

namespace FwAssist.Helpers.Gefahrgut;

public sealed class GefahrgutModule : IHelperModule
{
    public const string HelperId = "gefahrgut";

    public string Id => HelperId;

    public string Title => "Gefahrgut";

    public string Summary => "UN-Nummer nachschlagen: Stoff, Klasse, Gefahrzettel.";

    // Drawn for this app: an orange Warntafel's outline with its two number fields cut out, filled
    // even-odd. The launcher icon in the Android head draws the same path.
    public string IconPathData => "F0 M1,5 H23 V19 H1 Z M3,7 H21 V11 H3 Z M3,13 H21 V17 H3 Z";

    public ObservableObject CreateViewModel(HelperServices services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return new GefahrgutViewModel();
    }
}
