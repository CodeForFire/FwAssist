using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Helfer.Composition;

namespace Helfer.Acceptance.Tests;

// Every control a screen reader cannot name from its own text carries an AutomationProperties.Name
// (AGENTS.md, "Avalonia UI"). Walks every page the app has: home, settings and each helper.
public class AutomationPropertiesTests
{
    private static IEnumerable<string> Unnamed(Control root) =>
        root.GetVisualDescendants()
            .OfType<Control>()
            .Where(c => c is ToggleSwitch or TextBox
                || (c is Button b && (b.Content is not string text || string.IsNullOrWhiteSpace(text))))
            .Where(c => c.IsEffectivelyVisible && string.IsNullOrWhiteSpace(AutomationProperties.GetName(c)))
            .Select(c => $"{c.GetType().Name} {c.Name}");

    [AvaloniaFact]
    public void Every_text_less_control_on_every_page_has_an_automation_name()
    {
        var phone = new PhoneRender();
        var problems = new List<string>();

        problems.AddRange(Unnamed(phone.Window).Select(p => "Übersicht: " + p));

        foreach (var module in CompositionRoot.Modules)
        {
            phone.Shell.Open(module.Id);
            PhoneRender.Settle();
            problems.AddRange(Unnamed(phone.Window).Select(p => module.Title + ": " + p));
        }

        phone.Shell.GoHome();
        phone.Shell.Home.OpenSettingsCommand.Execute(null);
        PhoneRender.Settle();
        problems.AddRange(Unnamed(phone.Window).Select(p => "Einstellungen: " + p));

        Assert.Empty(problems);
    }
}
