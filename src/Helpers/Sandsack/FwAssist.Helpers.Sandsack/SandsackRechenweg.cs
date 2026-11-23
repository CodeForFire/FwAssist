using FwAssist.Core;

namespace FwAssist.Helpers.Sandsack;

/// <summary>
/// How the Sandsack helper gets to its Richtwerte, one German line per step, for the screen (#87).
/// Each line shows the rule, and once the values it needs are entered, it shows the user's own
/// numbers worked through. Every figure comes from <see cref="Sandsackvorgaben"/>, and every
/// result comes from <see cref="SandsackRechner"/>. The text cannot drift from the arithmetic.
/// </summary>
public static class SandsackRechenweg
{
    private static readonly string Stufen = Liste(Sandsackvorgaben.Dammstufen.Select(s => s.HoeheMeter));

    private static readonly string SaeckeJeMeter = Liste(Sandsackvorgaben.Dammstufen.Select(s => s.SaeckeJeMeter));

    private static readonly string SandJeMeter = Liste(Sandsackvorgaben.Dammstufen.Select(s => s.SandKubikmeterJeMeter));

    private static readonly string Nass = Formatting.Number(Sandsackvorgaben.SandTonnenJeKubikmeter, 1);

    private static readonly string TrockenMin = Formatting.Number(Sandsackvorgaben.SandTrockenMinTonnenJeKubikmeter, 1);

    private static readonly string TrockenMax = Formatting.Number(Sandsackvorgaben.SandTrockenMaxTonnenJeKubikmeter, 1);

    private static readonly string Arbeitsgaenge = Formatting.Number(Sandsackvorgaben.ArbeitsgaengeJeSack);

    private static readonly string Leistung = Formatting.Number(Sandsackvorgaben.SaeckeJeHelferUndStunde, 0);

    private static readonly string Basisfaktor = Formatting.Number(Sandsackvorgaben.BasisbreiteJeHoehe, 0);

    private static readonly string WarumNass =
        $"Nasser Sand wiegt bis zu {Formatting.Number(SandsackRechner.NassMehrgewichtProzent, 0)} % mehr als trockener. "
        + "Gerechnet wird mit nassem Sand, damit Ladung und Fahrten reichen.";

    /// <summary>Why every result errs towards too much.</summary>
    public const string SichereSeite =
        "Zur sicheren Seite gerechnet: Säcke „quer verlegt“, der obere Sandwert "
        + "und die langsamste Arbeitsleistung der Taschenkarte.";

