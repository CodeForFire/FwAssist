using System.Diagnostics.CodeAnalysis;
using Android.Content;
using Android.Content.PM;
using Helfer.AppLogic.Services;

namespace Helfer.App.Android.Services;

/// <summary>
/// A helper's own launcher icon is its activity-alias, enabled or disabled through the package
/// manager. "Default" means what the manifest says, which is enabled for every alias.
/// </summary>
internal sealed class AndroidLauncherIcons : ILauncherIcons
{
    private readonly Context _context;
    private readonly string _packageName;

    public AndroidLauncherIcons(Context context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
        _packageName = context.PackageName
            ?? throw new ArgumentException("The context has no package name, so no alias can be addressed.", nameof(context));
    }

    public bool IsEnabled(string helperId)
    {
        var state = _context.PackageManager?.GetComponentEnabledSetting(Component(helperId));
        return state is not ComponentEnabledState.Disabled and not ComponentEnabledState.DisabledUser;
    }

    [SuppressMessage(
        "Design",
        "CA1031",
        Justification = "The package manager reports a refusal as an exception; it surfaces as SettingsViewModel.Error through the false return.")]
    public bool TrySetEnabled(string helperId, bool enabled)
    {
        try
        {
            // DontKillApp: without it Android stops the process to apply the change, and the
            // settings screen the user is looking at disappears under their finger.
            _context.PackageManager?.SetComponentEnabledSetting(
                Component(helperId),
                enabled ? ComponentEnabledState.Enabled : ComponentEnabledState.Disabled,
                ComponentEnableOption.DontKillApp);
            return _context.PackageManager is not null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private ComponentName Component(string helperId) =>
        new(_packageName, LauncherAlias.For(helperId));
}
