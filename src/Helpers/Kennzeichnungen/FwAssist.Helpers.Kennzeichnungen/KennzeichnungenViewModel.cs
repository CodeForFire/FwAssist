using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FwAssist.AppLogic.Services;
using FwAssist.Core;

namespace FwAssist.Helpers.Kennzeichnungen;

/// <summary>
/// A picture reference in three groups with one search field above it. The field takes either the
/// upper number of a Warntafel, which is read digit by digit, or a word, which searches every group
/// at once: a crew that does not recognise a marking does not know its group either. The same field
/// takes the lower number, the UN-Nummer: four digits find the Stoff in ADR Tabelle A, and a word
/// searches its Benennungen too. The group is kept in <see cref="IHelperSettings"/>; the search is
/// not, it is typed and read in the same breath.
/// </summary>
public sealed partial class KennzeichnungenViewModel : ObservableObject
{
    /// <summary>Stoffe shown at most. A short word finds hundreds; nobody scrolls past the first screens.</summary>
    public const int MaxStoffe = 50;

    private const string GruppeKey = KennzeichnungenModule.HelperId + ".gruppe";

    // "atzend" finds "ätzend" and "GASFLASCHE" finds "Gasflasche": gloves type neither umlauts
    // nor capitals reliably. The culture is fixed so that the phone's setting changes nothing.
    private static readonly CompareInfo German = CultureInfo.GetCultureInfo("de-DE").CompareInfo;

    private readonly IHelperSettings _settings;

    private IReadOnlyList<Stoff>? _stoffHits;

