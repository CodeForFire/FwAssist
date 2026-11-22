namespace FwAssist.AppLogic.Services.Location;

/// <summary>A new heading from <see cref="ICompass.HeadingChanged"/>.</summary>
public sealed class HeadingEventArgs(double degrees) : EventArgs
{
    /// <summary>Degrees clockwise from true north, 0 up to 360.</summary>
    public double Degrees { get; } = degrees;
}
