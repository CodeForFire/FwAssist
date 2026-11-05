using System.Text;

namespace Helfer.Helpers.Funkalphabet;

/// <summary>
/// A German spelling table and how to spell a text with it. Two exist: <see cref="Alt"/>, the names
/// of DIN 5009:1983 that radio traffic grew up with, and <see cref="Din2022"/>, the cities of the
/// current standard. DIN wrote the new one for business and administration and does not bind
/// emergency services to it, so which one applies is the user's organisation's call, not this app's.
/// </summary>
public sealed class Buchstabiertafel
{
    // Sch before S and Ch before C: the longest match wins.
    private readonly List<CodeWord> _longestFirst;

    private Buchstabiertafel(IReadOnlyList<CodeWord> entries)
    {
        Entries = entries;
        _longestFirst = entries.OrderByDescending(e => e.Symbol.Length).ToList();
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

    /// <summary>The table in reading order.</summary>
    public IReadOnlyList<CodeWord> Entries { get; }

    /// <summary>
    /// The text as it is said: one code word per letter, a digit or sign as itself, and one
    /// <see cref="CodeWord.Gap"/> between words.
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
    private static string ToKey(string text) =>
        Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(text))
            .Normalize(NormalizationForm.FormC)
            .ToUpperInvariant()
            .Replace('ẞ', 'ß');

    private CodeWord Match(string key, int index)
    {
        foreach (var entry in _longestFirst)
        {
            if (string.CompareOrdinal(key, index, entry.Symbol, 0, entry.Symbol.Length) == 0)
            {
                return entry;
            }
        }

        // Anything else stands for itself, and a character outside the BMP stays in one piece.
        Rune.DecodeFromUtf16(key.AsSpan(index), out var rune, out _);
        var symbol = rune.ToString();
        return new CodeWord(symbol, symbol);
    }
}
