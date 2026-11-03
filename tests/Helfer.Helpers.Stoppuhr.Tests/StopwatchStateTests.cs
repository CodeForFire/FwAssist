namespace Helfer.Helpers.Stoppuhr.Tests;

public class StopwatchStateTests
{
    private static readonly DateTimeOffset T0 = new(2026, 11, 3, 14, 0, 0, TimeSpan.Zero);

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
        Assert.Equal(TimeSpan.FromSeconds(90), s.ElapsedAt(T0.AddSeconds(90)));
    }

    [Fact]
    public void Stopping_banks_the_time_and_a_new_run_adds_to_it()
    {
        var s = StopwatchState.Zero.Start(T0).Stop(T0.AddSeconds(30));

        Assert.False(s.IsRunning);
        Assert.Equal(TimeSpan.FromSeconds(30), s.ElapsedAt(T0.AddHours(1)));

        s = s.Start(T0.AddSeconds(100));
        Assert.Equal(TimeSpan.FromSeconds(40), s.ElapsedAt(T0.AddSeconds(110)));
    }

    [Fact]
    public void Starting_twice_keeps_the_first_start()
    {
        var s = StopwatchState.Zero.Start(T0);

        Assert.Same(s, s.Start(T0.AddSeconds(10)));
    }

    [Fact]
    public void Stopping_a_stopped_stopwatch_changes_nothing() =>
        Assert.Same(StopwatchState.Zero, StopwatchState.Zero.Stop(T0));

    [Fact]
    public void A_clock_set_back_while_running_takes_no_banked_time_away()
    {
        var s = StopwatchState.Zero.Start(T0).Stop(T0.AddSeconds(30)).Start(T0.AddSeconds(60));

        Assert.Equal(TimeSpan.FromSeconds(30), s.ElapsedAt(T0));
    }
}
