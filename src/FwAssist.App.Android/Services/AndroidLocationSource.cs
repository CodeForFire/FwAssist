using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using FwAssist.AppLogic.Services.Location;
using FwAssist.Core.Map;
using Location = Android.Locations.Location;

namespace FwAssist.App.Android.Services;

/// <summary>
/// The phone's position from the platform's <see cref="LocationManager"/>, GPS and network, only
/// while the Richtungspfeil runs and the app is in the foreground. The fixes go to the one caller
/// and nowhere else: nothing is stored, nothing is sent.
/// </summary>
internal sealed class AndroidLocationSource : Java.Lang.Object, ILocationSource, ILocationListener
{
    /// <summary>The request code MainActivity hands back in OnRequestPermissionsResult.</summary>
    public const int PermissionRequestCode = 4201;

    // How often to ask for a fix, in milliseconds; walking speed needs no more.
    private const long Interval = 1000;

    // A platform fix older than this, in milliseconds, is no use as a first position.
    private const long FreshEnough = 30_000;

    // A less accurate fix (the network's) does not replace a better one (the GPS's) younger than this.
    private static readonly TimeSpan KeepBetterFor = TimeSpan.FromSeconds(5);

    private static readonly string[] Permissions = [Manifest.Permission.AccessFineLocation, Manifest.Permission.AccessCoarseLocation];

    private readonly Context _context;
    private readonly TimeProvider _time;
    private TaskCompletionSource<bool>? _permission;
    private bool _running;
    private bool _paused;

    public AndroidLocationSource(Context context, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(time);
        _context = context;
        _time = time;
    }

    public event EventHandler<GeoFixEventArgs>? FixChanged;

    /// <summary>The activity that can show the permission dialog; MainActivity sets it on every create.</summary>
    public Activity? Activity { get; set; }

    /// <summary>The last fix handed out, for the compass's declination; null before the first.</summary>
    public GeoFix? Last { get; private set; }

    private LocationManager? Manager => _context.GetSystemService(Context.LocationService) as LocationManager;

    public async Task<LocationAccess> StartAsync(CancellationToken cancellationToken = default)
    {
        if (!HasPermission())
        {
            if (Activity is not { } activity)
            {
                return LocationAccess.Denied;
            }

            _permission?.TrySetResult(false);
            var pending = new TaskCompletionSource<bool>();
            _permission = pending;
            activity.RequestPermissions(Permissions, PermissionRequestCode);
            using (cancellationToken.Register(() => pending.TrySetCanceled(cancellationToken)))
            {
                if (!await pending.Task)
                {
                    return LocationAccess.Denied;
                }
            }
        }

        if (Manager is not { } manager || EnabledProviders(manager).Count == 0)
        {
            return LocationAccess.SwitchedOff;
        }

        _running = true;
        if (!_paused)
        {
            Request(manager);
        }

        return LocationAccess.Granted;
    }

    public void Stop()
    {
        _running = false;
        Manager?.RemoveUpdates(this);
    }

    /// <summary>The app went to the background: the GPS stops until it comes back.</summary>
    public void Pause()
    {
        _paused = true;
        if (_running)
        {
            Manager?.RemoveUpdates(this);
        }
    }

    /// <summary>The app is in the foreground again: a running Richtungspfeil gets fixes again.</summary>
    public void Resume()
    {
        _paused = false;
        if (_running && Manager is { } manager && HasPermission())
        {
            Request(manager);
        }
    }

    /// <summary>The answer to the permission dialog, handed on by MainActivity.</summary>
    public void OnPermissionResult(int requestCode, Permission[] grantResults)
    {
        ArgumentNullException.ThrowIfNull(grantResults);
        if (requestCode != PermissionRequestCode)
        {
            return;
        }

        var pending = _permission;
        _permission = null;
        pending?.TrySetResult(grantResults.Any(r => r == Permission.Granted));
    }

    public void OnLocationChanged(Location location)
    {
        ArgumentNullException.ThrowIfNull(location);
        if (GeoPoint.TryCreate(location.Latitude, location.Longitude) is not { } position)
        {
            return;
        }

        var accuracy = location.HasAccuracy ? location.Accuracy : 50.0;
        var now = _time.GetUtcNow();
        if (Last is { } last && accuracy > last.AccuracyMetres && now - last.Time < KeepBetterFor)
        {
            return;
        }

        var fix = new GeoFix(position, accuracy, now);
        Last = fix;
        FixChanged?.Invoke(this, new GeoFixEventArgs(fix));
    }

    private bool HasPermission() =>
        Permissions.Any(p => _context.CheckSelfPermission(p) == Permission.Granted);

    private static List<string> EnabledProviders(LocationManager manager) =>
        new[] { LocationManager.GpsProvider, LocationManager.NetworkProvider }
            .Where(p => manager.IsProviderEnabled(p))
            .ToList();

    // Asks every enabled provider for fixes on the main looper, so they arrive on the UI thread,
    // and hands out a recent last-known position at once so the arrow need not wait for the GPS.
    private void Request(LocationManager manager)
    {
        foreach (var provider in EnabledProviders(manager))
        {
            manager.RequestLocationUpdates(provider, Interval, 0f, this);
            if (manager.GetLastKnownLocation(provider) is { } known
                && Java.Lang.JavaSystem.CurrentTimeMillis() - known.Time < FreshEnough)
            {
                OnLocationChanged(known);
            }
        }
    }
}
