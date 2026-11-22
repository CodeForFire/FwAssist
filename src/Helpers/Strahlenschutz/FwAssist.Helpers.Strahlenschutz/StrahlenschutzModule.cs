using CommunityToolkit.Mvvm.ComponentModel;
using FwAssist.AppLogic.Helpers;
using FwAssist.AppLogic.Services;

namespace FwAssist.Helpers.Strahlenschutz;

public sealed class StrahlenschutzModule : IHelperModule
{
    public const string HelperId = "strahlenschutz";

    public string Id => HelperId;

    public string Title => "Strahlenschutz";

    public string Summary => "Dosisleistung aus TKZ und Abstand, Absperrgrenze.";

    // Drawn for this app: a trefoil, three blades of 60 degrees around a disc. The launcher icon in
    // the Android head draws the same path.
    public string IconPathData => "M22.50,12 A10.5,10.5 0 0 0 17.25,2.91 L13.60,9.23 A3.2,3.2 0 0 1 15.20,12 Z M6.75,2.91 A10.5,10.5 0 0 0 1.50,12 L8.80,12 A3.2,3.2 0 0 1 10.40,9.23 Z M6.75,21.09 A10.5,10.5 0 0 0 17.25,21.09 L13.60,14.77 A3.2,3.2 0 0 1 10.40,14.77 Z M12,9.8 A2.2,2.2 0 1 0 12,14.2 A2.2,2.2 0 1 0 12,9.8 Z";

    public ObservableObject CreateViewModel(HelperServices services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return new StrahlenschutzViewModel(services.Settings ?? new InMemoryHelperSettings());
    }
}
