using FwAssist.AppLogic.Helpers;
using FwAssist.AppLogic.Services;
using FwAssist.AppLogic.ViewModels;
using FwAssist.Helpers.DekonPlatz;
using FwAssist.Helpers.DekonPlatz.Ui;
using FwAssist.Helpers.Funkalphabet;
using FwAssist.Helpers.Funkalphabet.Ui;
using FwAssist.Helpers.RthLandeplatz;
using FwAssist.Helpers.RthLandeplatz.Ui;
using FwAssist.Helpers.Schaummittel;
using FwAssist.Helpers.Schaummittel.Ui;
using FwAssist.Helpers.Stoppuhr;
using FwAssist.Helpers.Stoppuhr.Ui;
using FwAssist.Ui;

namespace FwAssist.Composition;

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

    public static ShellViewModel CreateShell(
        HelperServices services,
        ILauncherIcons launcherIcons,
        IExternalLinks externalLinks,
        string appVersion) =>
        new(Modules, services, launcherIcons, externalLinks, appVersion);
}
