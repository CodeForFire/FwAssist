using CommunityToolkit.Mvvm.ComponentModel;
using FwAssist.AppLogic.Helpers;

namespace FwAssist.AppLogic.Tests;

internal sealed class TestModule(string id, string title) : IHelperModule
{
    public int Created { get; private set; }

    public string Id => id;

    public string Title => title;

    public string Summary => "Zum Testen.";

    public string IconPathData => "M0,0 H24 V24 H0 Z";

    public ObservableObject CreateViewModel(HelperServices services)
    {
        Created++;
        return new Page();
    }

    /// <summary>The page this module's helper shows; counts how often the user left it.</summary>
    internal sealed class Page : ObservableObject, IHelperPage
    {
        public int Left { get; private set; }

        public void Leave() => Left++;
    }
}
