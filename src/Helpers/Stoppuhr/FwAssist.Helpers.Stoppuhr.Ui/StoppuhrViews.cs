using FwAssist.Ui;

namespace FwAssist.Helpers.Stoppuhr.Ui;

/// <summary>Tells the shell which view shows which of this helper's view models.</summary>
public static class StoppuhrViews
{
    public static void Register(ViewRegistry views)
    {
        ArgumentNullException.ThrowIfNull(views);
        views.Register<StoppuhrViewModel>(() => new StoppuhrView());
    }
}
