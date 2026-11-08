using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Helfer.Composition;
using Helfer.Helpers.DekonPlatz;
using Helfer.Helpers.Funkalphabet;
using Helfer.Helpers.RthLandeplatz;
using Helfer.Helpers.Schaummittel;

namespace Helfer.Acceptance.Tests;

// The screens a new user sees, in order. Doubles as the screenshot source: `make screenshots`,
// `make phone-screenshots` and `make play-screenshots` run this class with RENDER_OUT set.
public class DemoFlowRenderTests
{
    private static List<string?> Texts(Control root) =>
        root.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text).ToList();

    // The words in the spelled list only: the reference table below it shows every code word anyway.
    private static List<string?> SpelledWords(Control root)
    {
        var spelled = root.GetVisualDescendants().OfType<ItemsControl>().Single(c => c.Name == "Spelled");
        Assert.True(spelled.IsVisible);
        return spelled.GetVisualDescendants().OfType<TextBlock>()
            .Where(t => t.Classes.Contains("display")).Select(t => t.Text).ToList();
    }

    // The result lines only, label then value: the form above them holds similar words.
    private static List<string?> ResultTexts(Control root)
    {
        var results = root.GetVisualDescendants().OfType<ItemsControl>().Single(c => c.Name == "Results");
        Assert.True(results.IsEffectivelyVisible);
        return Texts(results);
    }

    private static SchaummittelViewModel OpenSchaummittel(PhoneRender phone)
    {
        phone.Shell.Open(SchaummittelModule.HelperId);
        PhoneRender.Settle();
        return phone.Window.GetVisualDescendants().OfType<Control>()
            .Select(c => c.DataContext).OfType<SchaummittelViewModel>().First();
    }

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

        Assert.Contains("ALT (DIN 5009:1983)", Texts(phone.Window));
        Assert.Equal(["Schule", "Ludwig", "Anton", "Ulrich", "Charlotte"], SpelledWords(phone.Window));

        phone.Capture("04-funkalphabet.png");

        var alt = phone.Window.GetVisualDescendants().OfType<RadioButton>().Single(b => b.Name == "AltTable");
        var din = phone.Window.GetVisualDescendants().OfType<RadioButton>().Single(b => b.Name == "Din2022Table");
        Assert.True(alt.IsChecked);
        din.IsChecked = true;
        PhoneRender.Settle();

        Assert.False(alt.IsChecked);
        Assert.Contains("DIN 5009:2022", Texts(phone.Window));
        Assert.Equal(
            ["Salzwedel", "Chemnitz", "Hamburg", "Leipzig", "Aachen", "Unna", "Chemnitz", "Hamburg"],
            SpelledWords(phone.Window));

        var clear = phone.Window.GetVisualDescendants().OfType<Button>().Single(b => b.Name == "ClearButton");
        Assert.NotNull(clear.Command);
        clear.Command.Execute(null);
        PhoneRender.Settle();

        Assert.True(string.IsNullOrEmpty(input.Text));
    }

    [AvaloniaFact]
    public void The_schaummittel_rechner_works_out_a_room_and_shows_only_that_mode()
    {
        var phone = new PhoneRender();
        var page = OpenSchaummittel(phone);
        var panels = phone.Window.GetVisualDescendants().OfType<StackPanel>().ToList();
        Assert.False(panels.Single(p => p.Name == "RaumFields").IsVisible);

        phone.Window.GetVisualDescendants().OfType<RadioButton>().Single(b => b.Name == "RaumMode").IsChecked = true;
        page.Verschaeumungszahl.Text = "10";
        page.Laenge.Text = "10";
        page.Breite.Text = "5";
        page.Hoehe.Text = "3";
        PhoneRender.Settle();

        Assert.True(page.IsRaum);
        Assert.True(panels.Single(p => p.Name == "RaumFields").IsVisible);
        Assert.False(panels.Single(p => p.Name == "VorratFields").IsVisible);
        Assert.Equal(
            ["Raum", "150,0 m³", "Schaum", "150,0 m³", "Gemisch", "15.000 l", "Schaummittel", "450 l", "Wasser", "14.550 l"],
            ResultTexts(phone.Window));

        phone.Capture("05-schaummittel.png");
    }

    [AvaloniaFact]
    public void A_bad_schaummittel_value_shows_its_error_under_the_field()
    {
        var phone = new PhoneRender();
        var page = OpenSchaummittel(phone);

        page.Zumischrate.Text = "300";
        PhoneRender.Settle();

        var errors = phone.Window.GetVisualDescendants().OfType<TextBlock>()
            .Where(t => t.Classes.Contains("field-error") && t.IsEffectivelyVisible)
            .Select(t => t.Text);
        Assert.Equal(["Von 0,1 bis 50 % möglich."], errors);
    }

    [AvaloniaTheory]
    [InlineData("Zumischrate in %", "1 %")]
    [InlineData("Verschäumungszahl", "LEICHT 500")]
    public void A_schaummittel_preset_sits_beside_its_field_at_glove_size(string field, string preset)
    {
        var phone = new PhoneRender();
        OpenSchaummittel(phone);

        var input = phone.Window.GetVisualDescendants().OfType<TextBox>()
            .Single(t => AutomationProperties.GetName(t) == field);
        var button = phone.Window.GetVisualDescendants().OfType<Button>().Single(b => Equals(b.Content, preset));
        var inputTop = input.TranslatePoint(default, phone.Window);
        var buttonTop = button.TranslatePoint(default, phone.Window);

        Assert.NotNull(inputTop);
        Assert.NotNull(buttonTop);
        Assert.Equal(inputTop.Value.Y, buttonTop.Value.Y, precision: 0);
        Assert.True(button.Bounds.Height >= 48, $"{preset} is {button.Bounds.Height} high");
    }

    // 780 dp is about what a 915 dp phone leaves between its status and navigation bars.
    [AvaloniaFact]
    public void A_whole_schaummittel_room_calculation_fits_without_scrolling()
    {
        var phone = new PhoneRender();
        phone.Window.Height = 780;
        var page = OpenSchaummittel(phone);

        page.IsRaum = true;
        page.Verschaeumungszahl.Text = "10";
        page.Laenge.Text = "10";
        page.Breite.Text = "5";
        page.Hoehe.Text = "3";
        PhoneRender.Settle();

        var form = phone.Window.GetVisualDescendants().OfType<ScrollViewer>().Single(s => s.Name == "Form");
        Assert.True(
            form.Extent.Height <= form.Viewport.Height + 1,
            $"The form is {form.Extent.Height} high in a viewport of {form.Viewport.Height}");
    }

    private static Control Named(PhoneRender phone, string name) =>
        phone.Window.GetVisualDescendants().OfType<Control>().Single(c => c.Name == name);

    private static List<string> VisibleTexts(Control root) =>
        root.GetVisualDescendants().OfType<TextBlock>()
            .Where(t => t.IsEffectivelyVisible)
            .Select(t => t.Text ?? string.Empty)
            .ToList();

    private static List<string?> DrawingNames(Control root) =>
        root.GetVisualDescendants().OfType<UserControl>()
            .Where(c => c.GetType().Name.EndsWith("Drawing", StringComparison.Ordinal))
            .Select(AutomationProperties.GetName)
            .ToList();

    [AvaloniaFact]
    public void The_rth_landeplatz_shows_only_the_values_of_its_mode()
    {
        var phone = new PhoneRender();
        phone.Shell.Open(RthLandeplatzModule.HelperId);
        PhoneRender.Settle();

        Assert.Contains(Landeplatzvorgaben.PilotDecides, VisibleTexts(phone.Window));
        Assert.DoesNotContain(Landeplatzvorgaben.AbortRule, VisibleTexts(phone.Window));
        Assert.Contains("Fläche: 30 m x 30 m oder größer", VisibleTexts(phone.Window));
        Assert.DoesNotContain(VisibleTexts(phone.Window), t => t.Contains("35 m x 70 m", StringComparison.Ordinal));

        phone.Capture("06-rth-landeplatz-tag.png");

        ((RadioButton)Named(phone, "NachtMode")).IsChecked = true;
        PhoneRender.Settle();

        Assert.Contains(Landeplatzvorgaben.PilotDecides, VisibleTexts(phone.Window));
        Assert.Contains(Landeplatzvorgaben.AbortRule, VisibleTexts(phone.Window));
        Assert.Contains("Fläche: 35 m x 70 m oder größer", VisibleTexts(phone.Window));
        Assert.DoesNotContain(VisibleTexts(phone.Window), t => t.Contains("30 m x 30 m", StringComparison.Ordinal));

        phone.Capture("07-rth-landeplatz-nacht.png");
    }

    [AvaloniaFact]
    public void The_pilot_line_and_the_abort_rule_stay_outside_the_scrolling_steps()
    {
        var phone = new PhoneRender();
        phone.Shell.Open(RthLandeplatzModule.HelperId);
        PhoneRender.Settle();

        var scroller = Named(phone, "Sections").GetVisualAncestors().OfType<ScrollViewer>().First();

        Assert.False(scroller.IsVisualAncestorOf(Named(phone, "PilotDecides")));
        Assert.False(scroller.IsVisualAncestorOf(Named(phone, "AbortRule")));
    }

    [AvaloniaFact]
    public void Every_rth_landeplatz_drawing_tells_a_screen_reader_what_it_shows()
    {
        var phone = new PhoneRender();
        phone.Shell.Open(RthLandeplatzModule.HelperId);
        PhoneRender.Settle();

        var tag = DrawingNames(phone.Window);
        ((RadioButton)Named(phone, "NachtMode")).IsChecked = true;
        PhoneRender.Settle();
        var nacht = DrawingNames(phone.Window);

        Assert.Equal(
            Landeplatzvorgaben.Tag.SelectMany(s => s.Drawings).Select(d => d.Description),
            tag);
        Assert.Equal(
            Landeplatzvorgaben.Nacht.SelectMany(s => s.Drawings).Select(d => d.Description),
            nacht);
    }

    [AvaloniaFact]
    public void The_rth_landeplatz_names_its_sources()
    {
        var phone = new PhoneRender();
        phone.Shell.Open(RthLandeplatzModule.HelperId);
        PhoneRender.Settle();

        var texts = Texts(Named(phone, "SourceList"));

        Assert.All(Landeplatzvorgaben.Sources, source => Assert.Contains(source, texts));
        Assert.Contains(Landeplatzvorgaben.SourceNote, texts);
    }

    [AvaloniaFact]
    public void The_dekon_platz_shows_only_the_values_of_its_stufe()
    {
        var phone = new PhoneRender();
        phone.Shell.Open(DekonPlatzModule.HelperId);
        PhoneRender.Settle();

        Assert.Contains("7. Person warm halten", VisibleTexts(phone.Window));
        Assert.DoesNotContain(VisibleTexts(phone.Window), t => t.Contains("10 m x 25 m", StringComparison.Ordinal));
        phone.Capture("08-dekon-stufe-1.png");

        ((RadioButton)Named(phone, "StufeZwei")).IsChecked = true;
        PhoneRender.Settle();

        Assert.Contains("2. Fläche abgrenzen: 9 x 4 m, bei Schwarz-Weiß 8 x 4 m", VisibleTexts(phone.Window));
        Assert.DoesNotContain("7. Person warm halten", VisibleTexts(phone.Window));
        phone.Capture("09-dekon-stufe-2.png");

        ((RadioButton)Named(phone, "StufeDrei")).IsChecked = true;
        PhoneRender.Settle();

        Assert.Contains("Platzbedarf: 10 m x 25 m", VisibleTexts(phone.Window));
        Assert.DoesNotContain(VisibleTexts(phone.Window), t => t.Contains("9 x 4 m", StringComparison.Ordinal));
        phone.Capture("10-dekon-stufe-3.png");
    }

    [AvaloniaFact]
    public void Every_dekon_platz_drawing_tells_a_screen_reader_what_it_shows()
    {
        var phone = new PhoneRender();
        phone.Shell.Open(DekonPlatzModule.HelperId);
        PhoneRender.Settle();

        ((RadioButton)Named(phone, "StufeZwei")).IsChecked = true;
        PhoneRender.Settle();
        var zwei = DrawingNames(phone.Window);
        ((RadioButton)Named(phone, "StufeDrei")).IsChecked = true;
        PhoneRender.Settle();
        var drei = DrawingNames(phone.Window);

        Assert.Equal(Dekonvorgaben.StufeZwei.SelectMany(s => s.Drawings).Select(d => d.Description), zwei);
        Assert.Equal(Dekonvorgaben.StufeDrei.SelectMany(s => s.Drawings).Select(d => d.Description), drei);
    }

    [AvaloniaFact]
    public void Choosing_another_stufe_starts_at_the_top_of_its_page()
    {
        var phone = new PhoneRender();
        phone.Shell.Open(DekonPlatzModule.HelperId);
        PhoneRender.Settle();
        ((RadioButton)Named(phone, "StufeDrei")).IsChecked = true;
        PhoneRender.Settle();
        var scroller = (ScrollViewer)Named(phone, "Scroller");
        scroller.Offset = new Vector(0, 200);
        PhoneRender.Settle();
        Assert.True(scroller.Offset.Y > 100, "The page must have scrolled for this test to mean anything.");

        ((RadioButton)Named(phone, "StufeZwei")).IsChecked = true;
        PhoneRender.Settle();

        Assert.Equal(0, scroller.Offset.Y, precision: 0);
    }

    [AvaloniaFact]
    public void The_dekon_platz_names_its_sources()
    {
        var phone = new PhoneRender();
        phone.Shell.Open(DekonPlatzModule.HelperId);
        PhoneRender.Settle();

        var texts = Texts(Named(phone, "SourceList"));

        Assert.All(Dekonvorgaben.Sources, source => Assert.Contains(source, texts));
        Assert.Contains(Dekonvorgaben.SourceNote, texts);
    }
}
