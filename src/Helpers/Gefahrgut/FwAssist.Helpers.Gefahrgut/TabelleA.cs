using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FwAssist.Helpers.Gefahrgut;

/// <summary>
/// ADR Tabelle A (3.2.1) by UN-Nummer, as bundled in TabelleA.tsv. Source: the official German text
/// promulgated in the Bundesgesetzblatt, the Bekanntmachung of 16 November 2021 (BGBl. 2021 II S. 1184,
/// Anlageband) as amended by the 29. ADR-Änderungsverordnung (BGBl. 2022 II Nr. 20, Anlageband) and the
/// 30. ADR-Änderungsverordnung (BGBl. 2025 II Nr. 57). An amtliches Werk under § 5 Abs. 1 UrhG.
/// scripts/gefahrgut/extract_tabelle_a.py writes the file; the ADR changes every two years, and the
/// script's TSV files take the next amendments.
/// </summary>
public static partial class TabelleA
{
    public const string Source = "ADR 2025, Tabelle A (3.2.1)";

    public const string SourceNote =
        "Amtliche deutsche Übersetzung, Bekanntmachung vom 16. November 2021 (BGBl. 2021 II S. 1184), " +
        "geändert durch die 29. und die 30. ADR-Änderungsverordnung (BGBl. 2022 II Nr. 20, BGBl. 2025 II Nr. 57).";

    private const string ResourceName = "FwAssist.Helpers.Gefahrgut.TabelleA.tsv";

    // "atzend" finds "ÄTZEND" and "benzin" finds "BENZIN": gloves type neither umlauts nor capitals
    // reliably. The culture is fixed so that the phone's setting changes nothing.
    private static readonly CompareInfo German = CultureInfo.GetCultureInfo("de-DE").CompareInfo;

    private static readonly Lazy<Catalogue> Data = new(Load);

    /// <summary>Every row, in the order of Tabelle A.</summary>
    public static IReadOnlyList<Stoff> All => Data.Value.All;

    /// <summary>
    /// The query read as a UN-Nummer or the start of one: "1203", "UN 1203", "un1203" and "12" all
    /// count. <see langword="null"/> for anything else, which is then a search by name.
    /// </summary>
    public static string? ReadNumber(string? query)
    {
        var match = NumberPattern().Match(query?.Trim() ?? string.Empty);
        return match.Success ? match.Groups["digits"].Value : null;
    }

    /// <summary>The rows of one UN-Nummer, written as on the Warntafel or with "UN" in front.</summary>
    public static bool TryFind(string? unNummer, [MaybeNullWhen(false)] out IReadOnlyList<Stoff> stoffe)
    {
        if (ReadNumber(unNummer) is { Length: 4 } number && Data.Value.ByNumber.TryGetValue(number, out var rows))
        {
            stoffe = rows;
            return true;
        }

        stoffe = null;
        return false;
    }

    /// <summary>
    /// Every row the query finds. A number finds the UN-Nummern it starts, in their order. A word
    /// finds the Benennungen that contain it, those that begin with it first.
    /// </summary>
    public static IReadOnlyList<Stoff> Search(string? query)
    {
        var text = query?.Trim() ?? string.Empty;
        if (text.Length == 0)
        {
            return [];
        }

        if (ReadNumber(text) is { } number)
        {
            return Data.Value.All.Where(s => s.UnNummer.StartsWith(number, StringComparison.Ordinal)).ToArray();
        }

        const CompareOptions options = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;
        return Data.Value.All
            .Select(s => (Stoff: s, At: German.IndexOf(s.Benennung, text, options)))
            .Where(hit => hit.At >= 0)
            .OrderBy(hit => hit.At == 0 ? 0 : 1)
            .Select(hit => hit.Stoff)
            .ToArray();
    }

    [GeneratedRegex(@"^(?:UN\s*)?(?<digits>\d{1,4})$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex NumberPattern();

    private static Catalogue Load()
    {
        using var stream = typeof(TabelleA).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException(ResourceName + " is not embedded.");
        using var reader = new StreamReader(stream);
        return Parse(reader);
    }

    /// <summary>Reads the TSV the extraction script writes. Lines starting with # are comments.</summary>
    private static Catalogue Parse(TextReader reader)
    {
        var rows = new List<Stoff>();
        while (reader.ReadLine() is { } line)
        {
            if (line.Length == 0 || line[0] == '#')
            {
                continue;
            }

            var cells = line.Split('\t');
            if (cells.Length != 6)
            {
                throw new InvalidDataException("TabelleA.tsv: expected 6 columns, got " + cells.Length.ToString(CultureInfo.InvariantCulture) + ": " + line);
            }

            var status = cells[5] switch
            {
                "" => StoffStatus.Gefahrgut,
                "verboten" => StoffStatus.BefoerderungVerboten,
                "frei" => StoffStatus.NichtAdr,
                _ => throw new InvalidDataException("TabelleA.tsv: unknown status: " + line),
            };
            var gefahrnummer = cells[4].Length == 0 ? null : cells[4];
            rows.Add(new Stoff(cells[0], cells[1], cells[2], cells[3], gefahrnummer, status));
        }

        var byNumber = rows
            .GroupBy(s => s.UnNummer, StringComparer.Ordinal)
            .ToFrozenDictionary<IGrouping<string, Stoff>, string, IReadOnlyList<Stoff>>(g => g.Key, g => g.ToArray(), StringComparer.Ordinal);
        return new Catalogue(rows.ToArray(), byNumber);
    }

    private sealed record Catalogue(IReadOnlyList<Stoff> All, FrozenDictionary<string, IReadOnlyList<Stoff>> ByNumber);
}
