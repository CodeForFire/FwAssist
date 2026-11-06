namespace Helfer.Helpers.Schaummittel;

/// <summary>
/// The foam arithmetic, both ways. Schaummittel is the Zumischrate's share of the Gemisch, the rest
/// of the Gemisch is Wasser, and the Verschäumungszahl says how many litres of foam one litre of
/// Gemisch makes.
/// <para>
/// Rounding always errs towards having too much: what is needed rounds up, what can be made rounds
/// down. No allowance for lost foam is built in; the caller passes one as a Zuschlag.
/// </para>
/// </summary>
public static class SchaumRechner
{
    private const decimal LiterPerKubikmeter = 1000m;

    // Repeating decimals land just beside the true value, and a ceiling would turn 100.00000001 into
    // 101. Six places are far finer than anything shown and far coarser than that noise.
    private const int NoiseDecimals = 6;

    /// <summary>What the supplies make. Without a <paramref name="wasserLiter"/> water is unlimited.</summary>
    public static VorratErgebnis Vorrat(
        decimal schaummittelLiter,
        decimal? wasserLiter,
        decimal zumischProzent,
        decimal verschaeumungszahl)
    {
        var anteil = zumischProzent / 100m;
        var gemisch = schaummittelLiter / anteil;
        var limit = Limit.Schaummittel;
        if (wasserLiter is { } wasser && wasser / (1m - anteil) < gemisch)
        {
            gemisch = wasser / (1m - anteil);
            limit = Limit.Wasser;
        }

        return new VorratErgebnis(
            RoundDown(gemisch * verschaeumungszahl / LiterPerKubikmeter, 1),
            RoundDown(gemisch, 0),
            RoundUp(gemisch * anteil, 0),
            RoundUp(gemisch * (1m - anteil), 0),
            limit);
    }

    /// <summary>What it takes to fill a room, with a Zuschlag on top for foam that is lost.</summary>
    public static RaumErgebnis Raum(
        decimal laengeMeter,
        decimal breiteMeter,
        decimal hoeheMeter,
        decimal zuschlagProzent,
        decimal zumischProzent,
        decimal verschaeumungszahl)
    {
        var anteil = zumischProzent / 100m;
        var volumen = laengeMeter * breiteMeter * hoeheMeter;
        var schaum = volumen * (1m + (zuschlagProzent / 100m));
        var gemisch = schaum * LiterPerKubikmeter / verschaeumungszahl;

        return new RaumErgebnis(
            RoundUp(volumen, 1),
            RoundUp(schaum, 1),
            RoundUp(gemisch, 0),
            RoundUp(gemisch * anteil, 0),
            RoundUp(gemisch * (1m - anteil), 0));
    }

    private static decimal RoundUp(decimal value, int decimals)
    {
        var scale = Scale(decimals);
        return Math.Ceiling(Math.Round(value * scale, NoiseDecimals)) / scale;
    }

    private static decimal RoundDown(decimal value, int decimals)
    {
        var scale = Scale(decimals);
        return Math.Floor(Math.Round(value * scale, NoiseDecimals)) / scale;
    }

    private static decimal Scale(int decimals) => decimals == 0 ? 1m : 10m;
}

/// <summary>Which supply decides how much foam can be made.</summary>
public enum Limit
{
    Schaummittel,
    Wasser,
}

/// <summary>What the supplies make. Litres are whole, foam is in tenths of a cubic metre.</summary>
public sealed record VorratErgebnis(
    decimal SchaumKubikmeter,
    decimal GemischLiter,
    decimal SchaummittelLiter,
    decimal WasserLiter,
    Limit Limit);

/// <summary>What a room needs. Litres are whole, volumes are in tenths of a cubic metre.</summary>
public sealed record RaumErgebnis(
    decimal VolumenKubikmeter,
    decimal SchaumKubikmeter,
    decimal GemischLiter,
    decimal SchaummittelLiter,
    decimal WasserLiter);
