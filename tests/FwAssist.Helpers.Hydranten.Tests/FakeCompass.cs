using FwAssist.AppLogic.Services.Location;

namespace FwAssist.Helpers.Hydranten.Tests;

/// <summary>A compass a test turns by hand; <see cref="Present"/> false is a phone without one.</summary>
internal sealed class FakeCompass : ICompass
{
    public event EventHandler<HeadingEventArgs>? HeadingChanged;

    public bool Present { get; set; } = true;

    public bool IsRunning { get; private set; }

    public bool Start()
    {
        IsRunning = Present;
        return Present;
    }

    public void Stop() => IsRunning = false;

    public void Turn(double degrees) => HeadingChanged?.Invoke(this, new HeadingEventArgs(degrees));
}
