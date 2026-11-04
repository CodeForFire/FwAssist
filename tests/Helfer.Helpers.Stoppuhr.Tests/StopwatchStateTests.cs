namespace Helfer.Helpers.Stoppuhr.Tests;

public class StopwatchStateTests
{
    // An arbitrary monotonic instant: only differences between instants mean anything.
    private static readonly TimeSpan T0 = TimeSpan.FromHours(50);

    private static TimeSpan At(double seconds) => T0 + TimeSpan.FromSeconds(seconds);

    [Fact]
    public void A_fresh_stopwatch_shows_zero_and_is_not_running()
    {
        Assert.False(StopwatchState.Zero.IsRunning);
        Assert.Equal(TimeSpan.Zero, StopwatchState.Zero.ElapsedAt(T0));
    }

    [Fact]
    public void Running_time_comes_from_the_clock()
    {
        var s = StopwatchState.Zero.Start(T0);

        Assert.True(s.IsRunning);
        Assert.Equal(TimeSpan.FromSeconds(90), s.ElapsedAt(At(90)));
    }

    [Fact]
    public void Stopping_banks_the_time_and_a_new_run_adds_to_it()
    {
        var s = StopwatchState.Zero.Start(T0).Stop(At(30));

        Assert.False(s.IsRunning);
        Assert.Equal(TimeSpan.FromSeconds(30), s.ElapsedAt(At(3600)));

        s = s.Start(At(100));
        Assert.Equal(TimeSpan.FromSeconds(40), s.ElapsedAt(At(110)));
    }

    [Fact]
    public void Starting_twice_keeps_the_first_start()
    {
        var s = StopwatchState.Zero.Start(T0);

        Assert.Same(s, s.Start(At(10)));
    }

    [Fact]
    public void Stopping_a_stopped_stopwatch_changes_nothing() =>
        Assert.Same(StopwatchState.Zero, StopwatchState.Zero.Stop(T0));
}
