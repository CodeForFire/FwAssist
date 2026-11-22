namespace FwAssist.AppLogic.Services.Location;

/// <summary>Which way the top of the phone points, only while something has started it.</summary>
public interface ICompass
{
    /// <summary>The heading in degrees clockwise from true north, 0 up to 360. Raised on the UI thread.</summary>
    event EventHandler<HeadingEventArgs>? HeadingChanged;

    /// <summary>Starts the readings; false when the phone has no compass.</summary>
    bool Start();

    /// <summary>Stops the readings. Stopping twice does nothing.</summary>
    void Stop();
}
