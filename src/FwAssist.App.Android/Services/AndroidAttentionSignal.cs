using Android.Content;
using Android.Media;
using Android.OS;
using FwAssist.AppLogic.Services;

namespace FwAssist.App.Android.Services;

/// <summary>
/// A double beep from Android's own tone generator and a short vibration, as the phone's ringer
/// allows: both when it rings, the vibration alone on vibrate, nothing on silent. The tone is
/// built in, so the app ships no sound file. It plays on the notification stream, so it follows
/// that stream's volume.
/// </summary>
internal sealed class AndroidAttentionSignal : IAttentionSignal, IDisposable
{
    private const long VibrationMilliseconds = 400;

    private readonly Context _context;

    // Made on the first signal and kept for the process, like the shell that plays it.
    private ToneGenerator? _tone;

    public AndroidAttentionSignal(Context context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public void Play()
    {
        var ringer = (_context.GetSystemService(Context.AudioService) as AudioManager)?.RingerMode ?? RingerMode.Normal;
        if (ringer == RingerMode.Silent)
        {
            return;
        }

        if (ringer == RingerMode.Normal)
        {
            _tone ??= new ToneGenerator(global::Android.Media.Stream.Notification, Volume.Max);
            _tone.StartTone(Tone.PropBeep2);
        }

        Vibrate();
    }

    public void Dispose() => _tone?.Dispose();

    // Android 7 has no VibrationEffect, and its older call is deprecated; it gets the tone alone.
    private void Vibrate()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(26)
            || _context.GetSystemService(Java.Lang.Class.FromType(typeof(Vibrator))) is not Vibrator { HasVibrator: true } vibrator)
        {
            return;
        }

        using var effect = VibrationEffect.CreateOneShot(VibrationMilliseconds, VibrationEffect.DefaultAmplitude);
        vibrator.Vibrate(effect);
    }
}
