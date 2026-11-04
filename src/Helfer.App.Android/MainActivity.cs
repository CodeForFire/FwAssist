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
            new HelperServices(new BootClockTimeProvider(), new DispatcherTimerTicker()),
            new AndroidLauncherIcons(
                ApplicationContext ?? throw new InvalidOperationException("Android created the activity without an application context.")),
            AppVersionText.Of(typeof(MainActivity).Assembly));
        var shell = _shell;
        Helfer.Ui.App.CreateShell = () => shell;
        CompositionRoot.RegisterViews(Helfer.Ui.App.Views);

        // A launch, not a recreation: a recreated Activity keeps whatever page it had. A launch can
        // still meet a shell from earlier, because on Android 11 and older Back finishes the root
        // Activity while the process, and _shell with it, lives on.
        if (savedInstanceState is null)
        {
            Route(Intent);
        }

        base.OnCreate(savedInstanceState);
    }

    // An icon tapped while the app is already running (singleTask). Returning from the recent-apps
    // list sends no intent and keeps whatever page was open.
    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        Route(intent);
    }

    // A helper's own icon opens that helper, the app's icon the home screen.
    private static void Route(Intent? intent)
    {
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
