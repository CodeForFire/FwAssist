using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Avalonia.Android;
using FwAssist.App.Android.Services;
using FwAssist.AppLogic.Helpers;
using FwAssist.AppLogic.Services;
using FwAssist.AppLogic.Services.Map;
using FwAssist.AppLogic.ViewModels;
using FwAssist.Composition;
using FwAssist.Ui.Services;

namespace FwAssist.App.Android;

/// <summary>
/// The only Activity. Declared in AndroidManifest.xml, not by an [Activity] attribute, so the
/// launcher aliases can follow it there; [Register] gives it the Java name the manifest uses.
/// </summary>
[Register("de/codeforfire/fwassist/MainActivity")]
public class MainActivity : AvaloniaMainActivity
{
    // One client for the app's life, as HttpClient is meant to be used: only the Kartenpakete use it.
    private static readonly HttpClient Http = new();

    // Per process, not per Activity: Android recreates the Activity for a configuration change it
    // does not handle (locale, font scale, split screen), and a running Stoppuhr must survive that.
    private static ShellViewModel? _shell;

    // Per process like the shell, which holds them; the Activity they ask permissions and the
    // screen rotation of is set again on every create.
    private static AndroidLocationSource? _location;
    private static AndroidCompass? _compass;
    private static AndroidAttentionSignal? _signal;

    protected override void OnCreate(global::Android.OS.Bundle? savedInstanceState)
    {
        var context = ApplicationContext
            ?? throw new InvalidOperationException("Android created the activity without an application context.");
        var files = context.FilesDir?.AbsolutePath
            ?? throw new InvalidOperationException("Android gave the app no files directory.");
        var time = new BootClockTimeProvider();
        _location ??= new AndroidLocationSource(context, time);
        var location = _location;
        _compass ??= new AndroidCompass(context, () => location.Last);
        _signal ??= new AndroidAttentionSignal(context);
        _location.Activity = this;
        _compass.Activity = this;
        _shell ??= CompositionRoot.CreateShell(
            new HelperServices(
                time,
                new DispatcherTimerTicker(),
                new AndroidHelperSettings(context, AndroidHelperSettings.BackedUp),
                new RegionPackLibrary(Http, Path.Join(files, "maps")),
                _location,
                _compass,
                _signal,
                new AndroidHelperSettings(context, AndroidHelperSettings.DeviceOnly)),
            new AndroidLauncherIcons(context),
            new AndroidExternalLinks(context),
            AppVersionText.Of(typeof(MainActivity).Assembly));
        var shell = _shell;
        FwAssist.Ui.App.CreateShell = () => shell;
        CompositionRoot.RegisterViews(FwAssist.Ui.App.Views);

        // A launch, not a recreation: a recreated Activity keeps whatever page it had. A launch can
        // still meet a shell from earlier, because on Android 11 and older Back finishes the root
        // Activity while the process, and _shell with it, lives on.
        if (savedInstanceState is null)
        {
            Route(Intent);
        }

        base.OnCreate(savedInstanceState);
    }

    // The GPS and the compass run only while the app is in front, whatever the Richtungspfeil says.
    protected override void OnPause()
    {
        _location?.Pause();
        _compass?.Pause();
        base.OnPause();
    }

    protected override void OnResume()
    {
        base.OnResume();
        _location?.Resume();
        _compass?.Resume();
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        _location?.OnPermissionResult(requestCode, grantResults);
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
