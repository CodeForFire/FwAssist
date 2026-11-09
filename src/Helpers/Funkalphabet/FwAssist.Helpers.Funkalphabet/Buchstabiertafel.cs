using System.Globalization;
using System.Text;

namespace FwAssist.Helpers.Funkalphabet;

/// <summary>
/// A spelling table and how to spell a text with it. Three exist: <see cref="Alt"/>, the names of
/// DIN 5009:1983 that radio traffic grew up with and that the screen calls BOS, <see cref="Din2022"/>,
/// the cities of the current standard, and <see cref="Nato"/>. DIN wrote the new one for business
/// and administration and does not bind emergency services to it, so which one applies is the
/// user's organisation's call, not this app's.
/// </summary>
public sealed class Buchstabiertafel
{
    // Spoken one at a time in every table, and "zwo" so that it is not misheard as "drei". These
    // are radio procedure, not DIN's letters, so they stay out of Entries. Declared before the
    // tables: their initializers read this.
    private static readonly CodeWord[] Digits =
    [
        new("0", "null"),
        new("1", "eins"),
        new("2", "zwo"),
        new("3", "drei"),
        new("4", "vier"),
        new("5", "fünf"),
        new("6", "sechs"),
        new("7", "sieben"),
        new("8", "acht"),
        new("9", "neun"),
    ];

    // Sch before S and Ch before C: the longest match wins.
    private readonly List<CodeWord> _longestFirst;

    private Buchstabiertafel(IReadOnlyList<CodeWord> entries)
    {
        Entries = entries;
        _longestFirst = entries.Concat(Digits).OrderByDescending(e => e.Symbol.Length).ToList();
    }

    public static Buchstabiertafel Alt { get; } = new(
    [
        new("A", "Anton"),
        new("Ä", "Ärger"),
        new("B", "Berta"),
        new("C", "Cäsar"),
        new("CH", "Charlotte"),
        new("D", "Dora"),
        new("E", "Emil"),
        new("F", "Friedrich"),
        new("G", "Gustav"),
        new("H", "Heinrich"),
        new("I", "Ida"),
        new("J", "Julius"),
        new("K", "Kaufmann"),
        new("L", "Ludwig"),
        new("M", "Martha"),
        new("N", "Nordpol"),
        new("O", "Otto"),
        new("Ö", "Ökonom"),
        new("P", "Paula"),
        new("Q", "Quelle"),
        new("R", "Richard"),
        new("S", "Samuel"),
        new("SCH", "Schule"),
        new("ß", "Eszett"),
        new("T", "Theodor"),
        new("U", "Ulrich"),
        new("Ü", "Übermut"),
        new("V", "Viktor"),
        new("W", "Wilhelm"),
        new("X", "Xanthippe"),
        new("Y", "Ypsilon"),
        new("Z", "Zacharias"),
    ]);

    public static Buchstabiertafel Din2022 { get; } = new(
    [
        new("A", "Aachen"),
        new("Ä", "Umlaut Aachen"),
        new("B", "Berlin"),
        new("C", "Chemnitz"),
        new("D", "Düsseldorf"),
        new("E", "Essen"),
        new("F", "Frankfurt"),
        new("G", "Goslar"),
        new("H", "Hamburg"),
        new("I", "Ingelheim"),
        new("J", "Jena"),
        new("K", "Köln"),
        new("L", "Leipzig"),
        new("M", "München"),
        new("N", "Nürnberg"),
        new("O", "Offenbach"),
        new("Ö", "Umlaut Offenbach"),
        new("P", "Potsdam"),
        new("Q", "Quickborn"),
        new("R", "Rostock"),
        new("S", "Salzwedel"),
        new("ß", "Eszett"),
        new("T", "Tübingen"),
        new("U", "Unna"),
        new("Ü", "Umlaut Unna"),
        new("V", "Völklingen"),
        new("W", "Wuppertal"),
        new("X", "Xanten"),
        new("Y", "Ypsilon"),
        new("Z", "Zwickau"),
    ]);

