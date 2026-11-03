namespace Helfer.Testing;

/// <summary>A clock that moves only when a test moves it.</summary>
internal sealed class FakeTimeProvider : TimeProvider
{
    private DateTimeOffset _now;

    public FakeTimeProvider(DateTimeOffset? start = null) =>
        _now = start ?? new DateTimeOffset(2026, 11, 3, 14, 0, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => _now;

    public void Advance(TimeSpan by) => _now += by;
}
