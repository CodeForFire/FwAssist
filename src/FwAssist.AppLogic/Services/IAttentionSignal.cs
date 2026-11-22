namespace FwAssist.AppLogic.Services;

/// <summary>
/// A short tone and vibration that tell the user something has happened while they look at the
/// street rather than the phone. It follows the phone's sound setting: tone and vibration when it
/// rings, the vibration alone on vibrate, nothing on silent.
/// </summary>
public interface IAttentionSignal
{
    /// <summary>Plays the signal once and returns at once; it never waits for the tone to end.</summary>
    void Play();
}
