using Helfer.AppLogic.Services;

namespace Helfer.Testing;

/// <summary>Launcher icons in memory; <see cref="Refuse"/> makes the next changes fail.</summary>
internal sealed class FakeLauncherIcons : ILauncherIcons
{
    private readonly HashSet<string> _disabled = new(StringComparer.Ordinal);

    public bool Refuse { get; set; }

    /// <summary>How many times the app asked to change an icon.</summary>
    public int Writes { get; private set; }

    /// <summary>A change made outside the app, in the launcher or by the system.</summary>
    public void ChangeOutside(string helperId, bool enabled)
    {
        if (enabled)
        {
            _disabled.Remove(helperId);
        }
        else
        {
            _disabled.Add(helperId);
        }
    }

    public bool IsEnabled(string helperId) => !_disabled.Contains(helperId);

    public bool TrySetEnabled(string helperId, bool enabled)
    {
        Writes++;
        if (Refuse)
        {
            return false;
        }

        if (enabled)
        {
            _disabled.Remove(helperId);
        }
        else
        {
            _disabled.Add(helperId);
        }

        return true;
    }
}
