namespace Helfer.AppLogic.Services;

/// <summary>Hands a web address to whatever opens links on this device.</summary>
public interface IExternalLinks
{
    /// <summary>
    /// Opens <paramref name="uri"/> outside the app. Returns <see langword="false"/> when the
    /// platform could not, for example because the phone has no browser.
    /// </summary>
    bool TryOpen(Uri uri);
}
