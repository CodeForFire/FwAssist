namespace FwAssist.AppLogic.Services.Location;

/// <summary>
/// The phone's live position, only while something has started it. Nothing is stored and nothing
/// leaves the device: a fix goes to the view model that asked for it and nowhere else.
/// </summary>
public interface ILocationSource
{
    /// <summary>A new fix. Raised on the UI thread.</summary>
    event EventHandler<GeoFixEventArgs>? FixChanged;

    /// <summary>
    /// Asks for the location permission if it is not granted yet, then starts the fixes. A caller
    /// that is told anything but <see cref="LocationAccess.Granted"/> gets no fixes.
    /// </summary>
    Task<LocationAccess> StartAsync(CancellationToken cancellationToken = default);

    /// <summary>Stops the fixes, so the GPS does not drain the battery. Stopping twice does nothing.</summary>
    void Stop();
}
