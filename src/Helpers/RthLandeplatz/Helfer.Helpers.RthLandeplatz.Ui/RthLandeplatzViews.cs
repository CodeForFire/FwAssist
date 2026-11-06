using Helfer.Ui;

namespace Helfer.Helpers.RthLandeplatz.Ui;

/// <summary>Tells the shell which view shows which of this helper's view models.</summary>
public static class RthLandeplatzViews
{
    public static void Register(ViewRegistry views)
    {
        ArgumentNullException.ThrowIfNull(views);
        views.Register<RthLandeplatzViewModel>(() => new RthLandeplatzView());
    }
}
