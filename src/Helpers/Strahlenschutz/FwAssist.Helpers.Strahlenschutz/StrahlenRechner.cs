namespace FwAssist.Helpers.Strahlenschutz;

/// <summary>
/// The Strahlenschutz arithmetic. Dose rates are in µSv/h, distances in metres. All of it assumes a
/// point source, whose dose rate falls with the square of the distance (Abstandsquadratgesetz).
/// <para>
/// Rounding always errs towards caution: a dose rate rounds up to two significant figures, a
/// distance to keep away rounds up to the whole metre.
/// </para>
/// </summary>
public static class StrahlenRechner
{
    // Repeating decimals land just beside the true value, and a ceiling would turn 2.0000000001 into
    // 3. Six places are far finer than anything shown and far coarser than that noise.
    private const int NoiseDecimals = 6;

    // Two significant figures: the mantissa a dose rate is rounded to lies from 10 to 99.
    private const decimal MantissaLimit = 100m;

    /// <summary>
    /// The dose rate an undamaged package with this <paramref name="tkz"/> gives at
    /// <paramref name="abstandMeter"/> from its surface, at most.
    /// </summary>
    public static decimal Tkz(decimal tkz, decimal abstandMeter)
    {
        var amBezugsabstand = Math.Max(tkz, Strahlenschutzvorgaben.TkzNullBis)
            * Strahlenschutzvorgaben.MikrosievertProStundeJeTkz;
        return Abstand(amBezugsabstand, Strahlenschutzvorgaben.TkzBezugsabstandMeter, abstandMeter);
    }

    /// <summary>The dose rate at <paramref name="zielMeter"/>, from one measured at <paramref name="messMeter"/>.</summary>
    public static decimal Abstand(decimal dosisleistung, decimal messMeter, decimal zielMeter)
    {
        var verhaeltnis = messMeter / zielMeter;
        return RoundUpSignificant(dosisleistung * verhaeltnis * verhaeltnis);
    }

    /// <summary>
    /// How far from the source the dose rate measured at <paramref name="messMeter"/> falls to
    /// <paramref name="grenzwert"/>, in whole metres.
    /// </summary>
    public static decimal Absperrgrenze(decimal dosisleistung, decimal messMeter, decimal grenzwert)
    {
        // decimal has no square root. A double carries 15 significant digits, far more than a
        // distance in whole metres needs, and the ceiling below absorbs what it loses.
        var faktor = (decimal)Math.Sqrt((double)(dosisleistung / grenzwert));
        return Math.Ceiling(Math.Round(messMeter * faktor, NoiseDecimals));
    }

    /// <summary>Rounds up to two significant figures: 0,123 becomes 0,13, and 1.234 becomes 1.300.</summary>
    public static decimal RoundUpSignificant(decimal value)
    {
        if (value <= 0m)
        {
            return 0m;
        }

        var step = 1m;
        while (value / step >= MantissaLimit)
        {
            step *= 10m;
        }

        while (value / step < MantissaLimit / 10m)
        {
            step /= 10m;
        }

        return Math.Ceiling(Math.Round(value / step, NoiseDecimals)) * step;
    }
}
