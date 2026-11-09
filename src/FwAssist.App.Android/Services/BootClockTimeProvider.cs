using Android.OS;

namespace FwAssist.App.Android.Services;

/// <summary>
/// <see cref="TimeProvider.System"/> with its monotonic timestamp taken from
/// <see cref="SystemClock.ElapsedRealtimeNanos"/>. .NET's own timestamp is CLOCK_MONOTONIC, which
/// stands still while the phone is asleep: a Stoppuhr running in a pocket with the screen off would
/// lose every minute the CPU was suspended. Elapsed realtime keeps counting through sleep and,
/// unlike the wall clock, never jumps when the user changes the phone's time.
/// </summary>
internal sealed class BootClockTimeProvider : TimeProvider
{
    public override long TimestampFrequency => 1_000_000_000;

    public override long GetTimestamp() => SystemClock.ElapsedRealtimeNanos();
}
