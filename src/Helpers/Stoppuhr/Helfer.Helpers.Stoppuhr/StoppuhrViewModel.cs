using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Helfer.AppLogic.Services;
using Helfer.Core;

namespace Helfer.Helpers.Stoppuhr;

/// <summary>
/// The demo helper: start, stop, zurücksetzen. Small on purpose: it exists to prove the shell, the
/// launcher icon and the ticker end to end, and to be the pattern the next helper copies.
/// </summary>
public sealed partial class StoppuhrViewModel : ObservableObject
{
    private readonly TimeProvider _time;
    private readonly ITicker _ticker;
    private StopwatchState _state = StopwatchState.Zero;
    private IDisposable? _tick;

    public StoppuhrViewModel(TimeProvider time, ITicker ticker)
    {
        ArgumentNullException.ThrowIfNull(time);
        ArgumentNullException.ThrowIfNull(ticker);
        _time = time;
        _ticker = ticker;
        _display = Formatting.Duration(TimeSpan.Zero);
    }

    [ObservableProperty]
    private string _display;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StartStopLabel))]
    [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
    private bool _isRunning;

    public string StartStopLabel => IsRunning ? "STOPP" : "START";

    [RelayCommand]
    private void StartStop()
    {
        var now = _time.GetUtcNow();
        _state = _state.IsRunning ? _state.Stop(now) : _state.Start(now);
        IsRunning = _state.IsRunning;

        // Tick only while running: a stopped stopwatch has nothing to redraw, and a ticker with no
        // subscriber stops its timer, so an idle app does not wake the phone once a second.
        if (IsRunning)
        {
            _tick ??= _ticker.Subscribe(Refresh);
        }
        else
        {
            _tick?.Dispose();
            _tick = null;
        }

        Refresh();
    }

    private bool CanReset() => !IsRunning;

    [RelayCommand(CanExecute = nameof(CanReset))]
    private void Reset()
    {
        _state = StopwatchState.Zero;
        Refresh();
    }

    private void Refresh() => Display = Formatting.Duration(_state.ElapsedAt(_time.GetUtcNow()));
}
