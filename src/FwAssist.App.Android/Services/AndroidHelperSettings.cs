using Android.Content;
using FwAssist.AppLogic.Services;

namespace FwAssist.App.Android.Services;

/// <summary>
/// Helper settings in one of the app's private SharedPreferences files. The app sends none of it
/// anywhere; Android's own backup may copy <see cref="BackedUp"/>, but never <see cref="DeviceOnly"/>.
/// </summary>
internal sealed class AndroidHelperSettings : IHelperSettings
{
    /// <summary>Small preferences that Android's backup may carry to a new phone.</summary>
    public const string BackedUp = "helper-settings";

    /// <summary>
    /// Einsatz data. Resources/xml/data_extraction_rules.xml and backup_rules.xml exclude this file
    /// by name, as <c>helper-settings-device.xml</c>; rename it only together with them.
    /// </summary>
    public const string DeviceOnly = "helper-settings-device";

    private readonly ISharedPreferences _preferences;

    public AndroidHelperSettings(Context context, string fileName)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrEmpty(fileName);
        _preferences = context.GetSharedPreferences(fileName, FileCreationMode.Private)
            ?? throw new InvalidOperationException("Android returned no SharedPreferences.");
    }

    public string? Get(string key)
    {
        try
        {
            return _preferences.GetString(key, null);
        }
        catch (Java.Lang.ClassCastException)
        {
            // The key holds something other than a string. This app never writes one, but a helper
            // must still open: it treats the value as missing and falls back to its default.
            return null;
        }
    }

    public void Set(string key, string value)
    {
        // Apply, not Commit: a helper saves as the user types, and this must not block the UI thread.
        using var editor = _preferences.Edit()
            ?? throw new InvalidOperationException("Android returned no SharedPreferences editor.");
        editor.PutString(key, value);
        editor.Apply();
    }
}
