namespace FwAssist.App.Android.Services;

/// <summary>The naming rule for a helper's activity-alias, kept in one place.</summary>
internal static class LauncherAlias
{
    public const string Prefix = "de.codeforfire.helfer.launcher.";

    public static string For(string helperId) => Prefix + helperId;

    /// <summary>The helper an alias opens; <see langword="null"/> for the app's own icon.</summary>
    public static string? HelperIdOf(string? componentClassName) =>
        componentClassName is not null && componentClassName.StartsWith(Prefix, StringComparison.Ordinal)
            ? componentClassName[Prefix.Length..]
            : null;
}
