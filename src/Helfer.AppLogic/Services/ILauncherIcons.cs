namespace Helfer.AppLogic.Services;

/// <summary>
/// The home-screen icons a helper can have of its own, beside the app's one. On Android each is an
/// <c>activity-alias</c> in the manifest that the app enables or disables; the view models only
/// see this seam, so the settings screen is testable without a phone.
/// </summary>
public interface ILauncherIcons
{
    /// <summary>Whether the helper's own icon is currently on the launcher.</summary>
    bool IsEnabled(string helperId);

    /// <summary>
    /// Shows or hides the helper's own icon. Returns <see langword="false"/> when the platform
    /// refused, so the caller can put the switch back and say so.
    /// </summary>
    bool TrySetEnabled(string helperId, bool enabled);
}
