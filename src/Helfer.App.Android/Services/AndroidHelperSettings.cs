using Android.Content;
using Helfer.AppLogic.Services;

namespace Helfer.App.Android.Services;

/// <summary>Helper settings in the app's private SharedPreferences. Nothing leaves the phone.</summary>
internal sealed class AndroidHelperSettings : IHelperSettings
{
    private const string FileName = "helper-settings";

    private readonly ISharedPreferences _preferences;

    public AndroidHelperSettings(Context context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _preferences = context.GetSharedPreferences(FileName, FileCreationMode.Private)
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
