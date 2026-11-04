namespace Helfer.Helpers.Stoppuhr;

/// <summary>
/// A stopwatch as a value: the time banked by earlier runs, plus the instant the current run began.
/// Instants are read off a monotonic clock (<see cref="TimeProvider.GetTimestamp"/>), never the
/// wall clock, so changing the phone's time neither adds nor removes any. Elapsed time is always
/// computed from that clock, never counted up by ticks, so a phone that sleeps through a minute
/// still shows the right time when it wakes.
/// </summary>
public sealed record StopwatchState(TimeSpan Banked, TimeSpan? RunningSince)
{
    public static StopwatchState Zero { get; } = new(TimeSpan.Zero, null);

    public bool IsRunning => RunningSince is not null;

    /// <summary>Elapsed time at the monotonic instant <paramref name="now"/>.</summary>
    public TimeSpan ElapsedAt(TimeSpan now) => RunningSince is { } since ? Banked + (now - since) : Banked;

    public StopwatchState Start(TimeSpan now) => IsRunning ? this : this with { RunningSince = now };

    public StopwatchState Stop(TimeSpan now) => IsRunning ? new StopwatchState(ElapsedAt(now), null) : this;
}
