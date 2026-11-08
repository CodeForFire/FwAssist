using Helfer.AppLogic.Services;

namespace Helfer.Testing;

/// <summary>Records the links the app opened; <see cref="Refuse"/> makes them fail.</summary>
internal sealed class FakeExternalLinks : IExternalLinks
{
    private readonly List<Uri> _opened = [];

    public bool Refuse { get; set; }

    public IReadOnlyList<Uri> Opened => _opened;

    public bool TryOpen(Uri uri)
    {
        _opened.Add(uri);
        return !Refuse;
    }
}
