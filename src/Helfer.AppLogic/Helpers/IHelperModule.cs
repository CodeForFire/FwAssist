using CommunityToolkit.Mvvm.ComponentModel;

namespace Helfer.AppLogic.Helpers;

/// <summary>
/// One helper tool. Everything the shell needs to list it, give it a launcher icon and open it.
/// A helper never references another helper; what they share lives in Core, AppLogic or Ui.
/// See AGENTS.md, "Helpers".
/// </summary>
public interface IHelperModule
{
    /// <summary>
    /// Stable, lower-case ASCII key. It names the helper's <c>activity-alias</c> in the Android
    /// manifest, so changing it removes the icon a user already placed on their home screen.
    /// </summary>
    string Id { get; }

    /// <summary>German name on the tile and under the launcher icon, e.g. "Stoppuhr".</summary>
    string Title { get; }

    /// <summary>One short German line under the title on the home screen.</summary>
    string Summary { get; }

    /// <summary>
    /// The tile icon as SVG-style path data on a 24×24 grid. Plain data rather than an Avalonia
    /// geometry, so this project stays free of Avalonia.
    /// </summary>
    string IconPathData { get; }

    /// <summary>
    /// Creates the helper's view model. Called once per process: the shell keeps the instance, so
    /// a stopwatch keeps running while the user looks at another helper.
    /// </summary>
    ObservableObject CreateViewModel(HelperServices services);
}
