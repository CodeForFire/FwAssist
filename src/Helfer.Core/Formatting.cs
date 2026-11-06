using System.Globalization;

namespace Helfer.Core;

/// <summary>
/// Everything the user reads goes through the fixed de-DE culture, whatever the phone is set to:
/// a Feuerwehr in Germany reads "1.234" and "14:05", and a test must not depend on the machine.
/// Anything persisted uses <see cref="CultureInfo.InvariantCulture"/> instead.
/// </summary>
public static class Formatting
{
    private static readonly CultureInfo De = CultureInfo.GetCultureInfo("de-DE");

    /// <summary>A whole number the way the user reads it (de-DE grouping).</summary>
    public static string Number(int value) => value.ToString("N0", De);

    /// <summary>A number the way the user reads it, with a fixed count of decimals: "1.234,5".</summary>
    public static string Number(decimal value, int decimals) =>
        value.ToString("N" + decimals.ToString(CultureInfo.InvariantCulture), De);

    /// <summary>
    /// A number as typed on a phone. A comma and a dot both mean the decimal separator, because
    /// which one a number keyboard offers depends on the keyboard, so "1.234" is 1,234 and never
    /// a thousand. No sign and no exponent: every amount in this app is positive.
    /// </summary>
    public static bool TryParseNumber(string text, out decimal value)
    {
        ArgumentNullException.ThrowIfNull(text);
        return decimal.TryParse(
            text.Trim().Replace(',', '.'),
            NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out value);
    }

    public static string Timestamp(DateTimeOffset t) => t.ToString("dd.MM.yyyy HH:mm", De);

    /// <summary>Clock time alone, "14:05".</summary>
    public static string TimeOfDay(DateTimeOffset t) => t.ToString("HH:mm", De);

    /// <summary>
    /// A running time as a stopwatch shows it: "04:07" below an hour, "1:04:07" from one hour on.
    /// Truncated to the second, never rounded up, so the display never shows a second that has not
    /// passed. A negative span reads as zero.
    /// </summary>
    public static string Duration(TimeSpan span)
    {
        if (span < TimeSpan.Zero)
        {
            span = TimeSpan.Zero;
        }

        var hours = (long)span.TotalHours;
        return hours > 0
            ? string.Create(De, $"{hours}:{span.Minutes:00}:{span.Seconds:00}")
            : string.Create(De, $"{span.Minutes:00}:{span.Seconds:00}");
    }
}
