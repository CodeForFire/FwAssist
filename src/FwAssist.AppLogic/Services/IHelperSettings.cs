namespace FwAssist.AppLogic.Services;

/// <summary>
/// Small values a helper keeps across a process death, as text. Android kills a background app
/// without warning, so what the user would miss cannot live only in a view model. Keys start with
/// the helper's id, so two helpers cannot collide.
/// </summary>
public interface IHelperSettings
{
    /// <summary>The stored value, or <see langword="null"/> when there is none yet.</summary>
    string? Get(string key);

    void Set(string key, string value);
}
