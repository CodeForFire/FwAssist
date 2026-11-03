using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Helfer.Helpers.Stoppuhr;

namespace Helfer.Acceptance.Tests;

// The screens a new user sees, in order. Doubles as the screenshot source: `make screenshots`,
// `make phone-screenshots` and `make play-screenshots` run this class with RENDER_OUT set.
public class DemoFlowRenderTests
{
    private static List<string?> Texts(Control root) =>
        root.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text).ToList();

    [AvaloniaFact]
    public void Home_lists_every_helper_with_its_summary()
    {
        var phone = new PhoneRender();

        var texts = Texts(phone.Window);
        Assert.Contains("Stoppuhr", texts);
        Assert.Contains(new StoppuhrModule().Summary, texts);
        Assert.Contains("Version 0.1.0", texts);

        phone.Capture("01-uebersicht.png");
    }

    [AvaloniaFact]
    public void Tapping_the_stoppuhr_tile_opens_it_and_the_time_runs()
    {
        var phone = new PhoneRender();
        var tile = phone.Window.GetVisualDescendants().OfType<Button>().First(b => b.Classes.Contains("tile"));

        tile.Command!.Execute(null);
        PhoneRender.Settle();
        var start = phone.Window.GetVisualDescendants().OfType<Button>().First(b => Equals(b.Content, "START"));
        start.Command!.Execute(null);
        phone.Time.Advance(TimeSpan.FromSeconds(247));
        phone.Ticker.Tick();
        PhoneRender.Settle();

        var texts = Texts(phone.Window);
        Assert.Contains("04:07", texts);
        Assert.Contains("Stoppuhr", texts);
        Assert.True(phone.Window.GetVisualDescendants().OfType<Button>().Single(b => b.Name == "HomeButton").IsVisible);

        phone.Capture("02-stoppuhr.png");
    }

    [AvaloniaFact]
    public void Settings_list_a_launcher_icon_switch_per_helper()
    {
        var phone = new PhoneRender();
        phone.Shell.Home.OpenSettingsCommand.Execute(null);
        PhoneRender.Settle();

        var toggle = Assert.Single(phone.Window.GetVisualDescendants().OfType<ToggleSwitch>());
        Assert.True(toggle.IsChecked);

        phone.Capture("03-einstellungen.png");
    }
}
