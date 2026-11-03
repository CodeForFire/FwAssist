using CommunityToolkit.Mvvm.ComponentModel;
using Helfer.AppLogic.Helpers;

namespace Helfer.AppLogic.Tests;

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

    internal sealed class Page : ObservableObject;
}
