using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace FwAssist.Helpers.Kennzeichnungen;

/// <summary>
/// The upper number on an orange Warntafel, the Nummer zur Kennzeichnung der Gefahr. Source: ADR
/// 2025, Unterabschnitt 5.3.2.3: the German text of ADR 2021, checked against the 2023 edition
/// (formatting changes only) and the 2025 amendments (30. ADR-Änderungsverordnung, BGBl. 2025 II
/// Nr. 57, which strikes "78 radioaktiver Stoff, ätzend"). The draft for ADR 2027 leaves this
/// subsection alone. The digit meanings are 5.3.2.3.1 and the listed
/// meanings are the table of 5.3.2.3.2, quoted with its footnote 3) written out once as
/// <see cref="WaterNote"/> instead of after every X entry. The ADR's German text is shared with
/// Switzerland and spells "einschliesslich"; the screen spells it the German way.
/// </summary>
public static class Gefahrnummern
{
    public const string Source = "ADR 2025, Unterabschnitt 5.3.2.3";

    /// <summary>What an X in front means, ADR 5.3.2.3.1 and footnote 3) of 5.3.2.3.2.</summary>
    public const string WaterNote =
        "X: reagiert gefährlich mit Wasser. Wasser nur im Einverständnis mit Sachverständigen verwenden.";

    /// <summary>ADR 5.3.2.3.1: these combinations mean what 5.3.2.3.2 says, not what their digits say.</summary>
    public const string SpecialNote = "Diese Ziffernfolge hat eine eigene Bedeutung. Die Ziffern gelten hier nicht einzeln.";

    /// <summary>A number the ADR does not list: the digits are read anyway, but the plate may be misread.</summary>
    public const string UnlistedNote = "Nicht in der Liste des ADR. Nummer auf der Tafel noch einmal prüfen.";

    // ADR 5.3.2.3.1, "Die Ziffern weisen im Allgemeinen auf folgende Gefahren hin".
    private static readonly FrozenDictionary<char, string> DigitMeanings = new Dictionary<char, string>
    {
        ['2'] = "Entweichen von Gas durch Druck oder durch chemische Reaktion",
        ['3'] = "Entzündbarkeit von flüssigen Stoffen (Dämpfen) und Gasen oder selbsterhitzungsfähiger flüssiger Stoff",
        ['4'] = "Entzündbarkeit von festen Stoffen oder selbsterhitzungsfähiger fester Stoff",
        ['5'] = "Oxidierende (brandfördernde) Wirkung",
        ['6'] = "Giftigkeit oder Ansteckungsgefahr",
        ['7'] = "Radioaktivität",
        ['8'] = "Ätzwirkung",
        ['9'] = "Gefahr einer spontanen heftigen Reaktion",
    }.ToFrozenDictionary();

    // ADR 5.3.2.3.1, "Folgende Ziffernkombinationen haben jedoch eine besondere Bedeutung".
    private static readonly FrozenSet<string> SpecialCombinations = new[]
    {
        "22", "323", "333", "362", "382", "423", "44", "446", "462", "482", "539", "606", "623", "642", "823", "842", "90", "99",
    }.ToFrozenSet(StringComparer.Ordinal);