    /// <summary>
    /// The steps, in the order the results are shown. A value that is <see langword="null"/> has
    /// not been entered, or is not valid, and its steps show the rule alone.
    /// </summary>
    public static IReadOnlyList<string> Zeilen(decimal? laengeMeter, decimal? hoeheMeter, decimal? helfer)
    {
        if (hoeheMeter is not { } hoehe)
        {
            return
            [
                $"Höhe: aufgerundet auf die nächste Stufe der Tabelle, {Stufen} m.",
                $"Sandsäcke: Länge × Säcke je m der Stufe ({SaeckeJeMeter}), aufgerundet.",
                $"Sand: Länge × m³ je m der Stufe ({SandJeMeter}), aufgerundet auf 0,1 m³.",
                $"Sand, nass: m³ × {Nass} t/m³.",
                $"Sand, trocken: {TrockenMin} bis {TrockenMax} t/m³. {WarumNass}",
                $"Arbeitszeit: Säcke × {Arbeitsgaenge} Arbeitsgänge (Füllen und Tragen, Verlegen) ÷ {Leistung} Säcke "
                    + "je Helfer und Stunde ÷ Helfer, aufgerundet auf die Viertelstunde. Nur ganze Helfer zählen.",
                $"Basisbreite: Höhe × {Basisfaktor}.",
                SichereSeite,
            ];
        }

        var stufe = SandsackRechner.Stufe(hoehe);
        var basis = SandsackRechner.Basisbreite(hoehe);
        var hoeheZeile = $"Höhe: {Wie(hoehe)} m, gerechnet mit der Stufe {Formatting.Number(stufe.HoeheMeter, 1)} m "
            + $"(Stufen {Stufen} m).";
        var basisZeile = $"Basisbreite: {Wie(hoehe)} m × {Basisfaktor} = {Formatting.Number(basis, 1)} m"
            + $"{Aufgerundet(hoehe * Sandsackvorgaben.BasisbreiteJeHoehe, basis)}.";

        if (laengeMeter is not { } laenge)
        {
            return
            [
                hoeheZeile,
                $"Sandsäcke: Länge × {Wie(stufe.SaeckeJeMeter)} je m, aufgerundet.",
                $"Sand: Länge × {Wie(stufe.SandKubikmeterJeMeter)} m³ je m, aufgerundet auf 0,1 m³.",
                $"Sand, nass: m³ × {Nass} t/m³.",
                $"Sand, trocken: {TrockenMin} bis {TrockenMax} t/m³. {WarumNass}",
                $"Arbeitszeit: Säcke × {Arbeitsgaenge} ÷ {Leistung} je Helfer und Stunde ÷ Helfer, "
                    + "aufgerundet auf die Viertelstunde.",
                basisZeile,
                SichereSeite,
            ];
        }

        var saecke = SandsackRechner.Saecke(laenge, stufe);
        var sand = SandsackRechner.SandKubikmeter(laenge, stufe);
        var nass = SandsackRechner.SandTonnen(sand);
        var (trockenMin, trockenMax) = SandsackRechner.SandTrockenTonnen(sand);
        var sandText = Formatting.Number(sand, 1);

        return
        [
            hoeheZeile,
            $"Sandsäcke: {Wie(laenge)} m × {Wie(stufe.SaeckeJeMeter)} je m = {Formatting.Number(saecke, 0)}"
                + $"{Aufgerundet(laenge * stufe.SaeckeJeMeter, saecke)}.",
            $"Sand: {Wie(laenge)} m × {Wie(stufe.SandKubikmeterJeMeter)} m³ je m = {sandText} m³"
                + $"{Aufgerundet(laenge * stufe.SandKubikmeterJeMeter, sand)}.",
            $"Sand, nass: {sandText} m³ × {Nass} t/m³ = {Formatting.Number(nass, 1)} t.",
            $"Sand, trocken: {sandText} m³ × {TrockenMin} bis {TrockenMax} t/m³ = "
                + $"{Formatting.Number(trockenMin, 1)} bis {Formatting.Number(trockenMax, 1)} t. {WarumNass}",
            Arbeitszeit(saecke, helfer),
            basisZeile,
            SichereSeite,
        ];
    }

    /// <summary>A working time as the result shows it: "80 h", "5 h 45 min".</summary>
    public static string Dauer(TimeSpan dauer)
    {
        var stunden = (int)dauer.TotalHours;
        return dauer.Minutes == 0
            ? $"{Formatting.Number(stunden)} h"
            : $"{Formatting.Number(stunden)} h {Formatting.Number(dauer.Minutes)} min";
    }

    private static string Arbeitszeit(decimal saecke, decimal? helfer)
    {
        var saeckeText = Formatting.Number(saecke, 0);
        if (helfer is not { } anzahl)
        {
            return $"Arbeitszeit: {saeckeText} Säcke × {Arbeitsgaenge} ÷ {Leistung} je Helfer und Stunde ÷ Helfer, "
                + "aufgerundet auf die Viertelstunde.";
        }

        var ganze = Math.Floor(anzahl);
        var zeit = SandsackRechner.Arbeitszeit(saecke, anzahl);
        var viertelstunden = saecke * Sandsackvorgaben.ArbeitsgaengeJeSack * 4m
            / Sandsackvorgaben.SaeckeJeHelferUndStunde / ganze;
        var gerundet = viertelstunden == Math.Floor(viertelstunden) ? string.Empty : ", aufgerundet auf die Viertelstunde";
        var nurGanze = ganze == anzahl ? string.Empty : " Nur ganze Helfer zählen.";
        return $"Arbeitszeit: {saeckeText} Säcke × {Arbeitsgaenge} ÷ {Leistung} je Helfer und Stunde "
            + $"÷ {Formatting.Number(ganze, 0)} Helfer = {Dauer(zeit)}{gerundet}.{nurGanze}";
    }

    private static string Aufgerundet(decimal exakt, decimal gerundet) =>
        exakt == gerundet ? string.Empty : ", aufgerundet";

    /// <summary>A number with as many decimals as it was written with: "100", "0,8", "9,6".</summary>
    private static string Wie(decimal value) => Formatting.Number(value, value.Scale);

    /// <summary>
    /// The figures of the table's rows, "0,5 / 1,0 / 1,5 / 2,0". No-break spaces keep the list on
    /// one line, so a narrow phone never wraps it between two of its figures.
    /// </summary>
    private static string Liste(IEnumerable<decimal> values) => string.Join(" / ", values.Select(Wie));
}
