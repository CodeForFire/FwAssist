using Helfer.AppLogic.Helpers;
using Helfer.Testing;

namespace Helfer.Helpers.Stoppuhr.Tests;

public class StoppuhrViewModelTests
{
    private readonly FakeTimeProvider _time = new();
    private readonly ManualTicker _ticker = new();

    private StoppuhrViewModel Vm() => new(_time, _ticker);

    [Fact]
    public void Starts_at_zero_with_the_start_button()
    {
        var vm = Vm();

        Assert.Equal("00:00", vm.Display);
        Assert.Equal("START", vm.StartStopLabel);
        Assert.False(vm.IsRunning);
    }

    [Fact]
    public void A_running_stopwatch_follows_the_clock_on_every_tick()
    {
        var vm = Vm();
        vm.StartStopCommand.Execute(null);

        _time.Advance(TimeSpan.FromSeconds(65));
        _ticker.Tick();

        Assert.Equal("01:05", vm.Display);
        Assert.Equal("STOPP", vm.StartStopLabel);
    }

    [Fact]
    public void Changing_the_phones_time_does_not_change_the_measured_time()
    {
        var vm = Vm();
        vm.StartStopCommand.Execute(null);
        _time.Advance(TimeSpan.FromMinutes(1));

        _time.SetWallClock(_time.GetUtcNow() - TimeSpan.FromMinutes(5));
        _ticker.Tick();
        Assert.Equal("01:00", vm.Display);

        _time.SetWallClock(_time.GetUtcNow() + TimeSpan.FromHours(2));
        vm.StartStopCommand.Execute(null);
        Assert.Equal("01:00", vm.Display);
    }

    [Fact]
    public void It_ticks_only_while_running()
    {
        var vm = Vm();
        Assert.Equal(0, _ticker.SubscriberCount);

        vm.StartStopCommand.Execute(null);
        Assert.Equal(1, _ticker.SubscriberCount);

        vm.StartStopCommand.Execute(null);
        Assert.Equal(0, _ticker.SubscriberCount);
    }

    [Fact]
    public void Stopping_freezes_the_display_at_the_moment_of_the_press()
    {
        var vm = Vm();
        vm.StartStopCommand.Execute(null);
        _time.Advance(TimeSpan.FromSeconds(12.7));

        vm.StartStopCommand.Execute(null);
        _time.Advance(TimeSpan.FromMinutes(5));
        _ticker.Tick();

        Assert.Equal("00:12", vm.Display);
    }

    [Fact]
    public void Zuruecksetzen_is_only_possible_while_stopped_and_returns_to_zero()
    {
        var vm = Vm();
        vm.StartStopCommand.Execute(null);
        _time.Advance(TimeSpan.FromSeconds(30));
        Assert.False(vm.ResetCommand.CanExecute(null));

        vm.StartStopCommand.Execute(null);
        Assert.True(vm.ResetCommand.CanExecute(null));
        vm.ResetCommand.Execute(null);

        Assert.Equal("00:00", vm.Display);
    }

    [Fact]
    public void The_module_builds_its_view_model_from_the_shell_services()
    {
        var module = new StoppuhrModule();

        var vm = module.CreateViewModel(new HelperServices(_time, _ticker));

        Assert.IsType<StoppuhrViewModel>(vm);
        Assert.Equal(StoppuhrModule.HelperId, module.Id);
    }
}
