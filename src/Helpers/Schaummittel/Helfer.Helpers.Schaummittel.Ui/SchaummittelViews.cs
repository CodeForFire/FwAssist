using Helfer.Ui;

namespace Helfer.Helpers.Schaummittel.Ui;

/// <summary>Tells the shell which view shows which of this helper's view models.</summary>
public static class SchaummittelViews
{
    public static void Register(ViewRegistry views)
    {
        ArgumentNullException.ThrowIfNull(views);
        views.Register<SchaummittelViewModel>(() => new SchaummittelView());
    }
}
