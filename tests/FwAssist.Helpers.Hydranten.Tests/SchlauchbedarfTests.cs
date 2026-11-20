using FwAssist.Core.Map;

namespace FwAssist.Helpers.Hydranten.Tests;

public class SchlauchbedarfTests
{
    [Fact]
    public void A_point_is_no_distance_from_itself()
    {
        var p = new GeoPoint(48.1785, 11.2555);

        Assert.Equal(0, Schlauchbedarf.Metres(p, p), precision: 6);
    }

    [Fact]
    public void One_thousandth_of_a_degree_of_latitude_is_about_111_metres()
    {
        var metres = Schlauchbedarf.Metres(new GeoPoint(48.0, 11.0), new GeoPoint(48.001, 11.0));

        Assert.Equal(111.19, metres, precision: 2);
    }

    [Fact]
    public void A_degree_of_longitude_shrinks_with_the_cosine_of_the_latitude()
    {
        var atEquator = Schlauchbedarf.Metres(new GeoPoint(0, 11.0), new GeoPoint(0, 11.001));
        var at60 = Schlauchbedarf.Metres(new GeoPoint(60, 11.0), new GeoPoint(60, 11.001));

        Assert.Equal(atEquator / 2, at60, precision: 2);
    }

    [Fact]
    public void The_distance_is_the_same_both_ways()
    {
        var a = new GeoPoint(48.1785, 11.2555);
        var b = new GeoPoint(48.1774, 11.2581);

        Assert.Equal(Schlauchbedarf.Metres(a, b), Schlauchbedarf.Metres(b, a), precision: 9);
    }

    [Fact]
    public void Munich_to_Augsburg_is_about_56_kilometres_as_the_crow_flies()
    {
        var metres = Schlauchbedarf.Metres(new GeoPoint(48.1374, 11.5755), new GeoPoint(48.3705, 10.8978));

        Assert.InRange(metres, 55_500, 56_500);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-5, 0)]
    [InlineData(0.1, 1)]
    [InlineData(20, 1)]
    [InlineData(20.01, 2)]
    [InlineData(40, 2)]
    [InlineData(120, 6)]
    [InlineData(121, 7)]
    public void Every_started_20_metres_take_one_more_b_schlauch(double metres, int expected) =>
        Assert.Equal(expected, Schlauchbedarf.BSchlaeuche(metres));

    [Fact]
    public void A_b_schlauch_is_20_metres() => Assert.Equal(20, Schlauchbedarf.BSchlauchLaenge);
}
