using FwAssist.AppLogic.Services;

namespace FwAssist.Testing;

/// <summary>An <see cref="ITicker"/> that ticks when the test calls <see cref="Tick"/>.</summary>
internal sealed class ManualTicker : ITicker
{
    private readonly TickSubscribers _subscribers = new();

    public int SubscriberCount => _subscribers.Count;

    public IDisposable Subscribe(Action onTick)
    {
        _subscribers.Add(onTick);
        return new Subscription(() => _subscribers.Remove(onTick));
    }

    public void Tick() => _subscribers.Notify();

    private sealed class Subscription(Action dispose) : IDisposable
    {
        private Action? _dispose = dispose;

        public void Dispose()
        {
            _dispose?.Invoke();
            _dispose = null;
        }
    }
}
