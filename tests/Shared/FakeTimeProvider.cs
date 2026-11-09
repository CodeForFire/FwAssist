namespace FwAssist.Testing;

/// <summary>
/// A clock that moves only when a test moves it. The wall clock and the monotonic timestamp move
/// together under <see cref="Advance"/>; <see cref="SetWallClock"/> moves the wall clock alone, the
/// way a user changing the phone's time does.
/// </summary>
internal sealed class FakeTimeProvider : TimeProvider
{
    private DateTimeOffset _now;
    private long _timestamp;

    public FakeTimeProvider(DateTimeOffset? start = null) =>
        _now = start ?? new DateTimeOffset(2026, 11, 3, 14, 0, 0, TimeSpan.Zero);

    public override long TimestampFrequency => TimeSpan.TicksPerSecond;

    public override DateTimeOffset GetUtcNow() => _now;

    public override long GetTimestamp() => _timestamp;

    public void Advance(TimeSpan by)
    {
        _now += by;
        _timestamp += by.Ticks;
    }

    public void SetWallClock(DateTimeOffset now) => _now = now;
}
