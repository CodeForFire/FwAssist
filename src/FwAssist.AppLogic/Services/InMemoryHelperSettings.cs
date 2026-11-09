namespace FwAssist.AppLogic.Services;

/// <summary>Settings for a host with no storage, and for tests. Gone with the process.</summary>
public sealed class InMemoryHelperSettings : IHelperSettings
{
    private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal);

    public string? Get(string key) => _values.GetValueOrDefault(key);

    public void Set(string key, string value) => _values[key] = value;
}
