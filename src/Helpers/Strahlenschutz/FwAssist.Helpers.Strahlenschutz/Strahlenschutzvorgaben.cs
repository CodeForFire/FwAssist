namespace FwAssist.Helpers.Strahlenschutz;

/// <summary>
/// Every reference value the Strahlenschutz helper uses, each with its source:
/// <list type="bullet">
/// <item>ADR: ADR 5.1.5.3.1, how the Transportkennzahl (TKZ, Transport Index) is set.</item>
/// <item>TK: Taschenkarte ABC, AK Gefahrgut, 2023, "Absperren" (the source attached to #25).</item>
/// </list>
/// The maintainer checked the limits from TK against the current FwDV 500 (2022) after #22 was
/// merged, and they agree.
/// </summary>
public static class Strahlenschutzvorgaben
{
    /// <summary>
    /// ADR 5.1.5.3.1 (a) and (b): the TKZ is the highest dose rate at 1 m from the package's outer
    /// surface in mSv/h, times 100. So 1 TKZ is 0,01 mSv/h, which is 10 µSv/h, at 1 m. ADR rounds the
    /// TKZ up to one decimal, so the dose rate it gives is an upper bound for an undamaged package.
    /// </summary>
    public const decimal MikrosievertProStundeJeTkz = 10m;

    /// <summary>
    /// ADR 5.1.5.3.1 (c): a value of 0,05 or less may be given as 0. A TKZ of 0 is therefore counted as
    /// 0,05, the most it can stand for.
    /// </summary>
    public const decimal TkzNullBis = 0.05m;

    /// <summary>The distance the TKZ refers to, in metres (ADR 5.1.5.3.1 (a)).</summary>
    public const decimal TkzBezugsabstandMeter = 1m;

    /// <summary>
    /// TK, "Absperren": the Gefahrenbereich ends at 25 µSv/h at an A-Einsatz. Checked against FwDV 500 (2022).
    /// </summary>
    public const decimal GrenzwertGefahrenbereich = 25m;

    /// <summary>
    /// TK, "Absperren": the Gefahrenbereich is at least 50 m around the Schadenstelle. Checked against
    /// FwDV 500 (2022).
    /// </summary>
    public const decimal MindestGefahrenbereichMeter = 50m;

    /// <summary>Named on screen, and no more than named. A number and its unit never wrap apart.</summary>
    public const string Quellen =
        "Quellen: ADR 5.1.5.3.1 (TKZ), Taschenkarte ABC (25 µSv/h, 50 m). "
        + "Keine dieser Stellen hat diese App geprüft oder herausgegeben.";
}
