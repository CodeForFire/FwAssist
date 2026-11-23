namespace FwAssist.Helpers.Sandsack;

/// <summary>
/// Every reference value the Sandsack helper uses. All of them come from one source, the
/// "Taschenkarte Deichverteidigung" of the THW Ortsverband Emden, Stand 12/2007 (TK below), the
/// card attached to #19. The card itself calls its figures "nur ca. Werte; Abhängig vom
/// Sandsacktyp", which is why the screen calls the results Richtwerte.
/// <para>
/// Two other calculators were weighed and not used for numbers: THW Markt Schwaben's
/// Sandsackrechner does not publish how it counts, and THW Hamburg-Nord's table gives fewer sacks
/// and leaves filling and transport out.
/// </para>
/// </summary>
public static class Sandsackvorgaben
{
    /// <summary>
    /// TK, table "Sandsackdamm": sacks and sand per metre of Sandsackdamm, by height. The card lists
    /// each height for a dam of 1 m, 5 m and 100 m. Each value here is the largest per-metre figure of
    /// those three rows, in the column "quer verlegt", which needs more sacks than "längs verlegt",
    /// and the upper end of the sand range ("min - max"). Only the sand for 2,0 m comes from the 5 m
    /// row (48 m³, so 9,6 m³ per metre); every other value is the 1 m row's.
    /// </summary>
    public static IReadOnlyList<Dammstufe> Dammstufen { get; } =
    [
        new(0.5m, 45m, 1m),
        new(1.0m, 160m, 3m),
        new(1.5m, 350m, 6m),
        new(2.0m, 580m, 9.6m),
    ];

    /// <summary>
    /// TK, "Sandsack": "40 - 60 Sandsäcke / Helfer / Stunde (entw. Füllen/Transport od. Verlegen)".
    /// The lower end, so the time errs long.
    /// </summary>
    public const decimal SaeckeJeHelferUndStunde = 40m;

    /// <summary>
    /// The two jobs TK's rate counts separately: a sack is filled and carried once, and laid once.
    /// </summary>
    public const int ArbeitsgaengeJeSack = 2;

    /// <summary>
    /// TK, "Deichverteidigungsmaterialien", Sand: "Gewicht trocken 1.200 - 1.600 Kg/m³; naß
    /// 2.000 Kg/m³", in tonnes. The wet figure, the heaviest, so a load errs heavy.
    /// </summary>
    public const decimal SandTonnenJeKubikmeter = 2m;

    /// <summary>
    /// TK, "Deichverteidigungsmaterialien", Sand: "Gewicht trocken 1.200 - 1.600 Kg/m³", the lower
    /// end, in tonnes. Shown to explain the span only. No result is planned with it.
    /// </summary>
    public const decimal SandTrockenMinTonnenJeKubikmeter = 1.2m;

    /// <summary>The upper end of TK's dry weight; see <see cref="SandTrockenMinTonnenJeKubikmeter"/>.</summary>
    public const decimal SandTrockenMaxTonnenJeKubikmeter = 1.6m;

    /// <summary>TK, "Sandsackdamm, normale Stabilität": "b = > 2*h".</summary>
    public const decimal BasisbreiteJeHoehe = 2m;

    /// <summary>Named on screen, and no more than named.</summary>
    public const string Quellen =
        "Quelle: Taschenkarte Deichverteidigung, THW OV Emden, Stand 12/2007. "
        + "Keine dieser Stellen hat diese App geprüft oder herausgegeben.";
}

/// <summary>
/// One row of TK's table "Sandsackdamm": a dam up to <paramref name="HoeheMeter"/> high needs
/// <paramref name="SaeckeJeMeter"/> sacks and <paramref name="SandKubikmeterJeMeter"/> m³ sand per
/// metre of length.
/// </summary>
public sealed record Dammstufe(decimal HoeheMeter, decimal SaeckeJeMeter, decimal SandKubikmeterJeMeter);
