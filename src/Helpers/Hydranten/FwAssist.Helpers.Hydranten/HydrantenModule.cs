using CommunityToolkit.Mvvm.ComponentModel;
using FwAssist.AppLogic.Helpers;

namespace FwAssist.Helpers.Hydranten;

public sealed class HydrantenModule : IHelperModule
{
    public const string HelperId = "hydranten";

    public string Id => HelperId;

    public string Title => "Hydrantenkarte";

    public string Summary => "Hydranten an der Einsatzstelle finden.";

    // Drawn for this app: an Überflurhydrant, its cap, body with two side outlets, and foot.
    // The launcher icon in the Android head draws the same path (Resources/drawable).
    public string IconPathData => "M10,2 H14 V4 H10 Z M8,5 H16 V7 H8 Z M9,8 H15 V19 H9 Z M5,10 H8 V13 H5 Z M16,10 H19 V13 H16 Z M7,20 H17 V22 H7 Z";

    public ObservableObject CreateViewModel(HelperServices services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return new HydrantenkarteViewModel(services.Maps, services.Settings);
    }
}