    public KennzeichnungenViewModel(IHelperSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings;
        _gruppe = Enum.TryParse<KennzeichenGruppe>(settings.Get(GruppeKey), out var stored) && Enum.IsDefined(stored)
            ? stored
            : KennzeichenGruppe.Warntafel;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsWarntafel))]
    [NotifyPropertyChangedFor(nameof(IsGefahrzettel))]
    [NotifyPropertyChangedFor(nameof(IsGhs))]
    [NotifyPropertyChangedFor(nameof(IsRohrleitung))]
    [NotifyPropertyChangedFor(nameof(IsGasflasche))]
    [NotifyPropertyChangedFor(nameof(IsUnNummer))]
    [NotifyPropertyChangedFor(nameof(GroupNote))]
    [NotifyPropertyChangedFor(nameof(HasGroupNote))]
    [NotifyPropertyChangedFor(nameof(Entries))]
    [NotifyPropertyChangedFor(nameof(ListTitle))]
    [NotifyPropertyChangedFor(nameof(HasEntries))]
    private KennzeichenGruppe _gruppe;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Entries))]
    [NotifyPropertyChangedFor(nameof(ListTitle))]
    [NotifyPropertyChangedFor(nameof(IsSearching))]
    [NotifyPropertyChangedFor(nameof(HasGroupNote))]
    [NotifyPropertyChangedFor(nameof(HasNoHits))]
    [NotifyPropertyChangedFor(nameof(HasEntries))]
    [NotifyPropertyChangedFor(nameof(Gefahrnummer))]
    [NotifyPropertyChangedFor(nameof(HasGefahrnummer))]
    [NotifyPropertyChangedFor(nameof(GefahrnummerMeaning))]
    [NotifyPropertyChangedFor(nameof(GefahrnummerNotes))]
    [NotifyPropertyChangedFor(nameof(Stoffe))]
    [NotifyPropertyChangedFor(nameof(HasStoffe))]
    [NotifyPropertyChangedFor(nameof(StoffeTitle))]
    [NotifyPropertyChangedFor(nameof(HasMoreStoffe))]
    [NotifyPropertyChangedFor(nameof(MoreStoffeNote))]
    private string? _query = string.Empty;

    // A RadioButton also writes false when another one is chosen, and that must not change the group.
    public bool IsWarntafel
    {
        get => Gruppe == KennzeichenGruppe.Warntafel;
        set => Choose(value, KennzeichenGruppe.Warntafel);
    }

    public bool IsGefahrzettel
    {
        get => Gruppe == KennzeichenGruppe.Gefahrzettel;
        set => Choose(value, KennzeichenGruppe.Gefahrzettel);
    }

    public bool IsGhs
    {
        get => Gruppe == KennzeichenGruppe.Ghs;
        set => Choose(value, KennzeichenGruppe.Ghs);
    }

    public bool IsRohrleitung
    {
        get => Gruppe == KennzeichenGruppe.Rohrleitung;
        set => Choose(value, KennzeichenGruppe.Rohrleitung);
    }

    public bool IsGasflasche
    {
        get => Gruppe == KennzeichenGruppe.Gasflasche;
        set => Choose(value, KennzeichenGruppe.Gasflasche);
    }

    public bool IsUnNummer
    {
        get => Gruppe == KennzeichenGruppe.UnNummer;
        set => Choose(value, KennzeichenGruppe.UnNummer);
    }

    public bool IsSearching => !string.IsNullOrWhiteSpace(Query);

    /// <summary>
    /// What a colour code cannot promise, said above the group that relies on one: EN 1089-3's
    /// introduction makes the Gefahrgutaufkleber the binding mark, and TRGS 201 Anhang 3 gives its
    /// colours as an example and asks for the Stoff and the direction on the pipe as well.
    /// </summary>
    public string GroupNote => Gruppe switch
    {
        KennzeichenGruppe.Gasflasche =>
            "Verbindlich ist der Gefahrgutaufkleber auf der Flasche. Die Farbe der Schulter hilft nur aus der Entfernung.",
        KennzeichenGruppe.Rohrleitung =>
            "Die Farben sind ein Beispiel aus der TRGS. Verbindlich ist die Beschriftung am Rohr mit Stoff und Fließrichtung.",
        KennzeichenGruppe.UnNummer =>
            "Die untere Nummer der orangen Warntafel eingeben, die UN-Nummer. Oder einen Teil des Stoffnamens.",
        _ => string.Empty,
    };

    /// <summary>Only on the group's own page: a search mixes groups, and the note would mislabel half of it.</summary>
    public bool HasGroupNote => !IsSearching && GroupNote.Length > 0;

    /// <summary>While searching: the hits of every group. Otherwise: the chosen group.</summary>
    public IReadOnlyList<Kennzeichen> Entries =>
        IsSearching ? Kennzeichenkatalog.All.Where(Matches).ToArray() : Kennzeichenkatalog.Of(Gruppe);

    /// <summary>Above the list, so a crew always knows whether it reads a group or a search.</summary>
    public string ListTitle => IsSearching
        ? "TREFFER IN ALLEN GRUPPEN"
        : Gruppe switch
        {
            KennzeichenGruppe.Gefahrzettel => "GEFAHRZETTEL (ADR, BEFÖRDERUNG)",
            KennzeichenGruppe.Ghs => "GHS-PIKTOGRAMME (CLP, VERPACKUNG UND ARBEITSPLATZ)",
            KennzeichenGruppe.Rohrleitung => "ROHRLEITUNGEN NACH DEM DURCHFLUSSSTOFF",
            KennzeichenGruppe.Gasflasche => "GASFLASCHEN: FARBE DER SCHULTER (EN 1089-3)",
            _ => "WARNTAFELN UND KENNZEICHEN AM FAHRZEUG",
        };

    /// <summary>False only for a search that finds nothing, such as a Gefahrnummer: no heading over an empty list.</summary>
    public bool HasEntries => Entries.Count > 0;

    /// <summary>A search that is neither a number nor found anywhere says so instead of an empty page.</summary>
    public bool HasNoHits => IsSearching && Gefahrnummer is null && Entries.Count == 0 && !HasStoffe;

    /// <summary>The query read as the upper number of a Warntafel, or <see langword="null"/> when it is none.</summary>
    public Gefahrnummer? Gefahrnummer => Gefahrnummern.TryRead(Query, out var number) ? number : null;

    public bool HasGefahrnummer => Gefahrnummer is not null;

    /// <summary>The ADR's entry, or the warning that the number is not listed.</summary>
    public string GefahrnummerMeaning => Gefahrnummer switch
    {
        null => string.Empty,
        { Meaning: { } meaning } => meaning,
        _ => Gefahrnummern.UnlistedNote,
    };

    /// <summary>The digit lines, then what X and a special combination mean, in that order.</summary>
    public IReadOnlyList<string> GefahrnummerNotes
    {
        get
        {
            if (Gefahrnummer is not { } number)
            {
                return [];
            }

            var notes = number.Digits.Select(d => d.Digit + "  " + d.Meaning).ToList();
            if (number.IsSpecialCombination)
            {
                notes.Add(Gefahrnummern.SpecialNote);
            }

            if (number.ReactsWithWater)
            {
                notes.Add(Gefahrnummern.WaterNote);
            }

            return notes;
        }
    }

    /// <summary>The first <see cref="MaxStoffe"/> rows of Tabelle A the search finds.</summary>
    public IReadOnlyList<Stoff> Stoffe => StoffHits.Count > MaxStoffe ? StoffHits.Take(MaxStoffe).ToArray() : StoffHits;

    public bool HasStoffe => StoffHits.Count > 0;

    /// <summary>Above the Stoffe: one UN-Nummer, its rows, or how many a word found.</summary>
    public string StoffeTitle => StoffHits switch
    {
        [] => string.Empty,
        [var only] => "UN " + only.UnNummer,
        _ when StoffHits.All(s => string.Equals(s.UnNummer, StoffHits[0].UnNummer, StringComparison.Ordinal)) =>
            "UN " + StoffHits[0].UnNummer + ": " + Count(StoffHits.Count) + " EINTRAGUNGEN",
        _ => Count(StoffHits.Count) + " STOFFE IN TABELLE A",
    };

    public bool HasMoreStoffe => StoffHits.Count > MaxStoffe;

    public string MoreStoffeNote => HasMoreStoffe
        ? "Nur die ersten " + Count(MaxStoffe) + " Stoffe. Genauer suchen, um die übrigen zu sehen."
        : string.Empty;

    private IReadOnlyList<Stoff> StoffHits => _stoffHits ??= TabelleA.Search(Query);

    // The Stoffe are worked out once per query, before the properties that read them are told.
    partial void OnQueryChanging(string? value) => _stoffHits = null;

    partial void OnGruppeChanged(KennzeichenGruppe value) => _settings.Set(GruppeKey, value.ToString());

    [RelayCommand]
    private void Clear() => Query = string.Empty;

    private static string Count(int value) => Formatting.Number(value);

    private bool Matches(Kennzeichen entry)
    {
        var query = Query?.Trim() ?? string.Empty;
        const CompareOptions options = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;
        return German.IndexOf(entry.Code, query, options) >= 0
            || German.IndexOf(entry.Name, query, options) >= 0
            || German.IndexOf(entry.Look, query, options) >= 0
            || German.IndexOf(entry.Meaning, query, options) >= 0;
    }

    private void Choose(bool isChosen, KennzeichenGruppe gruppe)
    {
        if (isChosen)
        {
            Gruppe = gruppe;
        }
    }
}
