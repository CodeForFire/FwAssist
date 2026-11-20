namespace FwAssist.AppLogic.Services.Location;

/// <summary>A new fix from <see cref="ILocationSource.FixChanged"/>.</summary>
public sealed class GeoFixEventArgs(GeoFix fix) : EventArgs
{
    public GeoFix Fix { get; } = fix ?? throw new ArgumentNullException(nameof(fix));
}