    // ADR 5.3.2.3.2, in its order.
    private static readonly (string Number, string Meaning)[] Table =
    [
        ("20", "erstickendes Gas oder Gas, das keine Nebengefahr aufweist"),
        ("22", "tiefgekühlt verflüssigtes Gas, erstickend"),
        ("223", "tiefgekühlt verflüssigtes Gas, entzündbar"),
        ("225", "tiefgekühlt verflüssigtes Gas, oxidierend (brandfördernd)"),
        ("23", "entzündbares Gas"),
        ("238", "entzündbares Gas, ätzend"),
        ("239", "entzündbares Gas, das spontan zu einer heftigen Reaktion führen kann"),
        ("25", "oxidierendes (brandförderndes) Gas"),
        ("26", "giftiges Gas"),
        ("263", "giftiges Gas, entzündbar"),
        ("265", "giftiges Gas, oxidierend (brandfördernd)"),
        ("268", "giftiges Gas, ätzend"),
        ("28", "ätzendes Gas"),
        ("30", "entzündbarer flüssiger Stoff (Flammpunkt von 23 °C bis einschließlich 60 °C) oder entzündbarer flüssiger Stoff oder fester Stoff in geschmolzenem Zustand mit einem Flammpunkt über 60 °C, auf oder über seinen Flammpunkt erwärmt, oder selbsterhitzungsfähiger flüssiger Stoff"),
        ("323", "entzündbarer flüssiger Stoff, der mit Wasser reagiert und entzündbare Gase bildet"),
        ("X323", "entzündbarer flüssiger Stoff, der mit Wasser gefährlich reagiert und entzündbare Gase bildet"),
        ("33", "leicht entzündbarer flüssiger Stoff (Flammpunkt unter 23 °C)"),
        ("333", "pyrophorer flüssiger Stoff"),
        ("X333", "pyrophorer flüssiger Stoff, der mit Wasser gefährlich reagiert"),
        ("336", "leicht entzündbarer flüssiger Stoff, giftig"),
        ("338", "leicht entzündbarer flüssiger Stoff, ätzend"),
        ("X338", "leicht entzündbarer flüssiger Stoff, ätzend, der mit Wasser gefährlich reagiert"),
        ("339", "leicht entzündbarer flüssiger Stoff, der spontan zu einer heftigen Reaktion führen kann"),
        ("36", "entzündbarer flüssiger Stoff (Flammpunkt von 23 °C bis einschließlich 60 °C), schwach giftig, oder selbsterhitzungsfähiger flüssiger Stoff, giftig"),
        ("362", "entzündbarer flüssiger Stoff, giftig, der mit Wasser reagiert und entzündbare Gase bildet"),
        ("X362", "entzündbarer flüssiger Stoff, giftig, der mit Wasser gefährlich reagiert und entzündbare Gase bildet"),
        ("368", "entzündbarer flüssiger Stoff, giftig, ätzend"),
        ("38", "entzündbarer flüssiger Stoff (Flammpunkt von 23 °C bis einschließlich 60 °C), schwach ätzend, oder selbsterhitzungsfähiger flüssiger Stoff, ätzend"),
        ("382", "entzündbarer flüssiger Stoff, ätzend, der mit Wasser reagiert und entzündbare Gase bildet"),
        ("X382", "entzündbarer flüssiger Stoff, ätzend, der mit Wasser gefährlich reagiert und entzündbare Gase bildet"),
        ("39", "entzündbarer flüssiger Stoff, der spontan zu einer heftigen Reaktion führen kann"),
        ("40", "entzündbarer fester Stoff oder selbsterhitzungsfähiger Stoff oder selbstzersetzlicher Stoff oder polymerisierender Stoff"),
        ("423", "fester Stoff, der mit Wasser reagiert und entzündbare Gase bildet, oder entzündbarer fester Stoff, der mit Wasser reagiert und entzündbare Gase bildet, oder selbsterhitzungsfähiger fester Stoff, der mit Wasser reagiert und entzündbare Gase bildet"),
        ("X423", "fester Stoff, der mit Wasser gefährlich reagiert und entzündbare Gase bildet, oder entzündbarer fester Stoff, der mit Wasser gefährlich reagiert und entzündbare Gase bildet, oder selbsterhitzungsfähiger fester Stoff, der mit Wasser gefährlich reagiert und entzündbare Gase bildet"),
        ("43", "selbstentzündlicher (pyrophorer) fester Stoff"),
        ("X432", "selbstentzündlicher (pyrophorer) fester Stoff, der mit Wasser gefährlich reagiert und entzündbare Gase bildet"),
        ("44", "entzündbarer fester Stoff, der sich bei erhöhter Temperatur in geschmolzenem Zustand befindet"),
        ("446", "entzündbarer fester Stoff, giftig, der sich bei erhöhter Temperatur in geschmolzenem Zustand befindet"),
        ("46", "entzündbarer oder selbsterhitzungsfähiger fester Stoff, giftig"),
        ("462", "fester Stoff, giftig, der mit Wasser reagiert und entzündbare Gase bildet"),
        ("X462", "fester Stoff, der mit Wasser gefährlich reagiert und giftige Gase bildet"),
        ("48", "entzündbarer oder selbsterhitzungsfähiger fester Stoff, ätzend"),
        ("482", "fester Stoff, ätzend, der mit Wasser reagiert und entzündbare Gase bildet"),
        ("X482", "fester Stoff, der mit Wasser gefährlich reagiert und ätzende Gase bildet"),
        ("50", "oxidierender (brandfördernder) Stoff"),
        ("539", "entzündbares organisches Peroxid"),
        ("55", "stark oxidierender (brandfördernder) Stoff"),
        ("556", "stark oxidierender (brandfördernder) Stoff, giftig"),
        ("558", "stark oxidierender (brandfördernder) Stoff, ätzend"),
        ("559", "stark oxidierender (brandfördernder) Stoff, der spontan zu einer heftigen Reaktion führen kann"),
        ("56", "oxidierender (brandfördernder) Stoff, giftig"),
        ("568", "oxidierender (brandfördernder) Stoff, giftig, ätzend"),
        ("58", "oxidierender (brandfördernder) Stoff, ätzend"),
        ("59", "oxidierender (brandfördernder) Stoff, der spontan zu einer heftigen Reaktion führen kann"),
        ("60", "giftiger oder schwach giftiger Stoff"),
        ("606", "ansteckungsgefährlicher Stoff"),
        ("623", "giftiger flüssiger Stoff, der mit Wasser reagiert und entzündbare Gase bildet"),
        ("63", "giftiger Stoff, entzündbar (Flammpunkt von 23 °C bis einschließlich 60 °C)"),
        ("638", "giftiger Stoff, entzündbar (Flammpunkt von 23 °C bis einschließlich 60 °C), ätzend"),
        ("639", "giftiger Stoff, entzündbar (Flammpunkt nicht über 60 °C), der spontan zu einer heftigen Reaktion führen kann"),
        ("64", "giftiger fester Stoff, entzündbar oder selbsterhitzungsfähig"),
        ("642", "giftiger fester Stoff, der mit Wasser reagiert und entzündbare Gase bildet"),
        ("65", "giftiger Stoff, oxidierend (brandfördernd)"),
        ("66", "sehr giftiger Stoff"),
        ("663", "sehr giftiger Stoff, entzündbar (Flammpunkt nicht über 60 °C)"),
        ("664", "sehr giftiger fester Stoff, entzündbar oder selbsterhitzungsfähig"),
        ("665", "sehr giftiger Stoff, oxidierend (brandfördernd)"),
        ("668", "sehr giftiger Stoff, ätzend"),
        ("X668", "sehr giftiger Stoff, ätzend, der mit Wasser gefährlich reagiert"),
        ("669", "sehr giftiger Stoff, der spontan zu einer heftigen Reaktion führen kann"),
        ("68", "giftiger Stoff, ätzend"),
        ("69", "giftiger oder schwach giftiger Stoff, der spontan zu einer heftigen Reaktion führen kann"),
        ("70", "radioaktiver Stoff"),
        ("768", "radioaktiver Stoff, giftig, ätzend"),
        ("80", "ätzender oder schwach ätzender Stoff"),
        ("X80", "ätzender oder schwach ätzender Stoff, der mit Wasser gefährlich reagiert"),
        ("823", "ätzender flüssiger Stoff, der mit Wasser reagiert und entzündbare Gase bildet"),
        ("83", "ätzender oder schwach ätzender Stoff, entzündbar (Flammpunkt von 23 °C bis einschließlich 60 °C)"),
        ("X83", "ätzender oder schwach ätzender Stoff, entzündbar (Flammpunkt von 23 °C bis einschließlich 60 °C), der mit Wasser gefährlich reagiert"),
        ("836", "ätzender oder schwach ätzender Stoff, entzündbar (Flammpunkt von 23 °C bis einschließlich 60 °C) und giftig"),
        ("839", "ätzender oder schwach ätzender Stoff, entzündbar (Flammpunkt von 23 °C bis einschließlich 60 °C), der spontan zu einer heftigen Reaktion führen kann"),
        ("X839", "ätzender oder schwach ätzender Stoff, entzündbar (Flammpunkt von 23 °C bis einschließlich 60 °C), der spontan zu einer heftigen Reaktion führen kann und der mit Wasser gefährlich reagiert"),
        ("84", "ätzender fester Stoff, entzündbar oder selbsterhitzungsfähig"),
        ("842", "ätzender fester Stoff, der mit Wasser reagiert und entzündbare Gase bildet"),
        ("85", "ätzender oder schwach ätzender Stoff, oxidierend (brandfördernd)"),
        ("856", "ätzender oder schwach ätzender Stoff, oxidierend (brandfördernd) und giftig"),
        ("86", "ätzender oder schwach ätzender Stoff, giftig"),
        ("88", "stark ätzender Stoff"),
        ("X88", "stark ätzender Stoff, der mit Wasser gefährlich reagiert"),
        ("883", "stark ätzender Stoff, entzündbar (Flammpunkt von 23 °C bis einschließlich 60 °C)"),
        ("884", "stark ätzender fester Stoff, entzündbar oder selbsterhitzungsfähig"),
        ("885", "stark ätzender Stoff, oxidierend (brandfördernd)"),
        ("886", "stark ätzender Stoff, giftig"),
        ("X886", "stark ätzender Stoff, giftig, der mit Wasser gefährlich reagiert"),
        ("89", "ätzender oder schwach ätzender Stoff, der spontan zu einer heftigen Reaktion führen kann"),
        ("90", "umweltgefährdender Stoff; verschiedene gefährliche Stoffe"),
        ("99", "verschiedene gefährliche erwärmte Stoffe"),
    ];