    // NATO has no word for the German letters, so they go out as their usual two-letter spelling:
    // the far end writes down AE, OE, UE or SS.
    public static Buchstabiertafel Nato { get; } = new(
    [
        new("A", "Alfa"),
        new("Ä", "Alfa Echo"),
        new("B", "Bravo"),
        new("C", "Charlie"),
        new("D", "Delta"),
        new("E", "Echo"),
        new("F", "Foxtrot"),
        new("G", "Golf"),
        new("H", "Hotel"),
        new("I", "India"),
        new("J", "Juliett"),
        new("K", "Kilo"),
        new("L", "Lima"),
        new("M", "Mike"),
        new("N", "November"),
        new("O", "Oscar"),
        new("Ö", "Oscar Echo"),
        new("P", "Papa"),
        new("Q", "Quebec"),
        new("R", "Romeo"),
        new("S", "Sierra"),
        new("ß", "Sierra Sierra"),
        new("T", "Tango"),
        new("U", "Uniform"),
        new("Ü", "Uniform Echo"),
        new("V", "Victor"),
        new("W", "Whiskey"),
        new("X", "X-ray"),
        new("Y", "Yankee"),
        new("Z", "Zulu"),
    ]);

    /// <summary>The table in reading order.</summary>
    public IReadOnlyList<CodeWord> Entries { get; }

    /// <summary>
    /// The text as it is said: one code word per letter, one word per digit, a sign as itself, and
    /// one <see cref="CodeWord.Gap"/> between words.
    /// </summary>
    public IReadOnlyList<CodeWord> Spell(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var key = ToKey(text);
        var spelled = new List<CodeWord>();
        var index = 0;
        while (index < key.Length)
        {
            if (char.IsWhiteSpace(key[index]))
            {
                if (spelled.Count > 0 && spelled[^1] != CodeWord.Gap)
                {
                    spelled.Add(CodeWord.Gap);
                }

                index++;
                continue;
            }

            var word = Match(key, index);
            spelled.Add(word);
            index += word.Symbol.Length;
        }

        if (spelled.Count > 0 && spelled[^1] == CodeWord.Gap)
        {
            spelled.RemoveAt(spelled.Count - 1);
        }

        return spelled;
    }

    // The round through UTF-8 swaps a lone surrogate, which a TextBox's MaxLength can leave behind
    // by cutting an emoji in half, for U+FFFD: Normalize throws on one. FormC folds an umlaut typed
    // as a letter plus a combining mark into the one character the table lists. Upper-casing leaves
    // ß alone, so a capital ẞ is folded onto it by hand. This is a lookup key, not display text.
    private static string ToKey(string text)
    {
        var folded = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(text))
            .Normalize(NormalizationForm.FormC)
            .ToUpperInvariant()
            .Replace('ẞ', 'ß');
        return string.Concat(folded.EnumerateRunes().Where(IsVisible).Select(r => r.ToString()));
    }

    // A joiner, soft hyphen or variation selector would be a row that looks empty. A combining mark
    // still here after FormC has no letter to fold into. Variation selectors are marks, not Format.
    private static bool IsVisible(Rune rune) =>
        Rune.GetUnicodeCategory(rune) is not (UnicodeCategory.Format or UnicodeCategory.NonSpacingMark);

    private CodeWord Match(string key, int index)
    {
        foreach (var entry in _longestFirst)
        {
            if (string.CompareOrdinal(key, index, entry.Symbol, 0, entry.Symbol.Length) == 0)
            {
                return entry;
            }
        }

        // A sign stands for itself, and a character outside the BMP stays in one piece.
        Rune.DecodeFromUtf16(key.AsSpan(index), out var rune, out _);
        var symbol = rune.ToString();
        return new CodeWord(symbol, symbol);
    }
}
