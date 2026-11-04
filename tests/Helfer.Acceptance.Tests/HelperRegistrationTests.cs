using System.Xml.Linq;
using Avalonia.Headless.XUnit;
using Helfer.AppLogic.Helpers;
using Helfer.Composition;
using Helfer.Testing;

namespace Helfer.Acceptance.Tests;

// The three places a helper is registered -- CompositionRoot.Modules, CompositionRoot.RegisterViews
// and an activity-alias in the Android manifest -- have to agree. A helper missing from the
// manifest has no launcher icon; an alias whose helper is gone opens the home screen forever.
public class HelperRegistrationTests
{
    private const string AliasPrefix = "de.codeforfire.helfer.launcher.";
    private static readonly XNamespace Android = "http://schemas.android.com/apk/res/android";

    private static List<XElement> Aliases() =>
        XDocument.Load(Path.Join(AppContext.BaseDirectory, "AndroidManifest.xml"))
            .Descendants("activity-alias")
            .ToList();

    [Fact]
    public void Every_helper_has_exactly_one_launcher_alias_and_every_alias_a_helper()
    {
        var aliasIds = Aliases()
            .Select(a => (string?)a.Attribute(Android + "name") ?? string.Empty)
            .Select(n => n.StartsWith(AliasPrefix, StringComparison.Ordinal) ? n[AliasPrefix.Length..] : "(bad name) " + n)
            .Order(StringComparer.Ordinal)
            .ToList();

        Assert.Equal(CompositionRoot.Modules.Select(m => m.Id).Order(StringComparer.Ordinal), aliasIds);
    }

    [Fact]
    public void Each_alias_targets_the_main_activity_and_is_labelled_with_the_helper_title()
    {
        foreach (var module in CompositionRoot.Modules)
        {
            var alias = Aliases().Single(a => (string?)a.Attribute(Android + "name") == AliasPrefix + module.Id);
            Assert.Equal("de.codeforfire.helfer.MainActivity", (string?)alias.Attribute(Android + "targetActivity"));
            Assert.Equal(module.Title, (string?)alias.Attribute(Android + "label"));
            Assert.Equal("@drawable/ic_helper_" + module.Id, (string?)alias.Attribute(Android + "icon"));
        }
    }

    [Fact]
    public void Helper_ids_are_unique_lower_case_ascii()
    {
        var ids = CompositionRoot.Modules.Select(m => m.Id).ToList();

        Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());
        Assert.All(ids, id => Assert.Matches("^[a-z][a-z0-9]*$", id));
    }

    [AvaloniaFact]
    public void Every_helper_page_has_a_registered_view()
    {
        var services = new HelperServices(new FakeTimeProvider(), new ManualTicker());

        foreach (var module in CompositionRoot.Modules)
        {
            var page = module.CreateViewModel(services);
            Assert.True(Helfer.Ui.App.Views.Has(page.GetType()), $"{module.Title} has no view registered");
        }
    }
}
