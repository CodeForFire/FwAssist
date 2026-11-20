using FwAssist.Core.Map;

namespace FwAssist.Helpers.Hydranten.Tests;

public class PeilungTests
{
    private static readonly GeoPoint Here = new(48.1785, 11.2555);

    [Theory]
    [InlineData(0.001, 0, 0)]
    [InlineData(0, 0.001, 90)]
    [InlineData(-0.001, 0, 180)]
    [InlineData(0, -0.001, 270)]
    public void Due_north_east_south_and_west_are_0_90_180_and_270_degrees(double dLat, double dLon, double expected) =>
        Assert.Equal(expected, Peilung.Grad(Here, new GeoPoint(Here.Latitude + dLat, Here.Longitude + dLon)), precision: 0);

    [Fact]
    public void North_east_lies_between_north_and_east_once_the_longitude_is_scaled()
    {
        // At 48° a degree of longitude is about two thirds of one of latitude.
        var bearing = Peilung.Grad(Here, new GeoPoint(Here.Latitude + 0.001, Here.Longitude + (0.001 / Math.Cos(48.1785 * Math.PI / 180))));

        Assert.Equal(45, bearing, precision: 0);
    }

    [Fact]
    public void A_bearing_is_never_negative() =>
        Assert.InRange(Peilung.Grad(Here, new GeoPoint(Here.Latitude + 0.001, Here.Longitude - 0.0001)), 270, 360);

    [Theory]
    [InlineData(0, 0)]
    [InlineData(360, 0)]
    [InlineData(-90, 270)]
    [InlineData(450, 90)]
    [InlineData(-370, 350)]
    public void Normalize_turns_any_angle_into_0_up_to_360(double degrees, double expected) =>
        Assert.Equal(expected, Peilung.Normalize(degrees), precision: 9);

    [Theory]
    [InlineData(0, "N")]
    [InlineData(22, "N")]
    [InlineData(23, "NO")]
    [InlineData(90, "O")]
    [InlineData(135, "SO")]
    [InlineData(180, "S")]
    [InlineData(225, "SW")]
    [InlineData(270, "W")]
    [InlineData(315, "NW")]
    [InlineData(338, "N")]
    [InlineData(-45, "NW")]
    public void Himmelsrichtung_names_the_nearest_of_eight(double degrees, string expected) =>
        Assert.Equal(expected, Peilung.Himmelsrichtung(degrees));
}
