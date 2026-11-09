using System.Diagnostics.CodeAnalysis;

namespace FwAssist.AppLogic.Services;

/// <summary>
/// The http(s)-only check in front of <see cref="IExternalLinks"/>, copied from Lagebuch. On Android
/// a link ends up in <c>Intent.ActionView</c>, a known intent-redirection surface for schemes such
/// as intent:// or content://, so nothing else may reach it.
/// </summary>
public static class HttpUrlValidator
{
    /// <summary>True only for an absolute, well-formed http or https URL.</summary>
    public static bool TryGetHttpUri(string? input, [NotNullWhen(true)] out Uri? uri)
    {
        if (Uri.TryCreate(input, UriKind.Absolute, out var candidate) &&
            (candidate.Scheme == Uri.UriSchemeHttp || candidate.Scheme == Uri.UriSchemeHttps))
        {
            uri = candidate;
            return true;
        }

        uri = null;
        return false;
    }
}
