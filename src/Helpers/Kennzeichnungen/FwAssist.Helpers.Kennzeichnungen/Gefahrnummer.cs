namespace FwAssist.Helpers.Kennzeichnungen;

/// <summary>
/// A Nummer zur Kennzeichnung der Gefahr as read by <see cref="Gefahrnummern.TryRead"/>.
/// <see cref="Meaning"/> is the ADR's own entry, or <see langword="null"/> for a number it does not
/// list. <see cref="Digits"/> is empty for a combination with a meaning of its own, because reading
/// its digits one by one would be wrong: 606 is not "giftig, giftig".
/// </summary>
public sealed record Gefahrnummer(
    string Number,
    string? Meaning,
    bool ReactsWithWater,
    bool IsSpecialCombination,
    IReadOnlyList<Ziffer> Digits);
