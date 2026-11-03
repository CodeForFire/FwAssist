namespace Helfer.Helpers.Stoppuhr;

/// <summary>
/// A stopwatch as a value: the time banked by earlier runs, plus the moment the current run began.
/// Elapsed time is always computed from the clock, never counted up by ticks, so a phone that
/// sleeps through a minute still shows the right time when it wakes.
/// </summary>
public sealed record StopwatchState(TimeSpan Banked, DateTimeOffset? RunningSince)
{
    public static StopwatchState Zero { get; } = new(TimeSpan.Zero, null);

    public bool IsRunning => RunningSince is not null;

    /// <summary>
    /// Elapsed time at <paramref name="now"/>. A clock that went backwards (the user changed the
    /// phone's time) adds nothing rather than taking banked time away.
    /// </summary>
    public TimeSpan ElapsedAt(DateTimeOffset now)
    {
        if (RunningSince is not { } since)
        {
            return Banked;
        }

        var run = now - since;
        return run > TimeSpan.Zero ? Banked + run : Banked;
    }

    public StopwatchState Start(DateTimeOffset now) => IsRunning ? this : this with { RunningSince = now };

    public StopwatchState Stop(DateTimeOffset now) => IsRunning ? new StopwatchState(ElapsedAt(now), null) : this;
}
