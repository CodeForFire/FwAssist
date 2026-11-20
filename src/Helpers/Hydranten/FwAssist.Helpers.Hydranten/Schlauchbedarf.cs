using FwAssist.Core.Map;

namespace FwAssist.Helpers.Hydranten;

/// <summary>
/// How far a hydrant is from the Einsatzstelle and how many B-Schläuche that takes. The distance
/// is the straight line over the ground, Luftlinie: a hose laid along a street needs more, which
/// is why every figure here says "Luftlinie" on screen.
/// </summary>
public static class Schlauchbedarf
{
    /// <summary>One B-Schlauch, in metres.</summary>
    public const int BSchlauchLaenge = 20;

    // The mean radius of the earth (IUGG), in metres.
    private const double EarthRadius = 6_371_000;

    /// <summary>The great-circle distance between two points in metres (haversine).</summary>
    public static double Metres(GeoPoint from, GeoPoint to)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);
        var lat1 = Radians(from.Latitude);
        var lat2 = Radians(to.Latitude);
        var dLat = lat2 - lat1;
        var dLon = Radians(to.Longitude - from.Longitude);
        var h = (Math.Sin(dLat / 2) * Math.Sin(dLat / 2)) + (Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2));
        return 2 * EarthRadius * Math.Asin(Math.Min(1, Math.Sqrt(h)));
    }

    /// <summary>How many B-Schläuche cover <paramref name="metres"/>: every started length counts, and no distance is less than none.</summary>
    public static int BSchlaeuche(double metres) =>
        (int)Math.Ceiling(Math.Max(0, metres) / BSchlauchLaenge);

    private static double Radians(double degrees) => degrees * Math.PI / 180;
}
