using Android.Content;
using Android.Runtime;
using Avalonia.Android;
using Helfer.App.Android.Services;
using Helfer.AppLogic.Helpers;
using Helfer.AppLogic.Services;
using Helfer.AppLogic.ViewModels;
using Helfer.Composition;
using Helfer.Ui.Services;

namespace Helfer.App.Android;

/// <summary>
/// The only Activity. Declared in AndroidManifest.xml, not by an [Activity] attribute, so the
/// launcher aliases can follow it there; [Register] gives it the Java name the manifest uses.
/// </summary>
[Register("de/codeforfire/helfer/MainActivity")]
public class MainActivity : AvaloniaMainActivity
{
    // Per process, not per Activity: Android recreates the Activity for a configuration change it
    // does not handle (locale, font scale, split screen), and a running Stoppuhr must survive that.
    private static ShellViewModel? _shell;

    protected override void OnCreate(global::Android.OS.Bundle? savedInstanceState)
    {
        _shell ??= CompositionRoot.CreateShell(
            new HelperServices(TimeProvider.System, new DispatcherTimerTicker()),
            new AndroidLauncherIcons(ApplicationContext!),
            AppVersionText.Of(typeof(MainActivity).Assembly));
        var shell = _shell;
        Helfer.Ui.App.CreateShell = () => shell;
        CompositionRoot.RegisterViews(Helfer.Ui.App.Views);

        // A cold start from a helper's own icon. A recreated Activity keeps whatever page it had.
        if (savedInstanceState is null)
        {
            _ = OpenFromLauncher(Intent);
        }

        base.OnCreate(savedInstanceState);
    }

    // An icon tapped while the app is already running (singleTask): a helper's own icon opens that
    // helper, the app's icon the home screen. Returning from the recent-apps list sends no intent
    // and keeps whatever page was open.
    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        if (OpenFromLauncher(intent) is false && intent?.Action == Intent.ActionMain)
        {
            _shell?.GoHome();
        }
    }

    // Null when there is no shell yet, false when the intent named no helper.
    private static bool? OpenFromLauncher(Intent? intent)
    {
        if (_shell is null)
        {
            return null;
        }

        if (LauncherAlias.HelperIdOf(intent?.Component?.ClassName) is not { } helperId)
        {
            return false;
        }

        _shell.OpenFromLauncher(helperId);
        return true;
    }
}
