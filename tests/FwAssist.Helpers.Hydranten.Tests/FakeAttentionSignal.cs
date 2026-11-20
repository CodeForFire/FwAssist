using FwAssist.AppLogic.Services;

namespace FwAssist.Helpers.Hydranten.Tests;

/// <summary>A signal that only counts how often it was played.</summary>
internal sealed class FakeAttentionSignal : IAttentionSignal
{
    public int Played { get; private set; }

    public void Play() => Played++;
}
