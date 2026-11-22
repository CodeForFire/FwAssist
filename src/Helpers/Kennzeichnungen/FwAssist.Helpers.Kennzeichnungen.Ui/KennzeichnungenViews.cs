using FwAssist.Ui;

namespace FwAssist.Helpers.Kennzeichnungen.Ui;

/// <summary>Tells the shell which view shows which of this helper's view models.</summary>
public static class KennzeichnungenViews
{
    public static void Register(ViewRegistry views)
    {
        ArgumentNullException.ThrowIfNull(views);
        views.Register<KennzeichnungenViewModel>(() => new KennzeichnungenView());
    }
}
