using Helfer.Ui;

namespace Helfer.Helpers.Funkalphabet.Ui;

/// <summary>Tells the shell which view shows which of this helper's view models.</summary>
public static class FunkalphabetViews
{
    public static void Register(ViewRegistry views)
    {
        ArgumentNullException.ThrowIfNull(views);
        views.Register<FunkalphabetViewModel>(() => new FunkalphabetView());
    }
}
