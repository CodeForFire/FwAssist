using CommunityToolkit.Mvvm.ComponentModel;
using FwAssist.AppLogic.Helpers;

namespace FwAssist.Helpers.Stoppuhr;

public sealed class StoppuhrModule : IHelperModule
{
    public const string HelperId = "stoppuhr";

    public string Id => HelperId;

    public string Title => "Stoppuhr";

    public string Summary => "Zeit messen, zum Beispiel bei einer Übung.";

    // Drawn for this app: a crown, a ring and one hand, filled even-odd so the ring stays hollow.
    // The launcher icon in the Android head draws the same path (Resources/drawable).
    public string IconPathData => "F0 M9,1 H15 V3 H9 Z M12,4 A9,9 0 1 0 12,22 A9,9 0 1 0 12,4 Z M12,6 A7,7 0 1 1 12,20 A7,7 0 1 1 12,6 Z M11,8 H13 V13 H11 Z";

    public ObservableObject CreateViewModel(HelperServices services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return new StoppuhrViewModel(services.Time, services.Ticker);
    }
}
