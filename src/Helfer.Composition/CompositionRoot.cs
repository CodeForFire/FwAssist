using Helfer.AppLogic.Helpers;
using Helfer.AppLogic.Services;
using Helfer.AppLogic.ViewModels;
using Helfer.Helpers.DekonPlatz;
using Helfer.Helpers.DekonPlatz.Ui;
using Helfer.Helpers.Funkalphabet;
using Helfer.Helpers.Funkalphabet.Ui;
using Helfer.Helpers.RthLandeplatz;
using Helfer.Helpers.RthLandeplatz.Ui;
using Helfer.Helpers.Schaummittel;
using Helfer.Helpers.Schaummittel.Ui;
using Helfer.Helpers.Stoppuhr;
using Helfer.Helpers.Stoppuhr.Ui;
using Helfer.Ui;

namespace Helfer.Composition;

/// <summary>
/// Wiring by hand, as in Lagebuch: no container. Adding a helper means one line in
/// <see cref="Modules"/>, one in <see cref="RegisterViews"/>, and its alias in the manifest.
/// </summary>
public static class CompositionRoot
{
    /// <summary>Every helper, in the order the home screen lists them.</summary>
    public static IReadOnlyList<IHelperModule> Modules { get; } =
    [
        new StoppuhrModule(),
        new FunkalphabetModule(),
        new SchaummittelModule(),
        new RthLandeplatzModule(),
        new DekonPlatzModule(),
    ];

    public static void RegisterViews(ViewRegistry views)
    {
        ArgumentNullException.ThrowIfNull(views);
        StoppuhrViews.Register(views);
        FunkalphabetViews.Register(views);
        SchaummittelViews.Register(views);
        RthLandeplatzViews.Register(views);
        DekonPlatzViews.Register(views);
    }

    public static ShellViewModel CreateShell(HelperServices services, ILauncherIcons launcherIcons, string appVersion) =>
        new(Modules, services, launcherIcons, appVersion);
}
