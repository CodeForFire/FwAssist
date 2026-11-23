using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FwAssist.Core;

namespace FwAssist.Helpers.Gefahrgut;

/// <summary>
/// One search field over ADR Tabelle A: the lower number of a Warntafel, or a word from the
/// Benennung. The search is not kept: it is typed and read in the same breath, as in Kennzeichnungen.
/// </summary>
public sealed partial class GefahrgutViewModel : ObservableObject
{
    /// <summary>Hits shown at most. A short word finds hundreds; nobody scrolls past the first screens.</summary>
    public const int MaxResults = 50;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSearching))]
    [NotifyPropertyChangedFor(nameof(Results))]
    [NotifyPropertyChangedFor(nameof(HasResults))]
    [NotifyPropertyChangedFor(nameof(HasNoHits))]
    [NotifyPropertyChangedFor(nameof(ResultTitle))]
    [NotifyPropertyChangedFor(nameof(HasMore))]
    [NotifyPropertyChangedFor(nameof(MoreNote))]
    private string? _query = string.Empty;

    private IReadOnlyList<Stoff>? _hits;

    public bool IsSearching => !string.IsNullOrWhiteSpace(Query);

    /// <summary>The first <see cref="MaxResults"/> hits.</summary>
    public IReadOnlyList<Stoff> Results => Hits.Count > MaxResults ? Hits.Take(MaxResults).ToArray() : Hits;

    public bool HasResults => Hits.Count > 0;

    /// <summary>A search that finds nothing says so instead of an empty page.</summary>
    public bool HasNoHits => IsSearching && Hits.Count == 0;

    /// <summary>Above the list: what was found, so a crew knows whether it reads one number or a search.</summary>
    public string ResultTitle => Hits switch
    {
        [] => string.Empty,
        [var only] => "UN " + only.UnNummer,
        _ when Hits.All(s => string.Equals(s.UnNummer, Hits[0].UnNummer, StringComparison.Ordinal)) =>
            "UN " + Hits[0].UnNummer + ": " + Count(Hits.Count) + " EINTRAGUNGEN",
        _ => Count(Hits.Count) + " TREFFER",
    };

    public bool HasMore => Hits.Count > MaxResults;

    public string MoreNote => HasMore
        ? "Nur die ersten " + Count(MaxResults) + " Treffer. Genauer suchen, um die übrigen zu sehen."
        : string.Empty;

    private IReadOnlyList<Stoff> Hits => _hits ??= TabelleA.Search(Query);

    // The hits are worked out once per query, before the properties that read them are told.
    partial void OnQueryChanging(string? value) => _hits = null;

    [RelayCommand]
    private void Clear() => Query = string.Empty;

    private static string Count(int value) => Formatting.Number(value);
}
