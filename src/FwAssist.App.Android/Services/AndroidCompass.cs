using System.Diagnostics.CodeAnalysis;
using Android.App;
using Android.Content;
using Android.Hardware;
using Android.Runtime;
using Android.Views;
using FwAssist.AppLogic.Services.Location;

namespace FwAssist.App.Android.Services;

/// <summary>
/// The heading from the rotation-vector sensor, corrected from magnetic to true north with the
/// declination at the last fix and turned by how the screen is rotated, so it is the direction the
/// top of the screen points. Runs only while the Richtungspfeil does and the app is in the
/// foreground.
/// </summary>
internal sealed class AndroidCompass : Java.Lang.Object, ICompass, ISensorEventListener
{
    // How much of each new reading goes into the heading: steadies the arrow in a shaking hand.
    private const double Smoothing = 0.25;

    private readonly Context _context;
    private readonly Func<GeoFix?> _lastFix;
    private bool _running;
    private bool _paused;
    private double? _smoothed;

    public AndroidCompass(Context context, Func<GeoFix?> lastFix)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(lastFix);
        _context = context;
        _lastFix = lastFix;
    }

    public event EventHandler<HeadingEventArgs>? HeadingChanged;

    /// <summary>The activity whose screen rotation turns the heading; MainActivity sets it on every create.</summary>
    public Activity? Activity { get; set; }

    private SensorManager? Manager => _context.GetSystemService(Context.SensorService) as SensorManager;

    public bool Start()
    {
        if (Manager?.GetDefaultSensor(SensorType.RotationVector) is null)
        {
            return false;
        }

        _running = true;
        _smoothed = null;
        if (!_paused)
        {
            Register();
        }

        return true;
    }

    public void Stop()
    {
        _running = false;
        Manager?.UnregisterListener(this);
    }

    /// <summary>The app went to the background: the sensor stops until it comes back.</summary>
    public void Pause()
    {
        _paused = true;
        if (_running)
        {
            Manager?.UnregisterListener(this);
        }
    }

    /// <summary>The app is in the foreground again: a running Richtungspfeil gets readings again.</summary>
    public void Resume()
    {
        _paused = false;
        if (_running)
        {
            Register();
        }
    }

    public void OnSensorChanged(SensorEvent? e)
    {
        if (e?.Values is not { Count: >= 3 } values)
        {
            return;
        }

        var rotation = new float[9];
        SensorManager.GetRotationMatrixFromVector(rotation, values.Take(4).ToArray());
        var orientation = new float[3];
        SensorManager.GetOrientation(rotation, orientation);
        var magnetic = orientation[0] * 180.0 / Math.PI;
        var heading = Normalize(magnetic + Declination() + ScreenRotation());

        // Smoothed the short way round, so 359° and 1° average to 0°, not 180°.
        _smoothed = _smoothed is { } previous
            ? Normalize(previous + (Smoothing * Math.IEEERemainder(heading - previous, 360)))
            : heading;
        HeadingChanged?.Invoke(this, new HeadingEventArgs(_smoothed.Value));
    }

    public void OnAccuracyChanged(Sensor? sensor, [GeneratedEnum] SensorStatus accuracy)
    {
        // The rotation vector reports its own accuracy in every reading; nothing to do here.
    }

    private static double Normalize(double degrees)
    {
        var turn = degrees % 360;
        return turn < 0 ? turn + 360 : turn;
    }

    private void Register()
    {
        if (Manager is { } manager && manager.GetDefaultSensor(SensorType.RotationVector) is { } sensor)
        {
            manager.RegisterListener(this, sensor, SensorDelay.Ui);
        }
    }

    // East of magnetic north is positive. Without a fix yet, none: in Germany it is about 3°.
    private double Declination() =>
        _lastFix() is { } fix
            ? new GeomagneticField((float)fix.Position.Latitude, (float)fix.Position.Longitude, 0f, Java.Lang.JavaSystem.CurrentTimeMillis()).Declination
            : 0;

    // A screen turned to landscape has its top pointing a quarter turn away from the phone's top.
    private double ScreenRotation() => DisplayRotation() switch
    {
        SurfaceOrientation.Rotation90 => 90,
        SurfaceOrientation.Rotation180 => 180,
        SurfaceOrientation.Rotation270 => 270,
        _ => 0,
    };

    [SuppressMessage("Interoperability", "CA1422", Justification = "WindowManager.DefaultDisplay is the only way below API 30, and only used there; API 30 and later read Activity.Display.")]
    private SurfaceOrientation? DisplayRotation()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(30))
        {
            return Activity?.Display?.Rotation;
        }

        // Deprecated from API 30, which the branch above covers; below it there is no other way.
#pragma warning disable CS0618
        return Activity?.WindowManager?.DefaultDisplay?.Rotation;
#pragma warning restore CS0618
    }
}
