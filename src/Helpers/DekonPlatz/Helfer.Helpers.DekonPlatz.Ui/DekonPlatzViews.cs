using Helfer.Ui;

namespace Helfer.Helpers.DekonPlatz.Ui;

/// <summary>Tells the shell which view shows which of this helper's view models.</summary>
public static class DekonPlatzViews
{
    public static void Register(ViewRegistry views)
    {
        ArgumentNullException.ThrowIfNull(views);
        views.Register<DekonPlatzViewModel>(() => new DekonPlatzView());
    }
}
