namespace FwAssist.Helpers.Kennzeichnungen;

/// <summary>
/// One row of ADR Tabelle A (3.2.1), reduced to what a crew reads off a vehicle: UN-Nummer (column 1),
/// Benennung und Beschreibung (2), Klasse (3a), Gefahrzettel (5) and the Nummer zur Kennzeichnung der
/// Gefahr (20). A UN-Nummer can have several rows, for instance one per Flammpunkt range.
/// </summary>
/// <param name="UnNummer">The four digits of column 1, with leading zeros: "0004".</param>
/// <param name="Benennung">Column 2 as the ADR prints it, capitals for the official name included.</param>
/// <param name="Klasse">Column 3a, e.g. "6.1".</param>
/// <param name="Gefahrzettel">The label numbers as Tabelle A writes them, e.g. "6.1+8"; empty when there is none.</param>
/// <param name="Gefahrnummer">The upper number of the Warntafel, or <see langword="null"/> where column 20 is empty.</param>
/// <param name="Status">Whether the row is carried under the ADR at all.</param>
public sealed record Stoff(
    string UnNummer,
    string Benennung,
    string Klasse,
    string Gefahrzettel,
    string? Gefahrnummer,
    StoffStatus Status)
{
    /// <summary>The line under the Benennung: Klasse and Gefahrzettel, or why the row has none.</summary>
    public string Merkmale => Status switch
    {
        StoffStatus.BefoerderungVerboten => "Klasse " + Klasse + " · Beförderung verboten",
        StoffStatus.NichtAdr => "Klasse " + Klasse + " · unterliegt nicht dem ADR",
        _ when Gefahrzettel.Length == 0 => "Klasse " + Klasse + " · kein Gefahrzettel",
        _ => "Klasse " + Klasse + " · Gefahrzettel " + Gefahrzettel.Replace("+", " + ", StringComparison.Ordinal),
    };

    /// <summary>The upper half of the plate drawn beside the row: blank where the ADR gives no number.</summary>
    public string Tafel => Gefahrnummer ?? string.Empty;
}
