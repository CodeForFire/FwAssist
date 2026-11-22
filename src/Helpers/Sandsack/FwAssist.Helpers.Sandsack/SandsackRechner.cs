namespace FwAssist.Helpers.Sandsack;

/// <summary>
/// The Sandsack arithmetic. Lengths and heights are in metres, sand in m³ and tonnes.
/// <para>
/// Rounding always errs towards having too much: a height rounds up to the next row of the table,
/// sacks up to the whole sack, sand up to a tenth, and the working time up to a quarter hour.
/// </para>
/// <para>
/// Unlike the other calculators, nothing here is rounded before its ceiling: any part of a sack,
/// however small, is real and needs a sack. There is no noise to absorb either. Sacks and sand
/// are decimal products, which are exact, and the time is one division, whose result is exact
/// whenever it ends on a quarter hour.
/// </para>
/// </summary>
public static class SandsackRechner
{
    private const decimal ViertelstundenJeStunde = 4m;

    /// <summary>The highest dam the table covers, in metres. A higher one is not calculated.</summary>
    public static decimal MaxHoeheMeter => Sandsackvorgaben.Dammstufen[^1].HoeheMeter;

    /// <summary>
    /// The row of the table a dam <paramref name="hoeheMeter"/> high is counted with: the first that
    /// is at least as high.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The dam is higher than <see cref="MaxHoeheMeter"/>.</exception>
    public static Dammstufe Stufe(decimal hoeheMeter)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(hoeheMeter, MaxHoeheMeter);
        return Sandsackvorgaben.Dammstufen.First(s => s.HoeheMeter >= hoeheMeter);
    }

    /// <summary>The sacks for <paramref name="laengeMeter"/> of dam, in whole sacks.</summary>
    public static decimal Saecke(decimal laengeMeter, Dammstufe stufe)
    {
        ArgumentNullException.ThrowIfNull(stufe);
        return RoundUp(laengeMeter * stufe.SaeckeJeMeter, 0);
    }

    /// <summary>The sand for <paramref name="laengeMeter"/> of dam, in m³ to one decimal.</summary>
    public static decimal SandKubikmeter(decimal laengeMeter, Dammstufe stufe)
    {
        ArgumentNullException.ThrowIfNull(stufe);
        return RoundUp(laengeMeter * stufe.SandKubikmeterJeMeter, 1);
    }

    /// <summary>What <paramref name="kubikmeter"/> of wet sand weigh, in tonnes to one decimal.</summary>
    public static decimal SandTonnen(decimal kubikmeter) =>
        RoundUp(kubikmeter * Sandsackvorgaben.SandTonnenJeKubikmeter, 1);

    /// <summary>
    /// How long <paramref name="helfer"/> take to fill, carry and lay <paramref name="saecke"/>,
    /// rounded up to the quarter hour. Part of a Helfer does not count: 2,5 Helfer work as 2.
    /// </summary>
    public static TimeSpan Arbeitszeit(decimal saecke, decimal helfer)
    {
        var ganzeHelfer = Math.Floor(helfer);
        ArgumentOutOfRangeException.ThrowIfLessThan(ganzeHelfer, 1m, nameof(helfer));

        var stunden = saecke * Sandsackvorgaben.ArbeitsgaengeJeSack
            / Sandsackvorgaben.SaeckeJeHelferUndStunde / ganzeHelfer;
        var viertelstunden = Math.Ceiling(stunden * ViertelstundenJeStunde);
        return TimeSpan.FromMinutes((double)(viertelstunden * 15m));
    }

    /// <summary>The narrowest base a dam <paramref name="hoeheMeter"/> high stands on, up to a tenth.</summary>
    public static decimal Basisbreite(decimal hoeheMeter) =>
        RoundUp(hoeheMeter * Sandsackvorgaben.BasisbreiteJeHoehe, 1);

    private static decimal RoundUp(decimal value, int decimals)
    {
        var scale = decimals == 0 ? 1m : 10m;
        return Math.Ceiling(value * scale) / scale;
    }
}
