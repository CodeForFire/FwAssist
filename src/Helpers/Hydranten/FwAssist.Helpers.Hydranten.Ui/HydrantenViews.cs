using FwAssist.Ui;

namespace FwAssist.Helpers.Hydranten.Ui;

/// <summary>Tells the shell which view shows which of this helper's view models.</summary>
public static class HydrantenViews
{
    public static void Register(ViewRegistry views)
    {
        ArgumentNullException.ThrowIfNull(views);
        views.Register<HydrantenkarteViewModel>(() => new HydrantenkarteView());
    }
}
