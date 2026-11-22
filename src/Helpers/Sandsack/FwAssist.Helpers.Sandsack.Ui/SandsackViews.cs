using FwAssist.Ui;

namespace FwAssist.Helpers.Sandsack.Ui;

/// <summary>Tells the shell which view shows which of this helper's view models.</summary>
public static class SandsackViews
{
    public static void Register(ViewRegistry views)
    {
        ArgumentNullException.ThrowIfNull(views);
        views.Register<SandsackViewModel>(() => new SandsackView());
    }
}