    private static readonly FrozenDictionary<string, string> Listed =
        Table.ToFrozenDictionary(e => e.Number, e => e.Meaning, StringComparer.Ordinal);

    /// <summary>Every number of ADR 5.3.2.3.2, in its order.</summary>
    public static IReadOnlyList<string> ListedNumbers { get; } = Table.Select(e => e.Number).ToArray();

    /// <summary>
    /// Reads a typed number: two or three digits, the first 2 to 9, optionally with an X in front.
    /// Spaces are ignored and a lower-case x counts. Class 1 has no such number (5.3.2.3.1), and a
    /// 1 appears in none, so a 1 anywhere is not a number this reads.
    /// </summary>
    public static bool TryRead(string? text, [NotNullWhen(true)] out Gefahrnummer? number)
    {
        number = null;
        if (text is null)
        {
            return false;
        }

        var key = string.Concat(text.Where(c => !char.IsWhiteSpace(c))).ToUpperInvariant();
        var reactsWithWater = key.StartsWith('X');
        var digits = reactsWithWater ? key[1..] : key;
        if (digits.Length is < 2 or > 3 || !digits.All(c => c == '0' || DigitMeanings.ContainsKey(c)) || digits[0] == '0')
        {
            return false;
        }

        var meaning = Listed.GetValueOrDefault(key);
        var isSpecial = SpecialCombinations.Contains(digits);
        number = new Gefahrnummer(
            key,
            meaning,
            reactsWithWater,
            isSpecial,
            isSpecial ? [] : ReadDigits(digits));
        return true;
    }

    // ADR 5.3.2.3.1: a doubled digit means the hazard is greater, and a 0 says one digit is enough.
    private static Ziffer[] ReadDigits(string digits)
    {
        var read = new List<Ziffer>(digits.Length);
        for (var i = 0; i < digits.Length; i++)
        {
            var digit = digits[i];
            if (digit == '0')
            {
                read.Add(new Ziffer(digit, "keine weitere Gefahr"));
            }
            else if (i > 0 && digits[i - 1] == digit)
            {
                read.Add(new Ziffer(digit, "verstärkt: " + DigitMeanings[digit]));
            }
            else
            {
                read.Add(new Ziffer(digit, DigitMeanings[digit]));
            }
        }

        return [.. read];
    }
}
