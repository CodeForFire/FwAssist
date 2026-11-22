using CommunityToolkit.Mvvm.ComponentModel;
using FwAssist.AppLogic.Helpers;
using FwAssist.AppLogic.Services;

namespace FwAssist.Helpers.Sandsack;

public sealed class SandsackModule : IHelperModule
{
    public const string HelperId = "sandsack";

    public string Id => HelperId;

    public string Title => "Sandsack";

    public string Summary => "Sandsäcke, Sand und Zeit für einen Sandsackdamm.";

    // Drawn for this app: three sacks stacked as a dam, two below and one on top. The launcher icon
    // in the Android head draws the same path.
    public string IconPathData => "M9.5,4.5 H14.5 A2.5,2.5 0 0 1 17,7 V9 A2.5,2.5 0 0 1 14.5,11.5 H9.5 A2.5,2.5 0 0 1 7,9 V7 A2.5,2.5 0 0 1 9.5,4.5 Z M4,12.5 H9 A2.5,2.5 0 0 1 11.5,15 V17 A2.5,2.5 0 0 1 9,19.5 H4 A2.5,2.5 0 0 1 1.5,17 V15 A2.5,2.5 0 0 1 4,12.5 Z M15,12.5 H20 A2.5,2.5 0 0 1 22.5,15 V17 A2.5,2.5 0 0 1 20,19.5 H15 A2.5,2.5 0 0 1 12.5,17 V15 A2.5,2.5 0 0 1 15,12.5 Z";

    public ObservableObject CreateViewModel(HelperServices services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return new SandsackViewModel(services.Settings ?? new InMemoryHelperSettings());
    }
}
