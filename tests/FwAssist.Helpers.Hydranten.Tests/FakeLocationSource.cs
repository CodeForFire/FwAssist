using FwAssist.AppLogic.Services.Location;
using FwAssist.Core.Map;

namespace FwAssist.Helpers.Hydranten.Tests;

/// <summary>A position source a test moves by hand; <see cref="Access"/> is what starting it answers.</summary>
internal sealed class FakeLocationSource : ILocationSource
{
    public event EventHandler<GeoFixEventArgs>? FixChanged;

    public LocationAccess Access { get; set; } = LocationAccess.Granted;

    /// <summary>When set, starting waits for it, as the permission dialog does.</summary>
    public TaskCompletionSource? PermissionGate { get; set; }

    public bool IsRunning { get; private set; }

    public int Starts { get; private set; }

    public int Stops { get; private set; }

    public async Task<LocationAccess> StartAsync(CancellationToken cancellationToken = default)
    {
        Starts++;
        if (PermissionGate is { } gate)
        {
            await gate.Task;
        }

        IsRunning = Access == LocationAccess.Granted;
        return Access;
    }

    public void Stop()
    {
        Stops++;
        IsRunning = false;
    }

    public void Move(GeoPoint position, DateTimeOffset at, double accuracy = 5) =>
        FixChanged?.Invoke(this, new GeoFixEventArgs(new GeoFix(position, accuracy, at)));
}
