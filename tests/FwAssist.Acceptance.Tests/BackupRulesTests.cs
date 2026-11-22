using System.Xml.Linq;

namespace FwAssist.Acceptance.Tests;

// Android's Auto Backup copies an app's preferences and files into the user's Google account. Two
// things must stay out: the device-only preferences, where the Hydrantenkarte keeps the Einsatzstelle,
// and the Kartenpakete, which are large enough to push the app past the backup quota, after which
// Android backs up nothing at all. Android 12+ reads data_extraction_rules, older versions backup_rules.
public class BackupRulesTests
{
    private const string DeviceOnlyPreferences = "helper-settings-device.xml";
    private const string Kartenpakete = "maps/";

    private static readonly XNamespace Android = "http://schemas.android.com/apk/res/android";

    private static XDocument Load(string name) => XDocument.Load(Path.Join(AppContext.BaseDirectory, name));

    private static List<(string Domain, string Path)> Excluded(XElement rules) =>
        rules.Elements("exclude")
            .Select(e => ((string?)e.Attribute("domain") ?? string.Empty, (string?)e.Attribute("path") ?? string.Empty))
            .ToList();

    [Fact]
    public void The_manifest_names_both_rule_files()
    {
        var application = Assert.Single(Load("AndroidManifest.xml").Descendants("application"));

        Assert.Equal("@xml/data_extraction_rules", (string?)application.Attribute(Android + "dataExtractionRules"));
        Assert.Equal("@xml/backup_rules", (string?)application.Attribute(Android + "fullBackupContent"));
    }

    [Fact]
    public void The_cloud_backup_of_android_12_and_newer_leaves_out_the_einsatzstelle_and_the_kartenpakete()
    {
        var cloud = Assert.Single(Load("data_extraction_rules.xml").Elements("data-extraction-rules").Elements("cloud-backup"));

        Assert.Contains(("sharedpref", DeviceOnlyPreferences), Excluded(cloud));
        Assert.Contains(("file", Kartenpakete), Excluded(cloud));
    }

    [Fact]
    public void The_backup_of_android_11_and_older_leaves_out_the_einsatzstelle_and_the_kartenpakete()
    {
        var rules = Assert.Single(Load("backup_rules.xml").Elements("full-backup-content"));

        Assert.Contains(("sharedpref", DeviceOnlyPreferences), Excluded(rules));
        Assert.Contains(("file", Kartenpakete), Excluded(rules));
    }
}
