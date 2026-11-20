using FwAssist.Core.Map;

namespace FwAssist.Helpers.Hydranten;

/// <summary>Which way one point lies from another, as the Richtungspfeil needs it.</summary>
public static class Peilung
{
    private static readonly string[] Richtungen = ["N", "NO", "O", "SO", "S", "SW", "W", "NW"];

    /// <summary>
    /// The initial great-circle bearing from <paramref name="from"/> to <paramref name="to"/>, in
    /// degrees clockwise from true north, 0 up to 360.
    /// </summary>
    public static double Grad(GeoPoint from, GeoPoint to)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);
        var lat1 = from.Latitude * Math.PI / 180;
        var lat2 = to.Latitude * Math.PI / 180;
        var dLon = (to.Longitude - from.Longitude) * Math.PI / 180;
        var y = Math.Sin(dLon) * Math.Cos(lat2);
        var x = (Math.Cos(lat1) * Math.Sin(lat2)) - (Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon));
        return Normalize(Math.Atan2(y, x) * 180 / Math.PI);
    }

    /// <summary><paramref name="degrees"/> as a turn from 0 up to 360, whichever way round it was counted.</summary>
    public static double Normalize(double degrees)
    {
        var turn = degrees % 360;
        return turn < 0 ? turn + 360 : turn;
    }

    /// <summary>The nearest of the eight Himmelsrichtungen: "N", "NO", "O", "SO", "S", "SW", "W", "NW".</summary>
    public static string Himmelsrichtung(double degrees) =>
        Richtungen[(int)Math.Round(Normalize(degrees) / 45) % 8];
}
