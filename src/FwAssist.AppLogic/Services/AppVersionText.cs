using System.Reflection;

namespace FwAssist.AppLogic.Services;

/// <summary>
/// The version the home screen shows. Read from the informational version, because that is the one
/// that keeps what was passed as <c>-p:Version=</c>: a beta's <c>-beta.1</c>, an rc's <c>-rc.2</c>.
/// <see cref="AssemblyName.Version"/> is the numeric four-part assembly version and drops both.
/// </summary>
public static class AppVersionText
{
    /// <summary>The display version of <paramref name="assembly"/>.</summary>
    public static string Of(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        return Display(
            assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
            assembly.GetName().Version);
    }

    /// <summary>
    /// <paramref name="informational"/> without its <c>+</c> build metadata, the commit SourceLink
    /// appends; <paramref name="fallback"/> when that leaves nothing, and <c>0.0.0</c> after that.
    /// </summary>
    internal static string Display(string? informational, Version? fallback)
    {
        var version = informational?.Split('+', 2)[0];
        return string.IsNullOrEmpty(version) ? fallback?.ToString() ?? "0.0.0" : version;
    }
}
