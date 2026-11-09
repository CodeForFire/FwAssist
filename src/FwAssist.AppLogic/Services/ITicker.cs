namespace FwAssist.AppLogic.Services;

/// <summary>
/// Periodic UI-thread tick (~once per second) used to refresh time-based view state.
/// The testability seam next to <see cref="TimeProvider"/>: tests substitute a ticker they fire
/// synchronously; the UI layer implements it over a real timer.
/// </summary>
public interface ITicker
{
    /// <summary>Invokes <paramref name="onTick"/> roughly once per second on the UI
    /// thread until the returned subscription is disposed.</summary>
    IDisposable Subscribe(Action onTick);
}
