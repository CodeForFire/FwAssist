using System.Xml.Linq;

namespace FwAssist.Acceptance.Tests;

// A permission makes Android assume the hardware behind it, and Google Play then hides the app from
// every device without that hardware unless the manifest says the feature is not required. The
// location permissions imply a GPS and "any location", which a Wi-Fi-only tablet lacks; the app works
// without either, only its Richtungspfeil does not.
public class ManifestFeatureTests
{
    private static readonly XNamespace Android = "http://schemas.android.com/apk/res/android";

    // The features Android derives from these permissions (developer.android.com, "<uses-feature>",
    // "Permissions that imply feature requirements"), for the permissions this app declares.
    private static readonly Dictionary<string, string[]> ImpliedFeatures = new(StringComparer.Ordinal)
    {
        ["android.permission.ACCESS_FINE_LOCATION"] = ["android.hardware.location", "android.hardware.location.gps"],
        ["android.permission.ACCESS_COARSE_LOCATION"] = ["android.hardware.location"],
    };

    private static XDocument Manifest() => XDocument.Load(Path.Join(AppContext.BaseDirectory, "AndroidManifest.xml"));

    [Fact]
    public void Every_feature_a_permission_implies_is_declared_not_required()
    {
        var manifest = Manifest();
        var permissions = manifest.Descendants("uses-permission")
            .Select(p => (string?)p.Attribute(Android + "name") ?? string.Empty);
        var optional = manifest.Descendants("uses-feature")
            .Where(f => string.Equals((string?)f.Attribute(Android + "required"), "false", StringComparison.Ordinal))
            .Select(f => (string?)f.Attribute(Android + "name") ?? string.Empty)
            .ToHashSet(StringComparer.Ordinal);

        var missing = permissions
            .SelectMany(p => ImpliedFeatures.GetValueOrDefault(p) ?? [])
            .Distinct(StringComparer.Ordinal)
            .Where(f => !optional.Contains(f))
            .Order(StringComparer.Ordinal)
            .ToList();

        Assert.Empty(missing);
    }

    [Fact]
    public void The_compass_is_not_required_either() =>
        Assert.Contains(
            Manifest().Descendants("uses-feature"),
            f => (string?)f.Attribute(Android + "name") == "android.hardware.sensor.compass"
                && (string?)f.Attribute(Android + "required") == "false");
}
