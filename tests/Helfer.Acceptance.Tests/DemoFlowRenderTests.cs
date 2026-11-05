using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Helfer.Composition;
using Helfer.Helpers.Funkalphabet;

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
        foreach (var module in CompositionRoot.Modules)
        {
            Assert.Contains(module.Title, texts);
            Assert.Contains(module.Summary, texts);
        }

        Assert.Contains("Version 0.1.0", texts);

        phone.Capture("01-uebersicht.png");
    }

    [AvaloniaFact]
    public void Tapping_the_stoppuhr_tile_opens_it_and_the_time_runs()
    {
        var phone = new PhoneRender();
        var tile = phone.Window.GetVisualDescendants().OfType<Button>().First(b => b.Classes.Contains("tile"));

        Assert.NotNull(tile.Command);
        tile.Command.Execute(null);
        PhoneRender.Settle();
        var start = phone.Window.GetVisualDescendants().OfType<Button>().First(b => Equals(b.Content, "START"));
        Assert.NotNull(start.Command);
        start.Command.Execute(null);
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

        var toggles = phone.Window.GetVisualDescendants().OfType<ToggleSwitch>().ToList();
        Assert.Equal(CompositionRoot.Modules.Count, toggles.Count);
        Assert.All(toggles, t => Assert.True(t.IsChecked));

        phone.Capture("03-einstellungen.png");
    }

    [AvaloniaFact]
    public void The_funkalphabet_spells_what_is_typed_and_switches_to_the_din_table()
    {
        var phone = new PhoneRender();
        phone.Shell.Open(FunkalphabetModule.HelperId);
        PhoneRender.Settle();
        var input = phone.Window.GetVisualDescendants().OfType<TextBox>().Single(t => t.Name == "Input");

        input.Text = "Schlauch";
        PhoneRender.Settle();

        var texts = Texts(phone.Window);
        Assert.Contains("ALT (DIN 5009:1983)", texts);
        Assert.Contains("Schule", texts);
        Assert.Contains("Zacharias", texts);

        phone.Capture("04-funkalphabet.png");

        var din = phone.Window.GetVisualDescendants().OfType<Button>().Single(b => b.Name == "UseDin2022Button");
        Assert.NotNull(din.Command);
        din.Command.Execute(null);
        PhoneRender.Settle();

        texts = Texts(phone.Window);
        Assert.Contains("DIN 5009:2022", texts);
        Assert.Contains("Salzwedel", texts);
        Assert.DoesNotContain("Schule", texts);
    }
}
